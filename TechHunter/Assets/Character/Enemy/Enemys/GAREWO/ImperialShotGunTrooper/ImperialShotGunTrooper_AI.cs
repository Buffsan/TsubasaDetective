using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

// 帝国散弾銃士。既存の敵派閥・攻撃判定・弾数UIに接続する専用状態機械。
public sealed class ImperialShotGunTrooper_AI : EnemyMoveBase
{
    [Header("散弾 / 装填")]
    [SerializeField] GameObject pelletPrefab;
    [SerializeField] Transform muzzle;
    [SerializeField] TMP_Text ammoText;
    [SerializeField] AudioClip shotSE;
    [SerializeField, Min(1)] int magazineSize = 2;
    [SerializeField, Min(1)] int pelletCount = 20;
    [SerializeField, Range(0, 90)] float spreadHalfAngle = 30f;
    [SerializeField, Min(0.1f)] float pelletSpeed = 30f;
    [SerializeField, Min(0.1f)] float pelletRange = 8f;
    [SerializeField, Min(0)] float pelletPowerPercent = 100f;
    [SerializeField, Min(0.02f)] float secondsPerShell = 2f;
    [Tooltip("1発ごとの装填時間のうち、待機アニメーションを再生する割合。残りはリロード。")]
    [SerializeField, Range(0f, 1f)] float reloadIdleRatio = 0.8f;
    [SerializeField, Min(0)] float shotCooldown = 1f;
    [SerializeField, Min(0)] float fireAnimationSeconds = 0.25f;
    [SerializeField, Min(0.02f)] float windupSeconds = 0.5f;
    [SerializeField] GameObject attackWarningPrefab;
    [SerializeField, Range(0, 1)] float warningOpacity = 0.26f;
    [SerializeField, Min(0.01f)] float warningWidth = 0.08f;

    [Header("立ち回り")]
    [SerializeField, Min(0.1f)] float preferredRange = 3.5f;
    [SerializeField, Min(0)] float unAimedSeconds = 1.5f;
    [SerializeField, Range(1, 90)] float aimHalfAngle = 12f;
    [SerializeField, Range(0, 1)] float offAimAttackChance = 0.2f;
    [SerializeField, Range(0, 1)] float orbitChance = 0.5f;
    [SerializeField, Min(1)] float rushSpeedMultiplier = 2f;
    [SerializeField, Min(1)] float retreatSpeedMultiplier = 1.5f;
    [SerializeField, Min(0.1f)] float orbitSeconds = 1.2f;
    [SerializeField, Min(0.1f)] float secondMoveTimeout = 3f;
    [SerializeField, Min(0)] float retreatDistanceThreshold = 0.35f;

    [Header("床タイル経路探索")]
    [Tooltip("未設定の場合は現在のステージの Stage_Floor を自動検索。")]
    [SerializeField] Tilemap floorMap;
    [Tooltip("任意。移動可能な床と重複する表示用 Tilemap は設定しない。")]
    [SerializeField] Tilemap blockedTiles;
    [SerializeField, Min(1)] int searchRadiusCells = 64;
    [SerializeField, Min(100)] int maxPathSearchNodes = 2000;
    [SerializeField, Min(1)] int retreatNodesPerTick = 180;
    [SerializeField, Min(0.05f)] float repathSeconds = 0.45f;
    [SerializeField, Min(0.02f)] float arrivalDistance = 0.1f;
    [SerializeField, Min(0)] float aimExposurePenalty = 5f;
    [Header("曲線移動")]
    [SerializeField, Min(0)] float cornerRadius = 0.45f;
    [SerializeField, Min(1)] float movementAcceleration = 24f;
    [SerializeField, Min(0.005f)] float movementClearance = 0.12f;
    [SerializeField, Min(0.05f)] float movementLookAhead = 0.55f;
    [SerializeField, Min(0.05f)] float coverReloadDelay = 0.25f;
    [SerializeField, Min(1)] int retreatMaxRetries = 3;


    [Header("実行時のナビゲーション診断（確認用）")]
    [SerializeField] Tilemap runtimeFloor;
    [SerializeField] TileMap_Controller runtimeStage;
    [SerializeField] string navigationStatus;
    [SerializeField] string runtimeState;
    [SerializeField] bool lowerAllies;
    [SerializeField] int lowerAllyCount;
    [SerializeField] Vector2 runtimeRetreatDestination;
    [SerializeField] string reloadReason;
    Tilemap lastFloorOverride, lastBlockedOverride;
    TileMap_Controller lastSpawnStage;
    float floorRetryTimer;


