using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// 床・壁判定と経路探索をAI本体から分離。退避は到達可能セルを分割探索する。
public sealed class ImperialShotGunTrooper_Navigation
{
    static readonly Vector3Int[] Steps = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
    readonly Collider2D[] obstacles = new Collider2D[32];
    readonly Dictionary<Vector3Int, bool> walkable = new Dictionary<Vector3Int, bool>();
    readonly Dictionary<Vector3Int, Vector3Int> parents = new Dictionary<Vector3Int, Vector3Int>();
    readonly Dictionary<Vector3Int, float> costs = new Dictionary<Vector3Int, float>();
    readonly HashSet<Vector3Int> closed = new HashSet<Vector3Int>();
    readonly List<Vector3Int> open = new List<Vector3Int>();
    readonly Queue<Vector3Int> retreatQueue = new Queue<Vector3Int>();
    readonly Transform owner;
    Tilemap floor, blocked;
    Vector3Int source, retreatBest;
    Vector2 retreatThreat, retreatStart;
    float retreatBestDistance;
    public readonly List<Vector3> Route = new List<Vector3>();
    public bool RetreatSearching { get; private set; }
    public string PathStatus { get; private set; } = "Not calculated";

    public ImperialShotGunTrooper_Navigation(Transform owner) { this.owner = owner; }

    public void SetMaps(Tilemap floorMap, Tilemap blockedMap)
    {
        if (floor == floorMap && blocked == blockedMap) return;
        floor = floorMap;
        blocked = blockedMap;
        Clear();
    }

    public void Clear()
    {
        Route.Clear();
        RetreatSearching = false;
        retreatQueue.Clear();
        walkable.Clear();
        parents.Clear();
        costs.Clear();
        closed.Clear();
        open.Clear();
    }

    bool IsBlocked(Collider2D hit)
    {
        // Stage_NotFloor は既存ステージで Stage_Floor と同じセルに描画される
        // 別レイヤー（Trigger）でもある。床の存在を打ち消す壁として扱わない。
        // 歩行可能範囲は指定した Stage_Floor のタイルそのものが決める。
        return hit != null && hit.transform != floor.transform && !hit.transform.IsChildOf(owner) &&
            (hit.CompareTag("Stage_Wall") || hit.CompareTag("Stage_Hole"));
    }

    bool Walkable(Vector3Int cell)
    {
        if (floor == null || !floor.HasTile(cell)) return false;
        if (walkable.TryGetValue(cell, out bool cached)) return cached;
        Vector3 worldCenter = floor.GetCellCenterWorld(cell);
        if (blocked != null && blocked != floor && blocked.HasTile(blocked.WorldToCell(new Vector3(worldCenter.x, worldCenter.y, blocked.transform.position.z))))
            return walkable[cell] = false;
        // Grid/Tilemap の実際のワールドサイズに合わせる。変形のある Grid にも対応。
        Vector3 nextX = floor.GetCellCenterWorld(cell + Vector3Int.right);
        Vector3 nextY = floor.GetCellCenterWorld(cell + Vector3Int.up);
        Vector2 probe = new Vector2(Mathf.Max(0.01f, Vector2.Distance(worldCenter, nextX) * 0.50f),
            Mathf.Max(0.01f, Vector2.Distance(worldCenter, nextY) * 0.50f));
        Vector2 center = worldCenter;
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        int n = Physics2D.OverlapBox(center, probe, 0f, filter, obstacles);
        bool good = true;
        if (n == obstacles.Length)
        {
            var overflow = new List<Collider2D>(obstacles.Length * 2);
            Physics2D.OverlapBox(center, probe, 0f, filter, overflow);
            foreach (Collider2D hit in overflow)
                if (IsBlocked(hit)) { good = false; break; }
        }
        else
        {
            for (int i = 0; i < n; i++)
                if (IsBlocked(obstacles[i])) { good = false; break; }
        }
        walkable[cell] = good;
        return good;
    }

    // Rigidbody2D の座標は XY のみ。Tilemap の Z が 0 でない配置にも対応。
    Vector3Int CellAt(Vector2 world)
    {
        return floor.WorldToCell(new Vector3(world.x, world.y, floor.transform.position.z));
    }

    public bool IsWalkablePosition(Vector2 world)
    {
        if (floor == null) return false;
        return Walkable(CellAt(world));
    }

    bool TrySource(Vector2 position, out Vector3Int cell)
    {
        cell = floor != null ? CellAt(position) : Vector3Int.zero;
        if (floor == null) return false;
        if (Walkable(cell)) return true;
        // 敵が床セルの境界に押し出された場合、近隣の歩けるセルへ復帰。
        Vector3Int original = cell;
        float best = float.PositiveInfinity;
        bool found = false;
        for (int x = -1; x <= 1; ++x)
        for (int y = -1; y <= 1; ++y)
        {
            Vector3Int candidate = original + new Vector3Int(x, y, 0);
            if (!Walkable(candidate)) continue;
            float dist = ((Vector2)floor.GetCellCenterWorld(candidate) - position).sqrMagnitude;
            if (dist < best) { best = dist; cell = candidate; found = true; }
        }
        return found;
    }

