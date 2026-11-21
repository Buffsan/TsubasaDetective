using UnityEngine;

// 月の巡礼者ハルバード使い。共通のターゲット/派閥/被ダメージ処理を再利用する。
public sealed class LunarPurgeSquad_axe_AI : EnemyMoveBase
{
    [Header("間合いと旋回")]
    [SerializeField, Min(0.5f)] float orbitStartDistance = 5f;
    [SerializeField, Min(0.5f)] float desiredDistance = 3.2f;
    [SerializeField, Min(0f)] float distanceMargin = 0.45f;
    [SerializeField, Range(0.1f, 2f)] float orbitSpeedRate = 0.55f;
    [SerializeField] Vector2 initialWait = new Vector2(0.9f, 1.5f);
    [SerializeField] Vector2 betweenAttacksWait = new Vector2(0.65f, 1.15f);
    [SerializeField, Min(0f)] float attackCooldown = 0.55f;

    [Header("横ステップ")]
    [SerializeField] Vector2 sidestepInterval = new Vector2(1.1f, 2.2f);
    [SerializeField, Range(0f, 1f)] float sidestepChance = 0.6f;
    [SerializeField, Min(0.05f)] float sidestepDuration = 0.18f;
    [SerializeField, Min(0.1f)] float sidestepSpeedRate = 3f;

    [Header("攻撃1: 斬撃/飛び込み斬撃")]
    [SerializeField, Min(0.1f)] float meleeRange = 1.8f;
    [SerializeField, Min(0.2f)] float lungeStartRange = 4.5f;
    [SerializeField, Range(0f, 1f)] float retreatChanceWhenClose = 0.35f;
    [SerializeField, Min(0.05f)] float retreatDuration = 0.2f;
    [SerializeField, Min(0.1f)] float retreatSpeedRate = 2.8f;
    [SerializeField, Min(0.05f)] float lungeDuration = 0.24f;
    [SerializeField, Min(0.1f)] float lungeSpeedRate = 3.3f;
    [SerializeField, Min(0f)] float quickWindup = 0.26f;
    [SerializeField] GameObject meleeHitboxPrefab;
    [SerializeField, Min(0f)] float meleeDamagePercent = 100f;

    [Header("攻撃2: 振り下ろし/加速斬撃波")]
    [SerializeField, Range(0f, 1f)] float heavyChance = 0.2f; // 攻撃1:2 = 4:1
    [SerializeField, Min(0.1f)] float heavyWindup = 1f;
    [SerializeField, Min(0f)] float aimFollowSeconds = 0.5f;
    [SerializeField] GameObject warningPrefab;
    [SerializeField] LunarPurgeSquad_axe_Wave wavePrefab;
    [SerializeField, Min(0f)] float waveDamagePercent = 100f;
    [SerializeField, Min(0.1f)] float waveRange = 6.5f;
    [SerializeField, Min(0f)] float waveSpawnOffset = 0.5f;
    [SerializeField] Vector2 waveHitboxSize = new Vector2(1.4f, 0.9f);
    [SerializeField, Min(0f)] float waveInitialSpeed = 3f;
    [SerializeField, Min(0f)] float waveAcceleration = 13f;
    [SerializeField, Min(0.1f)] float waveMaxSpeed = 19f;

    [Header("攻撃判定と硬直")]
    [SerializeField, Min(0.02f)] float slashActiveSeconds = 0.18f;
    [SerializeField, Min(0f)] float recoverySeconds = 0.4f;

    enum Phase { Roam, SideStep, Retreat, Lunge, Windup, Swing, Recovery }
    Phase phase;
    EnemyContoroller controller;
    Transform lastObservedTarget, committedTarget;
    Vector2 lastTargetPosition;
    Vector2 direction = Vector2.right;
    Vector2 stepDirection;
    int orbitSign;
    bool initialized, heavy, aimLocked;
    float phaseTimer, decisionTimer, sidestepTimer, cooldown;
    GameObject meleeObject, warningObject;
    Vector2 warningVisualSize = Vector2.one, warningVisualCenter;

    void Start()
    {
        controller = enemyBase as EnemyContoroller;
        if (controller == null || enemyBase.rb == null || enemyBase.animator == null)
        {
            Debug.LogError("LunarPurgeSquad_axe は EnemyContoroller / Rigidbody2D / Animator が必要です。", this);
            enabled = false;
            return;
        }
        controller.Choice_MoveToBase = EnemyContoroller.MoveToBase.Special;
        enemyBase.LookPlayerDistance = -1f;
        enemyBase.moveType = EnemyBase.MoveType.Move;
        orbitSign = Random.value < 0.5f ? -1 : 1;
        decisionTimer = RandomSeconds(initialWait);
        sidestepTimer = RandomSeconds(sidestepInterval);
        initialized = true;
        PlayAnim(0, "待機");
    }

