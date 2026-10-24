using UnityEngine;

// 月の粛清隊剣士。既存 EnemyContoroller / EnemyMoveBase を変更せず使用する。
public class LunarPurgeSquad_Knight_AI : EnemyMoveBase
{
    [Header("通常接近")]
    [SerializeField, Min(0.1f)] float attackRange = 1.7f;
    [SerializeField, Range(0f, 1f)] float orbitAttackChance = 0.5f;
    [SerializeField] Vector2 orbitBeforeAttackSeconds = new Vector2(0.7f, 1.4f);
    [SerializeField, Min(0f)] float attackCooldown = 0.65f;
    [SerializeField, Min(0.1f)] float approachTriggerDistance = 2.5f;

    [Header("踏み込み攻撃")]
    [SerializeField, Min(0f)] float windupSeconds = 0.7f;
    [SerializeField, Min(0f)] float aimLockBeforeAttack = 0.2f;
    [SerializeField, Min(0.05f)] float dashSeconds = 0.3f;
    [SerializeField, Min(0f)] float dashSpeedMultiplier = 4f;
    [SerializeField, Min(0f)] float recoverySeconds = 0.35f;
    [SerializeField] GameObject meleeHitboxPrefab;
    [SerializeField] GameObject attackWarningPrefab;
    [SerializeField, Range(1f, 300f)] float attackPowerPercent = 100f;

    [Header("攻撃警告の微調整（X=横幅 / Y=踏み込み方向）")]
    [SerializeField] Vector2 warningSizeMultiplier = Vector2.one;
    [SerializeField] Vector2 warningOffset = Vector2.zero;

    [Header("HP半減時の回り込み（1回）")]
    [SerializeField, Range(0.01f, 1f)] float repositionHpRatio = 0.5f;
    [SerializeField, Min(0f)] float initialRetreatSeconds = 0.65f;
    [SerializeField] Vector2 repositionSeconds = new Vector2(3f, 7f);
    [SerializeField, Min(0.1f)] float repositionPreferredDistance = 3f;
    [SerializeField, Min(0f)] float repositionSpeedMultiplier = 1.1f;
    [SerializeField, Min(0f)] float soloReengageDelay = 0f;

    EnemyContoroller controller;
    Transform lockedTarget;
    GameObject activeHitbox;
    GameObject activeWarning;
    BoxCollider2D[] meleeBoxes;
    Vector2 warningVisualCenter;
    Vector2 warningVisualSize = Vector2.one;
    bool hasWarningVisualBounds;
    Vector2 lockedDirection = Vector2.right;
    Vector2 lastKnownTarget;
    float startingHp;
    float cooldown;
    float repositionTimer;
    float repositionDuration;
    float orbitTimer;
    int orbitSign = 1;
    bool halfHpTriggered;
    bool repositioning;
    bool retreatFirst;
    bool orbitAttack;
    bool aimLocked;
    bool initialized;

    void Start()
    {
        controller = enemyBase as EnemyContoroller;
        if (controller == null || enemyBase.rb == null || enemyBase.animator == null)
        {
            Debug.LogError("月の粛清隊剣士: EnemyMoveBase.enemyBase に Rigidbody2D/Animator を持つ EnemyContoroller が必要です。", this);
            enabled = false;
            return;
        }
        startingHp = Mathf.Max(1f, enemyBase.HP);
        meleeBoxes = meleeHitboxPrefab != null
            ? meleeHitboxPrefab.GetComponentsInChildren<BoxCollider2D>(true)
            : new BoxCollider2D[0];
        controller.Choice_MoveToBase = EnemyContoroller.MoveToBase.Special;
        enemyBase.LookPlayerDistance = -1f; // 自動攻撃を無効化し、個別AIが攻撃を開始する。
        enemyBase.moveType = EnemyBase.MoveType.Move;
        initialized = true;
    }

    void Update()
    {
        if (!initialized) return;
        if (enemyBase.status != EnemyBase.Status.Nomal)
        {
            if (enemyBase.moveType == EnemyBase.MoveType.Attack || activeHitbox != null || activeWarning != null)
                CancelAttack();
            return;
        }
        // HP割合の判定は攻撃中でも実行する。実際の回避開始は攻撃終了後。
        if (!halfHpTriggered && enemyBase.HP > 0f &&
            enemyBase.HP <= startingHp * repositionHpRatio)
            halfHpTriggered = true;
    }

