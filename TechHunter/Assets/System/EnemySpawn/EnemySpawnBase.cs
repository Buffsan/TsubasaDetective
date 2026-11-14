using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawnBase : MonoBehaviour
{
    public List<EnemyInfo> enemyList = new List<EnemyInfo>();
    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;
    

    [SerializeField] GameObject SpawnEffect;
    [SerializeField] SystemManager manager;
    public system_GameModeManager gameModeManager;
    AudioManager audioManager => AudioManager.instance;

    public List<GameObject> SpawnList = new List<GameObject>();

    public GameObject ShopStage;
    public List<Stages> stages = new List<Stages>();
    Stages Save_Stages = null;
    
    public List<Vector3Int> tilePosition = new List<Vector3Int>();
    public List<BossStage> bossStages = new List<BossStage>();

    public Vector2 SpawnArea = new Vector2(7, 9);
    public Tilemap FloorTileMap;
    PlayerController playerController = PlayerController.Instance;

    float StartCount = 0;
    public int PendingSpawnCount { get; private set; }

    public TileMap_Controller Seve_TileMap_C;

   
    public void isBossMapSpawn() 
    {
        foreach (Stages stage in stages)
        {
            if (stage.stage.ToString() == ALL_System.systemManager.stage.ToString() && stage.NowStage == manager.stageNumber)
            {
                Save_Stages = stage;
                break;
            }
        }

        foreach (BossStage stage in Save_Stages.bossStages) 
        {
            if (stage.StageNumber == gameModeManager.SatgeNumber) 
            {
                GameObject CL_Map = Instantiate(stage.BossMap);
                Seve_TileMap_C = CL_Map.GetComponent<TileMap_Controller>();

                if (Seve_TileMap_C.BGM != audioManager.BgmSource.clip)
                {
                    audioManager.BgmSource.clip = Seve_TileMap_C.BGM;
                    audioManager.BgmSource.Play();
                }
            }
        }
    }
    public void isShopMapSpawn() 
    {
        GameObject CL_Map = Instantiate(ShopStage);
        Seve_TileMap_C = CL_Map.GetComponent<TileMap_Controller>();

        if (Seve_TileMap_C.BGM != audioManager.BgmSource.clip)
        {
            audioManager.BgmSource.clip = Seve_TileMap_C.BGM;
            audioManager.BgmSource.Play();
        }
    }
    public void isMapSpawn() 
    { 
    
        List<TileMap_Controller> map_sp = new List<TileMap_Controller>();
        foreach (Stages stage in stages) 
        {
            if (stage.stage.ToString() == ALL_System.systemManager.stage.ToString() && stage.NowStage == manager.stageNumber) 
            {
                Save_Stages = stage;
                break;
            }
        }

        if (Save_Stages == null)
        {
            Debug.LogError("No stage configuration found for " + manager.stage, this);
            return;
        }
        List<GameObject> eligibleMaps = gameModeManager.AllGameLevel >= 2
            ? Save_Stages.BigMap : Save_Stages.Map;

        foreach (GameObject mapPrefab in eligibleMaps)
        {
            if (mapPrefab == null) continue;
            TileMap_Controller map = mapPrefab.GetComponent<TileMap_Controller>();
            if (map != null && map.stage.ToString() == manager.stage.ToString())
                map_sp.Add(map);
        }
        // Allow older maps that still use the default Ferust stage tag.
        if (map_sp.Count == 0)
        {
            foreach (GameObject mapPrefab in eligibleMaps)
            {
                if (mapPrefab == null) continue;
                TileMap_Controller map = mapPrefab.GetComponent<TileMap_Controller>();
                if (map != null) map_sp.Add(map);
            }
        }
        if (map_sp.Count == 0)
        {
            Debug.LogError("No usable map prefabs for current stage.", this);
            return;
        }

        TileMap_Controller randomTileMap_C = map_sp[Random.Range(0, map_sp.Count)];
        ALL_System.camera_Controller.MoveCamera = new Vector3(randomTileMap_C.MoveCamera.x,30,-10);
        GameObject CL_Map = Instantiate(randomTileMap_C.gameObject);

         Seve_TileMap_C = CL_Map.GetComponent<TileMap_Controller>();
        if (Seve_TileMap_C.BGM != audioManager.BgmSource.clip) 
        {
            audioManager.BgmSource.clip = Seve_TileMap_C.BGM;
            audioManager.BgmSource.Play();
        }

        
    }
    public void SpawnEnemy(EnemySpawnData enemySpawnDataVALL)
    {
        if (enemySpawnDataVALL == null) return;

        // 通常敵の設定・出現方法は変更しない。
        if (enemySpawnDataVALL.enemyspawnGroup != null)
        {
            foreach (EnemySpawnGroup enemy in enemySpawnDataVALL.enemyspawnGroup)
            {
                if (enemy == null || enemy.SpawnNumber <= 0) continue;
                isCloneEnemy(GetEnemyID(enemy.enemyType), enemy.SpawnNumber);
            }
        }

        // EnemySpawnDataの新しい味方リスト。既存のEnemyTypeを共有する。
        if (enemySpawnDataVALL.allyspawnGroup != null)
        {
            foreach (EnemySpawnGroup ally in enemySpawnDataVALL.allyspawnGroup)
            {
                if (ally == null || ally.SpawnNumber <= 0) continue;
                isCloneAlly(GetEnemyID(ally.enemyType), ally.SpawnNumber);
            }
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
        { EnemySpawnGroup.EnemyType.CG_CosmicSurveyor, 11 },
        { EnemySpawnGroup.EnemyType.TERUSETO_TGST_Eaterofleftovers,12},
        { EnemySpawnGroup.EnemyType.TERUSETO_TGST_FoodServer,13},
        { EnemySpawnGroup.EnemyType.TERUSETO_TGST_ARAKAZEFoodServer,14},
        { EnemySpawnGroup.EnemyType.TERUSETO_FishHanter_Shoygun,15},
        { EnemySpawnGroup.EnemyType.MONSTAR_LastHoliday,16},
        { EnemySpawnGroup.EnemyType.TERUSETO_FP_walker,17},
        { EnemySpawnGroup.EnemyType.TERUSETO_KINGYO_Koaka,18},
        { EnemySpawnGroup.EnemyType.TERUSETO_KINGYO_LeaderKoaka,19},
        { EnemySpawnGroup.EnemyType.TERUSETO_FishHanter_Shield,20},
        { EnemySpawnGroup.EnemyType.TERUSETO_FishHanter_maquette,21},
        { EnemySpawnGroup.EnemyType.TERUSETO_KINGYO_Wakin,22},
        { EnemySpawnGroup.EnemyType.SHUELA_SS_StarMagician,23},
        { EnemySpawnGroup.EnemyType.SHUELA_SS_StarKnight,24},
        { EnemySpawnGroup.EnemyType.SHUELA_SS_FairySpear,25},
        { EnemySpawnGroup.EnemyType.SHUELA_SS_FairyAXE,26},
        { EnemySpawnGroup.EnemyType.SHUELA_SS_FairyHammer,27},
        { EnemySpawnGroup.EnemyType.SHUELA_SS_FairyMagic,28},
        { EnemySpawnGroup.EnemyType.GRATOHTO_SB_Sardine,29},
        { EnemySpawnGroup.EnemyType.GRATOHTO_SB_MiniSardine,30},
        { EnemySpawnGroup.EnemyType.GRATOHTO_SB_Squid,31},
        { EnemySpawnGroup.EnemyType.GRATOHTO_SB_Crab,32},
        { EnemySpawnGroup.EnemyType.GRATOHTO_SB_HermitCrab,33},
        { EnemySpawnGroup.EnemyType.GRATOHTO_SB_Krill,34},
        { EnemySpawnGroup.EnemyType.GRATOHTO_SB_Scallop,35},
        { EnemySpawnGroup.EnemyType.PASTAL_DE_BigSword,36},
        { EnemySpawnGroup.EnemyType.SHUELA_SS_GoldenSlug,37},
        { EnemySpawnGroup.EnemyType.CG_FarScout,38},
        { EnemySpawnGroup.EnemyType.CG_Heavy_Shield,39},
        { EnemySpawnGroup.EnemyType.CG_Tank,40},
        { EnemySpawnGroup.EnemyType.CG_Crrier,41},
        { EnemySpawnGroup.EnemyType.CG_CaptainKnight,42},
        { EnemySpawnGroup.EnemyType.TERUSETO_KINGYO_LeaderWakin,43},
        { EnemySpawnGroup.EnemyType.TERUSETO_KINGYO_OORanchu,44},
        { EnemySpawnGroup.EnemyType.TERUSETO_KINGYO_Ranchu,45},
        { EnemySpawnGroup.EnemyType.TERUSETO_KINGYO_Tanchou,46},
        { EnemySpawnGroup.EnemyType.TERUSETO_KINGYO_Zikin,47},
        { EnemySpawnGroup.EnemyType.SHUELA_SS_FairyChief,48},
        { EnemySpawnGroup.EnemyType.SHUELA_SS_FairySpellBlade,49},
        { EnemySpawnGroup.EnemyType.SHUELA_SS_MagicAcademyApprentice,50},
        { EnemySpawnGroup.EnemyType.PASTAL_SE_MountainRangeTrapezoid,51},
        { EnemySpawnGroup.EnemyType.PASTAL_SE_ArmorTriangle,52},
        { EnemySpawnGroup.EnemyType.PASTAL_SE_Conductor,53},
        { EnemySpawnGroup.EnemyType.PASTAL_SE_GrowthSector,54},
        { EnemySpawnGroup.EnemyType.DIANA_LunarPurgeSquad_Mace,55},


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

        foreach (var spawn in Save_Stages.enemySpawnDatas)
        {
            if (spawn.StageLevel == gameModeManager.AllGameLevel)
            {
                level1Spawns.Add(spawn);
            }
        }

        if (level1Spawns.Count == 0)
        {
            Debug.LogError("No enemy spawn waves configured for level " + gameModeManager.AllGameLevel, this);
            return;
        }
        EnemySpawnData randomSpawn = level1Spawns[Random.Range(0, level1Spawns.Count)];
        SpawnEnemy(randomSpawn);


        GameObject[] Enemys = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in Enemys) 
        {
            if (!manager.AllEnemy.Contains(enemy))
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
                    PendingSpawnCount++;
                     StartCoroutine(WaitSpawnEnemy(enemy.Enemy, waittime));
                    
                    waittime += 0.4f;

                }
            break;
            }
        }

         

    }
    private void isCloneAlly(int id, int count)
    {
        if (count <= 0) return;
        EnemyInfo info = enemyList.Find(entry =>
            entry != null && entry.ID == id && entry.Enemy != null);
        if (info == null)
        {
            Debug.LogWarning("EnemySpawnBase: 味方召喚の敵IDが見つかりません: " + id, this);
            return;
        }

        // 非同期の召喚中にステージが変わった場合は、古い味方を生成しない。
        TileMap_Controller sourceStage = Seve_TileMap_C;
        float delay = 0f;
        for (int i = 0; i < count; i++)
        {
            PendingSpawnCount++;
            StartCoroutine(WaitSpawnAlly(info.Enemy, delay, sourceStage));
            delay += 0.4f;
        }
    }

    private IEnumerator WaitSpawnAlly(GameObject prefab, float delay, TileMap_Controller sourceStage)
    {
        yield return new WaitForSeconds(delay);
        PendingSpawnCount = Mathf.Max(0, PendingSpawnCount - 1);

        if (sourceStage == null || sourceStage != Seve_TileMap_C || prefab == null)
            yield break;
        if (prefab.GetComponent<EnemyBase>() == null)
        {
            Debug.LogWarning("EnemySpawnBase: 味方PrefabにEnemyBaseがありません: " + prefab.name, prefab);
            yield break;
        }
        if (!TryFindAllySpawnPosition(sourceStage, out Vector3 position))
        {
            Debug.LogWarning("EnemySpawnBase: 味方が出現できる床タイルがありません。", sourceStage);
            yield break;
        }

        GameObject clone = Instantiate(prefab, position, Quaternion.identity);
        EnemyBase ally = clone.GetComponent<EnemyBase>();
        if (ally == null)
        {
            Destroy(clone);
            yield break;
        }

        // 既存の魅了ロジックで味方の派閥・索敵・攻撃・破棄時の管理を流用。
        // BecomeCharmedAlly()はコインを落とす処理も呼ぶため、先に報酬を無効化する。
        ally.GOLD = 0;
        ally.XP = 0;
        ally.BecomeCharmedAlly();
        if (!ally.IsCharmed)
        {
            Debug.LogWarning("EnemySpawnBase: 味方の初期化に失敗しました: " + prefab.name, clone);
            Destroy(clone);
            yield break;
        }

        // 味方はAllEnemyへ登録しない。BecomeCharmedAlly()がCharmedAlliesへ登録する。
        if (SpawnEffect != null)
            Instantiate(SpawnEffect, position, Quaternion.identity);
        if (manager != null)
            manager.WaitEnemyDieCount = 0f;
    }

    // 味方出現時だけステージのboolを参照。
    // 専用タイルが未設定/空の場合はStandFloor、さらに無い場合はFloorMap。
    private static bool TryFindAllySpawnPosition(TileMap_Controller stage, out Vector3 position)
    {
        position = Vector3.zero;
        if (stage.UseAllySpawnMap && TryGetRandomTileCenter(stage.AllySpawnMap, out position))
            return true;
        if (TryGetRandomTileCenter(stage.StandFloor, out position))
            return true;
        return TryGetRandomTileCenter(stage.FloorMap, out position);
    }

    private static bool TryGetRandomTileCenter(Tilemap map, out Vector3 position)
    {
        position = Vector3.zero;
        if (map == null) return false;

        // すべての有効タイルから均等に1つ選ぶ。リストの一時確保も不要。
        int count = 0;
        foreach (Vector3Int cell in map.cellBounds.allPositionsWithin)
        {
            if (!map.HasTile(cell)) continue;
            count++;
            if (Random.Range(0, count) == 0)
                position = map.GetCellCenterWorld(cell);
        }
        return count > 0;
    }

    public IEnumerator WaitSpawnEnemy(GameObject EnemyObject, float WaitTime)
    {
        //Debug.Log("Wait");
        yield return new WaitForSeconds(WaitTime);
        PendingSpawnCount = Mathf.Max(0, PendingSpawnCount - 1);
        manager.WaitEnemyDieCount = 0;
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
public class Stages
{
    public int NowStage = 0;
    public enum Stage
    {

        Ferust,
        Diana,
        Garewo,
        Kardia,
        Pastal,
        Teruseto,
        Shuela,
        Gratohto,
        LAST

    }
    public Stage stage = Stage.Ferust;
    public List<EnemySpawnData> enemySpawnDatas = new List<EnemySpawnData>();
    public List<GameObject> Map = new List<GameObject>();
    public List<GameObject> BigMap = new List<GameObject>();
    public List<BossStage> bossStages = new List<BossStage>();
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
        Shuela,
        Gratohto,
        LAST

    }
    public Stage stage = Stage.Ferust;

}
