using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 永遠の少女【本体】に追加する。分身はAIを実行せず、ここの履歴を再生する。
/// 分身nは本体の n * delaySeconds 秒前を再現する。
/// </summary>
public class BOSS_EternalMaidenReplay : MonoBehaviour
{
    [Header("本体の参照")]
    [SerializeField] EnemyBase source;
    [SerializeField] charaDamage originalDamage;
    [SerializeField] GameObject originalDashHitbox;
    [SerializeField] ParticleSystem originalAttackParticles;
    [SerializeField] BOSS_EternalMaidenEcho echoPrefab;

    [Header("分身生成（複製機設置後のみ）")]
    [Min(0.05f)][SerializeField] float delaySeconds = 0.7f;
    [Range(0.01f, 1f)][SerializeField] float hpLossPerEcho = 0.07f;
    [Min(0)][SerializeField] int maxEchoes = 12;
    [Min(0f)][SerializeField] float echoSpawnInterval = 0.25f;

    [Header("分身出現演出")]
    [SerializeField] GameObject echoSpawnEffectPrefab;
    [Min(0f)][SerializeField] float echoSpawnDelay = 1f;
    [Min(0f)][SerializeField] float echoSpawnEffectLifetime = 3f;
    [SerializeField] AudioClip echoSpawnSound;

    AudioManager echoSpawnAudio => AudioManager.instance;

    [Header("記録")]
    [Min(0.005f)][SerializeField] float sampleInterval = 0.02f;

    public EnemyBase Source { get { return source; } }
    public EnemyBase ReplicatorEnemyBase { get { return replicatorEnemyBase; } }
    public charaDamage OriginalDamage { get { return originalDamage; } }
    public float DelaySeconds { get { return delaySeconds; } }
    public int EchoCount { get { return echoes.Count; } }
    public bool IsMachineConfused
    {
        get
        {
            return replicatorEnemyBase != null &&
                   replicatorEnemyBase.status == EnemyBase.Status.ConfusionResistance;
        }
    }

    public struct Frame
    {
        public float time;
        public Vector2 position;
        public float rotationZ;
        public Vector3 visualLocalPosition;
        public Quaternion visualLocalRotation;
        public Vector3 visualLocalScale;
        public Sprite sprite;
        public Color color;
        public bool dashHitboxActive;
        public bool damageBodyActive;
        public float particleEmission;
    }

    public struct SpawnEvent
    {
        public int id;
        public float time;
        public GameObject prefab;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localScale;
        public float lifetime;
        public bool hasRigidbody;
        public Vector2 velocity;
        public float angularVelocity;
        public bool useImpulse;
        public Vector2 impulse;
        public float effectAnimatorSpeed;
        // 0=記録通り / 1=弾を発射時に再照準 / 2=レーザーを発射時に再照準＋ブレ
        public int aimMode;
        public float aimSpread;
        public float aimAngleOffset;
        // 分身自身の位置から発射できるよう、本体からの相対位置を保存する。
        public Vector3 spawnOffset;
    }

    readonly List<Frame> frames = new List<Frame>();
    readonly List<SpawnEvent> spawnEvents = new List<SpawnEvent>();
    readonly List<BOSS_EternalMaidenEcho> echoes = new List<BOSS_EternalMaidenEcho>();

    float maxHp;
    float lastSampleTime = float.NegativeInfinity;
    int nextEventId;
    int totalCreated;
    bool specialAction;
    bool machineActive;
    float machineUnlockHpRate = 0.93f;
    float nextEchoSpawnTime;
    bool echoSpawnPending;
    Transform machineTransform;
    // 混乱の解除は複製機自身が担当。ここでは現在の状態を参照するだけ。
    EnemyBase replicatorEnemyBase;

    void Awake()
    {
        if (source == null) source = GetComponent<EnemyBase>();
        if (originalDamage == null && source != null)
        {
            originalDamage = source.charaDamage != null ? source.charaDamage : source.CharaDamage;
            if (originalDamage == null) originalDamage = source.GetComponentInChildren<charaDamage>(true);
        }
    }

    void Start()
    {
        if (source == null || originalDamage == null || echoPrefab == null)
        {
            Debug.LogError("永遠の少女: Source / Original Damage / Echo Prefab を設定してください", this);
            enabled = false;
            return;
        }
        if (originalDamage.DamageType != 0)
        {
            Debug.LogError("永遠の少女: 本体の charaDamage は DamageType=0 にしてください", this);
            enabled = false;
            return;
        }
        maxHp = source.MAXHP;
        RecordFrame();
    }

