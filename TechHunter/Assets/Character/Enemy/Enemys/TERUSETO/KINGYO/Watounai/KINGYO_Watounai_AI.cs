using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// ワトウナイ：本体は移動せず、硬質な頭部だけを画面端まで射出する。
// 攻撃中は首を弱点として露出させ、最後の収納中は一方的な反撃機会になる。
// 既存のcharaDamage.DamageType=1によるダメージ転送を使う。
[DefaultExecutionOrder(200)]
public class KINGYO_Watounai_AI : EnemyMoveBase
{
    [Header("首と頭部（本体の子オブジェクト）")]
    [SerializeField] Transform headTransform;
    [SerializeField] Vector2 headSize = Vector2.one; // 頭部の元サイズに対する倍率
    [SerializeField] Animator headAnimator;
    [SerializeField] SpriteRenderer headSprite;
    [SerializeField] SpriteRenderer neckSprite;
    [SerializeField] BoxCollider2D neckDamageCollider;
    [SerializeField] CircleCollider2D headDamageCollider;
    [SerializeField] Collider2D headAttackCollider;
    [SerializeField] EnemyAttack headAttack;
    [SerializeField] charaDamage neckDamage;
    [SerializeField] charaDamage headDamage;
    [SerializeField] charaDamage weakDamageReceiver;
    [SerializeField] charaDamage bodyDamage;
    [SerializeField, Min(0f)] float weakNeckDefense = 0f;
    [SerializeField, Min(0.05f)] float neckColliderThickness = 0.45f;
    [SerializeField, Min(0.05f)] float neckVisualThicknessScale = 1f;
    [SerializeField] Vector2 neckBaseOffset = new Vector2(0f, 0.15f);
    [SerializeField] bool neckSpriteAlongX = true;
    [SerializeField] float headAngleOffset = 0f;

    [Header("連結首プール")]
    [SerializeField, Min(0.15f)] float neckSegmentSpacing = 0.65f;
    [SerializeField, Min(1)] int neckDamageSegmentInterval = 3; // 3なら3節に1節だけ被弾
    [SerializeField] Vector2 neckSegmentSize = Vector2.one; // 各首パーツの見た目と判定を一緒に拡縮
    [SerializeField, Min(1)] int initialNeckSegments = 40;
    [SerializeField, Min(1)] int maxNeckSegments = 100;
    [SerializeField, Min(0.1f)] float neckFollowTightness = 18f;
    [SerializeField, Min(0f)] float neckWaveAmplitude = 0.45f;
    [SerializeField, Min(0f)] float neckWaveFrequency = 8f;
    readonly List<Transform> neckPool = new List<Transform>();
    readonly List<SpriteRenderer> neckRenderers = new List<SpriteRenderer>();
    readonly List<BoxCollider2D> neckColliders = new List<BoxCollider2D>();
    readonly List<Vector2> neckPoints = new List<Vector2>();
    readonly List<Vector2> headTrail = new List<Vector2>();
    Vector3 initialHeadScale = Vector3.one;
    Vector2 headVelocity;
    Vector2 previousHeadPosition;
    float neckTime;

    [Header("射程と待機")]
    [SerializeField, Min(1f)] float minimumHeadReach = 20f;
    [SerializeField, Min(0f)] float attackRangeMargin = 0.5f;
    [SerializeField, Min(0.1f)] float closeRange = 3f;
    [SerializeField, Min(0f)] float shootingRestTime = 0.65f;

    [Header("攻撃1：ハープーンの高速着弾")]
    [SerializeField, Min(0.05f)] float windupTime = 0.5f;
    [SerializeField, Min(0f)] float rushInitialSpeed = 1.5f;
    [SerializeField, Min(0f)] float rushSlowStartTime = 0.14f;
    [SerializeField, Min(1f)] float rushAcceleration = 1100f;
    [SerializeField, Min(1f)] float rushSpeed = 260f;
    [SerializeField, Min(0.1f)] float impactRadius = 1.4f;
    [SerializeField, Min(0f)] float impactPowerPercent = 100f;
    [SerializeField, Min(0.05f)] float impactWait = 0.5f;
    [SerializeField, Range(1, 10)] int maximumRushes = 3;
    [SerializeField] GameObject impactWarningPrefab;
    [SerializeField] GameObject harpoonImpactEffect;
    [SerializeField, Min(0.1f)] float harpoonImpactEffectLifetime = 3f;

    [Header("ハープーンの移動予測")]
    [SerializeField] bool predictTargetMovement = true;
    [SerializeField, Range(0f, 1f)] float predictionStrength = 0.8f;
    [SerializeField, Min(0f)] float maxPredictionDistance = 5f;
    [SerializeField, Min(1f)] float maxTrackedTargetSpeed = 22f;
    [SerializeField, Range(0.01f, 1f)] float velocitySmoothing = 0.25f;

    [Header("着弾時：進行方向へ扇状の岩針")]
    [SerializeField] GameObject rockNeedlePrefab;
    [SerializeField, Range(3, 25)] int rockNeedlesPerRow = 11;
    [SerializeField, Range(1, 8)] int rockNeedleRows = 4;
    [SerializeField, Range(15f, 180f)] float rockFanAngle = 110f;
    [SerializeField, Min(0.1f)] float rockRowSpacing = 2.5f;
    [SerializeField, Range(0f, 1f)] float rockAngleJitter = 0.6f;
    [SerializeField, Range(0f, 1f)] float rockDistanceJitter = 0.35f;
    [SerializeField, Min(0f)] float rockDelayPerUnit = 0.065f;
    [SerializeField, Min(0f)] float rockRandomDelay = 0.18f;
    [SerializeField, Min(0f)] float rockNeedleLifetime = 5f;

    [Header("攻撃2：跳躍と咆哮")]
    [SerializeField, Min(0.1f)] float jumpDuration = 0.85f;
    [SerializeField, Min(0f)] float jumpArcHeight = 3f;
    [SerializeField, Min(0.1f)] float roarWarningTime = 1f;
    [SerializeField, Min(0.25f)] float roarRadius = 3f;
    [SerializeField, Min(0f)] float roarPowerPercent = 100f;
    [SerializeField] GameObject roarWarningPrefab;
    [Header("攻撃判定の可視化（Sceneビュー）")]
    [SerializeField] bool showAttackGizmos = true;

