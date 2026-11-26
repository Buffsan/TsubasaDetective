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
    Vector2 retreatThreat;
    float retreatBestDistance;
    public readonly List<Vector3> Route = new List<Vector3>();
    public bool RetreatSearching { get; private set; }

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
        return hit != null && !hit.transform.IsChildOf(owner) &&
            (hit.CompareTag("Stage_Wall") || hit.CompareTag("Stage_Hole") || hit.CompareTag("Stage_NotFloor"));
    }

    bool Walkable(Vector3Int cell)
    {
        if (floor == null || !floor.HasTile(cell) || (blocked != null && blocked.HasTile(cell))) return false;
        if (walkable.TryGetValue(cell, out bool cached)) return cached;
        Vector3 size = floor.cellSize;
        Vector2 probe = new Vector2(Mathf.Max(0.05f, Mathf.Abs(size.x) * 0.65f),
            Mathf.Max(0.05f, Mathf.Abs(size.y) * 0.65f));
        Vector2 center = floor.GetCellCenterWorld(cell);
        int n = Physics2D.OverlapBoxNonAlloc(center, probe, 0f, obstacles);
        bool good = true;
        if (n == obstacles.Length)
        {
            foreach (Collider2D hit in Physics2D.OverlapBoxAll(center, probe, 0f))
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

    public bool IsWalkablePosition(Vector2 world)
    {
        if (floor == null) return false;
        return Walkable(floor.WorldToCell(world));
    }

    bool TrySource(Vector2 position, out Vector3Int cell)
    {
        cell = floor != null ? floor.WorldToCell(position) : Vector3Int.zero;
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
        if (!TrySource(start, out source)) return false;
        Vector3Int finish = floor.WorldToCell(goal);
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
            if (floor.WorldToCell(start) != source) Route.Insert(0, floor.GetCellCenterWorld(source));
        }
        return Route.Count > 0;
    }

    public void BeginRetreat(Vector2 start, Vector2 threat)
    {
        Clear();
        if (!TrySource(start, out source)) return;
        retreatThreat = threat;
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
