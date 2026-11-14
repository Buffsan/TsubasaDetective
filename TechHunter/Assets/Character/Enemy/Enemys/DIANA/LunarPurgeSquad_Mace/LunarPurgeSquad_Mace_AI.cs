using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// 月の巡礼者の鈍器使い。歩行はせず、予告地点への跳躍着地で範囲攻撃する。
// 敵VS敵 / 魅了の命中可否は既存 EnemyAttack と EnemyContoroller に委譲する。
public class LunarPurgeSquad_Mace_AI : EnemyMoveBase
{
    [Header("攻撃間隔（秒）")]
    [SerializeField] Vector2 attackInterval = new Vector2(3f, 5f);

    [Header("予告・跳躍（秒）")]
    [SerializeField, Min(0.1f)] float warningDuration = 2f;
    [SerializeField, Min(0.1f)] float leapDuration = 1f;
    [SerializeField, Min(0f)] float leapHeight = 4f;
    [SerializeField, Min(0f)] float landingRecovery = 0.3f;

    [Header("着地攻撃")]
    [SerializeField] Vector2 impactSize = new Vector2(5f, 5f);
    [SerializeField, Min(0f)] float attackPowerPercent = 100f;
    [SerializeField] GameObject warningPrefab;
    [SerializeField] GameObject roarEffectPrefab;
    [SerializeField, Min(0f)] float roarEffectLifetime = 3f;

    [Header("着地点の探索")]
    [SerializeField, Range(1, 64)] int searchRadiusCells = 18;
    [SerializeField, Min(0f)] float wallClearance = 0.25f;

    enum JumpPhase { Waiting, Telegraph, Airborne, Recovery }
    JumpPhase phase;
    EnemyContoroller controller;
    float intervalRemaining;
    float attackElapsed;
    float recoveryRemaining;
    Vector2 takeoffPoint;
    Vector2 landingPoint;
    Vector3 bodyRestLocalPosition;
    RigidbodyType2D originalBodyType;
    bool physicsOverridden;
    bool initialized;
    GameObject activeWarning;

    void Start()
    {
        controller = enemyBase as EnemyContoroller;
        if (controller == null || enemyBase.rb == null ||
            enemyBase.animator == null || enemyBase.EnemyAnimBody == null)
        {
            Debug.LogError("LunarPurgeSquad_Mace: EnemyContoroller / Rigidbody2D / Animator / EnemyAnimBody の設定が必要です。", this);
            enabled = false;
            return;
        }

        controller.Choice_MoveToBase = EnemyContoroller.MoveToBase.Special;
        enemyBase.LookPlayerDistance = -1f;
        enemyBase.moveType = EnemyBase.MoveType.Move;
        bodyRestLocalPosition = enemyBase.EnemyAnimBody.transform.localPosition;
        phase = JumpPhase.Waiting;
        ResetAttackInterval();
        initialized = true;
        enemyBase.animator.SetInteger("Anim", 0);
    }

    void Update()
    {
        if (initialized && enemyBase != null &&
            enemyBase.status != EnemyBase.Status.Nomal &&
            phase != JumpPhase.Waiting)
            CancelAttack();
    }

    public override void EnemyMovePlay()
    {
        if (!initialized) return;
        enemyBase.rb.velocity = Vector2.zero; // 歩行禁止
        enemyBase.animator.SetInteger("Anim", 0);

        if (CurrentTarget == null) return;
        intervalRemaining -= Time.fixedDeltaTime;
        if (intervalRemaining <= 0f)
            BeginAttack();
    }

    public override void EnemyFriendlyPlay()
    {
        // EnemyOnly で有効なターゲットがいない場合も歩かない。
        if (enemyBase != null && enemyBase.rb != null)
            enemyBase.rb.velocity = Vector2.zero;
    }

    public override void EnemyAttackPlay()
    {
        if (!initialized) return;
        float dt = Time.fixedDeltaTime;
        enemyBase.rb.velocity = Vector2.zero;

        switch (phase)
        {
            case JumpPhase.Telegraph:
                attackElapsed += dt;
                enemyBase.AttackCount = attackElapsed;
                if (attackElapsed >= Mathf.Max(0f, warningDuration - leapDuration))
                {
                    BeginLeap();
                    TickLeap();
                }
                break;
            case JumpPhase.Airborne:
                attackElapsed += dt;
                enemyBase.AttackCount = attackElapsed;
                TickLeap();
                break;
            case JumpPhase.Recovery:
                recoveryRemaining -= dt;
                if (recoveryRemaining <= 0f)
                    FinishAttack();
                break;
        }
    }

    public override void EnemyStanPlay() { CancelAttack(); }
    public override void EnemyConfusionPlay() { CancelAttack(); }