    [Header("攻撃3：低速追尾・噛みつき")]
    [SerializeField, Min(0.1f)] float slowApproachSpeed = 2f;
    [SerializeField, Min(0.2f)] float biteStartDistance = 1.1f;
    [SerializeField, Min(0.1f)] float biteSpeed = 12f;
    [SerializeField, Min(0.05f)] float biteDuration = 0.3f;
    [SerializeField, Min(0.1f)] float biteReach = 1.6f;
    [SerializeField, Min(0.1f)] float approachTimeout = 4f;

    [Header("収納：攻撃チャンス")]
    [SerializeField, Min(0.1f)] float neckReturnSpeed = 10f;
    [SerializeField, Range(0f, 0.95f)] float neckReturnAcceleration = 0.65f;
    [SerializeField, Min(0.05f)] float afterReturnRecovery = 0.55f;

    const int Windup = 0;
    const int Rush = 1;
    const int ImpactPause = 2;
    const int RoarWarning = 3;
    const int RoarAfter = 4;
    const int Jump = 5;
    const int SlowApproach = 6;
    const int Bite = 7;
    const int Retract = 8;
    const int Recovery = 9;

    bool attacking;
    bool headExposed;
    int state;
    int attackFamily;
    int nextCloseAttack = 2;
    int rushCount;
    float stateTime;
    float delayUntilNextAttack;
    float currentRushSpeed;
    Quaternion retractHeadRotation;
    readonly List<Vector2> retractPath = new List<Vector2>();
    float retractTotalDistance;
    float retractDistanceAdvanced;
    float retractElapsed;
    Vector2 lockedPoint;
    Vector2 jumpStart;
    Vector2 jumpLanding;
    Vector2 currentHeadPosition;
    Vector2 lastHeadDirection = Vector2.right;
    Vector2 rushImpactDirection = Vector2.right;
    Transform trackedTarget;
    Vector2 lastTrackedPosition;
    Vector2 trackedVelocity;
    bool hasTrackedPosition;
    RigidbodyType2D previousBodyType;
    bool bodyTypeChanged;
    readonly List<GameObject> activeWarnings = new List<GameObject>();
    Material fallbackWarningMaterial;
    readonly List<Coroutine> rockFanCoroutines = new List<Coroutine>();

    Transform Target => CurrentTarget;
    Vector2 NeckBase => (Vector2)enemyBase.transform.position + neckBaseOffset;
    float HeadReach
    {
        get
        {
            // 固定画面の場合もカメラの表示幅以上の射程を確保する。
            Camera camera = Camera.main;
            if (camera != null && camera.orthographic)
                return Mathf.Max(minimumHeadReach,
                    2f * camera.orthographicSize * camera.aspect);
            return minimumHeadReach;
        }
    }

    void Awake()
    {
        if (enemyBase == null)
            enemyBase = GetComponentInParent<EnemyBase>();
    }

    void Start()
    {
        if (enemyBase == null) enemyBase = GetComponentInParent<EnemyBase>();
        if (enemyBase == null) { enabled = false; return; }
        if (headTransform != null) initialHeadScale = headTransform.localScale;
        ApplyHeadSize();

        if (bodyDamage == null)
            bodyDamage = enemyBase.transform.Find("DamageArea") != null
                ? enemyBase.transform.Find("DamageArea").GetComponent<charaDamage>()
                : enemyBase.CharaDamage;
        if (bodyDamage == null) bodyDamage = enemyBase.charaDamage;

        // 首の被弾 -> 低防御の受け口 -> 本体HP。
        // 頭部の被弾 -> 本体の通常判定（硬い頭部の防御力）。
        if (weakDamageReceiver != null)
        {
            weakDamageReceiver.DamageType = 0;
            weakDamageReceiver.DF = weakNeckDefense;
        }
        if (neckDamage != null && weakDamageReceiver != null)
        {
            neckDamage.DamageType = 1;
            neckDamage.passDamageAreas.Clear();
            neckDamage.passDamageAreas.Add(weakDamageReceiver);
        }
        if (headDamage != null && bodyDamage != null)
        {
            headDamage.DamageType = 1;
            headDamage.passDamageAreas.Clear();
            headDamage.passDamageAreas.Add(bodyDamage);
        }
        if (headAttack != null)
        {
            EnemyContoroller controller = enemyBase as EnemyContoroller;
            if (controller != null) controller.ConfigureAttackTarget(headAttack);
            else headAttack.ConfigureAttack(enemyBase, headAttack.AttackPowerPercent);
        }

        enemyBase.LookPlayerDistance = Mathf.Max(1f, HeadReach - attackRangeMargin);
        InitializeNeckPool();
        HideExtendedParts();
    }

    void ApplyHeadSize()
    {
        if (headTransform == null) return;
        headTransform.localScale = new Vector3(
            initialHeadScale.x * Mathf.Max(0.05f, headSize.x),
            initialHeadScale.y * Mathf.Max(0.05f, headSize.y),
            initialHeadScale.z);
    }

    void InitializeNeckPool()
    {
        if (neckSprite == null) return;
        // 既存のWatounaiNeckを雛形として利用し、攻撃中はクローンのみを表示する。
        // InstantiateでcharaDamage/Colliderも複製されるため共通クラスの変更は不要。
        for (int i = 0; i < Mathf.Min(initialNeckSegments, maxNeckSegments); i++)
            AddNeckSegment();
        neckSprite.gameObject.SetActive(false);
    }

