using UnityEngine;

// 月の巡礼者。EnemyContoroller の Special 移動を使用し、共通クラスは変更しない。
public class DK_PilgrimOfTheMoon_AI : EnemyMoveBase
{
    [Header("ターゲット")]
    [SerializeField] float stunPlayerPriorityDistance = 4f;
    [SerializeField] float targetSearchInterval = 0.25f;
    [SerializeField] float attackStartDistance = 1.5f;
    [SerializeField] float forcedAttackInterval = 2f;

    [Header("ジグザグ接近")]
    [SerializeField] float zigzagInterval = 0.4f;
    [SerializeField, Range(0f, 80f)] float zigzagAngle = 35f;

    [Header("攻撃の位置調整")]
    [SerializeField] float adjustmentDistance = 2f;
    [SerializeField] float retreatMaxTime = 0.8f;
    [SerializeField] float sideStepDuration = 0.25f;
    [SerializeField] float sideStepSpeedMultiplier = 1.2f;

    [Header("杭投げ（PrefabにはRigidbody2DとTrigger Collider2Dが必要）")]
    [SerializeField] GameObject stakePrefab;
    [SerializeField] float stakeSpeed = 22f;
    [SerializeField] float stakeLifetime = 5f;
    [SerializeField] float stakeRotationOffset = 0f;
    [SerializeField] float throwRecoveryTime = 0.3f;

    [Header("近接突進（PrefabにはTrigger Collider2Dが必要）")]
    [SerializeField] GameObject meleeHitboxPrefab;
    [SerializeField] float dashSpeedMultiplier = 4f;
    [SerializeField] float dashDuration = 0.35f;
    [SerializeField] float meleeRecoveryTime = 0.3f;

    [Header("攻撃警告（未設定時はControllerの四角形警告を使用）")]
    [SerializeField] GameObject stakeWarningPrefab;
    [SerializeField] GameObject meleeWarningPrefab;
    [SerializeField, Min(0.01f)] float warningDuration = 1f;

    GameObject activeAttackWarning;
    Transform currentTarget;
    Transform attackTarget;
    Vector2 lastAttackTargetPosition;
    Vector2 lockedDirection = Vector2.right;

    float noAttackTimer;
    float searchTimer;
    float zigzagTimer;
    bool zigzagRight = true;
    bool needsAdjustment;
    int orbitDirection = 1;
    int attackKind; // 0=杭投げ (20%)、1=近接攻撃 (80%)
    GameObject activeMeleeHitbox;

    void Start()
    {
        if (enemyBase == null)
        {
            Debug.LogError("月の巡礼者: EnemyMoveBase.enemyBase が未設定です。", this);
            enabled = false;
            return;
        }

        EnemyContoroller controller = enemyBase as EnemyContoroller;
        if (controller == null)
        {
            Debug.LogError("月の巡礼者: EnemyContoroller を使用してください。", this);
            enabled = false;
            return;
        }

        // EnemyContoroller の通常のプレイヤー直進移動・自動攻撃を無効にする。
        controller.Choice_MoveToBase = EnemyContoroller.MoveToBase.Special;
        enemyBase.LookPlayerDistance = -1f;
        enemyBase.moveType = EnemyBase.MoveType.Move;

        currentTarget = enemyBase.Player != null ? enemyBase.Player.transform : null;
        searchTimer = targetSearchInterval;
    }

    void Update()
    {
        if (enemyBase == null) return;

        // 死亡や特殊状態で突進判定が残留しないようにする。
        if (enemyBase.status != EnemyBase.Status.Nomal &&
            (enemyBase.moveType == EnemyBase.MoveType.Attack || activeMeleeHitbox != null))
        {
            CancelAttack();
        }
    }

    public override void EnemyStanPlay()
    {
        CancelAttack();
        if (enemyBase.Player != null &&
            Vector2.Distance(enemyBase.transform.position, enemyBase.Player.transform.position)
                <= stunPlayerPriorityDistance)
        {
            currentTarget = enemyBase.Player.transform;
            EnemyContoroller controller = enemyBase as EnemyContoroller;
            if (controller != null) controller.ForceTarget(currentTarget);
        }
    }

    public override void EnemyConfusionPlay()
    {
        CancelAttack();
    }