    void BeginAttack()
    {
        Transform target = CurrentTarget;
        if (target == null || !TryChooseLanding(target.position, out landingPoint))
        {
            // 安全な床が存在しないなら空中・壁へのジャンプをしない。
            ResetAttackInterval();
            return;
        }

        takeoffPoint = enemyBase.rb.position;
        enemyBase.isTargetLook(landingPoint);
        attackElapsed = 0f;
        enemyBase.AttackCount = 0f;
        enemyBase.moveType = EnemyBase.MoveType.Attack;
        enemyBase.animator.SetInteger("Anim", 2);
        phase = JumpPhase.Telegraph;

        // 2秒の警告を固定着地点に表示。残り1秒のジャンプ中も表示し続ける。
        Vector3 scale = new Vector3(Mathf.Max(0.1f, impactSize.x),
            Mathf.Max(0.1f, impactSize.y), 1f);
        activeWarning = SpawnAttackWarning(
            EnemyContoroller.WarningRectangle, warningPrefab,
            landingPoint, Quaternion.identity, warningDuration,
            null, 0f, true, scale);
        if (activeWarning == null)
            Debug.LogWarning("LunarPurgeSquad_Mace: 攻撃警告Prefabが設定されていません。", this);
    }

    void BeginLeap()
    {
        if (phase != JumpPhase.Telegraph) return;
        phase = JumpPhase.Airborne;
        originalBodyType = enemyBase.rb.bodyType;
        enemyBase.rb.velocity = Vector2.zero;
        enemyBase.rb.bodyType = RigidbodyType2D.Kinematic;
        physicsOverridden = true;
        enemyBase.animator.SetInteger("Anim", 3);
    }

    void TickLeap()
    {
        float duration = Mathf.Max(0.1f, leapDuration);
        float t = Mathf.Clamp01((attackElapsed -
            Mathf.Max(0f, warningDuration - leapDuration)) / duration);

        // 頂点ではゆっくり横移動し、最後の約3割で着地点まで急加速。
        float horizontal = t < 0.7f
            ? 0.12f * Mathf.SmoothStep(0f, 1f, t / 0.7f)
            : 0.12f + 0.88f * Mathf.Pow((t - 0.7f) / 0.3f, 3f);
        enemyBase.rb.position = Vector2.LerpUnclamped(takeoffPoint, landingPoint, horizontal);

        // 早めに飛び上がって滞空、落下の終盤ほど一気に加速する。
        float altitude;
        if (t < 0.55f)
            altitude = 1f - Mathf.Pow(1f - t / 0.55f, 3f);
        else if (t < 0.74f)
            altitude = 1f;
        else
            altitude = 1f - Mathf.Pow((t - 0.74f) / 0.26f, 3f);

        Vector3 visual = bodyRestLocalPosition;
        visual.y += Mathf.Max(0f, leapHeight) * Mathf.Max(0f, altitude);
        enemyBase.EnemyAnimBody.transform.localPosition = visual;

        if (t >= 1f)
            Land();
    }

    void Land()
    {
        enemyBase.rb.position = landingPoint;
        RestorePhysics(false);
        ClearWarning();
        enemyBase.animator.SetInteger("Anim", 4);

        if (enemyBase.AttackAudio != null && enemyBase.audioManager != null)
            enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);

        // クジラの Effects[0] と同じPrefabを視覚演出としてのみ使用。
        if (roarEffectPrefab != null)
        {
            GameObject effect = Instantiate(roarEffectPrefab, landingPoint, Quaternion.identity);
            foreach (EnemyAttack attack in effect.GetComponentsInChildren<EnemyAttack>(true))
                attack.enabled = false;
            Destroy(effect, Mathf.Max(0.1f, roarEffectLifetime));
        }