    void AddNeckSegment()
    {
        if (neckSprite == null || neckPool.Count >= maxNeckSegments) return;
        GameObject segment = Instantiate(neckSprite.gameObject, enemyBase.transform);
        segment.name = "WatounaiNeckSegment_" + neckPool.Count;
        segment.transform.localScale = new Vector3(Mathf.Max(0.05f, neckSegmentSize.x),
            Mathf.Max(0.05f, neckSegmentSize.y), 1f);
        SpriteRenderer renderer = segment.GetComponent<SpriteRenderer>();
        BoxCollider2D collider = segment.GetComponent<BoxCollider2D>();
        charaDamage damage = segment.GetComponent<charaDamage>();
        RepairMissingDamageOwners(segment);
        if (damage != null && weakDamageReceiver != null)
        {
            damage.DamageType = 1;
            damage.passDamageAreas.Clear();
            damage.passDamageAreas.Add(weakDamageReceiver);
        }
        if (collider != null)
        {
            collider.size = neckSpriteAlongX
                ? new Vector2(neckSegmentSpacing * 1.1f, neckColliderThickness)
                : new Vector2(neckColliderThickness, neckSegmentSpacing * 1.1f);
            collider.enabled = false;
        }
        if (renderer != null) renderer.enabled = false;
        segment.SetActive(false);
        neckPool.Add(segment.transform);
        neckRenderers.Add(renderer);
        neckColliders.Add(collider);
    }

    void ResetNeckPath()
    {
        headTrail.Clear();
        neckPoints.Clear();
        previousHeadPosition = NeckBase;
        headVelocity = Vector2.zero;
        neckTime = 0f;
        headTrail.Add(NeckBase);
    }

    // 軌跡を頭側から遡り、首の各節を同じ間隔で配置する。
    void UpdateNeckChain(float dt)
    {
        if (neckPool.Count == 0) return;
        ApplyHeadSize();
        neckTime += dt;
        Vector3 desiredScale = new Vector3(Mathf.Max(0.05f, neckSegmentSize.x),
            Mathf.Max(0.05f, neckSegmentSize.y), 1f);
        Vector2 root = NeckBase;
        Vector2 tip = currentHeadPosition;
        Vector2 displacement = tip - previousHeadPosition;
        if (dt > 0f) headVelocity = Vector2.Lerp(headVelocity, displacement / dt, 0.35f);
        previousHeadPosition = tip;

        if (headTrail.Count == 0) headTrail.Add(root);
        // 先頭は常に現在の頭部。頭部の軌跡を記録する。
        if (Vector2.Distance(headTrail[headTrail.Count - 1], tip) > 0.02f)
            headTrail.Add(tip);
        else
            headTrail[headTrail.Count - 1] = tip;

        float maximumTrail = maxNeckSegments * neckSegmentSpacing * 2f;
        float trailLength = 0f;
        for (int i = headTrail.Count - 1; i > 0; i--)
        {
            trailLength += Vector2.Distance(headTrail[i], headTrail[i - 1]);
            if (trailLength > maximumTrail)
            {
                headTrail.RemoveRange(0, i);
                break;
            }
        }
        // 伸びた首の距離に応じて表示数を調整。軌跡の曲がりを多少残せる余裕を取る。
        int count = Mathf.Clamp(Mathf.CeilToInt(Vector2.Distance(root, tip) /
            neckSegmentSpacing * 1.2f), 0, maxNeckSegments);
        while (neckPool.Count < count) AddNeckSegment();

        neckPoints.Clear();
        float remaining = neckSegmentSpacing * 0.5f;
        Vector2 cursor = tip;
        for (int i = headTrail.Count - 2; i >= 0 && neckPoints.Count < count; i--)
        {
            Vector2 target = headTrail[i];
            float length = Vector2.Distance(cursor, target);
            while (length >= remaining && neckPoints.Count < count)
            {
                cursor = Vector2.MoveTowards(cursor, target, remaining);
                neckPoints.Add(cursor);
                length = Vector2.Distance(cursor, target);
                remaining = neckSegmentSpacing;
            }
            remaining -= length;
            cursor = target;
        }

        // 記録が短いときは根元へ補間。根元は常に本体に固定する。
        while (neckPoints.Count < count)
        {
            Vector2 last = neckPoints.Count > 0 ? neckPoints[neckPoints.Count - 1] : tip;
            neckPoints.Add(Vector2.MoveTowards(last, root, neckSegmentSpacing));
        }
        // 軌跡が蛇行していても、根元が置き去りにならないよう補正する。
        for (int i = 0; i < neckPoints.Count; i++)
        {
            float anchorWeight = Mathf.Pow((i + 1f) / count, 3f);
            Vector2 straight = Vector2.Lerp(tip, root, (i + 0.5f) / count);
            Vector2 position = Vector2.Lerp(neckPoints[i], straight, anchorWeight);
            // 曲がりを残しつつ、首に小さな波を伝える。
            Vector2 axis = (tip - root).normalized;
            Vector2 normal = new Vector2(-axis.y, axis.x);
            float wave = Mathf.Sin(neckTime * neckWaveFrequency - i * 0.65f)
                * neckWaveAmplitude * Mathf.Sin(Mathf.PI * (i + 0.5f) / count);
            position += normal * wave;
            Transform segment = neckPool[i];
            if (segment.localScale != desiredScale) segment.localScale = desiredScale;
            if (!segment.gameObject.activeSelf)
            {
                segment.position = position;
                segment.gameObject.SetActive(true);
            }
            else
                segment.position = Vector2.Lerp(segment.position, position,
                    1f - Mathf.Exp(-neckFollowTightness * dt));
            Vector2 next = i == 0 ? tip : (Vector2)neckPool[i - 1].position;
            Vector2 prev = i == count - 1 ? root : neckPoints[i + 1];
            Vector2 tangent = next - prev;
            float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
            segment.rotation = Quaternion.Euler(0f, 0f, angle + (neckSpriteAlongX ? 0f : -90f));
            if (neckRenderers[i] != null) neckRenderers[i].enabled = true;
            if (neckColliders[i] != null)
                neckColliders[i].enabled = i % Mathf.Max(1, neckDamageSegmentInterval) == 0;
        }
        for (int i = count; i < neckPool.Count; i++)
        {
            if (neckColliders[i] != null) neckColliders[i].enabled = false;
            neckPool[i].gameObject.SetActive(false);
        }
    }

