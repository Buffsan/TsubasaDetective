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

    public enum FormationDirection { Vertical, Horizontal }

    [Header("隊列・出現ウェーブ")]
    [SerializeField] FormationDirection formationDirection = FormationDirection.Vertical;
    [SerializeField, Min(0.1f)] float formationSpacing = 1f;
    [SerializeField, Min(1)] int waveCount = 1;
    [SerializeField, Min(0f)] float waveIntervalSeconds = 10f;

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

    // Can also be invoked directly from a stage event.
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

        Vector3 center = Vector3.zero;
        int markers = 0;
        Tilemap map = tileMapController.SweepSpawnMap;
        foreach (Vector3Int cell in map.cellBounds.allPositionsWithin)
        {
            if (!map.HasTile(cell)) continue;
            center += map.GetCellCenterWorld(cell);
            markers++;
        }
        if (markers == 0)
        {
            Debug.LogWarning("SweepFormationSpawner: Paint at least one marker tile.", this);
            return;
        }

        // The painted tiles define the center; the Inspector decides row orientation.
        center /= markers;
        hasSpawned = true;
        StartCoroutine(SpawnWaves(center));
    }

    IEnumerator SpawnWaves(Vector3 center)
    {
        int count = Mathf.Max(1, membersCount);
        float spacing = Mathf.Max(0.1f, formationSpacing);
        float halfSpan = (count - 1) * 0.5f * spacing;

        float exitX = GetExitX(tileMapController.RightOuterBoundary);
        float farthestX = center.x +
            (formationDirection == FormationDirection.Horizontal ? halfSpan : 0f);
        if (float.IsInfinity(exitX) || exitX <= farthestX + 1f)
        {
            exitX = farthestX + Mathf.Max(1f, fallbackTravelDistance);
            Debug.LogWarning("SweepFormationSpawner: RightOuterBoundary has no usable " +
                "exit X; falling back to configured travel distance.", this);
        }

        int total = Mathf.Max(1, waveCount);
        for (int wave = 0; wave < total; wave++)
        {
            if (wave > 0 && waveIntervalSeconds > 0f)
                yield return new WaitForSeconds(waveIntervalSeconds);

            if (tileMapController == null || SystemManager.Instance == null ||
                SystemManager.Instance.gameMode != SystemManager.GameMode.Nomal)
                yield break;

            for (int i = 0; i < count; i++)
            {
                float offset = (i - (count - 1) * 0.5f) * spacing;
                Vector3 position = center +
                    (formationDirection == FormationDirection.Vertical
                        ? new Vector3(0f, -offset, 0f)
                        : new Vector3(offset, 0f, 0f));

                GameObject member = Instantiate(sweepSoldierPrefab, position, Quaternion.identity);
                SweepShieldRoute route = member.GetComponent<SweepShieldRoute>();
                if (route == null) route = member.AddComponent<SweepShieldRoute>();
                route.Initialize(position.y, exitX);
                spawnedMembers.Add(member);

                SystemManager manager = SystemManager.Instance;
                if (manager != null && !manager.AllEnemy.Contains(member))
                    manager.AllEnemy.Add(member);
            }
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