        DealLandingDamage();
        phase = JumpPhase.Recovery;
        recoveryRemaining = landingRecovery;
        if (landingRecovery <= 0f)
            FinishAttack();
    }

    void DealLandingDamage()
    {
        // 物理Triggerによる複数フレームの重複命中を避け、着地時に一度だけ判定。
        GameObject context = new GameObject("LunarPurgeSquad_Mace_Impact");
        context.transform.position = landingPoint;
        EnemyAttack attack = context.AddComponent<EnemyAttack>();
        attack.ConfigureAttack(enemyBase, attackPowerPercent);
        controller.ConfigureAttackTarget(attack);

        HashSet<EnemyBase> alreadyHit = new HashSet<EnemyBase>();
        bool playerHit = false;
        Collider2D[] hits = Physics2D.OverlapBoxAll(landingPoint,
            new Vector2(Mathf.Max(0.1f, impactSize.x), Mathf.Max(0.1f, impactSize.y)), 0f);

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;
            if (hit.gameObject.tag == "PlayerArea")
            {
                if (playerHit) continue;
                playerHit = true;
                attack.CheckHitNow(hit);
            }
            else if (hit.gameObject.tag == "EnemyArea")
            {
                charaDamage hurtbox = hit.GetComponent<charaDamage>();
                if (hurtbox != null && hurtbox.OwnerEnemyBase != null &&
                    alreadyHit.Add(hurtbox.OwnerEnemyBase))
                    attack.CheckHitNow(hit);
            }
        }
        Destroy(context);
    }

    bool TryChooseLanding(Vector3 targetPosition, out Vector2 result)
    {
        result = Vector2.zero;
        List<Tilemap> stageFloors = new List<Tilemap>();
        List<Tilemap> fallbackFloors = new List<Tilemap>();

        // 正式な床タグは Stage_Floor。StandFloor はシステム上の参照名であり、
        // タグ名としては使用しない。
        Tilemap registeredFloor = null;
        if (ALL_SystemManager.Instance != null &&
            ALL_SystemManager.Instance.enemySpawnBASE != null &&
            ALL_SystemManager.Instance.enemySpawnBASE.Seve_TileMap_C != null)
            registeredFloor = ALL_SystemManager.Instance.enemySpawnBASE.Seve_TileMap_C.StandFloor;

        // 登録済みの床も含めて、実際のタグを基準に分類する。
        if (registeredFloor != null && registeredFloor.gameObject.activeInHierarchy)
        {
            if (registeredFloor.CompareTag("Stage_Floor"))
                stageFloors.Add(registeredFloor);
            else if (registeredFloor.name == "Floor")
                fallbackFloors.Add(registeredFloor);
        }

        foreach (Tilemap map in FindObjectsOfType<Tilemap>())
        {
            if (map == null || !map.gameObject.activeInHierarchy) continue;

            if (map.CompareTag("Stage_Floor"))
            {
                if (!stageFloors.Contains(map)) stageFloors.Add(map);
            }
            else if (map.name == "Floor")
            {
                if (!fallbackFloors.Contains(map)) fallbackFloors.Add(map);
            }
        }

        // どちらのターゲット種別でも同じ探索を使い、
        // Stage_Floor 内で標的に最も近い安全なセルへ着地する。
        // 対象セルが存在しない場合だけ、名前が Floor のタイルマップを使用。
        return FindClosestValidCell(stageFloors, targetPosition, out result) ||
               FindClosestValidCell(fallbackFloors, targetPosition, out result);
    }

    bool FindClosestValidCell(List<Tilemap> maps, Vector3 target, out Vector2 result)
    {
        result = Vector2.zero;
        bool found = false;
        float nearest = float.PositiveInfinity;
        int radius = Mathf.Clamp(searchRadiusCells, 1, 64);

        foreach (Tilemap map in maps)
        {
            if (map == null) continue;
            Vector3Int center = map.WorldToCell(target);
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    Vector3Int cell = center + new Vector3Int(x, y, 0);
                    if (!map.HasTile(cell)) continue;
                    Vector2 candidate = map.GetCellCenterWorld(cell);
                    float sqr = ((Vector2)target - candidate).sqrMagnitude;
                    if (sqr >= nearest || IsWallAt(candidate)) continue;
                    found = true;
                    nearest = sqr;
                    result = candidate;
                }
            }
        }
        return found;
    }

    bool IsWallAt(Vector2 point)
    {
        if (wallClearance <= 0f) return false;
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(point, wallClearance))
            if (hit != null && hit.gameObject.tag == "Stage_Wall")
                return true;
        return false;
    }

    void FinishAttack()
    {
        phase = JumpPhase.Waiting;
        enemyBase.AttackCount = 0f;
        enemyBase.moveType = EnemyBase.MoveType.Move;
        enemyBase.animator.SetInteger("Anim", 0);
        ResetAttackInterval();
    }

    void ResetAttackInterval()
    {
        intervalRemaining = Random.Range(
            Mathf.Max(0.1f, Mathf.Min(attackInterval.x, attackInterval.y)),
            Mathf.Max(0.1f, Mathf.Max(attackInterval.x, attackInterval.y)));
    }

    void RestorePhysics(bool interrupted)
    {
        if (enemyBase == null) return;
        enemyBase.EnemyAnimBody.transform.localPosition = bodyRestLocalPosition;
        if (!physicsOverridden || enemyBase.rb == null) return;
        enemyBase.rb.position = interrupted ? takeoffPoint : landingPoint;
        enemyBase.rb.bodyType = originalBodyType;
        enemyBase.rb.velocity = Vector2.zero;
        physicsOverridden = false;
    }

    void ClearWarning()
    {
        if (activeWarning != null) Destroy(activeWarning);
        activeWarning = null;
    }

    void CancelAttack()
    {
        if (phase == JumpPhase.Waiting) return;
        RestorePhysics(true);
        ClearWarning();
        phase = JumpPhase.Waiting;
        enemyBase.AttackCount = 0f;
        if (enemyBase.moveType == EnemyBase.MoveType.Attack)
            enemyBase.moveType = EnemyBase.MoveType.Move;
        if (enemyBase.status != EnemyBase.Status.Die)
            enemyBase.animator.SetInteger("Anim", 0);
        ResetAttackInterval();
    }

    void OnDisable()
    {
        if (initialized && phase != JumpPhase.Waiting) CancelAttack();
    }
}