    public override void EnemyMovePlay()
    {
        if (enemyBase == null) return;
        if (attacking) CancelAttack();
        if (delayUntilNextAttack > 0f) delayUntilNextAttack -= Time.fixedDeltaTime;
        Transform target = Target;
        if (target == null)
        {
            enemyBase.rb.velocity = Vector2.zero;
            enemyBase.animator.SetInteger("Anim", 0);
            return;
        }
        LookAtCurrentTarget();
        Vector2 delta = (Vector2)target.position - (Vector2)enemyBase.transform.position;
        if (delta.magnitude >= HeadReach - attackRangeMargin)
        {
            enemyBase.rb.velocity = delta.normalized * enemyBase.SPEED;
            enemyBase.animator.SetInteger("Anim", 1);
        }
        else
        {
            enemyBase.rb.velocity = Vector2.zero;
            enemyBase.animator.SetInteger("Anim", 0);
        }
    }

    public override void EnemyAttackPlay()
    {
        if (enemyBase == null) return;
        if (!attacking)
        {
            if (delayUntilNextAttack > 0f)
            {
                delayUntilNextAttack -= Time.fixedDeltaTime;
                enemyBase.rb.velocity = Vector2.zero;
                return;
            }
            if (Target == null || Vector2.Distance(Target.position, NeckBase) >= HeadReach - attackRangeMargin)
            {
                FinishAttack();
                return;
            }
            BeginAttack();
        }
        if (weakDamageReceiver != null) weakDamageReceiver.DF = weakNeckDefense;
        float dt = Time.fixedDeltaTime;
        stateTime += dt;
        if (state != SlowApproach && state != Jump)
            enemyBase.rb.velocity = Vector2.zero;

        switch (state)
        {
            case Windup:
                if (stateTime >= windupTime) StartRushMotion();
                break;
            case Rush: StepRush(dt); break;
            case ImpactPause:
                if (stateTime >= impactWait) DecideAfterImpact();
                break;
            case RoarWarning:
                if (stateTime >= roarWarningTime) FireRoar();
                break;
            case RoarAfter:
                if (stateTime >= 0.15f)
                {
                    if (attackFamily == 2) StartLockedRush();
                    else BeginRetract();
                }
                break;
            case Jump: StepJump(); break;
            case SlowApproach: StepSlowApproach(); break;
            case Bite: StepBite(dt); break;
            case Retract: StepRetract(dt); break;
            case Recovery:
                if (stateTime >= afterReturnRecovery) FinishAttack();
                break;
        }
        if (attacking && headExposed) UpdateExtendedParts();
    }

    void BeginAttack()
    {
        attacking = true;
        rushCount = 0;
        currentHeadPosition = NeckBase;
        ResetNeckPath();
        HideExtendedParts();
        enemyBase.mode = EnemyBase.ModeType.M1;
        enemyBase.rb.velocity = Vector2.zero;

        float distance = Target != null ? Vector2.Distance(Target.position, NeckBase) : float.PositiveInfinity;
        if (distance > closeRange)
        {
            attackFamily = 1;
            StartLockedRush();
        }
        else
        {
            // 近距離攻撃2と3は交互に出す。両方を確実にテストできる。
            attackFamily = nextCloseAttack;
            nextCloseAttack = nextCloseAttack == 2 ? 3 : 2;
            if (attackFamily == 2) BeginJump();
            else SwitchState(SlowApproach);
        }
    }

    // 攻撃1・攻撃2の追撃共通：構え開始時に標的座標を記憶する。
    void FixedUpdate()
    {
        Transform target = Target;
        if (target == null || target != trackedTarget)
        {
            trackedTarget = target;
            trackedVelocity = Vector2.zero;
            hasTrackedPosition = false;
        }
        if (target == null) return;
        Vector2 pos = target.position;
        if (hasTrackedPosition)
        {
            Vector2 measured = (pos - lastTrackedPosition) / Mathf.Max(0.001f, Time.fixedDeltaTime);
            // ワープ等の異常な移動は予測に使用しない。
            if (measured.magnitude > maxTrackedTargetSpeed * 2f)
                trackedVelocity = Vector2.zero;
            else
                trackedVelocity = Vector2.Lerp(trackedVelocity,
                    Vector2.ClampMagnitude(measured, maxTrackedTargetSpeed), velocitySmoothing);
        }
        lastTrackedPosition = pos;
        hasTrackedPosition = true;
    }

    Vector2 PredictRushPoint(Transform target)
    {
        Vector2 current = target.position;
        if (!predictTargetMovement || !hasTrackedPosition || trackedTarget != target)
            return current;

        float distance = Vector2.Distance(currentHeadPosition, current);
        // 構え0.5秒＋低速伸長＋加速区間の概算を見込む。
        float flightTime = rushSlowStartTime +
            Mathf.Sqrt(2f * distance / Mathf.Max(1f, rushAcceleration));
        float horizon = windupTime + flightTime;
        Vector2 lead = Vector2.ClampMagnitude(
            trackedVelocity * horizon * predictionStrength, maxPredictionDistance);
        Vector2 predicted = current + lead;
        // 射程外へ予測した場合も境界に着弾できるようクランプ。
        Vector2 fromBase = predicted - NeckBase;
        float reach = Mathf.Max(0.1f, HeadReach - attackRangeMargin);
        return fromBase.magnitude > reach
            ? NeckBase + fromBase.normalized * reach : predicted;
    }

    void StartLockedRush()
    {
        if (Target == null) { BeginRetract(); return; }
        lockedPoint = PredictRushPoint(Target);
        if (Vector2.Distance(lockedPoint, NeckBase) > HeadReach - attackRangeMargin)
        {
            BeginRetract();
            return;
        }
        Vector2 aim = lockedPoint - currentHeadPosition;
        if (aim.sqrMagnitude > 0.001f) lastHeadDirection = aim.normalized;
        enemyBase.animator.SetInteger("Anim", 2);
        LookAtCurrentTarget();
        if (enemyBase.audioManager != null) enemyBase.audioManager.PlaySE(enemyBase.AttackStayAudio);
        SpawnCircleWarning(lockedPoint, impactRadius, windupTime, impactWarningPrefab);
        SwitchState(Windup);
    }