    enum State { Hunt, Windup, Cooldown, SecondMove, Retreat, Reload }
    State state;
    EnemyContoroller controller;
    ImperialShotGunTrooper_Navigation navigation;
    Transform lockedTarget, huntTarget;
    Vector2 aimOrigin, aimDirection, lastGoal, aimAtShot, firstShotPosition, retreatProgress, retreatThreat;
    Vector2 lastKnownThreat;
    bool knowsThreat;
    float coverHiddenTime, repositionTime;
    int movementFacing = 1;
    Vector2[] directions;
    readonly List<SpriteRenderer> warnings = new List<SpriteRenderer>();
    int routeIndex, lastAmmo = -1, side = 1, retreatRetries;
    float elapsed, reloadElapsed, unaimed, opportunityTimer, allyTimer, repathTimer, retreatStuck;
    bool rush, hasAim, routeGoalSet;
    bool routeAvoidAim;
    public int CurrentAmmo { get; private set; }
    public int MaximumAmmo => Mathf.Max(1, magazineSize);
    public bool IsReloading => state == State.Reload;
    Vector2 Position => enemyBase.rb.position;
    Vector2 Muzzle => muzzle != null ? (Vector2)muzzle.position :
        enemyBase.AttackSpawnPoint != null ? (Vector2)enemyBase.AttackSpawnPoint.transform.position : Position;
    float Dt => Time.fixedDeltaTime;
    float AimCos => Mathf.Cos(aimHalfAngle * Mathf.Deg2Rad);

    void Awake()
    {
        CurrentAmmo = MaximumAmmo;
        if (enemyBase == null) enemyBase = GetComponent<EnemyContoroller>();
    }

    void Start()
    {
        controller = enemyBase as EnemyContoroller;
        if (controller == null || pelletPrefab == null ||
            pelletPrefab.GetComponent<ImperialShotGunTrooper_Pellet>() == null)
        {
            Debug.LogError("帝国散弾銃士: Controllerまたは散弾Prefabが未設定です。", this);
            enabled = false;
            return;
        }
        controller.Choice_MoveToBase = EnemyContoroller.MoveToBase.Special;
        enemyBase.LookPlayerDistance = -1f;
        navigation = new ImperialShotGunTrooper_Navigation(enemyBase.transform);
        navigation.ConfigureSmoothing(cornerRadius, movementClearance);
        // 物理挙動の間も見た目を滑らかにする。停止必須の射撃・装填は従来通り即停止。
        enemyBase.rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        runtimeFloor = null;
        runtimeStage = null;
        runtimeState = state.ToString();
        ResolveFloor();
    }

    static bool UsableFloor(Tilemap map)
    {
        return map != null && map.isActiveAndEnabled && map.gameObject.scene.IsValid() &&
            map.gameObject.scene.isLoaded && map.CompareTag("Stage_Floor") && map.GetUsedTilesCount() > 0;
    }