    public override void EnemyMovePlay()
    {
        if (enemyBase.Player == null) return;

        // 共通Controllerの自動攻撃判定で使われる値は、実際のプレイヤー距離のまま維持。
        enemyBase.PlayerDistance = Vector2.Distance(
            enemyBase.transform.position, enemyBase.Player.transform.position);
        enemyBase.PlayerDirection = enemyBase.Player.transform.position - enemyBase.transform.position;

        searchTimer += Time.fixedDeltaTime;
        if (searchTimer >= targetSearchInterval || !IsTargetUsable(currentTarget))
        {
            searchTimer = 0f;
            ChooseTarget(false);
        }

        if (!IsTargetUsable(currentTarget)) currentTarget = enemyBase.Player.transform;
        if (currentTarget == null) return;

        Vector2 targetPos = currentTarget.position;
        Vector2 toward = targetPos - (Vector2)enemyBase.transform.position;
        float distance = toward.magnitude;
        enemyBase.isTargetLook(targetPos);

        noAttackTimer += Time.fixedDeltaTime;
        if (distance <= attackStartDistance || noAttackTimer >= forcedAttackInterval)
        {
            bool forced = noAttackTimer >= forcedAttackInterval;
            if (forced) ChooseTarget(true); // 強制攻撃時にはプレイヤーより近い敵を再検索。
            StartAttack();
            return;
        }

        zigzagTimer += Time.fixedDeltaTime;
        if (zigzagTimer >= zigzagInterval)
        {
            zigzagTimer = 0f;
            zigzagRight = !zigzagRight;
        }

        float angle = zigzagRight ? zigzagAngle : -zigzagAngle;
        Vector2 movement = Rotate(toward.normalized, angle);
        enemyBase.rb.velocity = movement * enemyBase.SPEED;
        enemyBase.animator.SetInteger("Anim", 1);
        if (!enemyBase.animator.GetCurrentAnimatorStateInfo(0).IsName("歩行"))
            enemyBase.animator.Play("歩行");
    }

    // 対象選択・派閥検索は共通EnemyContorollerに集約。
    void ChooseTarget(bool forcedAttack)
    {
        EnemyContoroller controller = enemyBase as EnemyContoroller;
        if (controller == null) return;

        // 通常はControllerの更新結果を利用。2秒の強制攻撃時だけ再選択する。
        if (forcedAttack) controller.ChooseTargetNow(true);
        currentTarget = controller.CurrentTarget;
    }

    bool IsTargetUsable(Transform target)
    {
        if (target == null) return false;
        if (enemyBase.Player != null && target == enemyBase.Player.transform) return true;
        EnemyBase other = target.GetComponent<EnemyBase>();
        return other != null && other != enemyBase &&
               other.status != EnemyBase.Status.Die && other.HP > 0f &&
               (other.Factions & Faction.Diana) == Faction.None;
    }

    void StartAttack()
    {
        if (currentTarget == null) return;
        attackTarget = currentTarget; // 攻撃終了まで別の相手へ変更しない。
        lastAttackTargetPosition = attackTarget.position;
        attackKind = Random.Range(0, 5) == 0 ? 0 : 1;
        orbitDirection = Random.value < 0.5f ? -1 : 1;
        needsAdjustment = Vector2.Distance(enemyBase.transform.position, lastAttackTargetPosition)
                          <= adjustmentDistance;
        noAttackTimer = 0f;
        EnemyContoroller controller = enemyBase as EnemyContoroller;
        if (controller != null) controller.ClearForcedTarget();
        A_Phase = 0;
        enemyBase.AttackCount = 0f;
        enemyBase.mode = EnemyBase.ModeType.M1;
        enemyBase.rb.velocity = Vector2.zero;
        enemyBase.moveType = EnemyBase.MoveType.Attack;
    }