    public override void EnemyStanPlay() { CancelAttack(); }
    public override void EnemyConfusionPlay() { CancelAttack(); }

    public override void EnemyMovePlay()
    {
        if (!initialized) return;
        Transform target = CurrentTarget;
        if (target == null)
        {
            enemyBase.rb.velocity = Vector2.zero;
            SetLocomotion(false);
            return;
        }
        LookAtCurrentTarget();
        cooldown = Mathf.Max(0f, cooldown - Time.fixedDeltaTime);

        if (halfHpTriggered && !repositioning && repositionDuration < 0.001f)
            BeginReposition();

        if (repositioning)
        {
            PlayReposition(target);
            return;
        }

        Vector2 toward = (Vector2)target.position - (Vector2)transform.position;
        float distance = toward.magnitude;
        if (distance <= attackRange && cooldown <= 0f)
        {
            StartAttack(target);
            return;
        }
        if (distance <= approachTriggerDistance && cooldown <= 0f && Random.value < 0.015f)
        {
            StartAttack(target);
            return;
        }
        enemyBase.rb.velocity = toward.normalized * enemyBase.SPEED;
        SetLocomotion(true);
    }

    void BeginReposition()
    {
        repositioning = true;
        retreatFirst = true;
        repositionTimer = 0f;
        bool hasAlly = HasLivingAlly();
        repositionDuration = hasAlly
            ? Mathf.Max(initialRetreatSeconds,
                Random.Range(Mathf.Min(repositionSeconds.x, repositionSeconds.y),
                             Mathf.Max(repositionSeconds.x, repositionSeconds.y)))
            : initialRetreatSeconds + soloReengageDelay;
        orbitSign = Random.value < 0.5f ? -1 : 1;
    }

    bool HasLivingAlly()
    {
        // この判定はHP半減時に一度だけ。敵50体程度の環境を想定。
        EnemyBase[] all = FindObjectsOfType<EnemyBase>();
        foreach (EnemyBase other in all)
            if (other != null && other != enemyBase &&
                other.status != EnemyBase.Status.Die && other.HP > 0f &&
                (other.Factions & enemyBase.Factions) != Faction.None)
                return true;
        return false;
    }

    void PlayReposition(Transform target)
    {
        repositionTimer += Time.fixedDeltaTime;
        Vector2 toward = (Vector2)target.position - (Vector2)transform.position;
        float distance = toward.magnitude;
        Vector2 inward = distance > 0.001f ? toward / distance : Vector2.right;
        Vector2 tangent = new Vector2(-inward.y, inward.x) * orbitSign;
        float retreat = retreatFirst ? -1f : (distance < repositionPreferredDistance - 0.5f ? -0.65f :
            distance > repositionPreferredDistance + 0.5f ? 0.55f : 0f);
        if (repositionTimer >= initialRetreatSeconds) retreatFirst = false;
        Vector2 direction = (tangent * (retreatFirst ? 0.2f : 1f) + inward * retreat).normalized;
        enemyBase.rb.velocity = direction * enemyBase.SPEED * repositionSpeedMultiplier;
        SetLocomotion(true);
        if (repositionTimer < repositionDuration) return;
        repositioning = false;
        enemyBase.rb.velocity = Vector2.zero;
        cooldown = 0f;
    }

    void StartAttack(Transform target)
    {
        lockedTarget = target;
        lastKnownTarget = target.position;
        orbitAttack = Random.value < orbitAttackChance;
        orbitTimer = Random.Range(Mathf.Min(orbitBeforeAttackSeconds.x, orbitBeforeAttackSeconds.y),
                                  Mathf.Max(orbitBeforeAttackSeconds.x, orbitBeforeAttackSeconds.y));
        orbitSign = Random.value < 0.5f ? -1 : 1;
        A_Phase = orbitAttack ? 0 : 1;
        enemyBase.AttackCount = 0f;
        enemyBase.rb.velocity = Vector2.zero;
        enemyBase.moveType = EnemyBase.MoveType.Attack;
        if (!orbitAttack) BeginWindup();
    }

