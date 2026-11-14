using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMap_Controller : MonoBehaviour
{
    PlayerController controller => PlayerController.Instance;

    public AudioClip BGM;

    public Vector2 MoveCamera = new Vector2(8,6);

    public Tilemap FloorMap;
    public Tilemap StandFloor;

    [Header("侵入不可エリア / 掃討部隊")]
    public Tilemap LeftOuterBoundary;
    public Tilemap RightOuterBoundary;
    public Tilemap SweepSpawnMap;

    [Header("味方の召喚専用タイル（任意）")]
    [SerializeField] Tilemap allySpawnMap;
    public Tilemap AllySpawnMap => allySpawnMap;

    // Prefer painted ally spawn tiles. Choose a nearby cell when available;
    // otherwise choose any marker. No tilemap or no painted cells means fallback.
    public bool TryGetAllySpawnPosition(Vector2 center, int searchDistance, out Vector3 result)
    {
        result = Vector3.zero;
        if (allySpawnMap == null) return false;

        List<Vector3Int> candidates = new List<Vector3Int>();
        List<Vector3Int> nearby = new List<Vector3Int>();
        float radius = Mathf.Max(0, searchDistance);
        float limitSqr = radius * radius;
        foreach (Vector3Int cell in allySpawnMap.cellBounds.allPositionsWithin)
        {
            if (!allySpawnMap.HasTile(cell)) continue;
            candidates.Add(cell);
            Vector2 world = allySpawnMap.GetCellCenterWorld(cell);
            if ((world - center).sqrMagnitude <= limitSqr)
                nearby.Add(cell);
        }
        if (candidates.Count == 0) return false;
        List<Vector3Int> pool = nearby.Count > 0 ? nearby : candidates;
        result = allySpawnMap.GetCellCenterWorld(pool[Random.Range(0, pool.Count)]);
        return true;
    }

    public GameObject StartPoint;
    public GameObject Goal;
    public GameObject EnemysObject;
    public enum Stage
    {

        Ferust,
        Diana,
        Garewo,
        Kardia,
        Pastal,
        Teruseto,
        Shuela

    }
    public Stage stage = Stage.Ferust;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
