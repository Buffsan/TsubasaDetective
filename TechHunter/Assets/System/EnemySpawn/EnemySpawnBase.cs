using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawnBase : MonoBehaviour
{
    public List<EnemyInfo> enemyList = new List<EnemyInfo>();
    

    [SerializeField] GameObject SpawnEffect;
    [SerializeField] SystemManager manager;
    public system_GameModeManager gameModeManager;

    public List<GameObject> SpawnList = new List<GameObject>();

    public List<EnemySpawnData> enemySpawnDatas = new List<EnemySpawnData>();

    public List<Vector3Int> tilePosition = new List<Vector3Int>();

    public List<GameObject> Map = new List<GameObject>();
    public List<GameObject> BigMap = new List<GameObject>();
    public List<BossStage> bossStages = new List<BossStage>();

    public Vector2 SpawnArea = new Vector2(7, 9);
    public Tilemap FloorTileMap;
    PlayerController playerController = PlayerController.Instance;

    float StartCount = 0;

    public TileMap_Controller Seve_TileMap_C;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.P)) 
        {
            isStartClone();
        }*/
    }
    private void FixedUpdate()
    {
        
    }
    public void isBossMapSpawn() 
    {
        foreach (BossStage stage in bossStages) 
        {
            if (stage.StageNumber == gameModeManager.SatgeNumber) 
            {
                GameObject CL_Map = Instantiate(stage.BossMap);
                Seve_TileMap_C = CL_Map.GetComponent<TileMap_Controller>();
            }
        }
    }
    public void isMapSpawn() 
    { 
    
        List<TileMap_Controller> map_sp = new List<TileMap_Controller>();

        if (gameModeManager.AllGameLevel >= 2)
        {
            foreach (GameObject mapprefab in BigMap)
            {

                TileMap_Controller tilemap_C = mapprefab.GetComponent<TileMap_Controller>();
                if (tilemap_C.stage == TileMap_Controller.Stage.Ferust)
                {
                    map_sp.Add(tilemap_C);
                }

            }
        }
        else
        {
            foreach (GameObject mapprefab in Map)
            {

                TileMap_Controller tilemap_C = mapprefab.GetComponent<TileMap_Controller>();
                if (tilemap_C.stage == TileMap_Controller.Stage.Ferust)
                {
                    map_sp.Add(tilemap_C);
                }

            }
        }

        TileMap_Controller randomTileMap_C = map_sp[Random.Range(0, map_sp.Count)];
        GameObject CL_Map = Instantiate(randomTileMap_C.gameObject);

         Seve_TileMap_C = CL_Map.GetComponent<TileMap_Controller>();


        
    }
    public void SpawnEnemy(EnemySpawnData enemySpawnDataVALL)
    {
        foreach (EnemySpawnGroup enemy in enemySpawnDataVALL.enemyspawnGroup)
        {
            int id = GetEnemyID(enemy.enemyType);
            isCloneEnemy(id, enemy.SpawnNumber);
        }
    }
    private static readonly Dictionary<EnemySpawnGroup.EnemyType, int> enemyIdMap = new()
    {
        { EnemySpawnGroup.EnemyType.CG_RightKnight, 5 },
        { EnemySpawnGroup.EnemyType.CG_Knight, 0 },
        { EnemySpawnGroup.EnemyType.CG_Sniper, 2 },
        { EnemySpawnGroup.EnemyType.CG_Spire, 3 },
        { EnemySpawnGroup.EnemyType.CG_shieldKnight, 1 },
        { EnemySpawnGroup.EnemyType.CG_Wizard, 4 },
        { EnemySpawnGroup.EnemyType.CG_Heavy_Knight, 9 },
        { EnemySpawnGroup.EnemyType.CG_Heavy_Sniper, 10 },
        { EnemySpawnGroup.EnemyType.CG_Heavy_Wizard, 3 },
        { EnemySpawnGroup.EnemyType.CG_Hanter, 7 },
        { EnemySpawnGroup.EnemyType.CG_StarWizard, 6 },
        { EnemySpawnGroup.EnemyType.MONSTAR_ErathSite, 8 },
    };

    public int GetEnemyID(EnemySpawnGroup.EnemyType type)
    {
        if (enemyIdMap.TryGetValue(type, out int id))
            return id;
        else
            return -1; // –¢’è‹`
    }
    public void isStartClone() 
    {
        FloorTileMap = Seve_TileMap_C.FloorMap;

        BoundsInt bounds = FloorTileMap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (FloorTileMap.HasTile(pos))
            {
                tilePosition.Add(pos);
            }
        }

        //playerController.cameraM.isChangeNumber(Seve_TileMap_C.MoveCamera);

        List<EnemySpawnData> level1Spawns = new List<EnemySpawnData>();

        foreach (var spawn in enemySpawnDatas)
        {
            if (spawn.StageLevel == gameModeManager.AllGameLevel)
            {
                level1Spawns.Add(spawn);
            }
        }

        EnemySpawnData randomSpawn = level1Spawns[Random.Range(0, level1Spawns.Count)];
        SpawnEnemy(randomSpawn);


        GameObject[] Enemys = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in Enemys) 
        {
            manager.AllEnemy.Add(enemy);
        }

    }
    public void isCloneEnemy(int IDvalue ,int EnemysNumber)
    {
        float waittime = 0;

        

        foreach (EnemyInfo enemy in enemyList) 
        {
            if (enemy.ID == IDvalue) 
            {

                for (int i = 0; i < EnemysNumber; i++)
                {
                    //Debug.Log(enemy.Name);

                    //WaitSpawnEnemy(enemy.Enemy,waittime);
                    StartCoroutine(WaitSpawnEnemy(enemy.Enemy, waittime));
                    
                    waittime += 0.4f;

                }
            break;
            }
        }

         

    }
    public IEnumerator WaitSpawnEnemy(GameObject EnemyObject, float WaitTime)
    {
        Debug.Log("Wait");
        yield return new WaitForSeconds(WaitTime);
        if (tilePosition.Count > 0)
        {

            Vector3Int randomTilePos = tilePosition[Random.Range(0, tilePosition.Count)];
            Vector3 worldPos = FloorTileMap.CellToWorld(randomTilePos) + FloorTileMap.tileAnchor;

            Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
            Vector3 spawnPotision = worldPos + randomOffset;

            GameObject CL_Enemy = Instantiate(EnemyObject, spawnPotision, Quaternion.identity);
            manager.AllEnemy.Add(CL_Enemy);
            Instantiate(SpawnEffect, spawnPotision, Quaternion.identity);
        }
    }
    public void SpawnEnemy(GameObject EnemyObject ) 
    {
        
        if (tilePosition.Count > 0) 
        {
            
        Vector3Int randomTilePos = tilePosition[Random.Range(0,tilePosition.Count)];
            Vector3 worldPos = FloorTileMap.CellToWorld(randomTilePos) + FloorTileMap.tileAnchor;

            Vector3 randomOffset = new Vector3(Random.Range(-0.5f,0.5f), Random.Range(-0.5f, 0.5f));
            Vector3 spawnPotision = worldPos + randomOffset;

            GameObject CL_Enemy = Instantiate(EnemyObject, spawnPotision, Quaternion.identity);
            Instantiate(SpawnEffect, spawnPotision, Quaternion.identity);
        }
    }

    public void GameStart() 
    { 
    
        

    }
}


[System.Serializable]
public class EnemyInfo 
{
    public int ID;
    public string Name;
    public GameObject Enemy;
}


[System.Serializable]
public class BossStage 
{
    public int StageNumber = 0;
    public GameObject BossMap;
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

}
