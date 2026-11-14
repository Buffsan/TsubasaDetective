using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Placed on the base stage prefab. Marker tiles select five independent start positions.
// A normal wave is allowed to begin before the sweep to prevent the soldiers from passing an empty map.
[DisallowMultipleComponent]
public class SweepFormationSpawner : MonoBehaviour
{
    [Header("ステージ参照")]
    [SerializeField] TileMap_Controller tileMapController;
    [SerializeField] GameObject sweepSoldierPrefab;

    [Header("自動発動")]
    [SerializeField] bool autoSpawnOnStart = true;
    [SerializeField] bool waitForNormalEnemies = true;
    [SerializeField, Min(0f)] float initialDelaySeconds = 1f;
    [SerializeField, Range(1, 24)] int membersCount = 5;

    [Header("反対側にタイルがない場合の保険")]
    [SerializeField, Min(1f)] float fallbackTravelDistance = 40f;

    readonly List<GameObject> spawnedMembers = new List<GameObject>();
    bool hasSpawned;

    IEnumerator Start()
    {
        if (!autoSpawnOnStart) yield break;
        if (tileMapController == null)
            tileMapController = GetComponent<TileMap_Controller>();
        if (tileMapController == null || tileMapController.SweepSpawnMap == null ||
            !HasSpawnMarkers(tileMapController.SweepSpawnMap))
            yield break; // Existing stages without markers remain completely unchanged.

        while (SystemManager.Instance == null ||
               SystemManager.Instance.gameMode != SystemManager.GameMode.Nomal ||
               (waitForNormalEnemies && !HasRequiredEnemies()))
            yield return null;

        if (initialDelaySeconds > 0f)
            yield return new WaitForSeconds(initialDelaySeconds);

        if (SystemManager.Instance != null &&
            SystemManager.Instance.gameMode == SystemManager.GameMode.Nomal)
            SpawnSweep();
    }

    bool HasRequiredEnemies()
    {
        SystemManager manager = SystemManager.Instance;
        if (manager == null) return false;
        foreach (GameObject enemy in manager.AllEnemy)
        {
            if (enemy == null) continue;
            EnemyBase owner = enemy.GetComponent<EnemyBase>();
            if (owner != null && owner.CountsForStageClear &&
                owner.status != EnemyBase.Status.Die && owner.HP > 0f)
                return true;
        }
        return false;
    }

    static bool HasSpawnMarkers(Tilemap map)
    {
        foreach (Vector3Int cell in map.cellBounds.allPositionsWithin)
            if (map.HasTile(cell)) return true;
        return false;
    }

    // Can also be invoked directly from a stage event or cutscene.
    public void SpawnSweep()
    {
        if (hasSpawned) return;
        if (tileMapController == null)
            tileMapController = GetComponent<TileMap_Controller>();
        if (tileMapController == null || tileMapController.SweepSpawnMap == null)
        {
            Debug.LogWarning("SweepFormationSpawner: SweepSpawnMap is missing.", this);
            return;
        }
        if (sweepSoldierPrefab == null)
        {
            Debug.LogWarning("SweepFormationSpawner: Shield prefab is missing.", this);
            return;
        }
        Tilemap spawnMap = tileMapController.SweepSpawnMap;
        List<Vector3> positions = new List<Vector3>();
        foreach (Vector3Int cell in spawnMap.cellBounds.allPositionsWithin)
            if (spawnMap.HasTile(cell))
                positions.Add(spawnMap.GetCellCenterWorld(cell));

        // The five marker cells form a vertical column, top to bottom.
        positions.Sort((a, b) => {
            int byY = b.y.CompareTo(a.y);
            return byY != 0 ? byY : a.x.CompareTo(b.x);
        });
        if (positions.Count < membersCount)
        {
            Debug.LogWarning("SweepFormationSpawner: Place at least " + membersCount +
                " marker tiles in SweepSpawnMap. Currently: " + positions.Count, this);
            return;
        }

        float exitX = GetExitX(tileMapController.RightOuterBoundary);
        float leftmostSpawnX = float.PositiveInfinity;
        for (int i = 0; i < membersCount; i++)
            leftmostSpawnX = Mathf.Min(leftmostSpawnX, positions[i].x);
        if (float.IsInfinity(exitX) || exitX <= leftmostSpawnX + 1f)
        {
            exitX = leftmostSpawnX + fallbackTravelDistance;
            Debug.LogWarning("SweepFormationSpawner: RightOuterBoundary has no usable exit tiles; " +
                "using fallback distance.", this);
        }

        hasSpawned = true;
        for (int i = 0; i < membersCount; i++)
        {
            GameObject member = Instantiate(sweepSoldierPrefab, positions[i], Quaternion.identity);
            SweepShieldRoute route = member.GetComponent<SweepShieldRoute>();
            if (route == null) route = member.AddComponent<SweepShieldRoute>();
            route.Initialize(positions[i].y, exitX);
            spawnedMembers.Add(member);

            SystemManager manager = SystemManager.Instance;
            if (manager != null && !manager.AllEnemy.Contains(member))
                manager.AllEnemy.Add(member);
        }
    }

    static float GetExitX(Tilemap rightBoundary)
    {
        if (rightBoundary == null) return float.PositiveInfinity;
        float minX = float.PositiveInfinity;
        foreach (Vector3Int cell in rightBoundary.cellBounds.allPositionsWithin)
        {
            if (!rightBoundary.HasTile(cell)) continue;
            minX = Mathf.Min(minX, rightBoundary.CellToWorld(cell).x);
        }
        return minX;
    }

    void OnDestroy()
    {
        // Stage transition cleanup: shields must not outlive their stage.
        foreach (GameObject member in spawnedMembers)
            if (member != null) Destroy(member);
        spawnedMembers.Clear();
    }
}