    void LateUpdate()
    {
        if (source == null || source.HP <= 0f || source.status == EnemyBase.Status.Die)
        {
            ClearEchoes();
            enabled = false;
            return;
        }

        if (Time.time - lastSampleTime >= sampleInterval) RecordFrame();

        // 第一形態では絶対に分身しない。複製機を地面に置いた後にのみ生成。
        if (machineActive && machineTransform != null && replicatorEnemyBase != null &&
            replicatorEnemyBase.status != EnemyBase.Status.ConfusionResistance &&
            replicatorEnemyBase.status != EnemyBase.Status.Die &&
            !specialAction && !echoSpawnPending && maxHp > 0f &&
            hpLossPerEcho > 0f && totalCreated < maxEchoes && Time.time >= nextEchoSpawnTime)
        {
            float hpRate = source.HP / maxHp;
            int wanted = hpRate < machineUnlockHpRate ?
                1 + Mathf.FloorToInt((machineUnlockHpRate - hpRate) / hpLossPerEcho) : 1;
            wanted = Mathf.Clamp(wanted, 0, maxEchoes);
            if (totalCreated < wanted)
                StartCoroutine(SpawnEchoWithEffect());
        }
        PruneHistory();
    }

    // P1→P2の投擲が終わった時にボス側から呼び出す。
    public void ActivateMachine(Transform machine, float unlockHpRate, EnemyBase machineEnemyBase = null)
    {
        machineTransform = machine;
        machineUnlockHpRate = unlockHpRate;
        // 複製機は実行時にInstantiateされるため、Prefabには事前にシーン参照を入れられない。
        // 投擲済みの複製機から参照を取り、すべての分身へ自動的に渡す。
        replicatorEnemyBase = machineEnemyBase != null ? machineEnemyBase :
            (machineTransform != null ? machineTransform.GetComponentInParent<EnemyBase>() : null);
        machineActive = machineTransform != null && replicatorEnemyBase != null;
        if (!machineActive)
            Debug.LogError("永遠の少女: 複製機のEnemyBaseが見つかりません。複製機PrefabにEnemyBaseを付けてください。", this);
        nextEchoSpawnTime = Time.time;
    }

    // まず複製機からエフェクトを出し、指定時間後に分身が現れる。
    // 生成予約中は次の予約を作らず、エフェクトの多重生成を防ぐ。
    IEnumerator SpawnEchoWithEffect()
    {
        echoSpawnPending = true;

        if (machineTransform != null && echoSpawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(echoSpawnEffectPrefab, machineTransform.position, Quaternion.identity);

            // Play On Awakeが無効なParticleSystemでも再生されるようにする。
            echoSpawnAudio.PlaySE(echoSpawnSound);
            ParticleSystem[] particles = effect.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particles.Length; i++)
                particles[i].Play();
            if (echoSpawnEffectLifetime > 0f)
                Destroy(effect, echoSpawnEffectLifetime);
        }

        if (echoSpawnDelay > 0f)
            yield return new WaitForSeconds(echoSpawnDelay);

        // 演出中に複製機が混乱したら、混乱解除まで出現を延期する。
        // 本体の特殊行動中も同様に出現しない。
        while (machineActive && machineTransform != null && replicatorEnemyBase != null &&
               source != null && source.HP > 0f && source.status != EnemyBase.Status.Die &&
               replicatorEnemyBase.status != EnemyBase.Status.Die &&
               (replicatorEnemyBase.status == EnemyBase.Status.ConfusionResistance || specialAction))
        {
            yield return null;
        }

        bool canSpawn = machineActive && machineTransform != null && replicatorEnemyBase != null &&
                        source != null && source.HP > 0f && source.status != EnemyBase.Status.Die &&
                        replicatorEnemyBase.status != EnemyBase.Status.Die &&
                        !specialAction && totalCreated < maxEchoes;
        if (canSpawn && CreateEcho())
            nextEchoSpawnTime = Time.time + echoSpawnInterval;