    static int Heuristic(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public bool FindPath(Vector2 start, Vector2 goal, int maxNodes, int searchRadius,
        Vector2 aimOrigin, Vector2 aimDirection, float aimCos, float exposurePenalty)
    {
        Clear();
        if (floor == null) { PathStatus = "Stage_Floor not found"; return false; }
        if (!TrySource(start, out source))
        {
            PathStatus = "No walkable start cell: " + CellAt(start);
            return false;
        }
        Vector3Int finish = CellAt(goal);
        open.Add(source);
        costs[source] = 0f;
        Vector3Int closest = source;
        int bestDistance = Heuristic(source, finish);
        float bestCost = 0f;
        int expanded = 0;
        while (open.Count > 0 && expanded < Mathf.Max(1, maxNodes))
        {
            int best = 0;
            float priority = float.PositiveInfinity;
            for (int i = 0; i < open.Count; ++i)
            {
                Vector3Int node = open[i];
                float score = costs[node] + Heuristic(node, finish);
                if (score < priority) { priority = score; best = i; }
            }
            Vector3Int cell = open[best];
            open.RemoveAt(best);
            if (!closed.Add(cell)) continue;
            ++expanded;
            int distance = Heuristic(cell, finish);
            if (distance < bestDistance || (distance == bestDistance && costs[cell] < bestCost))
            { closest = cell; bestDistance = distance; bestCost = costs[cell]; }
            if (cell == finish) { closest = finish; break; }
            foreach (Vector3Int step in Steps)
            {
                Vector3Int next = cell + step;
                if (closed.Contains(next) || Mathf.Abs(next.x - source.x) > searchRadius ||
                    Mathf.Abs(next.y - source.y) > searchRadius || !Walkable(next)) continue;
                Vector2 toCell = (Vector2)floor.GetCellCenterWorld(next) - aimOrigin;
                float penalty = 0f;
                if (exposurePenalty > 0f && aimDirection.sqrMagnitude > 0.01f &&
                    toCell.sqrMagnitude > 0.001f && Vector2.Dot(aimDirection, toCell.normalized) >= aimCos)
                    penalty = exposurePenalty;
                float cost = costs[cell] + 1f + penalty;
                if (costs.TryGetValue(next, out float known) && known <= cost) continue;
                costs[next] = cost;
                parents[next] = cell;
                if (!open.Contains(next)) open.Add(next);
            }
        }
        if (closest == source)
        {
            // 同一タイルの中でも実際に指定地点まで進めるようにする。
            if (source == finish && Walkable(finish)) Route.Add(goal);
            else if (((Vector2)floor.GetCellCenterWorld(source) - start).sqrMagnitude > 0.01f)
                Route.Add(floor.GetCellCenterWorld(source));
        }
        else
        {
            BuildRoute(closest);
            if (CellAt(start) != source) Route.Insert(0, floor.GetCellCenterWorld(source));
        }
        PathStatus = Route.Count == 0 ? "No route: " + source + " -> " + finish :
            (closest == finish ? "Path found: " : "Partial path: ") + Route.Count + " cells";
        return Route.Count > 0;
    }

    public void BeginRetreat(Vector2 start, Vector2 threat)
    {
        Clear();
        if (!TrySource(start, out source))
        {
            PathStatus = floor == null ? "Stage_Floor not found" : "No walkable retreat start: " + CellAt(start);
            return;
        }
        PathStatus = "Searching retreat";
        retreatThreat = threat;
        retreatStart = start;
        retreatBest = source;
        retreatBestDistance = ((Vector2)floor.GetCellCenterWorld(source) - threat).sqrMagnitude;
        retreatQueue.Enqueue(source);
        closed.Add(source);
        RetreatSearching = true;
    }

    // BFSで実際に到達できるStage_Floorの全連結領域を探索。
    // 1物理フレーム当たりのノード数に上限を持たせ、フリーズを避ける。
    public bool TickRetreatSearch(int nodesPerTick)
    {
        if (!RetreatSearching) return true;
        int budget = Mathf.Max(1, nodesPerTick);
        while (retreatQueue.Count > 0 && budget-- > 0)
        {
            Vector3Int cell = retreatQueue.Dequeue();
            float distance = ((Vector2)floor.GetCellCenterWorld(cell) - retreatThreat).sqrMagnitude;
            if (distance > retreatBestDistance) { retreatBestDistance = distance; retreatBest = cell; }
            foreach (Vector3Int step in Steps)
            {
                Vector3Int next = cell + step;
                if (closed.Contains(next) || !Walkable(next)) continue;
                closed.Add(next);
                parents[next] = cell;
                retreatQueue.Enqueue(next);
            }
        }
        if (retreatQueue.Count > 0) return false;
        RetreatSearching = false;
        BuildRoute(retreatBest);
        if (CellAt(retreatStart) != source) Route.Insert(0, floor.GetCellCenterWorld(source));
        PathStatus = "Retreat path: " + Route.Count + " cells";
        return true;
    }

    void BuildRoute(Vector3Int destination)
    {
        Route.Clear();
        Vector3Int current = destination;
        while (current != source)
        {
            Route.Add(floor.GetCellCenterWorld(current));
            if (!parents.TryGetValue(current, out Vector3Int parent))
            { Route.Clear(); return; }
            current = parent;
        }
        Route.Reverse();
    }
}