    public override void EnemyAttackPlay()
    {
        if (enemyBase == null) return;
        if (attackTarget != null && IsTargetUsable(attackTarget))
            lastAttackTargetPosition = attackTarget.position;

        switch (A_Phase)
        {
            case 0:
                enemyBase.rb.velocity = Vector2.zero;
                enemyBase.animator.Play("汎用攻撃待機", 0, 0f);
                if (enemyBase.AttackStayAudio != null)
                    enemyBase.audioManager.PlaySE(enemyBase.AttackStayAudio);
                NextPhase();
                break;
            case 1:
                PositionForAttack();
                break;
            case 2:
                enemyBase.rb.velocity = Vector2.zero;
                // 斬撃は発動直前に再照準するため、警告も予備動作中に追従させる。
                if (attackKind == 1) UpdateMeleeWarningDirection();
                enemyBase.AttackCount += Time.fixedDeltaTime;
                if (enemyBase.AttackCount >= 1f)
                {
                    if (attackKind == 0) ThrowStake();
                    else BeginDash();
                    NextPhase();
                }
                break;
            case 3:
                enemyBase.AttackCount += Time.fixedDeltaTime;
                if (attackKind == 0)
                {
                    enemyBase.rb.velocity = Vector2.zero;
                    if (enemyBase.AttackCount >= throwRecoveryTime) FinishAttack();
                }
                else
                {
                    enemyBase.rb.velocity = lockedDirection * enemyBase.SPEED * dashSpeedMultiplier;
                    if (enemyBase.AttackCount >= dashDuration)
                    {
                        ClearMeleeHitbox();
                        enemyBase.rb.velocity = Vector2.zero;
                        NextPhase();
                    }
                }
                break;
            case 4:
                enemyBase.rb.velocity = Vector2.zero;
                enemyBase.AttackCount += Time.fixedDeltaTime;
                if (enemyBase.AttackCount >= meleeRecoveryTime) FinishAttack();
                break;
        }
    }

    void PositionForAttack()
    {
        Vector2 toTarget = lastAttackTargetPosition - (Vector2)enemyBase.transform.position;
        enemyBase.isTargetLook(lastAttackTargetPosition);

        if (!needsAdjustment)
        {
            StartWindup();
            return;
        }

        enemyBase.AttackCount += Time.fixedDeltaTime;
        if (attackKind == 0)
        {
            // 杭投げ: 距離が約2になるまで後退（安全のため最大時間を設ける）。
            if (toTarget.magnitude >= adjustmentDistance ||
                enemyBase.AttackCount >= retreatMaxTime)
            {
                StartWindup();
                return;
            }

            Vector2 backwards = toTarget.sqrMagnitude > 0.0001f
                ? -toTarget.normalized : Vector2.left;
            enemyBase.rb.velocity = backwards * enemyBase.SPEED;
        }
        else
        {
            // 近接: ターゲットの周囲を同じ方向へ2回サイドステップする。
            if (enemyBase.AttackCount >= sideStepDuration * 2f)
            {
                StartWindup();
                return;
            }

            Vector2 inward = toTarget.sqrMagnitude > 0.0001f
                ? toTarget.normalized : Vector2.right;
            Vector2 tangent = new Vector2(-inward.y, inward.x) * orbitDirection;
            Vector2 orbit = (tangent + inward * 0.18f).normalized;
            enemyBase.rb.velocity = orbit * enemyBase.SPEED * sideStepSpeedMultiplier;
        }
    }

    void StartWindup()
    {
        enemyBase.rb.velocity = Vector2.zero;
        if (attackKind == 0)
        {
            // 角度は「杭投げ初動」の再生時に確定し、発射まで変えない。
            lockedDirection = AimAtLastTarget();
            enemyBase.animator.Play("杭投げ初動", 0, 0f);
            // 「杭投げループ」への遷移はAnimator Controller側に任せる。
        }
        else
        {
            enemyBase.animator.Play("近接攻撃待機", 0, 0f);
        }
        ShowAttackWarning();
        NextPhase();
    }