    static float RandomSeconds(Vector2 v)
    {
        float a = Mathf.Max(0f, Mathf.Min(v.x, v.y));
        float b = Mathf.Max(a, Mathf.Max(v.x, v.y));
        return Random.Range(a, b);
    }

    public override void EnemyStanPlay() { Cancel(); }
    public override void EnemyConfusionPlay() { Cancel(); }
    public override void EnemyFriendlyEnter() { Cancel(); }
    public override void EnemyFriendlyPlay()
    {
        if (enemyBase != null && enemyBase.rb != null)
            enemyBase.rb.velocity = Vector2.zero;
    }

    public override void EnemyMovePlay()
    {
        if (!initialized) return;
        Transform target = CurrentTarget;
        if (target == null)
        {
            enemyBase.rb.velocity = Vector2.zero;
            PlayAnim(0, "待機");
            return;
        }
        if (lastObservedTarget != target)
        {
            lastObservedTarget = target;
            orbitSign = Random.value < 0.5f ? -1 : 1;
            decisionTimer = RandomSeconds(initialWait);
            sidestepTimer = RandomSeconds(sidestepInterval);
            phase = Phase.Roam;
        }

        LookAtCurrentTarget();
        float dt = Time.fixedDeltaTime;
        cooldown = Mathf.Max(0f, cooldown - dt);
        Vector2 toward = (Vector2)target.position - enemyBase.rb.position;
        float distance = toward.magnitude;
        Vector2 inward = distance > 0.001f ? toward / distance : Vector2.right;
        Vector2 tangent = new Vector2(-inward.y, inward.x) * orbitSign;

        if (distance > orbitStartDistance)
        {
            phase = Phase.Roam;
            enemyBase.rb.velocity = inward * enemyBase.SPEED;
            PlayAnim(1, "歩行");
            return;
        }

        decisionTimer -= dt;
        sidestepTimer -= dt;
        if (phase == Phase.SideStep)
        {
            phaseTimer += dt;
            enemyBase.rb.velocity = stepDirection * enemyBase.SPEED * sidestepSpeedRate;
            PlayAnim(1, "歩行");
            if (phaseTimer >= sidestepDuration)
            {
                phase = Phase.Roam;
                sidestepTimer = RandomSeconds(sidestepInterval);
            }
            return;
        }

        if (cooldown <= 0f && decisionTimer <= 0f &&
            distance <= Mathf.Max(meleeRange, lungeStartRange))
        {
            StartAttack(target, distance);
            return;
        }
        if (sidestepTimer <= 0f)
        {
            sidestepTimer = RandomSeconds(sidestepInterval);
            if (Random.value < sidestepChance)
            {
                phase = Phase.SideStep;
                phaseTimer = 0f;
                stepDirection = tangent;
                enemyBase.rb.velocity = tangent * enemyBase.SPEED * sidestepSpeedRate;
                PlayAnim(1, "歩行");
                return;
            }
        }

        float radial = distance < desiredDistance - distanceMargin ? -0.9f :
            distance > desiredDistance + distanceMargin ? 0.75f : 0f;
        enemyBase.rb.velocity = (tangent + inward * radial).normalized *
            enemyBase.SPEED * orbitSpeedRate;
        PlayAnim(1, "歩行");
    }

    void StartAttack(Transform target, float distance)
    {
        committedTarget = target;
        lastTargetPosition = target.position;
        heavy = Random.value < heavyChance;
        aimLocked = false;
        phaseTimer = 0f;
        enemyBase.rb.velocity = Vector2.zero;
        enemyBase.moveType = EnemyBase.MoveType.Attack;
        ClearMelee();
        ClearWarning();

        if (heavy)
        {
            BeginWindup();
            return;
        }
        if (distance <= meleeRange && Random.value < retreatChanceWhenClose)
        {
            phase = Phase.Retreat;
            Vector2 away = (Vector2)transform.position - (Vector2)target.position;
            stepDirection = away.sqrMagnitude > 0.0001f ? away.normalized : -direction;
            PlayAnim(1, "歩行");
        }
        else if (distance > meleeRange)
        {
            phase = Phase.Lunge;
            PlayAnim(1, "歩行");
        }
        else BeginWindup();
    }

