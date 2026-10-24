using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

// 帝国突撃兵。EnemyContorollerのSpecial移動を使用し、共通クラスは変更しない。
public class ImperialAssaultTrooper_AI : EnemyMoveBase
{
    [Header("射撃")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform muzzle;
    [SerializeField, Min(1)] int magazineSize = 24;
    [SerializeField, Min(1)] int minimumBurst = 12;
    [SerializeField, Min(0.02f)] float shotInterval = 0.12f;
    [SerializeField, Min(0f)] float reloadSeconds = 2f;
    [SerializeField, Min(0f)] float spreadDegrees = 7f;
    [SerializeField, Min(1f)] float bulletSpeed = 19f;
    [SerializeField, Min(0.1f)] float bulletLifetime = 5f;
    [SerializeField, Min(0f)] float aimTurnDegreesPerSecond = 100f;
    [SerializeField, Min(0f)] float firingTurnDegreesPerSecond = 28f;
    [SerializeField] LayerMask lineOfSightMask; // 遮蔽物ColliderのLayerをInspectorで設定
    [SerializeField] float preferredRange = 5f;
    [SerializeField] float maxEngagementRange = 12f;

    [Header("攻撃範囲表示・射撃SE")]
    [SerializeField] GameObject attackWarningPrefab;
    [SerializeField, Range(0.1f, 1f)] float warningOpacity = 0.26f;
    [SerializeField, Min(0.1f)] float warningMinWidth = 0.35f;
    [SerializeField] AudioClip shotSE;

    [Header("床タイル経路探索")]
    [SerializeField] Tilemap standFloor;
    [SerializeField] Tilemap blockedTiles;
    [SerializeField, Range(1, 50)] int searchRadiusCells = 24;
    [SerializeField, Min(0.05f)] float repathSeconds = 0.45f;
    [SerializeField, Min(0.02f)] float arrivalDistance = 0.18f;

    [Header("弾数UI（既存HPゲージ中央のTextMeshProを指定）")]
    [SerializeField] TMP_Text ammoText;

    public int CurrentAmmo { get; private set; }
    public int MaximumAmmo => magazineSize;
    public bool IsReloading => reloading;

    EnemyContoroller controller;
    Transform lockedTarget;
    Transform lastNavigationTarget;
    readonly List<Vector3> route = new List<Vector3>();
    int waypoint;
    float repathTimer;
    float firingTimer;
    float reloadTimer;
    int mandatoryShotsRemaining;
    bool reloading;
    bool firing;
    Vector2 aimDirection = Vector2.right;
    GameObject warningObject;
    SpriteRenderer warningRenderer;
    int lastDisplayedAmmo = -1;
    readonly Collider2D[] nearbyObstacles = new Collider2D[16];

    void Awake()
    {
        CurrentAmmo = Mathf.Max(1, magazineSize);
        if (enemyBase == null) enemyBase = GetComponent<EnemyContoroller>();
    }
    void LateUpdate()
    {
        if (ammoText == null || lastDisplayedAmmo == CurrentAmmo) return;
        ammoText.text = CurrentAmmo + "/" + MaximumAmmo;
        lastDisplayedAmmo = CurrentAmmo;
    }
    void Start()
    {
        if (enemyBase == null) enemyBase = GetComponent<EnemyContoroller>();
        controller = enemyBase as EnemyContoroller;
        if (controller == null) { Debug.LogError("帝国突撃兵にはEnemyContorollerが必要です。", this); enabled = false; return; }
        controller.Choice_MoveToBase = EnemyContoroller.MoveToBase.Special;
        // 標準の距離依存攻撃遷移を抑止する。攻撃開始は本AIが制御。
        enemyBase.LookPlayerDistance = -1f;
        if (standFloor == null && ALL_SystemManager.Instance != null &&
            ALL_SystemManager.Instance.enemySpawnBASE != null &&
            ALL_SystemManager.Instance.enemySpawnBASE.Seve_TileMap_C != null)
            standFloor = ALL_SystemManager.Instance.enemySpawnBASE.Seve_TileMap_C.StandFloor;
    }

    public override void EnemyMovePlay()
    {
        if (enemyBase == null || enemyBase.status != EnemyBase.Status.Nomal) return;
        Transform target = CurrentTarget;
        if (target == null) { Stop(); lastNavigationTarget = null; repathTimer = 0f; return; }
        if (lastNavigationTarget != target) { lastNavigationTarget = target; repathTimer = 0f; }
        Vector2 toTarget = target.position - enemyBase.transform.position;
        bool visible = IsVisible(target);
        TurnAim(toTarget, aimTurnDegreesPerSecond);
        if (CurrentAmmo <= 0) { BeginReload(); return; }
        if (visible && toTarget.magnitude <= maxEngagementRange)
        {
            lockedTarget = target;
            mandatoryShotsRemaining = Mathf.Min(minimumBurst, CurrentAmmo);
            firing = true;
            firingTimer = 0f;
            enemyBase.moveType = EnemyBase.MoveType.Attack;
            Stop(false);
            if (enemyBase.animator != null) enemyBase.animator.SetInteger("Anim", 2);
            CreateAttackWarning();
            return;
        }
        if (visible && toTarget.magnitude <= preferredRange) { Stop(); return; }
        Navigate(target.position);
    }

    public override void EnemyAttackPlay()
    {
        Stop(false);
        if (reloading) { TickReload(); return; }
        if (!firing) { FinishBurst(); return; }
        if (CurrentAmmo <= 0) { firing = false; BeginReload(); return; }

        // 開始時に固定した相手を追い続け、旋回速度は射撃中だけ落ちる。
        if (lockedTarget != null)
            TurnAim((Vector2)lockedTarget.position - (Vector2)enemyBase.transform.position,
                firingTurnDegreesPerSecond);
        UpdateAttackWarning();
        // 一度発射したら必ず12発射撃。途中で隠れられても最初の12発は続ける。
        // 12発以降は射線が通る間だけ、残弾を撃ち切る。
        bool visible = lockedTarget != null && IsVisible(lockedTarget);
        if (!visible && mandatoryShotsRemaining <= 0) { FinishBurst(); return; }
        firingTimer -= Time.fixedDeltaTime;
        if (firingTimer > 0f) return;
        FireOne();
        firingTimer = Mathf.Max(0.02f, shotInterval);
        if (mandatoryShotsRemaining > 0) mandatoryShotsRemaining--;
        if (CurrentAmmo <= 0) { firing = false; BeginReload(); }
    }

    void FireOne()
    {
        if (bulletPrefab == null) { Debug.LogWarning("帝国突撃兵の弾丸Prefabが未登録です。", this); FinishBurst(); return; }
        Vector2 origin = muzzle != null ? muzzle.position :
            (enemyBase.AttackSpawnPoint != null ? enemyBase.AttackSpawnPoint.transform.position : enemyBase.transform.position);
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg
                      + Random.Range(-spreadDegrees, spreadDegrees);
        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        GameObject shot = Instantiate(bulletPrefab, origin, Quaternion.Euler(0f, 0f, angle));
        ConfigureSummonedAttack(shot);
        ImperialAssaultTrooper_Bullet bullet = shot.GetComponent<ImperialAssaultTrooper_Bullet>();
        if (bullet == null) bullet = shot.AddComponent<ImperialAssaultTrooper_Bullet>();
        bullet.Launch(direction, bulletSpeed, bulletLifetime);
        CurrentAmmo--;
        if (shotSE != null && enemyBase.audioManager != null)
            enemyBase.audioManager.PlaySE(shotSE);
        if (enemyBase.animator != null)
        {
            enemyBase.animator.SetInteger("Anim", 3);
            enemyBase.animator.Play("攻撃", 0, 0f);
        }
    }

    void BeginReload()
    {
        firing = false;
        ClearAttackWarning();
        reloading = true;
        reloadTimer = reloadSeconds;
        enemyBase.moveType = EnemyBase.MoveType.Attack;
        Stop(false);
        if (enemyBase.animator != null) enemyBase.animator.SetInteger("Anim", 2);
    }
    void TickReload()
    {
        reloadTimer -= Time.fixedDeltaTime;
        if (reloadTimer > 0f) return;
        CurrentAmmo = Mathf.Max(1, magazineSize);
        reloading = false;
        FinishBurst();
    }
    void FinishBurst()
    {
        firing = false;
        mandatoryShotsRemaining = 0;
        lockedTarget = null;
        ClearAttackWarning();
        if (enemyBase.animator != null) enemyBase.animator.SetInteger("Anim", 0);
        enemyBase.moveType = EnemyBase.MoveType.Move;
        repathTimer = 0f;
    }
    void Stop(bool idleAnimation = true)
    {
        if (enemyBase.rb != null) enemyBase.rb.velocity = Vector2.zero;
        if (idleAnimation && enemyBase.animator != null) enemyBase.animator.SetInteger("Anim", 0);
    }
    void TurnAim(Vector2 toTarget, float angularSpeed)
    {
        if (toTarget.sqrMagnitude < 0.0001f) return;
        float currentAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle,
            angularSpeed * Time.fixedDeltaTime) * Mathf.Deg2Rad;
        aimDirection = new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));
        enemyBase.isTargetLook((Vector2)enemyBase.transform.position + aimDirection);
    }
    bool IsVisible(Transform target)
    {
        if (target == null) return false;
        Vector2 origin = muzzle != null ? muzzle.position :
            (enemyBase.AttackSpawnPoint != null ? enemyBase.AttackSpawnPoint.transform.position : enemyBase.transform.position);
        Vector2 delta = (Vector2)target.position - origin;
        if (delta.sqrMagnitude < 0.001f) return true;
        // wallタグはレイヤー設定なしでも検知。任意の追加遮蔽物はmaskで指定。
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, delta.normalized, delta.magnitude);
        foreach (RaycastHit2D hit in hits)
        {
            Collider2D obstacle = hit.collider;
            if (obstacle == null || obstacle.transform.IsChildOf(enemyBase.transform) ||
                obstacle.transform == target || obstacle.transform.IsChildOf(target)) continue;
            bool onMask = (lineOfSightMask.value & (1 << obstacle.gameObject.layer)) != 0;
            if (obstacle.CompareTag("wall") || onMask) return false;
        }
        return true;
    }
    public override void EnemyStanPlay() { Cancel(); }
    public override void EnemyConfusionPlay() { Cancel(); }
    void OnDisable() { if (enemyBase != null) Cancel(); }
    void Cancel()
    {
        firing = false;
        // スタン・混乱時はリロードを中断。残弾は維持する。
        reloading = false;
        mandatoryShotsRemaining = 0;
        lockedTarget = null;
        ClearAttackWarning();
        route.Clear();
        waypoint = 0;
        repathTimer = 0f;
        Stop();
        if (enemyBase != null && enemyBase.status == EnemyBase.Status.Nomal)
            enemyBase.moveType = EnemyBase.MoveType.Move;
    }
    // 共通攻撃警告Prefabを射撃中のみ帯状に表示する。
    void CreateAttackWarning()
    {
        ClearAttackWarning();
        if (attackWarningPrefab == null) return;
        warningObject = controller != null
            ? controller.SpawnAttackWarningPrefab(attackWarningPrefab, Vector3.zero,
                Quaternion.identity, 10f, null, 0f, false)
            : Instantiate(attackWarningPrefab);
        if (warningObject == null) return;
        warningRenderer = warningObject.GetComponentInChildren<SpriteRenderer>(true);
        if (warningRenderer == null)
        {
            Destroy(warningObject);
            warningObject = null;
            return;
        }
        // 既存フェードアニメが再生途中で消さないよう固定表示する。
        foreach (Animator animator in warningObject.GetComponentsInChildren<Animator>(true))
            animator.enabled = false;
        Color color = warningRenderer.color;
        color.a = warningOpacity;
        warningRenderer.color = color;
        warningRenderer.sortingOrder = Mathf.Max(warningRenderer.sortingOrder, -5);
        UpdateAttackWarning();
    }

    void UpdateAttackWarning()
    {
        if (warningObject == null || warningRenderer == null || enemyBase == null) return;
        Vector2 origin = muzzle != null ? (Vector2)muzzle.position :
            (enemyBase.AttackSpawnPoint != null ? (Vector2)enemyBase.AttackSpawnPoint.transform.position :
                (Vector2)enemyBase.transform.position);
        float range = GetUnobstructedWarningLength(origin);
        // 発砲のばらつきを含む帯の最大幅を表示する。実弾の判定とは独立。
        float width = Mathf.Max(warningMinWidth,
            2f * range * Mathf.Tan(Mathf.Min(80f, spreadDegrees) * Mathf.Deg2Rad));
        warningObject.transform.position = origin + aimDirection * (range * 0.5f);
        warningObject.transform.rotation = Quaternion.Euler(0f, 0f,
            Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg);
        if (warningRenderer.sprite != null)
        {
            Vector2 spriteSize = warningRenderer.sprite.bounds.size;
            Vector3 childScale = warningRenderer.transform.localScale;
            float sx = Mathf.Max(0.001f, spriteSize.x * Mathf.Abs(childScale.x));
            float sy = Mathf.Max(0.001f, spriteSize.y * Mathf.Abs(childScale.y));
            warningObject.transform.localScale = new Vector3(range / sx, width / sy, 1f);
        }
    }

    float GetUnobstructedWarningLength(Vector2 origin)
    {
        float length = Mathf.Max(0.1f, maxEngagementRange);
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, aimDirection, length);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || hit.collider.transform.IsChildOf(enemyBase.transform)) continue;
            bool onMask = (lineOfSightMask.value & (1 << hit.collider.gameObject.layer)) != 0;
            if (hit.collider.CompareTag("wall") || onMask)
                length = Mathf.Min(length, Mathf.Max(0.05f, hit.distance));
        }
        return length;
    }

    void ClearAttackWarning()
    {
        if (warningObject != null) Destroy(warningObject);
        warningObject = null;
        warningRenderer = null;
    }

    void Navigate(Vector3 destination)
    {
        if (standFloor == null) { Stop(); return; }
        repathTimer -= Time.fixedDeltaTime;
        // 経路がなくても毎FixedUpdateで全マス探索しない。
        if (repathTimer <= 0f)
        {
            repathTimer = Mathf.Max(0.05f, repathSeconds);
            FindPath(enemyBase.transform.position, destination);
        }
        if (waypoint >= route.Count) { Stop(); return; }
        Vector2 step = (Vector2)route[waypoint] - (Vector2)enemyBase.transform.position;
        if (step.magnitude <= arrivalDistance)
        {
            waypoint++;
            if (waypoint >= route.Count) { Stop(); return; }
            step = (Vector2)route[waypoint] - (Vector2)enemyBase.transform.position;
        }
        enemyBase.rb.velocity = step.normalized * enemyBase.SPEED;
        if (enemyBase.animator != null) enemyBase.animator.SetInteger("Anim", 1);
    }
    bool CanWalk(Vector3Int cell)
    {
        if (standFloor == null || !standFloor.HasTile(cell)) return false;
        if (blockedTiles != null && blockedTiles.HasTile(cell)) return false;
        // 候補の床セルがwall Colliderに埋まっていれば進入しない。
        Vector2 center = standFloor.GetCellCenterWorld(cell);
        Vector3 size = standFloor.cellSize;
        Vector2 probeSize = new Vector2(Mathf.Max(0.05f, size.x * 0.75f),
                                         Mathf.Max(0.05f, size.y * 0.75f));
        int count = Physics2D.OverlapBoxNonAlloc(center, probeSize, 0f, nearbyObstacles);
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = nearbyObstacles[i];
            if (hit == null || hit.transform == standFloor.transform ||
                hit.transform.IsChildOf(enemyBase.transform)) continue;
            if (hit.CompareTag("wall")) return false;
        }
        return true;
    }
    void FindPath(Vector3 start, Vector3 goal)
    {
        route.Clear();
        waypoint = 0;
        Vector3Int source = standFloor.WorldToCell(start);
        Vector3Int finish = standFloor.WorldToCell(goal);
        if (!CanWalk(source)) return;
        // 半径を制限した4近傍BFS。穴を跨いだ直進を行わない。
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> parent = new Dictionary<Vector3Int, Vector3Int>();
        queue.Enqueue(source);
        parent[source] = source;
        Vector3Int best = source;
        int bestScore = Mathf.Abs(source.x - finish.x) + Mathf.Abs(source.y - finish.y);
        Vector3Int[] steps = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        while (queue.Count > 0)
        {
            Vector3Int cell = queue.Dequeue();
            if (cell == finish) { best = cell; break; }
            foreach (Vector3Int offset in steps)
            {
                Vector3Int next = cell + offset;
                if (Mathf.Abs(next.x - source.x) > searchRadiusCells ||
                    Mathf.Abs(next.y - source.y) > searchRadiusCells ||
                    parent.ContainsKey(next) || !CanWalk(next)) continue;
                parent[next] = cell;
                queue.Enqueue(next);
                int score = Mathf.Abs(next.x - finish.x) + Mathf.Abs(next.y - finish.y);
                if (score < bestScore) { bestScore = score; best = next; }
            }
        }
        if (best == source) return;
        Vector3Int cursor = best;
        while (cursor != source)
        {
            route.Add(standFloor.GetCellCenterWorld(cursor));
            cursor = parent[cursor];
        }
        route.Reverse();
    }
}