        echoSpawnPending = false;
    }

    bool CreateEcho()
    {
        if (echoPrefab == null || machineTransform == null) return false;
        int index = totalCreated + 1;
        BOSS_EternalMaidenEcho echo = Instantiate(echoPrefab, machineTransform.position, Quaternion.identity);
        if (!echo.Initialize(this, index))
        {
            Destroy(echo.gameObject);
            enabled = false;
            return false;
        }
        echo.BeginJoining(machineTransform.position);
        echoes.Add(echo);
        totalCreated++;
        return true;
    }


    void RecordFrame()
    {
        Transform visual = source.EnemyAnimBody != null ? source.EnemyAnimBody.transform : transform;
        SpriteRenderer sprite = source.spriteRenderer;

        float particleRate = 0f;
        if (originalAttackParticles != null)
        {
            var emission = originalAttackParticles.emission;
            particleRate = emission.rateOverTime.constant;
        }

        frames.Add(new Frame
        {
            time = Time.time,
            position = transform.position,
            rotationZ = transform.eulerAngles.z,
            visualLocalPosition = visual.localPosition,
            visualLocalRotation = visual.localRotation,
            visualLocalScale = visual.localScale,
            sprite = sprite != null ? sprite.sprite : null,
            color = sprite != null ? sprite.color : Color.white,
            dashHitboxActive = originalDashHitbox != null && originalDashHitbox.activeSelf,
            damageBodyActive = source.DamageBody == null || source.DamageBody.activeSelf,
            particleEmission = particleRate
        });
        lastSampleTime = Time.time;
    }

    public bool TrySample(float when, out Frame result)
    {
        result = default(Frame);
        if (frames.Count == 0 || when < frames[0].time) return false;

        int end = frames.Count - 1;
        if (when >= frames[end].time)
        {
            result = frames[end];
            return true;
        }

        // 二分探索。分身数が増えても履歴探索が重くなりにくい。
        int lo = 0;
        int hi = end;
        while (lo + 1 < hi)
        {
            int mid = (lo + hi) / 2;
            if (frames[mid].time <= when) lo = mid;
            else hi = mid;
        }
        {
            Frame left = frames[lo];
            Frame right = frames[hi];

            float t = Mathf.InverseLerp(left.time, right.time, when);
            result = left;
            result.position = Vector2.Lerp(left.position, right.position, t);
            result.rotationZ = Mathf.LerpAngle(left.rotationZ, right.rotationZ, t);
            result.visualLocalPosition = Vector3.Lerp(left.visualLocalPosition, right.visualLocalPosition, t);
            result.visualLocalRotation = Quaternion.Slerp(left.visualLocalRotation, right.visualLocalRotation, t);
            result.visualLocalScale = Vector3.Lerp(left.visualLocalScale, right.visualLocalScale, t);
            // Sprite/当たり判定はフレーム境界で切り替える（補間しない）。
            return true;
        }
    }

    /// <summary>
    /// 本体で発射体を完成させた直後に呼ぶ。
    /// AddForce(..., Impulse) 型の弾は impulse を指定する。
    /// aimMode: 0=記録通り / 1=プレイヤー方向に弾を再照準 / 2=レーザーを再照準＋ランダムブレ。
    /// aimAngleOffset: 弾Prefabの向き補正（通常0度。ShotArrowは90度）。
    /// </summary>
    public void RecordSpawn(GameObject prefab, GameObject instance, float lifetime,
                            Vector2? impulse = null, Vector2? initialVelocity = null,
                            int aimMode = 0, float aimSpread = 0f, float aimAngleOffset = 0f)
    {
        if (prefab == null || instance == null) return;

        Rigidbody2D rb = instance.GetComponent<Rigidbody2D>();
        SaveComponent save = instance.GetComponent<SaveComponent>();
        spawnEvents.Add(new SpawnEvent
        {
            id = ++nextEventId,
            time = Time.time,
            prefab = prefab,
            position = instance.transform.position,
            rotation = instance.transform.rotation,
            localScale = instance.transform.localScale,
            lifetime = lifetime,
            hasRigidbody = rb != null,
            velocity = initialVelocity ?? (rb != null ? rb.velocity : Vector2.zero),
            angularVelocity = rb != null ? rb.angularVelocity : 0f,
            useImpulse = impulse.HasValue,
            impulse = impulse.GetValueOrDefault(),
            effectAnimatorSpeed = save != null && save.animator != null ? save.animator.speed : -1f,
            aimMode = aimMode,
            aimSpread = aimSpread,
            aimAngleOffset = aimAngleOffset,
            spawnOffset = instance.transform.position - transform.position
        });
    }

    public int LastEventIdBefore(float when)
    {
        for (int i = spawnEvents.Count - 1; i >= 0; i--)
            if (spawnEvents[i].time <= when) return spawnEvents[i].id;
        return 0;
    }

    /// <summary>各分身が自分の再生位置から呼び出す。イベントIDは分身ごとに保持。</summary>
    public int ReplayEvents(float when, int lastEventId, EnemyBase attackOwner)
    {
        for (int i = 0; i < spawnEvents.Count; i++)
        {
            SpawnEvent e = spawnEvents[i];
            if (e.id <= lastEventId) continue;
            if (e.time > when) break;
            CreateAttackCopy(e, attackOwner);
            lastEventId = e.id;
        }
        return lastEventId;
    }

    void CreateAttackCopy(SpawnEvent e, EnemyBase attackOwner)
    {
        if (e.prefab == null) return;

        // 照準を変える攻撃は、発射元も本体ではなく分身の現在位置に合わせる。
        Vector3 spawnPos = e.position;
        if (e.aimMode != 0 && attackOwner != null)
            spawnPos = attackOwner.transform.position + e.spawnOffset;

        GameObject obj = Instantiate(e.prefab, spawnPos, e.rotation);
        obj.transform.localScale = e.localScale;

        Vector2 velocity = e.velocity;
        Vector2 impulse = e.impulse;

        if (e.aimMode != 0 && attackOwner != null)
        {
            Transform player = attackOwner.Player != null ? attackOwner.Player.transform :
                               (source != null && source.Player != null ? source.Player.transform : null);
            if (player != null)
            {
                Vector2 direction = (Vector2)(player.position - obj.transform.position);
                if (direction.sqrMagnitude > 0.0001f)
                {
                    direction.Normalize();
                    // レーザーは本体のA4と同じく180度反転＋ランダムなブレ。
                    if (e.aimMode == 2)
                    {
                        obj.transform.up = direction;
                        obj.transform.Rotate(0f, 0f, 180f);
                        obj.transform.Rotate(0f, 0f, Random.Range(-e.aimSpread, e.aimSpread));
                    }
                    else if (e.aimMode == 1)
                    {
                        // 通常弾は発射時点のプレイヤー方向へ向ける。
                        obj.transform.up = direction;
                        if (e.aimAngleOffset != 0f)
                            obj.transform.Rotate(0f, 0f, e.aimAngleOffset);
                        velocity = direction * e.velocity.magnitude;
                        impulse = direction * e.impulse.magnitude;
                    }
                }
            }
        }

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null && e.hasRigidbody)
        {
            rb.velocity = velocity;
            rb.angularVelocity = e.angularVelocity;
            if (e.useImpulse) rb.AddForce(impulse, ForceMode2D.Impulse);
        }

        SaveComponent save = obj.GetComponent<SaveComponent>();
        if (save != null && save.animator != null && e.effectAnimatorSpeed >= 0f)
            save.animator.speed = e.effectAnimatorSpeed;

        EnemyAttack[] attacks = obj.GetComponentsInChildren<EnemyAttack>(true);
        for (int i = 0; i < attacks.Length; i++)
            attacks[i].enemyBase = attackOwner != null ? attackOwner : source;

        if (e.lifetime > 0f) Destroy(obj, e.lifetime);
    }

    // 特殊攻撃は未確定。通常の模倣を一時停止/再開する入口だけ用意。
    // BeginSpecialAction 後の分身の移動や攻撃は、専用の特殊攻撃コードで指揮する。
    public void BeginSpecialAction()
    {
        specialAction = true;
        for (int i = 0; i < echoes.Count; i++)
            if (echoes[i] != null) echoes[i].SetSpecialMode(true);
    }

    public void EndSpecialAction()
    {
        specialAction = false;
        for (int i = 0; i < echoes.Count; i++)
            if (echoes[i] != null) echoes[i].SetSpecialMode(false);
    }

    public List<BOSS_EternalMaidenEcho> GetEchoes()
    {
        echoes.RemoveAll(e => e == null);
        return new List<BOSS_EternalMaidenEcho>(echoes);
    }

    void PruneHistory()
    {
        float oldest = Time.time - delaySeconds * Mathf.Max(1, maxEchoes) - 1f;
        int removeFrames = 0;
        while (removeFrames + 1 < frames.Count - 1 && frames[removeFrames + 1].time < oldest)
            removeFrames++;
        if (removeFrames > 0) frames.RemoveRange(0, removeFrames);

        int removeEvents = 0;
        while (removeEvents < spawnEvents.Count && spawnEvents[removeEvents].time < oldest)
            removeEvents++;
        if (removeEvents > 0) spawnEvents.RemoveRange(0, removeEvents);
    }

    void ClearEchoes()
    {
        for (int i = 0; i < echoes.Count; i++)
            if (echoes[i] != null) Destroy(echoes[i].gameObject);
        echoes.Clear();
    }

    void OnDestroy() { ClearEchoes(); }
}