    // 現在位置または隣接セルに床があるタイルマップを優先。単体テストにも対応。
    Tilemap FindNearestFloor(Tilemap[] maps)
    {
        Tilemap best = null;
        Tilemap onlyMap = null;
        int validMaps = 0;
        float bestDistance = float.PositiveInfinity;
        foreach (Tilemap map in maps)
        {
            if (!UsableFloor(map)) continue;
            onlyMap = map;
            validMaps++;
            Vector3 world = new Vector3(Position.x, Position.y, map.transform.position.z);
            Vector3Int origin = map.WorldToCell(world);
            for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int cell = origin + new Vector3Int(x, y, 0);
                if (!map.HasTile(cell)) continue;
                float distance = ((Vector2)map.GetCellCenterWorld(cell) - Position).sqrMagnitude;
                if (distance < bestDistance) { best = map; bestDistance = distance; }
            }
        }
        return best != null ? best : validMaps == 1 ? onlyMap : null;
    }

    // Stage context is injected by EnemySpawnBase first; older summon methods and
    // enemies placed manually in a scene still have the previous safe fallbacks.
    TileMap_Controller ResolveStage()
    {
        if (controller.SpawnStage != null) return controller.SpawnStage;
        var stage = GetComponentInParent<TileMap_Controller>();
        if (stage == null && ALL_SystemManager.Instance != null &&
            ALL_SystemManager.Instance.enemySpawnBASE != null)
            stage = ALL_SystemManager.Instance.enemySpawnBASE.Seve_TileMap_C;
        return stage;
    }

    void ResolveFloor()
    {
        bool blockedChanged = lastBlockedOverride != blockedTiles;
        lastFloorOverride = floorMap;
        lastBlockedOverride = blockedTiles;
        lastSpawnStage = controller.SpawnStage;
        floorRetryTimer = 1f;
        runtimeStage = ResolveStage();
        Tilemap selected = UsableFloor(floorMap) ? floorMap : null;
        if (selected == null && runtimeStage != null)
        {
            // FloorMap は現行ステージによって EnemySpawnArea を参照する。
            // 召喚元ステージの StandFloor を最優先し、タグで本物の床だけを選ぶ。
            if (UsableFloor(runtimeStage.StandFloor)) selected = runtimeStage.StandFloor;
            else if (UsableFloor(runtimeStage.FloorMap)) selected = runtimeStage.FloorMap;
            else selected = FindNearestFloor(runtimeStage.GetComponentsInChildren<Tilemap>(false));
        }
        // ステージ管理なしのテストシーンでのみシーン全体を検索。
        // 召喚元が明確な場合は別ステージの床を誤取得しない。
        if (selected == null && runtimeStage == null)
            selected = FindNearestFloor(FindObjectsOfType<Tilemap>());
        if (runtimeFloor != selected || blockedChanged)
        {
            runtimeFloor = selected;
            repathTimer = 0f;
            routeGoalSet = false;
            routeIndex = 0;
            navigation.SetMaps(runtimeFloor, blockedTiles);
            if (state == State.Retreat) StartRetreatSearch();
        }
        else navigation.SetMaps(runtimeFloor, blockedTiles);
        navigationStatus = runtimeFloor != null ? "Floor: " + runtimeFloor.name :
            runtimeStage != null ? "Stage_Floor not found in " + runtimeStage.name : "Stage_Floor not found";
    }

    void LateUpdate()
    {
        if (ammoText != null && lastAmmo != CurrentAmmo)
        {
            ammoText.text = CurrentAmmo + "/" + MaximumAmmo;
            lastAmmo = CurrentAmmo;
        }
        // 死亡などFixedUpdateのAIが呼ばれない場合にも警告を消す。
        if (enemyBase != null && enemyBase.status != EnemyBase.Status.Nomal && state != State.Hunt)
            Cancel();
    }

    public override void EnemyMovePlay() => Tick();
    public override void EnemyAttackPlay() => Tick();
    public override void EnemyStanPlay() => Cancel();
    public override void EnemyConfusionPlay() => Cancel();
    public override void EnemyFriendlyEnter() => Cancel();
    public override void EnemyFriendlyExit() { unaimed = 0f; repathTimer = 0f; }
    public override void EnemyFriendlyPlay()
    {
        if (navigation == null) return;
        if (CurrentAmmo < MaximumAmmo)
        {
            if (state != State.Reload) BeginReload("No hostile target");
            TickReload();
            // 友好待機中の装填は FriendlyActive で継続させる。
            enemyBase.moveType = EnemyBase.MoveType.Move;
        }
        else Stop(0);
    }

    void OnDisable() { ClearWarnings(); if (navigation != null) Cancel(); }

    void SetState(State next)
    {
        state = next;
        runtimeState = next.ToString();
        elapsed = 0f;
        repathTimer = 0f;
        routeGoalSet = false;
        routeAvoidAim = false;
        routeIndex = 0;
        navigation.Clear();
        enemyBase.moveType = next == State.Hunt || next == State.Retreat || next == State.SecondMove
            ? EnemyBase.MoveType.Move : EnemyBase.MoveType.Attack;
    }

    void Cancel()
    {
        if (state == State.Hunt && lockedTarget == null && huntTarget == null && warnings.Count == 0) return;
        ClearWarnings();
        lockedTarget = null;
        huntTarget = null;
        unaimed = 0f;
        reloadElapsed = 0f;
        if (navigation != null && enemyBase != null) SetState(State.Hunt);
    }

    bool Valid(Transform target)
    {
        if (target == null || !controller.CanTarget(target)) return false;
        var player = PlayerController.Instance;
        return player == null || target != player.transform || player.HP > 0f;
    }
    void Anim(int value) { if (enemyBase.animator != null) enemyBase.animator.SetInteger("Anim", value); }
    void Stop(int anim) { enemyBase.rb.velocity = Vector2.zero; Anim(anim); }

    bool SameTeam(EnemyBase other)
    {
        Faction a = enemyBase.Factions, b = other.Factions;
        if (enemyBase.IsCharmed || other.IsCharmed)
            return enemyBase.IsCharmed && other.IsCharmed;
        Faction debug = Faction.DebugA | Faction.DebugB;
        if (((a | b) & debug) != 0) return (a & b & debug) != 0;
        // 通常戦闘では国が違ってもプレイヤーと敵対している敵は同じ戦闘側。
        // 敵対モードで実際に攻撃対象になっている個体は味方から除外する。
        if (controller.TargetMode != EnemyContoroller.TargetType.PlayerOnly &&
            controller.CanTarget(other.transform)) return false;
        if (controller.TargetMode != EnemyContoroller.TargetType.EnemyOnly)
        {
            var peer = other as EnemyContoroller;
            if (peer != null && peer.TargetMode == EnemyContoroller.TargetType.EnemyOnly) return false;
            return true;
        }
        return ((int)a & (int)b & 1023) != 0;
    }

    bool IsLowerAlly(EnemyBase other)
    {
        return other != null && other != enemyBase && other.isActiveAndEnabled &&
            other.HP > 0f && other.status != EnemyBase.Status.Die &&
            other.charadata != null && enemyBase.charadata != null && SameTeam(other) &&
            (int)other.charadata.enemyStrength > 0 &&
            (int)other.charadata.enemyStrength < (int)enemyBase.charadata.enemyStrength;
    }

    bool HasLowerAllies()
    {
        var manager = SystemManager.Instance;
        if (manager == null && ALL_SystemManager.Instance != null)
            manager = ALL_SystemManager.Instance.systemManager;
        lowerAllyCount = 0;
        if (manager != null)
        {
            if (manager.AllEnemy != null)
                foreach (GameObject obj in manager.AllEnemy)
                    if (obj != null && IsLowerAlly(obj.GetComponent<EnemyBase>())) lowerAllyCount++;
            foreach (EnemyBase ally in manager.CharmedAllies)
                if (IsLowerAlly(ally)) lowerAllyCount++;
        }
        if (manager == null || (manager.AllEnemy != null && manager.AllEnemy.Count == 0 &&
            manager.CharmedAllies.Count == 0 && runtimeStage == null))
        {
            // SystemManager のない／登録なしの単体テストシーンでも階級判定を可能にする。
            foreach (EnemyBase other in FindObjectsOfType<EnemyBase>())
                if (IsLowerAlly(other)) lowerAllyCount++;
        }
        lowerAllies = lowerAllyCount > 0;
        return lowerAllies;
    }

    void ReadAim(Transform target)
    {
        hasAim = false;
        aimDirection = Vector2.zero;
        aimOrigin = target != null ? (Vector2)target.position : Position;
        var player = PlayerController.Instance;
        if (player == null || target != player.transform || player.cursorController == null) return;
        Vector2 delta = (Vector2)player.cursorController.MousePos - aimOrigin;
        if (delta.sqrMagnitude < 0.001f) return;
        aimDirection = delta.normalized;
        hasAim = true;
    }

    bool AimedAt(Vector2 point, float cos)
    {
        Vector2 offset = point - aimOrigin;
        return hasAim && offset.sqrMagnitude > 0.001f && Vector2.Dot(aimDirection, offset.normalized) >= cos;
    }

    bool Visible(Transform target)
    {
        if (!Valid(target)) return false;
        Vector2 origin = state == State.Windup ? Muzzle : Position;
        Vector2 muzzleOffset = origin - Position;
        if (muzzleOffset.sqrMagnitude > 0.0001f &&
            ImperialShotGunTrooper_Pellet.WallDistance(Position, muzzleOffset.normalized,
                muzzleOffset.magnitude, ImperialShotGunTrooper_Pellet.RadiusOf(pelletPrefab)) <
                muzzleOffset.magnitude - 0.001f) return false;
        Vector2 delta = (Vector2)target.position - origin;
        float distance = delta.magnitude;
        return distance < 0.01f || ImperialShotGunTrooper_Pellet.WallDistance(origin,
            delta.normalized, distance, ImperialShotGunTrooper_Pellet.RadiusOf(pelletPrefab)) >= distance - 0.001f;
    }

    void Tick()
    {
        if (navigation == null || !enabled || enemyBase.status != EnemyBase.Status.Nomal) return;
        floorRetryTimer -= Dt;
        if (lastFloorOverride != floorMap || lastBlockedOverride != blockedTiles ||
            lastSpawnStage != controller.SpawnStage ||
            ((runtimeFloor == null || !runtimeFloor.isActiveAndEnabled) && floorRetryTimer <= 0f))
            ResolveFloor();
        // シーン直置きテストでプレイヤーが敵の Awake より後から生成された場合。
        if (enemyBase.Player == null && PlayerController.Instance != null)
            enemyBase.Player = PlayerController.Instance.gameObject;
        allyTimer -= Dt;
        if (allyTimer <= 0f) { HasLowerAllies(); allyTimer = 0.25f; }
        Transform target = CurrentTarget;
        if (Valid(target)) { lastKnownThreat = target.position; knowsThreat = true; }
        ReadAim(state == State.Hunt || state == State.Retreat ? target : lockedTarget);
        elapsed += Dt;
        repositionTime = Mathf.Max(0f, repositionTime - Dt);
        switch (state)
        {
            case State.Hunt: TickHunt(target); break;
            case State.Windup: TickWindup(); break;
            case State.Cooldown: TickCooldown(); break;
            case State.SecondMove: TickSecondMove(); break;
            case State.Retreat: TickRetreat(target); break;
            case State.Reload: TickReload(); break;
        }
    }

    void TickHunt(Transform target)
    {
        if (CurrentAmmo <= 0) { RecoverEmptyMagazine(target); return; }
        if (!Valid(target))
        {
            navigationStatus = "No valid target";
            if (CurrentAmmo < MaximumAmmo) BeginReload("No hostile target"); else Stop(0);
            return;
        }
        if (huntTarget != target)
        {
            huntTarget = target; unaimed = 0f; opportunityTimer = 0f; repathTimer = 0f;
        }
        Vector2 toTarget = (Vector2)target.position - Position;
        float distance = toTarget.magnitude;
        bool aimed = AimedAt(Position, AimCos);
        unaimed = aimed ? 0f : unaimed + Dt;
        bool visible = Visible(target);
        if (CurrentAmmo < MaximumAmmo && CoverConfirmed(visible))
        { BeginReload("Target behind cover (one shell left)"); return; }

        bool aggressive = !lowerAllies || unaimed >= unAimedSeconds;
        opportunityTimer -= Dt;
        if (!aggressive && opportunityTimer <= 0f)
        {
            opportunityTimer = 0.5f;
            aggressive = hasAim && !AimedAt(Position, Mathf.Cos(2f * aimHalfAngle * Mathf.Deg2Rad)) &&
                Random.value < offAimAttackChance;
        }
        if (visible && aggressive && repositionTime <= 0f && distance <= Mathf.Min(preferredRange, pelletRange))
        { BeginWindup(target); return; }
        if (visible && repositionTime > 0f && distance <= preferredRange)
        {
            // 銃口だけが壁に引っ掛かって攻撃待機が連続する場合にも位置を変える。
            Navigate(FlankGoal(target, lowerAllies), enemyBase.SPEED, lowerAllies, (Vector2)target.position);
            return;
        }
        if (visible && lowerAllies && (aimed || (!aggressive && distance <= preferredRange)))
        {
            // 迂回目標が壁・床の外にあって到達不能なら、照準回避コスト付きの追跡に戻す。
            Navigate(FlankGoal(target, true), enemyBase.SPEED, true, (Vector2)target.position);
            return;
        }
        // 射撃できないときは壁の向こうの標的へ床セルをたどって追跡。
        Navigate(target.position, enemyBase.SPEED, lowerAllies);
    }

    Vector2 FlankGoal(Transform target, bool avoidAim)
    {
        Vector2 center = target.position;
        Vector2 radial = Position - center;
        if (radial.sqrMagnitude < 0.01f) radial = Vector2.right;
        radial = radial.normalized * Mathf.Max(0.5f, preferredRange);
        Vector2 left = center + (Vector2)(Quaternion.Euler(0, 0, 55f * side) * radial);
        Vector2 right = center + (Vector2)(Quaternion.Euler(0, 0, -55f * side) * radial);
        bool leftGood = navigation.IsWalkablePosition(left) && (!avoidAim || !AimedAt(left, AimCos));
        bool rightGood = navigation.IsWalkablePosition(right) && (!avoidAim || !AimedAt(right, AimCos));
        if (leftGood) return left;
        if (rightGood) return right;
        Vector2 farSide = center + (Vector2)(Quaternion.Euler(0, 0, 90f * side) * radial);
        return navigation.IsWalkablePosition(farSide) ? farSide : center;
    }

    void BeginWindup(Transform target)
    {
        if (CurrentAmmo <= 0) { RecoverEmptyMagazine(target); return; }
        if (!Valid(target)) { SetState(State.Hunt); return; }
        lockedTarget = target;
        coverHiddenTime = 0f;
        SetState(State.Windup);
        Stop(2);
        enemyBase.isTargetLook(target.position);
        aimAtShot = (Vector2)target.position - Muzzle;
        if (aimAtShot.sqrMagnitude < 0.0001f) aimAtShot = Vector2.right;
        aimAtShot.Normalize();
        int count = Mathf.Max(1, pelletCount);
        directions = new Vector2[count];
        float angle = Mathf.Atan2(aimAtShot.y, aimAtShot.x) * Mathf.Rad2Deg;
        for (int i = 0; i < count; i++)
        {
            // 各区画に1本ずつ分散。予兆で決めた方向を最後まで変更しない。
            float t = (i + Random.Range(0.1f, 0.9f)) / count;
            float rad = (angle + Mathf.Lerp(-spreadHalfAngle, spreadHalfAngle, t)) * Mathf.Deg2Rad;
            directions[i] = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
        CreateWarnings();
        UpdateWarnings();
    }

    void TickWindup()
    {
        Stop(2);
        if (!Valid(lockedTarget) || !Visible(lockedTarget))
        {
            ClearWarnings();
            if (CurrentAmmo <= 0) RecoverEmptyMagazine(lockedTarget);
            else
            {
                // 射撃は中止するが、一瞬の遮蔽だけで装填を始めない。追跡側で再判定。
                lockedTarget = null;
                unaimed = 0f;
                repositionTime = 0.35f;
                SetState(State.Hunt);
            }
            return;
        }
        UpdateWarnings();
        if (elapsed + 0.0001f < windupSeconds) return;
        Fire();
    }

    void Fire()
    {
        ClearWarnings();
        Vector2 origin = Muzzle;
        foreach (Vector2 direction in directions)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            GameObject shot = Instantiate(pelletPrefab, origin, Quaternion.Euler(0, 0, angle));
            EnemyAttack attack = shot.GetComponent<EnemyAttack>();
            attack.ConfigureAttack(enemyBase, pelletPowerPercent);
            ConfigureSummonedAttack(shot);
            shot.GetComponent<ImperialShotGunTrooper_Pellet>().Launch(direction, pelletSpeed, pelletRange);
        }
        CurrentAmmo--;
        firstShotPosition = lockedTarget.position;
        lastKnownThreat = firstShotPosition;
        knowsThreat = true;
        coverHiddenTime = 0f;
        if (shotSE != null && enemyBase.audioManager != null) enemyBase.audioManager.PlaySE(shotSE);
        SetState(State.Cooldown);
        Stop(3);
        if (enemyBase.animator != null) enemyBase.animator.Play("攻撃", 0, 0f);
    }

    void TickCooldown()
    {
        Stop(elapsed < fireAnimationSeconds ? 3 : 4);
        bool valid = Valid(lockedTarget);
        bool visible = CurrentAmmo > 0 && valid && Visible(lockedTarget);
        bool behindCover = CurrentAmmo > 0 && CoverConfirmed(visible);
        if (elapsed + 0.0001f < shotCooldown) return;
        // 弾切れ時は遮蔽の有無に関係なく退避を優先（格下がいなければその場で装填）。
        if (CurrentAmmo <= 0) { RecoverEmptyMagazine(lockedTarget); return; }
        if (!valid) { SetState(State.Hunt); lockedTarget = null; return; }
        if (behindCover) { BeginReload("Target behind cover (one shell left)"); return; }
        if (!visible)
        {
            rush = true;
            SetState(State.SecondMove);
            return;
        }
        Vector2 targetPosition = lockedTarget.position;
        float distance = Vector2.Distance(Position, targetPosition);
        rush = distance > preferredRange ||
            distance - Vector2.Distance(Position, firstShotPosition) > retreatDistanceThreshold;
        if (rush || Random.value < orbitChance)
        {
            side = Random.value < 0.5f ? -1 : 1;
            SetState(State.SecondMove);
        }
        else BeginWindup(lockedTarget);
    }

    void TickSecondMove()
    {
        if (!Valid(lockedTarget)) { lockedTarget = null; SetState(State.Hunt); return; }
        bool visible = Visible(lockedTarget);
        if (CoverConfirmed(visible)) { BeginReload("Target behind cover (one shell left)"); return; }
        if (!visible)
        {
            Navigate(lockedTarget.position, enemyBase.SPEED * rushSpeedMultiplier, false);
            return;
        }
        float distance = Vector2.Distance(Position, lockedTarget.position);
        if ((rush && distance <= preferredRange) || (!rush && elapsed >= orbitSeconds) ||
            elapsed >= secondMoveTimeout)
        {
            if (distance <= pelletRange) BeginWindup(lockedTarget);
            else { lockedTarget = null; SetState(State.Hunt); }
            return;
        }
        Vector2 goal = rush ? (Vector2)lockedTarget.position : FlankGoal(lockedTarget, lowerAllies);
        Navigate(goal, enemyBase.SPEED * (rush ? rushSpeedMultiplier : 1f), !rush && lowerAllies);
    }

    bool CoverConfirmed(bool visible)
    {
        coverHiddenTime = visible ? 0f : coverHiddenTime + Dt;
        return coverHiddenTime + 0.0001f >= Mathf.Max(0.02f, coverReloadDelay);
    }

    void RecoverEmptyMagazine(Transform preferredTarget)
    {
        if (Valid(preferredTarget))
        { lastKnownThreat = preferredTarget.position; knowsThreat = true; }
        else if (Valid(CurrentTarget))
        { lastKnownThreat = CurrentTarget.position; knowsThreat = true; }

        if (HasLowerAllies() && knowsThreat)
            BeginRetreat(lastKnownThreat);
        else BeginReload(lowerAllies ? "No known hostile position" : "No lower-rank allies");
    }

    void BeginRetreat(Vector2 threat)
    {
        ClearWarnings();
        lockedTarget = null;
        SetState(State.Retreat);
        retreatRetries = 0;
        retreatThreat = threat;
        reloadReason = "Retreating before reload";
        StartRetreatSearch();
    }

    void StartRetreatSearch()
    {
        Stop(0);
        routeIndex = 0;
        retreatProgress = Position;
        retreatStuck = 0f;
        navigation.BeginRetreat(Position, retreatThreat);
    }

    void RetryRetreat(Transform target, string failure)
    {
        if (retreatRetries++ >= Mathf.Max(1, retreatMaxRetries))
        {
            // 本当に移動不能なら無限に停止し続けず、到達できた地点で装填する。
            BeginReload(failure);
            return;
        }
        if (Valid(target)) retreatThreat = target.position;
        StartRetreatSearch();
    }

    void TickRetreat(Transform target)
    {
        // 生存判定は共通の0.25秒更新を利用し、退避中に全敵を毎物理フレーム検索しない。
        if (!lowerAllies) { BeginReload("Lower-rank allies defeated"); return; }
        navigationStatus = navigation.PathStatus;
        if (navigation.RetreatSearching)
        {
            Stop(0);
            if (!navigation.TickRetreatSearch(retreatNodesPerTick)) return;
        }

        if (!navigation.RetreatHasDestination)
        {
            Stop(0);
            retreatStuck += Dt;
            if (retreatStuck >= 1f) RetryRetreat(target, "No reachable retreat floor");
            return;
        }

        runtimeRetreatDestination = navigation.RetreatDestination;
        navigationStatus = navigation.PathStatus;
        bool finished = FollowRoute(enemyBase.SPEED * retreatSpeedMultiplier);
        if (finished && Vector2.Distance(Position, navigation.RetreatDestination) <= Mathf.Max(arrivalDistance, 0.12f))
        {
            BeginReload("Reached farthest reachable floor");
            return;
        }

        if (Vector2.Distance(Position, retreatProgress) >= 0.18f)
        { retreatProgress = Position; retreatStuck = 0f; }
        else retreatStuck += Dt;
        if (retreatStuck >= 1.5f) RetryRetreat(target, "Retreat blocked after retries");
    }

    void BeginReload(string reason)
    {
        ClearWarnings();
        lockedTarget = null;
        SetState(State.Reload);
        reloadReason = reason;
        reloadElapsed = 0f;
        coverHiddenTime = 0f;
        Stop(reloadIdleRatio > 0f ? 0 : 4);
    }

    void TickReload()
    {
        reloadElapsed += Dt;
        float perShell = Mathf.Max(0.02f, secondsPerShell);
        while (reloadElapsed + 0.0001f >= perShell && CurrentAmmo < MaximumAmmo)
        {
            reloadElapsed = Mathf.Max(0f, reloadElapsed - perShell);
            CurrentAmmo++;
        }
        if (CurrentAmmo >= MaximumAmmo)
        {
            SetState(State.Hunt);
            huntTarget = null;
            unaimed = 0f;
            coverHiddenTime = 0f;
            Stop(0);
            return;
        }

        // 1発ごとに待機80% → リロード20%。装填が完了するたびにタイマーが
        // リセットされるため、0/2からなら1.6秒＋0.4秒を2回繰り返す。
        // アニメーションによらず、装填中の速度は必ずゼロにする。
        float idleDuration = perShell * Mathf.Clamp01(reloadIdleRatio);
        Stop(reloadElapsed + 0.0001f < idleDuration ? 0 : 4);
    }

    void Navigate(Vector2 goal, float speed, bool avoidAim, Vector2? fallbackGoal = null)
    {
        if (runtimeFloor == null) { navigationStatus = "Stage_Floor not found"; Stop(0); return; }
        repathTimer -= Dt;
        if (!routeGoalSet || repathTimer <= 0f || (goal - lastGoal).sqrMagnitude > 1f)
        {
            routeGoalSet = true;
            lastGoal = goal;
            routeIndex = 0;
            repathTimer = repathSeconds;
            float penalty = avoidAim && hasAim ? aimExposurePenalty : 0f;
            routeAvoidAim = penalty > 0f;
            bool found = navigation.FindPath(Position, goal, maxPathSearchNodes, searchRadiusCells,
                aimOrigin, aimDirection, AimCos, penalty);
            if (!found && fallbackGoal.HasValue && (fallbackGoal.Value - goal).sqrMagnitude > 0.01f)
                navigation.FindPath(Position, fallbackGoal.Value, maxPathSearchNodes, searchRadiusCells,
                    aimOrigin, aimDirection, AimCos, penalty);
        }
        navigationStatus = navigation.PathStatus;
        FollowRoute(speed);
    }

    bool FollowRoute(float speed)
    {
        Vector2 position = Position;
        while (routeIndex < navigation.Route.Count &&
            Vector2.Distance(position, navigation.Route[routeIndex]) <= arrivalDistance)
            routeIndex++;
        if (routeIndex >= navigation.Route.Count) { Stop(0); return true; }

        // 曲線上の少し先を見る。壁・床・照準を横切る短絡は禁止。
        int waypoint = routeIndex;
        for (int i = routeIndex + 1; i < navigation.Route.Count; i++)
        {
            if (Vector2.Distance(position, navigation.Route[i]) > movementLookAhead) break;
            if (!navigation.CanTravel(position, navigation.Route[i], routeAvoidAim && hasAim,
                aimOrigin, aimDirection, AimCos)) break;
            waypoint = i;
        }
        routeIndex = waypoint;
        Vector2 delta = (Vector2)navigation.Route[waypoint] - position;
        float finalSpeed = Mathf.Max(0.01f, speed);
        float acceleration = Mathf.Max(1f, movementAcceleration);
        if (waypoint == navigation.Route.Count - 1)
            finalSpeed = Mathf.Min(finalSpeed, Mathf.Sqrt(2f * acceleration * delta.magnitude));
        Vector2 desired = delta.normalized * Mathf.Min(finalSpeed, delta.magnitude / Dt);
        Vector2 velocity = navigation.IsWalkablePosition(position)
            ? Vector2.MoveTowards(enemyBase.rb.velocity, desired, acceleration * Dt) : desired;

        // 曲がる際の慣性が壁や床外を指す場合は、安全な経路方向へ戻す。
        if (!navigation.CanMoveStep(position, position + velocity * Dt))
            velocity = desired;
        if (!navigation.CanMoveStep(position, position + velocity * Dt))
        {
            Stop(0);
            // 一時的に止まっても、毎FixedUpdateでA*を繰り返さず短時間後に再探索。
            repathTimer = Mathf.Min(repathTimer, 0.10f);
            return false;
        }

        enemyBase.rb.velocity = velocity;
        // 微小な左右ブレによる振り向きの連打を防ぐ。
        if (Mathf.Abs(velocity.x) > 0.15f)
        {
            int facing = velocity.x > 0f ? 1 : -1;
            if (facing != movementFacing ||
                Mathf.Sign(enemyBase.EnemyAnimBody.transform.localScale.x) != facing)
            {
                movementFacing = facing;
                enemyBase.isTargetLook(position + Vector2.right * facing);
            }
        }
        Anim(1);
        return false;
    }

    void CreateWarnings()
    {
        ClearWarnings();
        SpriteRenderer template = attackWarningPrefab != null ?
            attackWarningPrefab.GetComponentInChildren<SpriteRenderer>(true) : null;
        if (template == null || template.sprite == null) return;
        for (int i = 0; i < directions.Length; ++i)
        {
            GameObject obj = new GameObject("Shotgun warning " + i);
            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = template.sprite;
            renderer.sharedMaterial = template.sharedMaterial;
            renderer.sortingLayerID = template.sortingLayerID;
            renderer.sortingOrder = template.sortingOrder;
            Color color = template.color;
            color.a = warningOpacity;
            renderer.color = color;
            warnings.Add(renderer);
        }
    }

    void UpdateWarnings()
    {
        Vector2 origin = Muzzle;
        float radius = ImperialShotGunTrooper_Pellet.RadiusOf(pelletPrefab);
        for (int i = 0; i < warnings.Count; ++i)
        {
            SpriteRenderer renderer = warnings[i];
            Vector2 dir = directions[i];
            float distance = ImperialShotGunTrooper_Pellet.WallDistance(origin, dir, pelletRange, radius);
            renderer.enabled = distance > 0.005f;
            Vector2 size = renderer.sprite.bounds.size;
            renderer.transform.position = origin + dir * distance * 0.5f;
            renderer.transform.right = dir;
            renderer.transform.localScale = new Vector3(distance / Mathf.Max(0.001f, size.x),
                Mathf.Max(warningWidth, 2f * radius) / Mathf.Max(0.001f, size.y), 1f);
        }
    }

    void ClearWarnings()
    {
        foreach (SpriteRenderer renderer in warnings)
            if (renderer != null) { renderer.gameObject.SetActive(false); Destroy(renderer.gameObject); }
        warnings.Clear();
    }
}
