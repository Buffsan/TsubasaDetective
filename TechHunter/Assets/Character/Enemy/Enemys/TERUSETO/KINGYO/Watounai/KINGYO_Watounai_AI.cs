using System.Collections.Generic;
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

    [Header("攻撃距離・移動")]
    [SerializeField, Min(1f)] float minimumHeadReach = 20f;
    [SerializeField, Min(0f)] float attackRangeMargin = 0.5f;
    [SerializeField, Min(0f)] float shootingRestTime = 0.45f;
    [SerializeField] GameObject straightWarningPrefab;
    [SerializeField, Min(0.05f)] float windupTime = 0.6f;

    [Header("砲弾頭突き・加速噛みつき")]
    [SerializeField, Min(0.05f)] float launchDuration = 0.45f;
    [SerializeField, Min(0f)] float launchInitialSpeed = 30f;
    [SerializeField, Min(0f)] float launchAcceleration = 100f;
    [SerializeField, Min(0)] int bitesBeforeSweep = 2;
    [SerializeField, Min(0)] int bitesAfterSweep = 1;
    [SerializeField, Min(0.05f)] float biteDuration = 0.46f;
    [SerializeField, Min(0f)] float biteInitialSpeed = 15f;
    [SerializeField, Min(0f)] float biteAcceleration = 85f;
    [SerializeField, Min(0.05f)] float bitePause = 0.12f;
    [SerializeField, Min(30f)] float biteTurnSpeed = 540f;
    [SerializeField, Min(0.1f)] float biteStopDistance = 0.65f;

    [Header("首を引いた後の薙ぎ払い")]
    [SerializeField, Min(0.05f)] float pullbackDuration = 0.25f;
    [SerializeField, Range(0.05f, 0.95f)] float pullbackFraction = 0.35f;
    [SerializeField, Min(0.05f)] float sweepDuration = 0.6f;
    [SerializeField, Range(30f, 270f)] float sweepAngle = 155f;
    [SerializeField, Min(0.5f)] float sweepMinimumRadius = 3.5f;
    [SerializeField, Min(0.5f)] float sweepTravelSpeed = 42f;

    [Header("収納：頭は攻撃せず、首は被弾可能")]
    [SerializeField, Min(0.1f)] float neckReturnSpeed = 10f;
    [SerializeField, Min(0.05f)] float afterReturnRecovery = 0.55f;
    [SerializeField, Min(0.1f)] float maxSequenceDuration = 8f;

    const int Windup = 0;
    const int Launch = 1;
    const int Bite = 2;
    const int BitePause = 3;
    const int Pullback = 4;
    const int Sweep = 5;
    const int Retract = 6;
    const int Recovery = 7;

    bool attacking;
    int state;
    int bitesDone;
    int totalBites;
    float stateTime;
    float sequenceTime;
    float headSpeed;
    float delayUntilNextAttack;
    float pullbackInitialDistance;
    float sweepRadius;
    float sweepCenterAngle;
    bool sweepDone;
    Vector2 currentHeadPosition;
    Vector2 launchDirection = Vector2.right;
    Vector2 fixedRushTarget;
    Vector2 headTravelDirection = Vector2.right;
    Vector2 lastHeadDirection = Vector2.right;
    bool warningMissingLogged;

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
            if (neckColliders[i] != null) neckColliders[i].enabled = true;
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

        Transform target = Target;
        if (target == null)
        {
            enemyBase.rb.velocity = Vector2.zero;
            enemyBase.animator.SetInteger("Anim", 0);
            return;
        }

        LookAtCurrentTarget();
        Vector2 direction = (Vector2)target.position - (Vector2)enemyBase.transform.position;
        float distance = direction.magnitude;

        // 画面端相当の射程より外の敵は、従来の移動速度で接近する。
        if (distance >= HeadReach - attackRangeMargin)
        {
            enemyBase.rb.velocity = direction.normalized * enemyBase.SPEED;
            enemyBase.animator.SetInteger("Anim", 1);
        }
        else
        {
            enemyBase.rb.velocity = Vector2.zero;
            enemyBase.animator.SetInteger("Anim", 0);
        }

        // Controllerが距離によってAttackへ移るため、独立した待機時間は短縮しない。
        if (delayUntilNextAttack > 0f)
            delayUntilNextAttack -= Time.fixedDeltaTime;
    }

    public override void EnemyAttackPlay()
    {
        if (enemyBase == null) return;
        enemyBase.rb.velocity = Vector2.zero;

        if (!attacking)
        {
            if (delayUntilNextAttack > 0f)
            {
                delayUntilNextAttack -= Time.fixedDeltaTime;
                return;
            }
            if (Target == null)
            {
                FinishAttack();
                return;
            }
            BeginAttack();
        }

        float dt = Time.fixedDeltaTime;
        stateTime += dt;
        sequenceTime += dt;
        if (sequenceTime >= maxSequenceDuration && state != Retract && state != Recovery)
            SwitchState(Retract);

        switch (state)
        {
            case Windup: StepWindup(); break;
            case Launch: StepLaunch(dt); break;
            case Bite: StepBite(dt); break;
            case BitePause: StepBitePause(); break;
            case Pullback: StepPullback(); break;
            case Sweep: StepSweep(); break;
            case Retract: StepRetract(dt); break;
            case Recovery:
                if (stateTime >= afterReturnRecovery) FinishAttack();
                break;
        }

        if (attacking)
            UpdateExtendedParts();
    }

    void BeginAttack()
    {
        attacking = true;
        state = Windup;
        stateTime = 0f;
        sequenceTime = 0f;
        bitesDone = 0;
        totalBites = Mathf.Max(0, bitesBeforeSweep) + Mathf.Max(0, bitesAfterSweep);
        sweepDone = false;
        currentHeadPosition = NeckBase;
        ResetNeckPath();

        Vector2 direction = Target != null
            ? (Vector2)Target.position - NeckBase : lastHeadDirection;
        launchDirection = direction.sqrMagnitude > 0.001f
            ? direction.normalized : lastHeadDirection;

        headTravelDirection = launchDirection;
        LookAtCurrentTarget();
        enemyBase.animator.SetInteger("Anim", 2);
        enemyBase.mode = EnemyBase.ModeType.M1;
        enemyBase.audioManager.PlaySE(enemyBase.AttackStayAudio);

        if (straightWarningPrefab != null)
        {
            GameObject warning = SpawnAttackWarning(EnemyContoroller.WarningRectangle,
                straightWarningPrefab, NeckBase, Quaternion.identity,
                windupTime, null, 0f, false);
            if (warning != null) warning.transform.up = launchDirection;
        }
        else if (!warningMissingLogged)
        {
            Debug.LogWarning("KINGYO_Watounai: 攻撃警告Prefabは未設定です。", this);
            warningMissingLogged = true;
        }
    }

    void StepWindup()
    {
        if (stateTime < windupTime) return;

        enemyBase.animator.Play("首無し", 0, 0f);
        if (headAnimator != null) headAnimator.Play("頭部", 0, 0f);
        if (headSprite != null) headSprite.enabled = true;
        if (neckSprite != null) neckSprite.enabled = true;
        // 突進直前の位置を固定する。突進中はターゲットの移動を追わない。
        fixedRushTarget = Target != null ? (Vector2)Target.position : NeckBase + launchDirection * HeadReach;
        Vector2 aim = fixedRushTarget - currentHeadPosition;
        if (aim.sqrMagnitude > 0.0001f) launchDirection = aim.normalized;
        headSpeed = launchInitialSpeed;
        enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
        SwitchState(Launch);
    }

    void StepLaunch(float dt)
    {
        SetHeadAttackActive(true);
        headSpeed += launchAcceleration * dt;
        headTravelDirection = launchDirection;
        // 目的地を通過しない。高速で移動し、記憶した位置に激突して停止する。
        MoveHead(Vector2.MoveTowards(currentHeadPosition, fixedRushTarget, headSpeed * dt));

        if (Vector2.Distance(currentHeadPosition, fixedRushTarget) <= 0.08f ||
            Vector2.Distance(NeckBase, currentHeadPosition) >= HeadReach - 0.05f)
        {
            SetHeadAttackActive(false);
            if (totalBites > 0) BeginBite();
            else SwitchState(Pullback);
        }
    }

    void BeginBite()
    {
        // 一度激突した後、現在のプレイヤー座標を改めて記憶する。
        fixedRushTarget = Target != null ? (Vector2)Target.position : currentHeadPosition;
        Vector2 aim = fixedRushTarget - currentHeadPosition;
        if (aim.sqrMagnitude > 0.0001f) headTravelDirection = aim.normalized;
        headSpeed = biteInitialSpeed;
        SwitchState(Bite);
        SetHeadAttackActive(true);
    }

    void StepBite(float dt)
    {
        // 各突進開始時に固定した位置へ直進。追い越さず、激突したら止まる。
        Vector2 toTarget = fixedRushTarget - currentHeadPosition;
        float distance = toTarget.magnitude;
        if (distance <= 0.08f)
        {
            CompleteBite();
            return;
        }

        headTravelDirection = toTarget / distance;
        headSpeed += biteAcceleration * dt;
        MoveHead(Vector2.MoveTowards(currentHeadPosition, fixedRushTarget, headSpeed * dt));
        if (Vector2.Distance(currentHeadPosition, fixedRushTarget) <= 0.08f ||
            Vector2.Distance(NeckBase, currentHeadPosition) >= HeadReach - 0.05f)
            CompleteBite();
    }

    void CompleteBite()
    {
        bitesDone++;
        SetHeadAttackActive(false);
        SwitchState(BitePause);
    }

    void StepBitePause()
    {
        if (stateTime < bitePause) return;

        if (!sweepDone && bitesDone >= bitesBeforeSweep)
        {
            pullbackInitialDistance = Vector2.Distance(NeckBase, currentHeadPosition);
            SwitchState(Pullback);
        }
        else if (bitesDone < totalBites)
            BeginBite();
        else
            SwitchState(Retract);
    }

    void StepPullback()
    {
        // 完全収納ではない。首をいったん引き下げて薙ぎ払いへつなぐ。
        SetHeadAttackActive(false);
        Vector2 dir = currentHeadPosition - NeckBase;
        if (dir.sqrMagnitude < 0.001f) dir = lastHeadDirection;
        float ratio = Mathf.Clamp01(stateTime / pullbackDuration);
        float from = Mathf.Max(0.2f, pullbackInitialDistance);
        float to = Mathf.Max(0.5f, from * pullbackFraction);
        MoveHead(NeckBase + dir.normalized * Mathf.Lerp(from, to, ratio));

        if (ratio >= 1f)
        {
            Vector2 facing = Target != null
                ? (Vector2)Target.position - NeckBase : lastHeadDirection;
            if (facing.sqrMagnitude < 0.001f) facing = lastHeadDirection;
            sweepCenterAngle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
            sweepRadius = Mathf.Clamp(
                Target != null ? Vector2.Distance(Target.position, NeckBase) : from,
                sweepMinimumRadius, HeadReach);
            SwitchState(Sweep);
        }
    }

    void StepSweep()
    {
        // 頭を大きく振り回す間、伸びた首全体が低防御の攻撃チャンス。
        float progress = Mathf.Clamp01(stateTime / sweepDuration);
        float angle = sweepCenterAngle + Mathf.Lerp(-sweepAngle * 0.5f,
            sweepAngle * 0.5f, progress);
        Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad));
        // 円弧上の次の位置へ移動する。瞬間移動させない。
        Vector2 destination = NeckBase + dir * sweepRadius;
        Vector2 next = Vector2.MoveTowards(currentHeadPosition, destination,
            sweepTravelSpeed * Time.fixedDeltaTime);
        Vector2 travel = next - currentHeadPosition;
        if (travel.sqrMagnitude > 0.0001f) headTravelDirection = travel.normalized;
        MoveHead(next);
        SetHeadAttackActive(true);

        if (progress >= 1f)
        {
            sweepDone = true;
            SetHeadAttackActive(false);
            if (bitesDone < totalBites) BeginBite();
            else SwitchState(Retract);
        }
    }

    void StepRetract(float dt)
    {
        // 収縮中は頭部攻撃を無効化。首の被弾判定は最後まで残る。
        SetHeadAttackActive(false);
        MoveHead(Vector2.MoveTowards(currentHeadPosition, NeckBase,
            neckReturnSpeed * dt));

        if (Vector2.Distance(currentHeadPosition, NeckBase) <= 0.06f)
        {
            HideExtendedParts();
            enemyBase.animator.Play("待機", 0, 0f);
            SwitchState(Recovery);
        }
    }

    void SwitchState(int newState)
    {
        state = newState;
        stateTime = 0f;
        enemyBase.mode = (EnemyBase.ModeType)Mathf.Clamp(newState, 0, 7);
    }

    void MoveHead(Vector2 wantedPosition)
    {
        Vector2 fromBase = wantedPosition - NeckBase;
        currentHeadPosition = fromBase.sqrMagnitude > HeadReach * HeadReach
            ? NeckBase + fromBase.normalized * HeadReach : wantedPosition;
        if (fromBase.sqrMagnitude > 0.001f)
            lastHeadDirection = fromBase.normalized;
    }

    void UpdateExtendedParts()
    {
        if (headTransform == null) return;
        Vector2 delta = currentHeadPosition - NeckBase;
        Vector2 direction = delta.sqrMagnitude > 0.001f ? delta.normalized : lastHeadDirection;
        headTransform.position = currentHeadPosition;
        // 頭は進行方向を向く。首の節だけが遅れて曲がる。
        Vector2 face = headVelocity.sqrMagnitude > 0.04f ? headVelocity.normalized : direction;
        headTransform.rotation = Quaternion.Euler(0f, 0f,
            Mathf.Atan2(face.y, face.x) * Mathf.Rad2Deg + headAngleOffset);
        if (headSprite != null) headSprite.enabled = state != Windup && state != Recovery;
        if (state != Windup && state != Recovery)
            UpdateNeckChain(Time.fixedDeltaTime);

        if (headDamageCollider != null)
            headDamageCollider.enabled = state != Windup && state != Recovery;
    }

    void SetHeadAttackActive(bool active)
    {
        if (headAttackCollider != null)
            headAttackCollider.enabled = active;
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
        if (enemyBase != null && headTransform != null)
            headTransform.position = NeckBase;
    }

    void FinishAttack()
    {
        attacking = false;
        HideExtendedParts();
        if (enemyBase == null) return;
        enemyBase.rb.velocity = Vector2.zero;
        enemyBase.animator.SetInteger("Anim", 0);
        enemyBase.animator.Play("待機", 0, 0f);
        enemyBase.mode = EnemyBase.ModeType.M1;
        enemyBase.AttackCount = 0f;
        delayUntilNextAttack = shootingRestTime;
        enemyBase.moveType = EnemyBase.MoveType.Move;
        enemyBase.PlayerDistance = 1000f;
    }

    void CancelAttack()
    {
        if (!attacking) return;
        attacking = false;
        HideExtendedParts();
        if (enemyBase != null)
        {
            enemyBase.rb.velocity = Vector2.zero;
            enemyBase.mode = EnemyBase.ModeType.M1;
            enemyBase.AttackCount = 0f;
            enemyBase.moveType = EnemyBase.MoveType.Move;
        }
    }

    public override void EnemyStanPlay() { CancelAttack(); }
    public override void EnemyConfusionPlay() { CancelAttack(); }

    void LateUpdate()
    {
        // 死亡処理ではEnemyStanPlayが呼ばれないので、伸びた頭部をここで消す。
        if (enemyBase != null && enemyBase.status == EnemyBase.Status.Die && attacking)
            CancelAttack();
    }

    void OnDisable()
    {
        if (attacking) CancelAttack();
    }
}