    void ThrowStake()
    {
        ClearAttackWarning();
        enemyBase.animator.Play("杭投げ攻撃", 0, 0f);
        if (enemyBase.AttackAudio != null)
            enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
        if (stakePrefab == null) return;

        Vector3 position = enemyBase.AttackSpawnPoint != null
            ? enemyBase.AttackSpawnPoint.transform.position : enemyBase.transform.position;
        float angle = Mathf.Atan2(lockedDirection.y, lockedDirection.x) * Mathf.Rad2Deg
                      + stakeRotationOffset;
        GameObject stake = Instantiate(stakePrefab, position, Quaternion.Euler(0f, 0f, angle));
        EnemyAttack attack = stake.GetComponent<EnemyAttack>();
        if (attack != null) attack.enemyBase = enemyBase;
        else Debug.LogError("杭PrefabにEnemyAttackがありません。", stake);

        Rigidbody2D bulletRb = stake.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.gravityScale = 0f;
            bulletRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            bulletRb.velocity = lockedDirection * stakeSpeed;
        }
        else Debug.LogError("杭PrefabにRigidbody2Dがありません。", stake);
        Destroy(stake, stakeLifetime); // 無限貫通は維持、寿命は有限。
    }

    void BeginDash()
    {
        ClearAttackWarning();
        lockedDirection = AimAtLastTarget();
        enemyBase.isTargetLook(lastAttackTargetPosition);
        enemyBase.animator.Play("近接攻撃", 0, 0f);
        if (enemyBase.AttackAudio != null)
            enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);

        if (meleeHitboxPrefab == null) return;
        Transform anchor = enemyBase.AttackSpawnPoint != null
            ? enemyBase.AttackSpawnPoint.transform : enemyBase.transform;
        activeMeleeHitbox = Instantiate(meleeHitboxPrefab, anchor.position, anchor.rotation);
        activeMeleeHitbox.transform.SetParent(anchor, true);

        EnemyAttack attack = activeMeleeHitbox.GetComponent<EnemyAttack>();
        if (attack != null) attack.enemyBase = enemyBase;
        else Debug.LogError("近接PrefabにEnemyAttackがありません。", activeMeleeHitbox);
    }

    // 警告は1秒の予備動作中に表示。既存Prefabのサイズ・子階層をそのまま利用する。
    void ShowAttackWarning()
    {
        ClearAttackWarning();
        GameObject warningPrefab = attackKind == 0 ? stakeWarningPrefab : meleeWarningPrefab;
        Vector2 direction = attackKind == 0 ? lockedDirection : AimAtLastTarget();
        Quaternion rotation = WarningRotation(direction);
        activeAttackWarning = SpawnAttackWarning(
            EnemyContoroller.WarningRectangle, warningPrefab,
            enemyBase.transform.position, rotation, warningDuration,
            null, 0f, true);
    }

    void UpdateMeleeWarningDirection()
    {
        if (activeAttackWarning == null) return;
        activeAttackWarning.transform.rotation = WarningRotation(AimAtLastTarget());
    }

    static Quaternion WarningRotation(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        return Quaternion.Euler(0f, 0f, angle);
    }

    void ClearAttackWarning()
    {
        if (activeAttackWarning != null) Destroy(activeAttackWarning);
        activeAttackWarning = null;
    }

    Vector2 AimAtLastTarget()
    {
        Vector2 aim = lastAttackTargetPosition - (Vector2)enemyBase.transform.position;
        if (aim.sqrMagnitude > 0.0001f) return aim.normalized;
        return enemyBase.EnemyAnimBody.transform.localScale.x < 0f ? Vector2.left : Vector2.right;
    }

    static Vector2 Rotate(Vector2 dir, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        return new Vector2(dir.x * cos - dir.y * sin, dir.x * sin + dir.y * cos);
    }

    void NextPhase()
    {
        A_Phase++;
        enemyBase.AttackCount = 0f;
    }

    void ClearMeleeHitbox()
    {
        if (activeMeleeHitbox != null) Destroy(activeMeleeHitbox);
        activeMeleeHitbox = null;
    }

    void FinishAttack()
    {
        ClearAttackWarning();
        ClearMeleeHitbox();
        A_Phase = 0;
        attackTarget = null;
        enemyBase.mode = EnemyBase.ModeType.M1;
        enemyBase.AttackCount = 0f;
        enemyBase.rb.velocity = Vector2.zero;
        enemyBase.moveType = EnemyBase.MoveType.Move;
        enemyBase.animator.SetInteger("Anim", 1);
        enemyBase.animator.Play("歩行", 0, 0f);
        searchTimer = targetSearchInterval;
    }

    void CancelAttack()
    {
        ClearAttackWarning();
        ClearMeleeHitbox();
        A_Phase = 0;
        attackTarget = null;
        enemyBase.mode = EnemyBase.ModeType.M1;
        enemyBase.AttackCount = 0f;
        noAttackTimer = 0f;
        enemyBase.moveType = EnemyBase.MoveType.Move;
        // スタン時のノックバック速度は共通処理に任せる。
    }

    void OnDisable()
    {
        ClearAttackWarning();
        ClearMeleeHitbox();
    }
}