    public override void EnemyAttackPlay()
    {
        if (!initialized) return;
        // 攻撃中は対象を変更しない。
        if (lockedTarget != null) lastKnownTarget = lockedTarget.position;
        // 発動0.2秒前に照準を固定。以降は標的が移動しても見た目・警告・攻撃方向を変えない。
        enemyBase.isTargetLook(aimLocked || A_Phase >= 2
            ? (Vector2)transform.position + lockedDirection
            : lastKnownTarget);
        float dt = Time.fixedDeltaTime;
        switch (A_Phase)
        {
            case 0: // 旋回して間合いを調整
                Vector2 inward = lastKnownTarget - (Vector2)transform.position;
                float distance = inward.magnitude;
                inward = distance > 0.001f ? inward / distance : Vector2.right;
                Vector2 tangent = new Vector2(-inward.y, inward.x) * orbitSign;
                float radial = distance < attackRange ? -0.45f : distance > approachTriggerDistance ? 0.6f : 0f;
                enemyBase.rb.velocity = (tangent + inward * radial).normalized * enemyBase.SPEED;
                SetLocomotion(true);
                enemyBase.AttackCount += dt;
                if (enemyBase.AttackCount >= orbitTimer) BeginWindup();
                break;
            case 1: // 構え後半の0.2秒は方向を固定して攻撃を予告する。
                enemyBase.rb.velocity = Vector2.zero;
                enemyBase.AttackCount += dt;
                if (!aimLocked && enemyBase.AttackCount >= Mathf.Max(0f, windupSeconds - aimLockBeforeAttack))
                    LockAim();
                UpdateWarning();
                if (enemyBase.AttackCount >= windupSeconds) BeginDash();
                break;
            case 2: // 踏み込み開始時に確定した方向へ突進
                enemyBase.rb.velocity = lockedDirection * enemyBase.SPEED * dashSpeedMultiplier;
                enemyBase.AttackCount += dt;
                if (enemyBase.AttackCount >= dashSeconds)
                {
                    ClearHitbox();
                    enemyBase.rb.velocity = Vector2.zero;
                    NextPhase();
                }
                break;
            case 3:
                enemyBase.rb.velocity = Vector2.zero;
                enemyBase.AttackCount += dt;
                if (enemyBase.AttackCount >= recoverySeconds) FinishAttack();
                break;
        }
    }

    void BeginWindup()
    {
        A_Phase = 1;
        aimLocked = false;
        enemyBase.AttackCount = 0f;
        enemyBase.rb.velocity = Vector2.zero;
        enemyBase.animator.SetInteger("Anim", 2);
        enemyBase.animator.Play("攻撃待機", 0, 0f);
        ClearWarning();

        Vector2 direction = AimDirection();
        Vector2 center, size;
        GetSweptHitboxArea(direction, out center, out size);
        try
        {
            activeWarning = SpawnAttackWarning(EnemyContoroller.WarningRectangle,
                attackWarningPrefab, center, WarningRotation(direction),
                Mathf.Max(0.05f, windupSeconds), null, 0f, true);
            if (activeWarning != null)
            {
                CacheWarningVisualBounds();
                UpdateWarning();
            }
        }
        catch (System.InvalidCastException ex)
        {
            Debug.LogWarning("月の粛清隊剣士: 攻撃警告Prefabの生成をスキップ: " + ex.Message, this);
        }
        // 構えが0.2秒以下なら開始時から照準を固定する。
        if (windupSeconds <= aimLockBeforeAttack) LockAim();
    }

    void LockAim()
    {
        if (aimLocked) return;
        lockedDirection = AimDirection();
        aimLocked = true;
        enemyBase.isTargetLook((Vector2)transform.position + lockedDirection);
        UpdateWarning();
    }

    Vector2 AimDirection()
    {
        Vector2 toward = lastKnownTarget - (Vector2)transform.position;
        return toward.sqrMagnitude > 0.0001f ? toward.normalized : lockedDirection;
    }