    void StartRushMotion()
    {
        if (!headExposed)
        {
            headExposed = true;
            enemyBase.animator.Play("首無し", 0, 0f);
            if (headAnimator != null) headAnimator.Play("頭部", 0, 0f);
            if (headSprite != null) headSprite.enabled = true;
        }
        currentRushSpeed = rushInitialSpeed;
        // ハープーンは着弾地点の警告円内だけでダメージを与える。
        // 移動経路での直撃判定は警告範囲と一致しないため無効にする。
        SetHeadAttackActive(false);
        if (enemyBase.audioManager != null) enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
        SwitchState(Rush);
    }

    void StepRush(float dt)
    {
        Vector2 delta = lockedPoint - currentHeadPosition;
        if (delta.sqrMagnitude > 0.001f)
        {
            lastHeadDirection = delta.normalized;
            rushImpactDirection = delta.normalized;
        }
        // 座標固定＋MoveTowardsで通り過ぎず確実に激突する。
        if (stateTime < rushSlowStartTime)
            currentRushSpeed = rushInitialSpeed;
        else
            currentRushSpeed = Mathf.Min(rushSpeed, currentRushSpeed + rushAcceleration * dt);
        MoveHead(Vector2.MoveTowards(currentHeadPosition, lockedPoint, currentRushSpeed * dt));
        if (Vector2.Distance(currentHeadPosition, lockedPoint) <= 0.03f)
        {
            SetHeadAttackActive(false);
            rushCount++;
            SpawnAreaAttack(lockedPoint, impactRadius, impactPowerPercent);
            if (harpoonImpactEffect != null)
                Destroy(Instantiate(harpoonImpactEffect, lockedPoint, Quaternion.identity), harpoonImpactEffectLifetime);
            SpawnRockFan(lockedPoint, rushImpactDirection);
            SwitchState(ImpactPause);
        }
    }

    void DecideAfterImpact()
    {
        if (Target == null) { BeginRetract(); return; }
        float distanceFromHead = Vector2.Distance(Target.position, currentHeadPosition);
        float distanceFromBase = Vector2.Distance(Target.position, NeckBase);
        // 射程内であれば距離に関係なく規定回数のハープーンを優先する。
        if (rushCount < maximumRushes && distanceFromBase < HeadReach - attackRangeMargin)
        {
            StartLockedRush();
            return;
        }
        // 3回目の着弾後、頭付近に標的がいれば咆哮へ移行する。
        if (rushCount >= maximumRushes && distanceFromHead <= closeRange)
            BeginRoar();
        else
            BeginRetract();
    }

    void BeginJump()
    {
        if (Target == null) { BeginRetract(); return; }
        jumpStart = enemyBase.rb.position;
        jumpLanding = Target.position;
        enemyBase.rb.velocity = Vector2.zero;
        // MovePositionはDynamicでも利用可能。混乱/スタン中断で
        // Kinematic→Dynamicに戻す際の座標不整合を避ける。
        bodyTypeChanged = false;
        enemyBase.animator.SetInteger("Anim", 2);
        SpawnCircleWarning(jumpLanding, impactRadius, jumpDuration, impactWarningPrefab);
        SwitchState(Jump);
    }

    // 和金やギンリンサンショクワキンと同じsinカーブ跳躍。
    void StepJump()
    {
        float progress = Mathf.Clamp01(stateTime / jumpDuration);
        Vector2 pos = Vector2.Lerp(jumpStart, jumpLanding, progress);
        pos.y += Mathf.Sin(progress * Mathf.PI) * jumpArcHeight;
        enemyBase.rb.MovePosition(pos);
        if (progress >= 1f)
        {
            enemyBase.rb.position = jumpLanding;
            RestoreBodyType();
            enemyBase.rb.velocity = Vector2.zero;
            currentHeadPosition = NeckBase;
            headExposed = true;
            enemyBase.animator.Play("首無し", 0, 0f);
            if (headSprite != null) headSprite.enabled = true;
            if (headAnimator != null) headAnimator.Play("頭部", 0, 0f);
            SpawnAreaAttack(jumpLanding, impactRadius, impactPowerPercent);
            BeginRoar();
        }
    }

    void BeginRoar()
    {
        SetHeadAttackActive(false);
        lockedPoint = attackFamily == 2 && rushCount == 0
            ? NeckBase : currentHeadPosition;
        if (enemyBase.audioManager != null) enemyBase.audioManager.PlaySE(enemyBase.AttackStayAudio);
        SpawnCircleWarning(lockedPoint, roarRadius, roarWarningTime, roarWarningPrefab);
        SwitchState(RoarWarning);
    }

    void FireRoar()
    {
        SpawnAreaAttack(lockedPoint, roarRadius, roarPowerPercent);
        if (enemyBase.audioManager != null) enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
        SwitchState(RoarAfter);
    }

    // 攻撃3は本体が低速で常時追尾して接近する。
    void StepSlowApproach()
    {
        if (Target == null || stateTime >= approachTimeout)
        {
            BeginRetract();
            return;
        }
        Vector2 toward = (Vector2)Target.position - (Vector2)enemyBase.transform.position;
        if (toward.magnitude <= biteStartDistance)
        {
            enemyBase.rb.velocity = Vector2.zero;
            currentHeadPosition = NeckBase;
            ResetNeckPath();
            headExposed = true;
            enemyBase.animator.Play("首無し", 0, 0f);
            if (headSprite != null) headSprite.enabled = true;
            if (headAnimator != null) headAnimator.Play("頭部", 0, 0f);
            SetHeadAttackActive(true);
            SwitchState(Bite);
        }
        else
        {
            enemyBase.rb.velocity = toward.normalized * slowApproachSpeed;
            enemyBase.animator.SetInteger("Anim", 1);
            LookAtCurrentTarget();
        }
    }

