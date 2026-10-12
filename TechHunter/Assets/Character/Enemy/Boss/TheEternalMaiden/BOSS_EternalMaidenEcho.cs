using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 永遠の少女の分身。分身Prefabには EnemyBase と charaDamage をそのまま付ける。
/// ただし EnemyContoroller / BOSS_TheEternalMaiden は付けない。
/// charaDamage は DamageType=1 にして、本体の DamageType=0 に転送する。
/// </summary>
public class BOSS_EternalMaidenEcho : MonoBehaviour
{
    [Header("分身の必須参照")]
    [SerializeField] EnemyBase localEnemy;
    [Header("複製機の混乱状態（生成時に自動接続）")]
    [SerializeField] EnemyBase replicatorEnemyBase;
    [SerializeField] Rigidbody2D body;
    [SerializeField] Transform animationBody;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] GameObject dashHitbox;
    [SerializeField] ParticleSystem attackParticles;

    [Header("複製機の混乱中：常時パーティクル")]
    [Tooltip("分身Prefabが元から持つ常時再生用ParticleSystem。攻撃用のAttack Particlesとは別のものを指定してください。")]
    [SerializeField] ParticleSystem defaultParticles;
    [Min(0f)][SerializeField] float confusionParticleEmission = 0.2f;

    [Header("複製機の混乱中：被弾0（2枚のSpriteを交互表示）")]
    [SerializeField] Sprite confusionSprite0;
    [SerializeField] Sprite confusionSprite1;
    [Min(0.01f)][SerializeField] float confusionSpriteInterval = 0.2f;

    [Header("複製機から隊列への合流")]
    [Min(0.1f)][SerializeField] float joinSpeed = 32f;
    [Min(0.02f)][SerializeField] float joinDistance = 0.2f;

    BOSS_EternalMaidenReplay owner;
    readonly List<Collider2D> damageColliders = new List<Collider2D>();
    int index;
    int lastEventId;
    bool specialMode;
    bool isVisible = true;
    bool joining;
    bool joined;
    bool pausedByMachine;
    float confusionStartedAt;
    bool phase3Frozen;
    float phase3FrozenUntil;
    float initialSpriteAlpha;
    ParticleSystem.MinMaxCurve originalDefaultEmission;
    bool hasOriginalDefaultEmission;

    public EnemyBase LocalEnemy { get { return localEnemy; } }
    public int Index { get { return index; } }

    /// <summary>生成直後、最初のStart/FixedUpdateより前に本体との転送先を接続する。</summary>
    public bool Initialize(BOSS_EternalMaidenReplay replay, int echoIndex)
    {
        owner = replay;
        index = echoIndex;
        replicatorEnemyBase = owner != null ? owner.ReplicatorEnemyBase : null;
        if (localEnemy == null) localEnemy = GetComponent<EnemyBase>();
        if (body == null) body = GetComponent<Rigidbody2D>();
        if (animationBody == null && localEnemy != null && localEnemy.EnemyAnimBody != null)
            animationBody = localEnemy.EnemyAnimBody.transform;
        if (spriteRenderer == null && localEnemy != null)
            spriteRenderer = localEnemy.spriteRenderer;
        if (spriteRenderer == null && animationBody != null)
            spriteRenderer = animationBody.GetComponentInChildren<SpriteRenderer>(true);

        if (owner == null || owner.OriginalDamage == null || localEnemy == null || spriteRenderer == null || replicatorEnemyBase == null)
        {
            Debug.LogError("永遠の少女の分身: Owner / 本体charaDamage / 分身EnemyBase / SpriteRenderer / 複製機EnemyBase が不足しています", this);
            return false;
        }

        // 分身Prefab自身の透明度を保持。本体のSprite色を模倣してもalphaは上書きしない。
        initialSpriteAlpha = spriteRenderer.color.a;

        // 混乱解除時に戻せるよう、Prefabに設定されたEmissionをそのまま保存する。
        if (defaultParticles != null)
        {
            var defaultEmission = defaultParticles.emission;
            originalDefaultEmission = defaultEmission.rateOverTime;
            hasOriginalDefaultEmission = true;
        }

        // 既存charaDamageをそのまま使用。転送先は本体DamageType=0だけ。
        charaDamage[] hitAreas = GetComponentsInChildren<charaDamage>(true);
        if (hitAreas.Length == 0)
        {
            Debug.LogError("永遠の少女の分身: 被弾用charaDamageがありません", this);
            return false;
        }
        for (int i = 0; i < hitAreas.Length; i++)
        {
            charaDamage receiver = hitAreas[i];
            receiver.DamageType = 1;
            receiver.passDamageAreas.Clear();
            receiver.passDamageAreas.Add(owner.OriginalDamage);

            // 被弾判定用Colliderは初期フレームが揃うまで隠す。
            Collider2D[] colliders = receiver.GetComponents<Collider2D>();
            for (int j = 0; j < colliders.Length; j++)
            {
                if (!damageColliders.Contains(colliders[j]))
                    damageColliders.Add(colliders[j]);
            }
        }

        // UnityのAnimation更新ではなく、元のSpriteを時間差でコピーする。
        if (animationBody != null)
        {
            Animator[] animators = animationBody.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++) animators[i].enabled = false;
        }

        EnemyAttack[] hitboxAttacks = GetComponentsInChildren<EnemyAttack>(true);
        for (int i = 0; i < hitboxAttacks.Length; i++)
            hitboxAttacks[i].enemyBase = localEnemy;

        if (dashHitbox != null) dashHitbox.SetActive(false);
        if (attackParticles != null)
            attackParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // 誕生した瞬間は複製機の位置に留め、隊列へ飛び込むまでは当たり判定なし。
        float when = Time.time - index * owner.DelaySeconds;
        lastEventId = owner.LastEventIdBefore(when);
        SetAttackAndDamageEnabled(false);
        return true;
    }

    public void SetSpecialMode(bool value)
    {
        specialMode = value;
        if (!value && owner != null)
        {
            // 特殊行動中に発生した通常の攻撃は、あとから一気に発射しない。
            lastEventId = owner.LastEventIdBefore(Time.time - index * owner.DelaySeconds);
        }
        if (value && dashHitbox != null) dashHitbox.SetActive(false);
        if (value && attackParticles != null)
            attackParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        // 特殊隊列中は本体の過去の無敵状態を引き継がず、分身は被弾可能にする。
        if (value) SetDamageCollidersEnabled(isVisible);
    }

    // 第三形態の特殊行動中、通常の遅延再生に代わって直接指定された隊列位置へ移動。
    // 移動中でも複製機が混乱したらその場で停止する。
    public bool SpecialMoveTowards(Vector2 destination, float speed, float tolerance)
    {
        if (!specialMode || pausedByMachine || phase3Frozen) return false;
        Vector2 current = body != null ? body.position : (Vector2)transform.position;
        Vector2 next = Vector2.MoveTowards(current, destination,
                                            Mathf.Max(0f, speed) * Time.fixedDeltaTime);
        if (body != null) body.MovePosition(next);
        else transform.position = next;
        return Vector2.Distance(next, destination) <= tolerance;
    }

    // フィニッシュ後は複製機を混乱させず、分身だけが3秒間行動不能になる。
    public void BeginPhase3Recovery(float duration)
    {
        phase3Frozen = true;
        phase3FrozenUntil = Time.time + Mathf.Max(0f, duration);
        confusionStartedAt = Time.time;
        if (body != null)
        {
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
        if (dashHitbox != null) dashHitbox.SetActive(false);
        if (attackParticles != null)
            attackParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        SetDefaultParticleConfused(true);
        // フィニッシュ後の3秒停止中も、本体の無敵フレームを引きずらない。
        SetDamageCollidersEnabled(isVisible);
    }

    // 混乱または第三形態の停止解除後は、通常時の遅延隊列へ高速で復帰する。
    void ReturnToReplayFormation()
    {
        if (owner == null || specialMode || phase3Frozen || pausedByMachine) return;
        float when = Time.time - index * owner.DelaySeconds;
        lastEventId = owner.LastEventIdBefore(when);
        joined = false;
        joining = true;
        SetAttackAndDamageEnabled(false);
    }

    void SyncPhase3Recovery()
    {
        if (!phase3Frozen || Time.time < phase3FrozenUntil) return;
        phase3Frozen = false;
        SetDefaultParticleConfused(pausedByMachine);
        ReturnToReplayFormation();
    }

    public void BeginJoining(Vector3 fromMachine)
    {
        joining = true;
        joined = false;
        transform.position = fromMachine;
        if (body != null) body.position = fromMachine;
        SetAttackAndDamageEnabled(false);
    }

    // ダメージ判定は「隊列状態」で管理し、本体の過去のDamageBody状態から独立させる。
    // 以前のコードでは ApplyFrame がDamageBody自体を非アクティブにできたため、
    // 混乱でApplyFrameが止まると、無敵のまま固定されることがあった。
    void SetDamageCollidersEnabled(bool enabledState)
    {
        // 分身が可視・被弾可能な段階で、PrefabのDamageBodyが以前の処理で
        // 無効になっていても復旧する。初回合流中はColliderだけを無効にする。
        if (enabledState && localEnemy != null && localEnemy.DamageBody != null &&
            localEnemy.DamageBody != gameObject && !localEnemy.DamageBody.activeSelf)
            localEnemy.DamageBody.SetActive(true);

        for (int i = 0; i < damageColliders.Count; i++)
        {
            Collider2D hitbox = damageColliders[i];
            if (hitbox != null && hitbox.enabled != enabledState)
                hitbox.enabled = enabledState;
        }
    }

    void SetAttackAndDamageEnabled(bool enabledState)
    {
        SetDamageCollidersEnabled(enabledState);
        if (!enabledState && dashHitbox != null) dashHitbox.SetActive(false);
        if (!enabledState && attackParticles != null)
            attackParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    // 複製機が混乱（ConfusionResistance）の間、分身の行動再生を停止する。
    // 混乱解除後は停止地点から現在の遅延隊列位置へ高速で戻り、合流後に再生する。
    void SyncMachineConfusion()
    {
        bool confused = replicatorEnemyBase != null &&
                        replicatorEnemyBase.status == EnemyBase.Status.ConfusionResistance;
        if (confused == pausedByMachine) return;
        pausedByMachine = confused;

        if (confused)
        {
            confusionStartedAt = Time.time;
            if (body != null)
            {
                body.velocity = Vector2.zero;
                body.angularVelocity = 0f;
            }
            if (dashHitbox != null) dashHitbox.SetActive(false);
            if (attackParticles != null)
                attackParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            SetDefaultParticleConfused(true);
            // 初回合流などで判定がOFFだった場合も、停止中は攻撃できる。
            SetDamageCollidersEnabled(isVisible);
            return;
        }

        // 第三形態の停止時間がまだ続くなら、エフェクトを元に戻さない。
        SetDefaultParticleConfused(phase3Frozen);
        // 特殊行動中なら円形隊列を続行し、通常時なら遅延隊列へ復帰する。
        if (!specialMode && !phase3Frozen) ReturnToReplayFormation();
    }

    // 常時用パーティクルのEmissionだけを変更する。攻撃用パーティクルには触れない。
    void SetDefaultParticleConfused(bool confused)
    {
        if (defaultParticles == null || !hasOriginalDefaultEmission) return;
        var emission = defaultParticles.emission;
        emission.rateOverTime = confused ?
            new ParticleSystem.MinMaxCurve(confusionParticleEmission) : originalDefaultEmission;
    }

    void OnDisable()
    {
        // 戦闘途中で無効化された場合もPrefab本来のEmissionに復元する。
        SetDefaultParticleConfused(false);
    }

    void FixedUpdate()
    {
        if (owner == null) return;
        SyncMachineConfusion();
        SyncPhase3Recovery();
        if (pausedByMachine || phase3Frozen || specialMode) return;
        float when = Time.time - index * owner.DelaySeconds;
        BOSS_EternalMaidenReplay.Frame frame;
        if (!owner.TrySample(when, out frame)) return;

        if (joining)
        {
            Vector2 next = Vector2.MoveTowards(transform.position, frame.position,
                                                 joinSpeed * Time.fixedDeltaTime);
            if (body != null) body.MovePosition(next);
            else transform.position = next;
            if (Vector2.Distance(next, frame.position) <= joinDistance)
            {
                joining = false;
                joined = true;
                if (body != null) body.MovePosition(frame.position);
                else transform.position = frame.position;
                lastEventId = owner.LastEventIdBefore(when);
                SetAttackAndDamageEnabled(true);
            }
            return;
        }

        if (!joined) return;
        if (body != null)
        {
            body.MovePosition(frame.position);
            body.MoveRotation(frame.rotationZ);
        }
        else
        {
            transform.position = frame.position;
            transform.rotation = Quaternion.Euler(0f, 0f, frame.rotationZ);
        }
    }

    void LateUpdate()
    {
        if (owner == null) return;
        SyncMachineConfusion();
        SyncPhase3Recovery();
        if (pausedByMachine || phase3Frozen)
        {
            ShowConfusionSprite();
            return;
        }
        if (specialMode) return;
        float when = Time.time - index * owner.DelaySeconds;
        if (!joined)
        {
            // 合流中も初期Spriteだけ反映し、過去の攻撃を誤って発射しない。
            BOSS_EternalMaidenReplay.Frame joiningFrame;
            if (spriteRenderer != null && owner.TrySample(when, out joiningFrame))
                spriteRenderer.sprite = joiningFrame.sprite;
            return;
        }
        if (!ApplyFrame(when)) return;
        lastEventId = owner.ReplayEvents(when, lastEventId, localEnemy);
    }

    // 分身のAnimatorはInitializeで無効化しているため、混乱中だけSpriteを直接差し替える。
    // 元の行動を再開すれば、ApplyFrameまたは合流中の更新が通常Spriteに戻す。
    void ShowConfusionSprite()
    {
        if (spriteRenderer == null) return;
        int frameIndex = Mathf.FloorToInt((Time.time - confusionStartedAt) /
                                          Mathf.Max(0.01f, confusionSpriteInterval)) % 2;
        Sprite frame = frameIndex == 0 ? confusionSprite0 : confusionSprite1;
        // Spriteが未設定なら表示を消さずに元の画像を維持する。
        if (frame != null) spriteRenderer.sprite = frame;
    }

    bool ApplyFrame(float when)
    {
        BOSS_EternalMaidenReplay.Frame frame;
        if (owner == null || !owner.TrySample(when, out frame))
        {
            SetVisible(false);
            return false;
        }
        SetVisible(true);
        if (body == null)
        {
            transform.position = frame.position;
            transform.rotation = Quaternion.Euler(0f, 0f, frame.rotationZ);
        }
        if (animationBody != null)
        {
            animationBody.localPosition = frame.visualLocalPosition;
            animationBody.localRotation = frame.visualLocalRotation;
            //animationBody.localScale = frame.visualLocalScale;
        }
        spriteRenderer.sprite = frame.sprite;
        // 被弾可否は本体の記録と独立。既存v15の無敵状態を復旧する。
        if (joined && !joining) SetDamageCollidersEnabled(true);
        Color replayColor = frame.color;
        replayColor.a = initialSpriteAlpha;
        spriteRenderer.color = replayColor;

        if (dashHitbox != null && dashHitbox.activeSelf != frame.dashHitboxActive)
            dashHitbox.SetActive(frame.dashHitboxActive);

        if (attackParticles != null)
        {
            var emission = attackParticles.emission;
            emission.rateOverTime = frame.particleEmission;
            if (frame.particleEmission > 0f && !attackParticles.isPlaying)
                attackParticles.Play();
        }
        return true;
    }

    void SetVisible(bool visible)
    {
        if (isVisible == visible) return;
        isVisible = visible;
        if (spriteRenderer != null) spriteRenderer.enabled = visible;
        // SetVisibleは描画だけでなく、合流・停止による被弾可否を尊重する。
        // 「表示復帰」と同時に合流中の無敵判定を誤って有効化しない。
        SetDamageCollidersEnabled(visible &&
            (joined || pausedByMachine || phase3Frozen || specialMode));
        if (!visible && dashHitbox != null) dashHitbox.SetActive(false);
    }
}