    static Quaternion WarningRotation(Vector2 direction)
    {
        // 攻撃警告のローカルY軸を踏み込み方向に向ける。
        return Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
    }

    // 踏み込み中にBoxCollider2Dが通過する全範囲を矩形として算出。
    // AttackSpawnPointの位置・回転・Scale、ColliderのOffset/Sizeと子Transformを反映する。
    void GetSweptHitboxArea(Vector2 direction, out Vector2 center, out Vector2 size)
    {
        Transform anchor = enemyBase.AttackSpawnPoint != null
            ? enemyBase.AttackSpawnPoint.transform : transform;
        Vector2 origin = anchor.position;
        Vector2 side = new Vector2(-direction.y, direction.x);
        float minForward = float.PositiveInfinity;
        float maxForward = float.NegativeInfinity;
        float minSide = float.PositiveInfinity;
        float maxSide = float.NegativeInfinity;

        if (meleeHitboxPrefab != null && meleeBoxes != null)
        {
            Transform prefabRoot = meleeHitboxPrefab.transform;
            foreach (BoxCollider2D box in meleeBoxes)
            {
                if (box == null || !box.enabled) continue;
                Vector2 half = box.size * 0.5f;
                for (int x = -1; x <= 1; x += 2)
                {
                    for (int y = -1; y <= 1; y += 2)
                    {
                        Vector3 boxLocalPoint = (Vector3)(box.offset +
                            new Vector2(half.x * x, half.y * y));
                        // Prefab内の子オブジェクト座標を、出現位置のワールド座標へ変換。
                        Vector3 prefabLocal = prefabRoot.InverseTransformPoint(
                            box.transform.TransformPoint(boxLocalPoint));
                        Vector3 worldPoint = anchor.TransformPoint(
                            Vector3.Scale(prefabLocal, prefabRoot.localScale));
                        Vector2 relative = (Vector2)worldPoint - origin;
                        float along = Vector2.Dot(relative, direction);
                        float across = Vector2.Dot(relative, side);
                        minForward = Mathf.Min(minForward, along);
                        maxForward = Mathf.Max(maxForward, along);
                        minSide = Mathf.Min(minSide, across);
                        maxSide = Mathf.Max(maxSide, across);
                    }
                }
            }
        }

        // BoxColliderが未設定の場合でも、警告を生成できるよう最低限の矩形を使う。
        if (float.IsInfinity(minForward))
        {
            minForward = -0.5f;
            maxForward = 0.5f;
            minSide = -0.5f;
            maxSide = 0.5f;
        }

        // 実際の踏み込みは一定方向に直進するため、初期矩形をその方向へ延長する。
        float dashTravel = Mathf.Max(0f, enemyBase.SPEED * dashSpeedMultiplier * dashSeconds);
        maxForward += dashTravel;
        center = origin + direction * ((minForward + maxForward) * 0.5f) +
                 side * ((minSide + maxSide) * 0.5f);
        center += side * warningOffset.x + direction * warningOffset.y;
        size = new Vector2(
            Mathf.Max(0.05f, (maxSide - minSide) * Mathf.Max(0.05f, warningSizeMultiplier.x)),
            Mathf.Max(0.05f, (maxForward - minForward) * Mathf.Max(0.05f, warningSizeMultiplier.y)));
    }

