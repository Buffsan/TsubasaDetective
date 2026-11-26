using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// 床・壁判定と経路探索をAI本体から分離。退避は到達可能セルを分割探索する。
public sealed class ImperialShotGunTrooper_Navigation
{
    static readonly Vector3Int[] Steps = {
        Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right,
        new Vector3Int(1, 1, 0), new Vector3Int(-1, 1, 0),
        new Vector3Int(1, -1, 0), new Vector3Int(-1, -1, 0)
    };
    readonly Collider2D[] obstacles = new Collider2D[32];
    readonly RaycastHit2D[] sweepHits = new RaycastHit2D[32];
    readonly List<RaycastHit2D> sweepOverflow = new List<RaycastHit2D>();
    readonly List<Vector3> rawRoute = new List<Vector3>();
    readonly List<Vector3> straightRoute = new List<Vector3>();
    readonly List<Vector3> cornerPoints = new List<Vector3>();
    float sampleSpacing = 0.15f;
    float clearance = 0.12f;
    float cornerRadius = 0.45f;
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
    public bool RetreatHasDestination { get; private set; }
    public Vector2 RetreatDestination { get; private set; }
    public string PathStatus { get; private set; } = "Not calculated";

    public ImperialShotGunTrooper_Navigation(Transform owner) { this.owner = owner; }

    public void SetMaps(Tilemap floorMap, Tilemap blockedMap)
    {
        if (floor == floorMap && blocked == blockedMap) return;
        floor = floorMap;
        blocked = blockedMap;
        if (floor != null)
        {
            Vector2 origin = floor.GetCellCenterWorld(Vector3Int.zero);
            float x = Vector2.Distance(origin, floor.GetCellCenterWorld(Vector3Int.right));
            float y = Vector2.Distance(origin, floor.GetCellCenterWorld(Vector3Int.up));
            sampleSpacing = Mathf.Max(0.015f, Mathf.Min(x, y) * 0.20f);
        }
        Clear();
    }

    public void ConfigureSmoothing(float radius, float sweepRadius)
    {
        cornerRadius = Mathf.Max(0f, radius);
        clearance = Mathf.Max(0.005f, sweepRadius);
    }

