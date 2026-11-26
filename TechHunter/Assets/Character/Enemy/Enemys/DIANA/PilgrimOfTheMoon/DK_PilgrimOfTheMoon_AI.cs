using System.Collections.Generic;
using UnityEngine.Tilemaps;
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

    [Header("跳躍・扇状杭攻撃")]
    [SerializeField] float leapTargetDistance = 5f;
    [SerializeField] float leapDuration = 0.8f;
    [SerializeField] float leapArcHeight = 3f;
    [SerializeField] float fanSpreadAngle = 80f;
    [SerializeField] float fanStakeSpacing = 0.45f;
    [SerializeField] float fanShotInterval = 0.15f;
    [SerializeField] float fanStakeSpinSpeed = 1080f; // degrees per second
    [SerializeField, Min(0f)] float fanStakeAimLeadTime = 0.2f;
    readonly List<GameObject> fanStakes = new List<GameObject>();
    readonly Vector2[] fanDirections = new Vector2[5];
    readonly GameObject[] fanWarnings = new GameObject[5];
    const float FanChargeTime = 1f;
    const float FanRecoveryTime = 1f;
    readonly int[] fanOrder = { 0, 4, 1, 3, 2 };
    int fanShotIndex;
    Vector2 leapStart, leapDestination, fanTargetPosition;
    RigidbodyType2D originalBodyType;
    bool leapPhysicsChanged;

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
    int attackKind; // 0=杭投げ、1=近接攻撃、2=跳躍・扇状杭
    GameObject activeMeleeHitbox;
    BoxCollider2D[] stakeHitboxes;
    BoxCollider2D[] meleeHitboxes;

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

        // 予備動作の毎FixedUpdateでGetComponentsInChildrenの配列を生成しない。
        if (stakePrefab != null) stakeHitboxes = stakePrefab.GetComponentsInChildren<BoxCollider2D>();
        if (meleeHitboxPrefab != null) meleeHitboxes = meleeHitboxPrefab.GetComponentsInChildren<BoxCollider2D>();

        ChooseTarget(false);
        searchTimer = targetSearchInterval;
    }

    void Update()
    {
        if (enemyBase == null) return;

        // 死亡・特殊状態で判定、警告、展開中の杭や跳躍物理設定を残さない。
        if (enemyBase.status != EnemyBase.Status.Nomal &&
            (enemyBase.moveType == EnemyBase.MoveType.Attack || activeMeleeHitbox != null ||
             activeAttackWarning != null || fanStakes.Count > 0 || leapPhysicsChanged))
        {
            CancelAttack();
        }
    }

    public override void EnemyStanPlay()
    {
        CancelAttack();
        EnemyContoroller controller = enemyBase as EnemyContoroller;
        if (controller != null && controller.TargetMode != EnemyContoroller.TargetType.EnemyOnly &&
            enemyBase.Player != null &&
            Vector2.Distance(enemyBase.transform.position, enemyBase.Player.transform.position)
                <= stunPlayerPriorityDistance)
        {
            currentTarget = enemyBase.Player.transform;
            controller.ForceTarget(currentTarget);
        }
    }

    public override void EnemyConfusionPlay()
    {
        CancelAttack();
    }

    public override void EnemyMovePlay()
    {
        // プレイヤー専用の旧挙動は維持し、敵専用モードではPlayerを参照しない。
        EnemyContoroller controller = enemyBase as EnemyContoroller;
        if (controller != null && controller.TargetMode == EnemyContoroller.TargetType.PlayerOnly && enemyBase.Player != null)
        {
            enemyBase.PlayerDistance = Vector2.Distance(
                enemyBase.transform.position, enemyBase.Player.transform.position);
            enemyBase.PlayerDirection = enemyBase.Player.transform.position - enemyBase.transform.position;
        }

        searchTimer += Time.fixedDeltaTime;
        if (searchTimer >= targetSearchInterval || !IsTargetUsable(currentTarget))
        {
            searchTimer = 0f;
            ChooseTarget(false);
        }

        if (!IsTargetUsable(currentTarget))
        {
            ChooseTarget(false);
            if (!IsTargetUsable(currentTarget))
            {
                currentTarget = null;
                enemyBase.rb.velocity = Vector2.zero;
                return;
            }
        }

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
        EnemyContoroller controller = enemyBase as EnemyContoroller;
        if (enemyBase.Player != null && target == enemyBase.Player.transform)
            return controller == null || controller.TargetMode != EnemyContoroller.TargetType.EnemyOnly;

        EnemyBase other = target.GetComponent<EnemyBase>();
        if (other == null || other == enemyBase || other.status == EnemyBase.Status.Die || other.HP <= 0f)
            return false;

        // EnemyOnly/PlayerAndEnemyでも独自の国別判定を行うとControllerと
        // 違う標的を拒否する。共通の派閥・除外設定に判定を集約する。
        if (controller != null && controller.TargetMode != EnemyContoroller.TargetType.PlayerOnly)
            return controller.CanTarget(target);

        // 通常のプレイヤー戦では従来の判定を維持する。
        return (other.Factions & Faction.Diana) == Faction.None;
    }

    void StartAttack()
    {
        if (currentTarget == null) return;
        attackTarget = currentTarget; // 攻撃終了まで別の相手へ変更しない。
        lastAttackTargetPosition = attackTarget.position;
        attackKind = Random.Range(0, 5) == 0 ? 0 : (Random.Range(0, 4) == 0 ? 2 : 1);
        if (attackKind == 2) fanTargetPosition = lastAttackTargetPosition;
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

        if (attackKind == 2) { PlayLeapFanAttack(); return; }

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
                UpdateAttackWarning();
                enemyBase.AttackCount += Time.fixedDeltaTime;
                if (enemyBase.AttackCount >= AttackWindupTime)
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
                    enemyBase.rb.velocity = lockedDirection * DashSpeed;
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


    // 跳躍と杭の展開は固定更新のフェーズで管理し、スタン時にも確実に中断する。
    void PlayLeapFanAttack()
    {
        enemyBase.rb.velocity = Vector2.zero;
        enemyBase.AttackCount += Time.fixedDeltaTime;
        switch (A_Phase)
        {
            case 0:
                enemyBase.animator.Play("跳躍待機", 0, 0f);
                leapStart = enemyBase.transform.position;
                leapDestination = FindLeapDestination(fanTargetPosition);
                NextPhase();
                break;
            case 1:
                if (enemyBase.AttackCount >= 1f)
                {
                    originalBodyType = enemyBase.rb.bodyType;
                    enemyBase.rb.bodyType = RigidbodyType2D.Kinematic;
                    leapPhysicsChanged = true;
                    leapStart = enemyBase.transform.position;
                    enemyBase.animator.Play("跳躍", 0, 0f);
                    NextPhase();
                }
                break;
            case 2:
                float progress = Mathf.Clamp01(enemyBase.AttackCount / Mathf.Max(0.05f, leapDuration));
                Vector2 pos = Vector2.Lerp(leapStart, leapDestination, progress);
                pos.y += Mathf.Sin(progress * Mathf.PI) * leapArcHeight;
                enemyBase.rb.MovePosition(pos);
                if (progress >= 1f)
                {
                    enemyBase.rb.MovePosition(leapDestination);
                    RestoreLeapPhysics();
                    enemyBase.animator.Play("着地", 0, 0f);
                    NextPhase();
                }
                break;
            case 3:
                if (enemyBase.AttackCount >= 0.5f)
                {
                    // 跳躍前に固定した攻撃対象の現在位置を、着地後に一度だけ取得する。
                    if (attackTarget != null && IsTargetUsable(attackTarget))
                        fanTargetPosition = attackTarget.position;
                    DeployFanStakes();
                    NextPhase();
                }
                break;
            case 4:
                UpdateFanStakeRotation(enemyBase.AttackCount);
                if (enemyBase.AttackCount >= FanChargeTime)
                {
                    enemyBase.animator.Play("詠唱発動", 0, 0f);
                    fanShotIndex = 0;
                    NextPhase();
                }
                break;
            case 5:
                UpdateFanStakeRotation(FanChargeTime + enemyBase.AttackCount);
                if (enemyBase.AttackCount >= fanShotIndex * FanShotInterval)
                {
                    FireFanStake(fanOrder[fanShotIndex]);
                    fanShotIndex++;
                    if (fanShotIndex >= 5) NextPhase();
                }
                break;
            case 6:
                if (enemyBase.AttackCount >= FanRecoveryTime) FinishAttack();
                break;
        }
    }

    Vector2 FindLeapDestination(Vector2 target)
    {
        // EnemySpawnArea のタイルだけを着地点として採用する。
        if (ALL_SystemManager.Instance == null ||
            ALL_SystemManager.Instance.enemySpawnBASE == null ||
            ALL_SystemManager.Instance.enemySpawnBASE.Seve_TileMap_C == null)
            return enemyBase.transform.position;

        Tilemap tilemap = ALL_SystemManager.Instance.enemySpawnBASE.Seve_TileMap_C.StandFloor;
        if (tilemap == null) return enemyBase.transform.position;
        Vector2 fallback = enemyBase.transform.position;
        float bestDifference = float.MaxValue;
        List<Vector2> preferred = new List<Vector2>();
        BoundsInt bounds = tilemap.cellBounds;
        foreach (Vector3Int cell in bounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(cell)) continue;
            Vector2 candidate = tilemap.GetCellCenterWorld(cell);
            float distance = Vector2.Distance(candidate, target);
            float difference = Mathf.Abs(distance - leapTargetDistance);
            if (difference < bestDifference)
            {
                bestDifference = difference;
                fallback = candidate;
            }
            if (difference <= 0.75f) preferred.Add(candidate);
        }
        return preferred.Count > 0 ? preferred[Random.Range(0, preferred.Count)] : fallback;
    }

    void DeployFanStakes()
    {
        ClearFanStakes();
        if (stakePrefab == null) return;
        Vector2 direction = fanTargetPosition - (Vector2)enemyBase.transform.position;
        if (direction.sqrMagnitude < 0.0001f) direction = Vector2.right;
        direction.Normalize();
        Vector2 origin = enemyBase.AttackSpawnPoint != null
            ? (Vector2)enemyBase.AttackSpawnPoint.transform.position
            : (Vector2)enemyBase.transform.position;
        for (int i = 0; i < 5; i++)
        {
            float angle = (i - 2) * fanSpreadAngle / 4f;
            Vector2 shotDirection = Rotate(direction, angle);
            fanDirections[i] = shotDirection;
            Vector2 position = origin + shotDirection * fanStakeSpacing;
            GameObject stake = Instantiate(stakePrefab, position, StakeRotation(shotDirection));
            ConfigureSummonedAttack(stake);
            Rigidbody2D rb = stake.GetComponent<Rigidbody2D>();
            if (rb != null) { rb.velocity = Vector2.zero; rb.gravityScale = 0f; }
            // 展開中は衝突しない。射出時に有効化する。
            Collider2D[] colliders = stake.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D collider in colliders) collider.enabled = false;
            fanStakes.Add(stake);
        }

        // 各杭は異なる位置・角度・射出時刻から飛ぶので、5本それぞれを警告する。
        for (int order = 0; order < fanOrder.Length; order++)
        {
            int index = fanOrder[order];
            Vector2 shotDirection = fanDirections[index];
            Vector2 originOfShot = fanStakes[index].transform.position;
            // 共通EnemyAttackの攻撃終了時消滅も反映する。
            float remainingAttack = FanRecoveryTime + (fanOrder.Length - 1 - order) * FanShotInterval;
            float flightDistance = StakeTravelDistance(remainingAttack);
            GameObject warning = CreateAttackWarning(stakeWarningPrefab, originOfShot,
                shotDirection, FanChargeTime + order * FanShotInterval);
            fanWarnings[index] = warning;
            FitSweptWarning(warning, stakePrefab, stakeHitboxes, originOfShot, StakeRotation(shotDirection),
                shotDirection, flightDistance);
        }
    }

    // 杭ごとの射出予定時刻から逆算し、射出0.2秒前に狙いを固定する。
    // 射出順は左端→右端→左内側→右内側→中央。
    void UpdateFanStakeRotation(float elapsedSinceDeploy)
    {
        float interval = FanShotInterval;
        for (int order = 0; order < fanOrder.Length; order++)
        {
            int index = fanOrder[order];
            if (index >= fanStakes.Count || fanStakes[index] == null) continue;
            GameObject stake = fanStakes[index];
            float shotTime = FanChargeTime + order * interval;
            if (elapsedSinceDeploy >= shotTime - fanStakeAimLeadTime)
            {
                stake.transform.rotation = StakeRotation(fanDirections[index]);
            }
            else
            {
                stake.transform.Rotate(0f, 0f, fanStakeSpinSpeed * Time.fixedDeltaTime);
            }
        }
    }

    void FireFanStake(int index)
    {
        if (fanWarnings[index] != null) Destroy(fanWarnings[index]);
        fanWarnings[index] = null;
        if (index >= fanStakes.Count || fanStakes[index] == null) return;
        GameObject stake = fanStakes[index];
        fanStakes[index] = null;
        stake.transform.rotation = StakeRotation(fanDirections[index]);
        foreach (Collider2D collider in stake.GetComponentsInChildren<Collider2D>())
            collider.enabled = true;
        Rigidbody2D rb = stake.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.velocity = fanDirections[index] * StakeSpeed;
        }
        Destroy(stake, stakeLifetime);
    }

    void ClearFanStakes()
    {
        foreach (GameObject stake in fanStakes)
            if (stake != null) Destroy(stake);
        fanStakes.Clear();
        for (int i = 0; i < fanWarnings.Length; i++)
        {
            if (fanWarnings[i] != null) Destroy(fanWarnings[i]);
            fanWarnings[i] = null;
        }
    }

    void RestoreLeapPhysics()
    {
        if (!leapPhysicsChanged || enemyBase == null || enemyBase.rb == null) return;
        enemyBase.rb.bodyType = originalBodyType;
        leapPhysicsChanged = false;
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

        GameObject stake = Instantiate(stakePrefab, AttackAnchor.position, StakeRotation(lockedDirection));
        ConfigureSummonedAttack(stake);
        EnemyAttack attack = stake.GetComponent<EnemyAttack>();
        if (attack != null) attack.enemyBase = enemyBase;
        else Debug.LogError("杭PrefabにEnemyAttackがありません。", stake);

        Rigidbody2D bulletRb = stake.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.gravityScale = 0f;
            bulletRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            bulletRb.velocity = lockedDirection * StakeSpeed;
        }
        else Debug.LogError("杭PrefabにRigidbody2Dがありません。", stake);
        Destroy(stake, stakeLifetime); // 無限貫通は維持、寿命は有限。
    }

    void BeginDash()
    {
        ClearAttackWarning();
        enemyBase.isTargetLook(lastAttackTargetPosition);
        lockedDirection = AimAtLastTarget();
        enemyBase.animator.Play("近接攻撃", 0, 0f);
        if (enemyBase.AttackAudio != null)
            enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);

        if (meleeHitboxPrefab == null) return;
        Transform anchor = AttackAnchor;
        activeMeleeHitbox = Instantiate(meleeHitboxPrefab, anchor.position, anchor.rotation);
        activeMeleeHitbox.transform.SetParent(anchor, true);
        ConfigureSummonedAttack(activeMeleeHitbox);

        EnemyAttack attack = activeMeleeHitbox.GetComponent<EnemyAttack>();
        if (attack != null) attack.enemyBase = enemyBase;
        else Debug.LogError("近接PrefabにEnemyAttackがありません。", activeMeleeHitbox);
    }

    // 実際の攻撃と予告で同じ速度・時間・生成地点を使用する。
    float AttackWindupTime => Mathf.Max(0.01f, warningDuration);
    float StakeSpeed => Mathf.Max(0f, stakeSpeed);
    float DashSpeed => Mathf.Max(0f, enemyBase.SPEED * dashSpeedMultiplier);
    float FanShotInterval => Mathf.Max(0.02f, fanShotInterval);
    Transform AttackAnchor => enemyBase.AttackSpawnPoint != null
        ? enemyBase.AttackSpawnPoint.transform : enemyBase.transform;

    Quaternion StakeRotation(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(0f, 0f, angle + stakeRotationOffset);
    }

    float StakeTravelDistance(float remainingAttackTime)
    {
        if (stakePrefab == null) return 0f;
        Rigidbody2D body = stakePrefab.GetComponent<Rigidbody2D>();
        if (body == null || !body.simulated || body.bodyType == RigidbodyType2D.Static)
            return 0f;

        // velocityは射出時にstakeSpeedで設定される。現行PrefabのEnemyAttackは
        // DirectionAttack_enumのため、寿命5秒より先に攻撃終了で消える。
        float flightTime = Mathf.Max(0f, stakeLifetime);
        EnemyAttack attack = stakePrefab.GetComponent<EnemyAttack>();
        if (attack != null && attack.dellCondition == EnemyAttack.DellCondition.DirectionAttack_enum)
        {
            // FixedUpdateの実行順・端数によって終了が最大1ステップずれる。
            float step = Time.fixedDeltaTime;
            flightTime = Mathf.Min(flightTime, Mathf.Max(0f, remainingAttackTime) + step);
        }
        return StakeSpeed * flightTime;
    }

    void ShowAttackWarning()
    {
        ClearAttackWarning();
        Vector2 direction = attackKind == 0 ? lockedDirection : AimAtLastTarget();
        activeAttackWarning = CreateAttackWarning(
            attackKind == 0 ? stakeWarningPrefab : meleeWarningPrefab,
            AttackAnchor.position, direction, AttackWindupTime);
        UpdateAttackWarning();
    }

    GameObject CreateAttackWarning(GameObject warningPrefab, Vector3 origin,
        Vector2 direction, float duration)
    {
        Quaternion rotation = WarningRotation(direction);
        GameObject warning = null;
        try
        {
            warning = SpawnAttackWarning(EnemyContoroller.WarningRectangle, warningPrefab,
                origin, rotation, duration, null, Time.fixedDeltaTime * 2f, true);
        }
        catch (System.InvalidCastException exception)
        {
            Debug.LogError("攻撃警告Prefabの生成に失敗しました: " +
                (warningPrefab != null ? warningPrefab.name : "null") +
                "。共通の警告Prefabで再試行します。\n" + exception, enemyBase);
        }

        if (warning == null && warningPrefab != null)
            warning = SpawnAttackWarning(EnemyContoroller.WarningRectangle, null,
                origin, rotation, duration, null, Time.fixedDeltaTime * 2f, true);
        return warning;
    }

    void UpdateAttackWarning()
    {
        if (activeAttackWarning == null) return;
        if (attackKind == 0)
        {
            // 杭投げは従来どおり待機開始時の方向固定。
            FitSweptWarning(activeAttackWarning, stakePrefab, stakeHitboxes, AttackAnchor.position,
                StakeRotation(lockedDirection), lockedDirection, StakeTravelDistance(throwRecoveryTime));
        }
        else
        {
            // BeginDashと同じ照準・向きでAttackPointの位置も更新する。
            enemyBase.isTargetLook(lastAttackTargetPosition);
            Vector2 direction = AimAtLastTarget();
            Transform anchor = AttackAnchor;
            FitSweptWarning(activeAttackWarning, meleeHitboxPrefab, meleeHitboxes, anchor.position,
                anchor.rotation, direction, DashSpeed * Mathf.Max(0f, dashDuration));
        }
    }

    // 判定PrefabのBoxCollider2Dを進行方向に投影し、移動前から移動後までの
    // 包絡長方形を作る。Prefabのoffset、scale、子Transform、射出時の回転も反映。
    // 実体のCollider.boundsはPrefabアセットでは空になるため、ローカル形状を使う。
    static bool TryGetSweptRectangle(GameObject hitboxPrefab, BoxCollider2D[] hitboxes, Vector2 origin,
        Quaternion rotation, Vector2 direction, float travel, out Vector2 center, out Vector2 size)
    {
        center = origin;
        size = Vector2.zero;
        if (hitboxPrefab == null || hitboxes == null) return false;
        Transform root = hitboxPrefab.transform;
        Matrix4x4 spawnMatrix = Matrix4x4.TRS(Vector3.zero, rotation, root.localScale);
        Vector2 lateral = new Vector2(direction.y, -direction.x);
        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        bool found = false;

        foreach (BoxCollider2D box in hitboxes)
        {
            if (box == null || !box.enabled || !box.isTrigger) continue;
            Matrix4x4 colliderMatrix = spawnMatrix * root.worldToLocalMatrix * box.transform.localToWorldMatrix;
            Vector2 half = box.size * 0.5f + Vector2.one * box.edgeRadius;
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            {
                Vector2 point = colliderMatrix.MultiplyPoint3x4(box.offset + new Vector2(x * half.x, y * half.y));
                Vector2 projected = new Vector2(Vector2.Dot(point, lateral), Vector2.Dot(point, direction));
                min = Vector2.Min(min, projected);
                max = Vector2.Max(max, projected);
            }
            found = true;
        }
        if (!found) return false;

        max.y += Mathf.Max(0f, travel);
        Vector2 mid = (min + max) * 0.5f;
        center = origin + lateral * mid.x + direction * mid.y;
        size = max - min;
        return true;
    }

    // 警告PrefabのSprite実寸を基準にワールド単位へ変換する。
    // 共通DengerObjectの子Sprite (0.92 x 0.92 / 中央pivot) のアニメーションを維持。
    static void FitSweptWarning(GameObject warning, GameObject hitboxPrefab, BoxCollider2D[] hitboxes, Vector2 origin,
        Quaternion rotation, Vector2 direction, float travel)
    {
        if (warning == null) return;
        if (!TryGetSweptRectangle(hitboxPrefab, hitboxes, origin, rotation, direction, travel,
                out Vector2 center, out Vector2 size))
        {
            warning.transform.position = origin;
            warning.transform.rotation = WarningRotation(direction);
            return;
        }

        SpriteRenderer renderer = warning.GetComponentInChildren<SpriteRenderer>();
        if (renderer == null || renderer.sprite == null) return;
        Transform root = warning.transform;
        // rootの既存スケールに影響されない、子Spriteのローカル境界を取得。
        Matrix4x4 spriteMatrix = root.worldToLocalMatrix * renderer.transform.localToWorldMatrix;
        Bounds spriteBounds = renderer.sprite.bounds;
        Vector2 nativeMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 nativeMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        for (int x = -1; x <= 1; x += 2)
        for (int y = -1; y <= 1; y += 2)
        {
            Vector2 point = spriteMatrix.MultiplyPoint3x4(spriteBounds.center +
                new Vector3(x * spriteBounds.extents.x, y * spriteBounds.extents.y, 0f));
            nativeMin = Vector2.Min(nativeMin, point);
            nativeMax = Vector2.Max(nativeMax, point);
        }
        Vector2 nativeSize = nativeMax - nativeMin;
        if (nativeSize.x <= 0.0001f || nativeSize.y <= 0.0001f) return;
        Vector2 scale = new Vector2(Mathf.Max(0.01f, size.x) / nativeSize.x,
            Mathf.Max(0.01f, size.y) / nativeSize.y);
        Quaternion warningRotation = WarningRotation(direction);
        Vector2 nativeCenter = (nativeMin + nativeMax) * 0.5f;
        Vector3 centerOffset = warningRotation * (Vector3)Vector2.Scale(nativeCenter, scale);
        root.rotation = warningRotation;
        root.localScale = new Vector3(scale.x, scale.y, root.localScale.z);
        root.position = new Vector3(center.x, center.y, root.position.z) - centerOffset;
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
        if (activeMeleeHitbox != null)
        {
            activeMeleeHitbox.SetActive(false);
            Destroy(activeMeleeHitbox);
        }
        activeMeleeHitbox = null;
    }

    void FinishAttack()
    {
        ClearAttackWarning();
        ClearMeleeHitbox();
        ClearFanStakes();
        RestoreLeapPhysics();
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
        ClearFanStakes();
        RestoreLeapPhysics();
        A_Phase = 0;
        if (enemyBase == null) return;
        attackTarget = null;
        enemyBase.mode = EnemyBase.ModeType.M1;
        enemyBase.AttackCount = 0f;
        noAttackTimer = 0f;
        enemyBase.moveType = EnemyBase.MoveType.Move;
        // スタン時のノックバック速度は共通処理に任せる。
    }

    void OnDisable()
    {
        CancelAttack();
    }
}