    // 警告Prefabの画像が1x1とは限らないため、実際のスプライト寸法を測って補正する。
    void CacheWarningVisualBounds()
    {
        hasWarningVisualBounds = false;
        warningVisualCenter = Vector2.zero;
        warningVisualSize = Vector2.one;
        if (activeWarning == null) return;
        float minX = float.PositiveInfinity, minY = float.PositiveInfinity;
        float maxX = float.NegativeInfinity, maxY = float.NegativeInfinity;
        Transform root = activeWarning.transform;
        foreach (SpriteRenderer spriteRenderer in activeWarning.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (!spriteRenderer.enabled || spriteRenderer.sprite == null) continue;
            Bounds spriteBounds = spriteRenderer.sprite.bounds;
            Vector2 spriteSize = spriteRenderer.drawMode == SpriteDrawMode.Simple
                ? (Vector2)spriteBounds.size : spriteRenderer.size;
            Vector3 visualCenter = spriteBounds.center;
            Vector2 half = spriteSize * 0.5f;
            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    Vector3 point = visualCenter + new Vector3(half.x * x, half.y * y, 0f);
                    Vector3 inRoot = root.InverseTransformPoint(
                        spriteRenderer.transform.TransformPoint(point));
                    minX = Mathf.Min(minX, inRoot.x);
                    maxX = Mathf.Max(maxX, inRoot.x);
                    minY = Mathf.Min(minY, inRoot.y);
                    maxY = Mathf.Max(maxY, inRoot.y);
                }
            }
        }
        if (!float.IsInfinity(minX) && maxX - minX > 0.0001f && maxY - minY > 0.0001f)
        {
            hasWarningVisualBounds = true;
            warningVisualSize = new Vector2(maxX - minX, maxY - minY);
            warningVisualCenter = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
        }
    }

    void UpdateWarning()
    {
        if (activeWarning == null) return;
        Vector2 direction = aimLocked ? lockedDirection : AimDirection();
        Vector2 center, size;
        GetSweptHitboxArea(direction, out center, out size);
        Transform warning = activeWarning.transform;
        warning.rotation = WarningRotation(direction);
        // 子オブジェクトの元Scaleは維持し、親のScaleで警告の縦横を調整。
        Vector2 unit = hasWarningVisualBounds ? warningVisualSize : Vector2.one;
        warning.localScale = new Vector3(size.x / unit.x, size.y / unit.y, warning.localScale.z);
        Vector3 localCenter = new Vector3(
            warningVisualCenter.x * warning.localScale.x,
            warningVisualCenter.y * warning.localScale.y, 0f);
        warning.position = (Vector3)center - warning.rotation * localCenter;
    }

    void BeginDash()
    {
        if (!aimLocked) LockAim();
        ClearWarning();
        enemyBase.animator.SetInteger("Anim", 3);
        enemyBase.animator.Play("攻撃", 0, 0f);
        if (enemyBase.AttackAudio != null)
            enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
        Transform anchor = enemyBase.AttackSpawnPoint != null
            ? enemyBase.AttackSpawnPoint.transform : transform;
        if (meleeHitboxPrefab != null)
        {
            activeHitbox = Instantiate(meleeHitboxPrefab, anchor.position, anchor.rotation, anchor);
            if (controller.ConfigureAttackObject(activeHitbox, attackPowerPercent) == 0)
                Debug.LogWarning("月の粛清隊剣士: 近接判定PrefabにEnemyAttackがありません。", activeHitbox);
        }
        NextPhase();
    }

    void NextPhase() { A_Phase++; enemyBase.AttackCount = 0f; }

    void FinishAttack()
    {
        ClearHitbox();
        ClearWarning();
        lockedTarget = null;
        aimLocked = false;
        enemyBase.AttackCount = 0f;
        A_Phase = 0;
        enemyBase.rb.velocity = Vector2.zero;
        cooldown = attackCooldown;
        enemyBase.moveType = EnemyBase.MoveType.Move;
        SetLocomotion(false);
    }

    void CancelAttack()
    {
        ClearHitbox();
        ClearWarning();
        lockedTarget = null;
        aimLocked = false;
        A_Phase = 0;
        enemyBase.AttackCount = 0f;
        if (enemyBase.moveType == EnemyBase.MoveType.Attack)
            enemyBase.moveType = EnemyBase.MoveType.Move;
        // スタン・混乱時の速度は既存共通処理に任せる。
    }

    void ClearHitbox()
    {
        if (activeHitbox != null) Destroy(activeHitbox);
        activeHitbox = null;
    }

    void ClearWarning()
    {
        if (activeWarning != null) Destroy(activeWarning);
        activeWarning = null;
    }

    void SetLocomotion(bool moving)
    {
        enemyBase.animator.SetInteger("Anim", moving ? 1 : 0);
        string state = moving ? "歩行" : "待機";
        if (!enemyBase.animator.GetCurrentAnimatorStateInfo(0).IsName(state))
            enemyBase.animator.Play(state);
    }

    void OnDisable() { ClearHitbox(); ClearWarning(); }
}