    public void Clear()
    {
        Route.Clear();
        RetreatSearching = false;
        RetreatHasDestination = false;
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

    static float Heuristic(Vector3Int a, Vector3Int b)
    {
        int x = Mathf.Abs(a.x - b.x), y = Mathf.Abs(a.y - b.y);
        return Mathf.Max(x, y) + 0.41421356f * Mathf.Min(x, y);
    }

    bool CanStep(Vector3Int cell, Vector3Int step)
    {
        if (!Walkable(cell + step)) return false;
        // 斜め移動は左右の床が存在するときだけ許可し、壁角・床の隙間のすり抜けを防ぐ。
        return step.x == 0 || step.y == 0 ||
            (Walkable(cell + new Vector3Int(step.x, 0, 0)) &&
             Walkable(cell + new Vector3Int(0, step.y, 0)));
    }

    bool IsAimedAt(Vector2 point, Vector2 origin, Vector2 direction, float cos)
    {
        Vector2 offset = point - origin;
        return direction.sqrMagnitude > 0.01f && offset.sqrMagnitude > 0.001f &&
            Vector2.Dot(direction, offset.normalized) >= cos;
    }

    bool IsWallHit(RaycastHit2D hit, Vector2 from, Vector2 to)
    {
        if (!IsBlocked(hit.collider)) return false;
        // 壁のすぐ横から離れる移動を、開始位置の接触だけで止めない。
        if (hit.distance <= 0.001f)
        {
            float a = (hit.collider.ClosestPoint(from) - from).sqrMagnitude;
            float b = (hit.collider.ClosestPoint(to) - to).sqrMagnitude;
            if (b > a + 0.00001f) return false;
        }
        return true;
    }

    bool ClearOfWalls(Vector2 from, Vector2 to)
    {
        Vector2 delta = to - from;
        float distance = delta.magnitude;
        if (distance <= 0.00001f) return true;
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        int count = Physics2D.CircleCast(from, clearance, delta / distance, filter, sweepHits, distance);
        if (count == sweepHits.Length)
        {
            sweepOverflow.Clear();
            Physics2D.CircleCast(from, clearance, delta / distance, filter, sweepOverflow, distance);
            foreach (RaycastHit2D hit in sweepOverflow)
                if (IsWallHit(hit, from, to)) return false;
        }
        else
            for (int i = 0; i < count; i++)
                if (IsWallHit(sweepHits[i], from, to)) return false;
        return true;
    }

    // 曲線を作るための安全判定。床の境界と壁の両方を確認し、照準回避時は
    // せっかく回避したカーソルの正面をショートカットしない。
    public bool CanTravel(Vector2 from, Vector2 to, bool avoidAim = false,
        Vector2 aimOrigin = default(Vector2), Vector2 aimDirection = default(Vector2), float aimCos = 1f)
    {
        if (floor == null || !IsWalkablePosition(from) || !IsWalkablePosition(to)) return false;
        int count = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(from, to) / sampleSpacing));
        Vector3Int previous = CellAt(from);
        for (int i = 1; i <= count; i++)
        {
            Vector2 point = Vector2.Lerp(from, to, (float)i / count);
            Vector3Int cell = CellAt(point);
            if (!Walkable(cell)) return false;
            Vector3Int step = cell - previous;
            if (step.x != 0 && step.y != 0 && !CanStep(previous, step)) return false;
            if (avoidAim && IsAimedAt(point, aimOrigin, aimDirection, aimCos)) return false;
            previous = cell;
        }
        return ClearOfWalls(from, to);
    }

    // Rigidbody の速度を丸めた結果も検査する。床の境界からの復帰だけは
    // 現在セルを出るまで許可し、通常の移動で床外に踏み出させない。
    public bool CanMoveStep(Vector2 from, Vector2 to)
    {
        if (floor == null) return false;
        if (!IsWalkablePosition(from)) return ClearOfWalls(from, to);
        return CanTravel(from, to);
    }

    void SmoothRoute(Vector2 start, bool avoidAim, Vector2 origin, Vector2 direction, float cos)
    {
        if (Route.Count == 0) return;
        rawRoute.Clear();
        rawRoute.AddRange(Route);
        straightRoute.Clear();
        Vector2 anchor = start;
        int index = 0;
        while (index < rawRoute.Count)
        {
            int chosen = index;
            // 最大8セル先までの見通しのよい区間だけ短縮する（壁と照準を無視しない）。
            int max = Mathf.Min(rawRoute.Count - 1, index + 7);
            for (int j = max; j > index; j--)
            {
                if (!CanTravel(anchor, rawRoute[j], avoidAim, origin, direction, cos)) continue;
                chosen = j;
                break;
            }
            straightRoute.Add(rawRoute[chosen]);
            anchor = rawRoute[chosen];
            index = chosen + 1;
        }
        Route.Clear();
        for (int i = 0; i < straightRoute.Count; i++)
        {
            Vector2 point = straightRoute[i];
            if (i == straightRoute.Count - 1 || cornerRadius <= 0f)
            {
                Route.Add(point);
                continue;
            }
            Vector2 before = i == 0 ? start : (Vector2)straightRoute[i - 1];
            Vector2 after = straightRoute[i + 1];
            Vector2 incoming = point - before, outgoing = after - point;
            float radius = Mathf.Min(cornerRadius, Mathf.Min(incoming.magnitude, outgoing.magnitude) * 0.40f);
            if (radius < 0.025f || Vector2.Dot(incoming.normalized, outgoing.normalized) > 0.995f)
            {
                Route.Add(point);
                continue;
            }
            Vector2 enter = point - incoming.normalized * radius;
            Vector2 exit = point + outgoing.normalized * radius;
            Vector2 last = Route.Count > 0 ? (Vector2)Route[Route.Count - 1] : start;
            bool safe = CanTravel(last, enter, avoidAim, origin, direction, cos);
            cornerPoints.Clear();
            cornerPoints.Add(enter);
            last = enter;
            // 角の前後を二次Bezierでつないで、曲がり方を徐々に変える。
            int samples = Mathf.Clamp(Mathf.CeilToInt(radius * 2f / sampleSpacing), 4, 16);
            for (int k = 1; safe && k <= samples; k++)
            {
                float t = (float)k / samples;
                Vector2 p = Vector2.Lerp(Vector2.Lerp(enter, point, t), Vector2.Lerp(point, exit, t), t);
                safe = CanTravel(last, p, avoidAim, origin, direction, cos);
                cornerPoints.Add(p);
                last = p;
            }
            if (safe) Route.AddRange(cornerPoints);
            else Route.Add(point);
        }
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
        float bestDistance = Heuristic(source, finish);
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
            float distance = Heuristic(cell, finish);
            if (distance < bestDistance || (Mathf.Approximately(distance, bestDistance) && costs[cell] < bestCost))
            { closest = cell; bestDistance = distance; bestCost = costs[cell]; }
            if (cell == finish) { closest = finish; break; }
            foreach (Vector3Int step in Steps)
            {
                Vector3Int next = cell + step;
                if (closed.Contains(next) || Mathf.Abs(next.x - source.x) > searchRadius ||
                    Mathf.Abs(next.y - source.y) > searchRadius || !CanStep(cell, step)) continue;
                Vector2 toCell = (Vector2)floor.GetCellCenterWorld(next) - aimOrigin;
                float penalty = 0f;
                if (exposurePenalty > 0f && aimDirection.sqrMagnitude > 0.01f &&
                    toCell.sqrMagnitude > 0.001f && Vector2.Dot(aimDirection, toCell.normalized) >= aimCos)
                    penalty = exposurePenalty;
                float cost = costs[cell] + (step.x != 0 && step.y != 0 ? 1.41421356f : 1f) + penalty;
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
        // 目標セルまで到達した場合は、セル中心ではなく指定位置へ向かう。
        if (closest == finish && Route.Count > 0 &&
            Vector2.Distance(Route[Route.Count - 1], goal) > 0.01f &&
            CanTravel(Route[Route.Count - 1], goal, exposurePenalty > 0f, aimOrigin, aimDirection, aimCos))
            Route.Add(goal);
        // 直前の区間の安全性を保った上で、曲線化する。
        SmoothRoute(start, exposurePenalty > 0f, aimOrigin, aimDirection, aimCos);
        PathStatus = Route.Count == 0 ? "No route: " + source + " -> " + finish :
            (closest == finish ? "Path found: " : "Partial path: ") + Route.Count + " waypoints";
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
                if (closed.Contains(next) || !CanStep(cell, step)) continue;
                closed.Add(next);
                parents[next] = cell;
                retreatQueue.Enqueue(next);
            }
        }
        if (retreatQueue.Count > 0) return false;
        RetreatSearching = false;
        BuildRoute(retreatBest);
        RetreatHasDestination = true;
        RetreatDestination = floor.GetCellCenterWorld(retreatBest);
        if (CellAt(retreatStart) != source) Route.Insert(0, floor.GetCellCenterWorld(source));
        if (Route.Count == 0 && Vector2.Distance(retreatStart, RetreatDestination) > 0.08f)
            Route.Add(RetreatDestination);
        SmoothRoute(retreatStart, false, Vector2.zero, Vector2.zero, 1f);
        PathStatus = "Retreat path: " + Route.Count + " waypoints";
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