    void StepBite(float dt)
    {
        Vector2 toward = Target != null
            ? (Vector2)Target.position - NeckBase : lastHeadDirection;
        if (toward.sqrMagnitude > 0.001f) lastHeadDirection = toward.normalized;
        Vector2 point = NeckBase + Vector2.ClampMagnitude(toward, biteReach);
        Vector2 headBeforeBite = currentHeadPosition;
        MoveHead(Vector2.MoveTowards(currentHeadPosition, point, biteSpeed * dt));
        CheckHeadTravelHit(headBeforeBite, currentHeadPosition);
        if (stateTime >= biteDuration) BeginRetract();
    }

    void BeginRetract()
    {
        RestoreBodyType();
        enemyBase.rb.velocity = Vector2.zero;
        SetHeadAttackActive(false);
        if (headExposed)
        {
            retractHeadRotation = headTransform != null ? headTransform.rotation : Quaternion.identity;
            BuildRetractPath();
            SwitchState(Retract);
        }
        else
        {
            HideExtendedParts();
            SwitchState(Recovery);
        }
    }

    // 頭部が伸びた首を逆に辿る。根元への直線移動ではない。
    void BuildRetractPath()
    {
        retractPath.Clear();
        retractPath.Add(currentHeadPosition);
        for (int i = 0; i < neckPool.Count; i++)
            if (neckPool[i] != null && neckPool[i].gameObject.activeSelf)
                retractPath.Add(neckPool[i].position);
        retractPath.Add(NeckBase);
        retractTotalDistance = 0f;
        for (int i = 1; i < retractPath.Count; i++)
            retractTotalDistance += Vector2.Distance(retractPath[i - 1], retractPath[i]);
        retractDistanceAdvanced = 0f;
        retractElapsed = 0f;
    }

    void StepRetract(float dt)
    {
        // 同じ総収納時間で、前半を遅く・後半を速くする二次イージング。
        float duration = retractTotalDistance / Mathf.Max(0.1f, neckReturnSpeed);
        retractElapsed += dt;
        float progress = duration > 0f ? Mathf.Clamp01(retractElapsed / duration) : 1f;
        float eased = Mathf.Lerp(progress, progress * progress, neckReturnAcceleration);
        float desiredDistance = retractTotalDistance * eased;
        float remaining = Mathf.Max(0f, desiredDistance - retractDistanceAdvanced);
        while (remaining > 0f && retractPath.Count > 1)
        {
            Vector2 next = retractPath[1];
            float distance = Vector2.Distance(currentHeadPosition, next);
            if (distance <= remaining + 0.0001f)
            {
                currentHeadPosition = next;
                retractPath.RemoveAt(0);
                remaining -= distance;
            }
            else
            {
                currentHeadPosition = Vector2.MoveTowards(currentHeadPosition, next, remaining);
                remaining = 0f;
            }
        }
        retractDistanceAdvanced = desiredDistance;
        if (retractPath.Count <= 1 || Vector2.Distance(currentHeadPosition, NeckBase) <= 0.04f)
        {
            HideExtendedParts();
            RestoreBodyAnimation();
            SwitchState(Recovery);
        }
    }

    // 収納時は頭と首を同一の経路上に直接配置する。
    // 通常時のLerp追従を適用しないため、頭に首が置き去りにならない。
    void UpdateRetractingNeckChain()
    {
        if (neckPool.Count == 0) return;
        ApplyHeadSize();
        float spacing = Mathf.Max(0.15f, neckSegmentSpacing);
        float length = 0f;
        Vector2 cursor = currentHeadPosition;
        for (int i = 1; i < retractPath.Count; i++)
        {
            length += Vector2.Distance(cursor, retractPath[i]);
            cursor = retractPath[i];
        }
        int count = Mathf.Min(maxNeckSegments, Mathf.CeilToInt(length / spacing));
        while (neckPool.Count < count) AddNeckSegment();

        Vector3 size = new Vector3(Mathf.Max(0.05f, neckSegmentSize.x),
            Mathf.Max(0.05f, neckSegmentSize.y), 1f);
        Vector2 sampleStart = currentHeadPosition;
        int pathIndex = 1;
        Vector2 sampleEnd = pathIndex < retractPath.Count ? retractPath[pathIndex] : NeckBase;
        for (int i = 0; i < count; i++)
        {
            float remaining = spacing * 0.5f;
            if (i > 0) remaining = spacing;
            while (remaining > 0f && pathIndex < retractPath.Count)
            {
                float distance = Vector2.Distance(sampleStart, sampleEnd);
                if (distance > remaining)
                {
                    sampleStart = Vector2.MoveTowards(sampleStart, sampleEnd, remaining);
                    remaining = 0f;
                }
                else
                {
                    sampleStart = sampleEnd;
                    remaining -= distance;
                    pathIndex++;
                    if (pathIndex < retractPath.Count) sampleEnd = retractPath[pathIndex];
                }
            }
            Transform segment = neckPool[i];
            segment.position = sampleStart;
            segment.localScale = size;
            Vector2 forward = i == 0 ? currentHeadPosition : (Vector2)neckPool[i - 1].position;
            Vector2 tangent = forward - sampleStart;
            if (tangent.sqrMagnitude < 0.0001f)
                tangent = currentHeadPosition - NeckBase;
            float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
            segment.rotation = Quaternion.Euler(0f, 0f, angle + (neckSpriteAlongX ? 0f : -90f));
            if (!segment.gameObject.activeSelf) segment.gameObject.SetActive(true);
            if (neckRenderers[i] != null) neckRenderers[i].enabled = true;
            if (neckColliders[i] != null)
                neckColliders[i].enabled = i % Mathf.Max(1, neckDamageSegmentInterval) == 0;
        }
        for (int i = count; i < neckPool.Count; i++)
        {
            if (neckColliders[i] != null) neckColliders[i].enabled = false;
            neckPool[i].gameObject.SetActive(false);
        }
    }

    void RestoreBodyAnimation()
    {
        if (enemyBase == null || enemyBase.animator == null) return;
        enemyBase.animator.SetInteger("Anim", 0);
        if (enemyBase.status == EnemyBase.Status.Nomal)
            enemyBase.animator.Play("待機", 0, 0f);
    }

    void SwitchState(int newState)
    {
        state = newState;
        stateTime = 0f;
    }