    public override void EnemyAttackPlay()
    {
        if (!initialized) return;
        if (committedTarget == null || !controller.CanTarget(committedTarget))
        {
            Cancel();
            return;
        }
        float dt = Time.fixedDeltaTime;
        phaseTimer += dt;
        switch (phase)
        {
            case Phase.Retreat:
                enemyBase.rb.velocity = stepDirection * enemyBase.SPEED * retreatSpeedRate;
                PlayAnim(1, "歩行");
                if (phaseTimer >= retreatDuration)
                {
                    phase = Phase.Lunge;
                    phaseTimer = 0f;
                }
                break;

            case Phase.Lunge:
                Vector2 toTarget = (Vector2)committedTarget.position - enemyBase.rb.position;
                if (toTarget.magnitude > meleeRange && phaseTimer < lungeDuration)
                {
                    enemyBase.rb.velocity = toTarget.normalized * enemyBase.SPEED * lungeSpeedRate;
                    PlayAnim(1, "歩行");
                }
                else BeginWindup();
                break;

            case Phase.Windup:
                enemyBase.rb.velocity = Vector2.zero;
                if (!aimLocked)
                {
                    lastTargetPosition = committedTarget.position;
                    direction = LastAimDirection();
                    if (!heavy || phaseTimer >= Mathf.Min(aimFollowSeconds, heavyWindup))
                        aimLocked = true;
                }
                enemyBase.isTargetLook((Vector2)transform.position + direction);
                if (heavy) UpdateWarning();
                if (phaseTimer >= (heavy ? heavyWindup : quickWindup))
                    Slash();
                break;

            case Phase.Swing:
                enemyBase.rb.velocity = Vector2.zero;
                enemyBase.isTargetLook((Vector2)transform.position + direction);
                PlayAnim(3, "攻撃");
                if (phaseTimer >= slashActiveSeconds)
                {
                    ClearMelee();
                    phase = Phase.Recovery;
                    phaseTimer = 0f;
                }
                break;

            case Phase.Recovery:
                enemyBase.rb.velocity = Vector2.zero;
                if (phaseTimer >= recoverySeconds) FinishAttack();
                break;

            default: Cancel(); break;
        }
    }

    void BeginWindup()
    {
        phase = Phase.Windup;
        phaseTimer = 0f;
        aimLocked = !heavy;
        enemyBase.rb.velocity = Vector2.zero;
        lastTargetPosition = committedTarget != null
            ? (Vector2)committedTarget.position : (Vector2)transform.position + direction;
        direction = LastAimDirection();
        enemyBase.isTargetLook((Vector2)transform.position + direction);
        PlayAnim(2, "攻撃待機", true);
        if (!heavy) return;

        Vector2 center, size;
        WarningArea(direction, out center, out size);
        warningObject = SpawnAttackWarning(EnemyContoroller.WarningRectangle, warningPrefab,
            center, WarningRotation(direction), heavyWindup, null, 0f, true);
        CacheWarningBounds();
        UpdateWarning();
    }

    Vector2 LastAimDirection()
    {
        Vector2 v = lastTargetPosition - (Vector2)transform.position;
        return v.sqrMagnitude > 0.0001f ? v.normalized : direction;
    }

    void Slash()
    {
        aimLocked = true;
        phase = Phase.Swing;
        phaseTimer = 0f;
        enemyBase.rb.velocity = Vector2.zero;
        enemyBase.isTargetLook((Vector2)transform.position + direction);
        ClearWarning();
        PlayAnim(3, "攻撃", true);
        if (enemyBase.AttackAudio != null && enemyBase.audioManager != null)
            enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
        Transform anchor = enemyBase.AttackSpawnPoint != null
            ? enemyBase.AttackSpawnPoint.transform : transform;
        if (meleeHitboxPrefab != null)
        {
            meleeObject = Instantiate(meleeHitboxPrefab, anchor.position, anchor.rotation, anchor);
            controller.ConfigureAttackObject(meleeObject, meleeDamagePercent);
        }
        if (!heavy) return;
        if (wavePrefab == null)
        {
            Debug.LogWarning("LunarPurgeSquad_axe: 斬撃波Prefab未設定", this);
            return;
        }
        Vector2 start = (Vector2)transform.position + direction * waveSpawnOffset;
        LunarPurgeSquad_axe_Wave wave = Instantiate(wavePrefab, start, Quaternion.identity);
        controller.ConfigureAttackObject(wave.gameObject, waveDamagePercent);
        wave.Initialize(direction, waveRange, waveHitboxSize, waveInitialSpeed,
            waveAcceleration, waveMaxSpeed);
    }