    void MoveHead(Vector2 position)
    {
        Vector2 delta = position - NeckBase;
        float limit = HeadReach;
        currentHeadPosition = delta.sqrMagnitude > limit * limit
            ? NeckBase + delta.normalized * limit : position;
        if (delta.sqrMagnitude > 0.001f) lastHeadDirection = delta.normalized;
    }

    void RestoreBodyType()
    {
        if (!bodyTypeChanged || enemyBase == null || enemyBase.rb == null) return;
        Vector2 stablePosition = enemyBase.rb.position;
        enemyBase.rb.bodyType = previousBodyType;
        enemyBase.rb.position = stablePosition;
        bodyTypeChanged = false;
    }

    // ワキンのRockNeedleを転用し、着弾方向に扇状の複数列で生成する。
    // UnityでPrefabの一部だけを複製した場合など、charaDamageの
    // 非公開enemyBase参照が切れていてもワトウナイ自身を所有者にする。
    // 共通charaDamageは変更せず、生成物に限って初期化する。
    static readonly FieldInfo DamageOwnerField = typeof(charaDamage).GetField(
        "enemyBase", BindingFlags.Instance | BindingFlags.NonPublic);

    void RepairMissingDamageOwners(GameObject spawnedObject)
    {
        if (spawnedObject == null || enemyBase == null) return;
        foreach (charaDamage damage in spawnedObject.GetComponentsInChildren<charaDamage>(true))
        {
            if (damage != null && damage.OwnerEnemyBase == null && DamageOwnerField != null)
                DamageOwnerField.SetValue(damage, enemyBase);
        }
    }

    void SpawnRockFan(Vector2 impactPoint, Vector2 direction)
    {
        if (rockNeedlePrefab == null) return;
        if (direction.sqrMagnitude < 0.0001f) direction = lastHeadDirection;
        float heading = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        int rows = Mathf.Max(1, rockNeedleRows);
        int columns = Mathf.Max(1, rockNeedlesPerRow);
        for (int row = 1; row <= rows; row++)
        {
            for (int i = 0; i < columns; i++)
            {
                float normalized = columns == 1 ? 0.5f : (float)i / (columns - 1);
                float spacingAngle = rockFanAngle / Mathf.Max(1, columns - 1);
                float angle = heading + (normalized - 0.5f) * rockFanAngle
                    + Random.Range(-spacingAngle, spacingAngle) * rockAngleJitter;
                float distance = (row + Random.Range(-rockDistanceJitter, rockDistanceJitter))
                    * rockRowSpacing;
                distance = Mathf.Max(0.1f, distance);
                float radians = angle * Mathf.Deg2Rad;
                Vector2 position = impactPoint +
                    new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * distance;
                // 遠い岩針ほど遅い。ランダム差を加えて整列した出現を避ける。
                float delay = distance * rockDelayPerUnit +
                    Random.Range(0f, rockRandomDelay);
                rockFanCoroutines.Add(StartCoroutine(SpawnDelayedRockNeedle(position, delay)));
            }
        }
    }

    IEnumerator SpawnDelayedRockNeedle(Vector2 position, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        if (rockNeedlePrefab == null || enemyBase == null ||
            enemyBase.status == EnemyBase.Status.Die) yield break;
        GameObject needle = Instantiate(rockNeedlePrefab, position, Quaternion.identity);
        RepairMissingDamageOwners(needle);
        ConfigureSummonedAttack(needle);
        Destroy(needle, rockNeedleLifetime);
    }

    // 着弾と咆哮は、どちらも既存EnemyAttackの派閥・ダメージ処理を使用。
    void SpawnAreaAttack(Vector2 position, float radius, float damagePercent)
    {
        GameObject volume = new GameObject("WatounaiAreaAttack");
        volume.layer = headAttackCollider != null
            ? headAttackCollider.gameObject.layer : gameObject.layer;
        volume.transform.position = position;
        Rigidbody2D body = volume.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.useFullKinematicContacts = true;
        body.gravityScale = 0f;
        CircleCollider2D collider = volume.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = radius;
        EnemyAttack hit = volume.AddComponent<EnemyAttack>();
        hit.ConfigureAttack(enemyBase, damagePercent);
        EnemyContoroller controller = enemyBase as EnemyContoroller;
        if (controller != null) controller.ConfigureAttackTarget(hit);
        // 生成直後から範囲内にいる対象はOnTriggerEnter2Dを待つだけでは
        // 取り逃すことがあるため、明示的に調べる。二重ヒットはEnemyAttack側で抑止。
        hit.BeginManualHitWindow();
        Physics2D.SyncTransforms();
        Collider2D[] targets = Physics2D.OverlapCircleAll(position, radius);
        for (int i = 0; i < targets.Length; i++)
            hit.CheckHitNow(targets[i]);
        Destroy(volume, 0.16f);
    }

    void SpawnCircleWarning(Vector2 center, float radius, float duration, GameObject prefab)
    {
        GameObject warning = null;
        EnemyContoroller controller = enemyBase as EnemyContoroller;
        if (prefab != null ||
            (controller != null && controller.HasAttackWarningPrefab(EnemyContoroller.WarningCircle)))
            warning = SpawnAttackWarning(EnemyContoroller.WarningCircle, prefab,
                center, Quaternion.identity, duration, null, 0f, true,
                new Vector3(radius * 2f, radius * 2f, 1f));

        // 共通警告Prefabがなくても、不可避の高速突進には必ず予測円を表示する。
        if (warning == null)
        {
            if (fallbackWarningMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null) fallbackWarningMaterial = new Material(shader);
            }
            warning = new GameObject("WatounaiWarningRing");
            warning.transform.position = center;
            LineRenderer line = warning.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 64;
            line.widthMultiplier = 0.08f;
            line.startColor = new Color(1f, 0.2f, 0.1f, 0.9f);
            line.endColor = new Color(1f, 0.2f, 0.1f, 0.9f);
            line.sortingOrder = 12;
            if (fallbackWarningMaterial != null)
                line.sharedMaterial = fallbackWarningMaterial;
            for (int i = 0; i < 64; i++)
            {
                float angle = (Mathf.PI * 2f * i) / 64f;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius, 0f));
            }
            Destroy(warning, duration);
        }
        activeWarnings.Add(warning);
    }

    void ClearWarnings()
    {
        foreach (GameObject warning in activeWarnings)
            if (warning != null) Destroy(warning);
        activeWarnings.Clear();
    }

    void UpdateExtendedParts()
    {
        if (headTransform == null) return;
        Vector2 delta = currentHeadPosition - NeckBase;
        Vector2 direction = delta.sqrMagnitude > 0.001f ? delta.normalized : lastHeadDirection;
        headTransform.position = currentHeadPosition;
        // 頭は進行方向を向く。首の節だけが遅れて曲がる。
        Vector2 face = headVelocity.sqrMagnitude > 0.04f ? headVelocity.normalized : direction;
        // 首の射出中は移動先固定でも、頭の視線は生きている標的に追従する。
        if (headExposed && state != Retract && Target != null)
        {
            Vector2 look = (Vector2)Target.position - currentHeadPosition;
            if (look.sqrMagnitude > 0.0001f) face = look.normalized;
        }
        headTransform.rotation = state == Retract ? retractHeadRotation :
            Quaternion.Euler(0f, 0f,
                Mathf.Atan2(face.y, face.x) * Mathf.Rad2Deg + headAngleOffset);
        if (headSprite != null) headSprite.enabled = headExposed;
        if (headExposed)
        {
            if (state == Retract) UpdateRetractingNeckChain();
            else UpdateNeckChain(Time.fixedDeltaTime);
        }

        if (headDamageCollider != null)
            headDamageCollider.enabled = headExposed;
    }

    // 首の射出・噛みつきに共通：移動区間を円形でスイープ判定する。
    // Colliderの通常Triggerと共存し、EnemyAttackの派閥判定とATK倍率を使う。
    void CheckHeadTravelHit(Vector2 from, Vector2 to)
    {
        if (headAttack == null || headAttackCollider == null ||
            !headAttackCollider.enabled) return;

        CircleCollider2D circle = headAttackCollider as CircleCollider2D;
        float radius = circle != null
            ? circle.radius * Mathf.Max(Mathf.Abs(circle.transform.lossyScale.x),
                Mathf.Abs(circle.transform.lossyScale.y))
            : 0.5f;
        Vector2 offset = (Vector2)(headAttackCollider.transform.position -
            headTransform.position);
        Vector2 start = from + offset;
        Vector2 end = to + offset;
        Vector2 travel = end - start;
        float distance = travel.magnitude;
        if (distance > 0.001f)
        {
            RaycastHit2D[] hits = Physics2D.CircleCastAll(start, radius,
                travel / distance, distance);
            for (int i = 0; i < hits.Length; i++)
                headAttack.CheckHitNow(hits[i].collider);
        }
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(end, radius);
        for (int i = 0; i < overlaps.Length; i++)
            headAttack.CheckHitNow(overlaps[i]);
    }

    void SetHeadAttackActive(bool active)
    {
        if (headAttackCollider != null)
            headAttackCollider.enabled = active;
        if (headAttack != null)
        {
            if (active) headAttack.BeginManualHitWindow();
            else headAttack.EndManualHitWindow();
        }
    }

    void HideExtendedParts()
    {
        SetHeadAttackActive(false);
        if (neckDamageCollider != null) neckDamageCollider.enabled = false;
        for (int i = 0; i < neckPool.Count; i++)
        {
            if (neckColliders[i] != null) neckColliders[i].enabled = false;
            if (neckPool[i] != null) neckPool[i].gameObject.SetActive(false);
        }
        headTrail.Clear();
        neckPoints.Clear();
        if (headDamageCollider != null) headDamageCollider.enabled = false;
        if (neckSprite != null) neckSprite.enabled = false;
        if (headSprite != null) headSprite.enabled = false;
        headExposed = false;
        if (enemyBase != null && headTransform != null)
            headTransform.position = NeckBase;
    }

    void FinishAttack()
    {
        attacking = false;
        RestoreBodyType();
        ClearWarnings();
        HideExtendedParts();
        if (enemyBase == null) return;
        enemyBase.rb.velocity = Vector2.zero;
        RestoreBodyAnimation();
        enemyBase.mode = EnemyBase.ModeType.M1;
        enemyBase.AttackCount = 0f;
        delayUntilNextAttack = shootingRestTime;
        enemyBase.moveType = EnemyBase.MoveType.Move;
        enemyBase.PlayerDistance = 1000f;
    }

    void CancelAttack()
    {
        attacking = false;
        RestoreBodyType();
        ClearWarnings();
        HideExtendedParts();
        if (enemyBase == null) return;
        enemyBase.rb.velocity = Vector2.zero;
        RestoreBodyAnimation();
        enemyBase.mode = EnemyBase.ModeType.M1;
        enemyBase.AttackCount = 0f;
        enemyBase.moveType = EnemyBase.MoveType.Move;
        delayUntilNextAttack = shootingRestTime;
    }

    public override void EnemyStanPlay() { if (attacking) CancelAttack(); }
    public override void EnemyConfusionPlay() { if (attacking) CancelAttack(); }

    void LateUpdate()
    {
        if (enemyBase != null && enemyBase.status == EnemyBase.Status.Die && attacking)
            CancelAttack();
    }

    void OnDisable()
    {
        if (attacking) CancelAttack();
        StopAllCoroutines();
        rockFanCoroutines.Clear();
    }

    void OnDrawGizmosSelected()
    {
        if (!showAttackGizmos || !Application.isPlaying || !attacking) return;
        // 黄色＝着弾範囲、赤＝咆哮範囲（Sceneビューで判定を確認）
        if (state == Windup || state == Rush || state == ImpactPause)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(lockedPoint, impactRadius);
        }
        if (state == RoarWarning || state == RoarAfter)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lockedPoint, roarRadius);
        }
    }

    void OnDestroy()
    {
        if (fallbackWarningMaterial != null) Destroy(fallbackWarningMaterial);
    }
}