    static Quaternion WarningRotation(Vector2 aim)
    {
        return Quaternion.Euler(0f, 0f, Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg - 90f);
    }

    void WarningArea(Vector2 aim, out Vector2 center, out Vector2 size)
    {
        Vector2 start = (Vector2)transform.position + aim * waveSpawnOffset;
        center = start + aim * waveRange * 0.5f;
        size = new Vector2(Mathf.Max(0.05f, waveHitboxSize.x),
            Mathf.Max(0.05f, waveRange + waveHitboxSize.y));
    }

    void CacheWarningBounds()
    {
        warningVisualCenter = Vector2.zero;
        warningVisualSize = Vector2.one;
        if (warningObject == null) return;
        float minX = float.PositiveInfinity, minY = float.PositiveInfinity;
        float maxX = float.NegativeInfinity, maxY = float.NegativeInfinity;
        Transform root = warningObject.transform;
        foreach (SpriteRenderer sr in warningObject.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (!sr.enabled || sr.sprite == null) continue;
            Vector2 size = sr.drawMode == SpriteDrawMode.Simple
                ? (Vector2)sr.sprite.bounds.size : sr.size;
            Vector3 c = sr.sprite.bounds.center;
            Vector2 half = size * 0.5f;
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            {
                Vector3 local = root.InverseTransformPoint(sr.transform.TransformPoint(
                    c + new Vector3(half.x * x, half.y * y, 0f)));
                minX = Mathf.Min(minX, local.x);
                minY = Mathf.Min(minY, local.y);
                maxX = Mathf.Max(maxX, local.x);
                maxY = Mathf.Max(maxY, local.y);
            }
        }
        if (!float.IsInfinity(minX) && maxX > minX && maxY > minY)
        {
            warningVisualCenter = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
            warningVisualSize = new Vector2(maxX - minX, maxY - minY);
        }
    }

    void UpdateWarning()
    {
        if (warningObject == null) return;
        Vector2 center, size;
        WarningArea(direction, out center, out size);
        Transform warning = warningObject.transform;
        warning.rotation = WarningRotation(direction);
        warning.localScale = new Vector3(size.x / warningVisualSize.x,
            size.y / warningVisualSize.y, warning.localScale.z);
        Vector3 offset = new Vector3(warningVisualCenter.x * warning.localScale.x,
            warningVisualCenter.y * warning.localScale.y, 0f);
        warning.position = (Vector3)center - warning.rotation * offset;
    }

    void FinishAttack()
    {
        ClearMelee();
        ClearWarning();
        committedTarget = null;
        phase = Phase.Roam;
        phaseTimer = 0f;
        heavy = false;
        aimLocked = false;
        cooldown = attackCooldown;
        decisionTimer = RandomSeconds(betweenAttacksWait);
        enemyBase.rb.velocity = Vector2.zero;
        enemyBase.moveType = EnemyBase.MoveType.Move;
        PlayAnim(0, "待機");
    }

    void Cancel()
    {
        if (!initialized) return;
        if (phase == Phase.Roam && warningObject == null && meleeObject == null) return;
        ClearMelee();
        ClearWarning();
        committedTarget = null;
        phase = Phase.Roam;
        phaseTimer = 0f;
        aimLocked = false;
        decisionTimer = RandomSeconds(betweenAttacksWait);
        cooldown = Mathf.Max(cooldown, 0.2f);
        if (enemyBase != null && enemyBase.moveType == EnemyBase.MoveType.Attack)
            enemyBase.moveType = EnemyBase.MoveType.Move;
    }

    void ClearMelee()
    {
        if (meleeObject != null) Destroy(meleeObject);
        meleeObject = null;
    }
    void ClearWarning()
    {
        if (warningObject != null) Destroy(warningObject);
        warningObject = null;
    }

    void PlayAnim(int value, string state, bool restart = false)
    {
        Animator anim = enemyBase.animator;
        if (anim == null) return;
        anim.SetInteger("Anim", value);
        if (restart || !anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer." + state))
            anim.Play("Base Layer." + state, 0, 0f);
    }

    void OnDisable() { ClearMelee(); ClearWarning(); }
}
