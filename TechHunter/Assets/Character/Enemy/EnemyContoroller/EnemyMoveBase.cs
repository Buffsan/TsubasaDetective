using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyMoveBase : MonoBehaviour
{
    public string EnemyName;
    public EnemyBase enemyBase;

    public int A_Phase =0;
    public int attacktype = 0;
    public float AttackCount = 0;
    [HideInInspector] public List<GameObject> Clone_DengerObjects = new List<GameObject>();

    public AudioManager AUDIO_manager => AudioManager.instance;
    public GameObject DengerAreaSpawn(GameObject Effect, Vector2 Pos, float AnimationSpeed)
    {

        GameObject CL_dengerArea = Instantiate(Effect, Pos, Quaternion.identity);
        Clone_DengerObjects.Add(CL_dengerArea);
        SaveComponent saveComponent = CL_dengerArea.GetComponent<SaveComponent>();
        saveComponent.animator.speed = 1.0f / AnimationSpeed;
        Destroy(CL_dengerArea, AnimationSpeed + 0.2f);
        return CL_dengerArea;
    }
    public void SpawnObjectNow(GameObject SpawnObject , Vector2 Target , float DestroyTime) 
    { 
        GameObject CL_Object = Instantiate(SpawnObject,Target,Quaternion.identity);
        Destroy(CL_Object,DestroyTime);
    }
    public GameObject SpawnObjectNow_Input(GameObject SpawnObject, Vector2 Target, float DestroyTime)
    {
        GameObject CL_Object = Instantiate(SpawnObject, Target, Quaternion.identity);
        Destroy(CL_Object, DestroyTime);
        return CL_Object;
    }
    public GameObject SpawnEnemySpawn(
    GameObject enemy,
    Vector2 targetPos,
    Quaternion rotate,
    int tileDistance)
    {
        Tilemap tilemap =
            ALL_SystemManager.Instance.enemySpawnBASE.Seve_TileMap_C.StandFloor;

        // 中心セル
        Vector3Int centerCell = tilemap.WorldToCell(targetPos);

        // ランダム角度
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        int dx = Mathf.RoundToInt(Mathf.Cos(angle) * tileDistance);
        int dy = Mathf.RoundToInt(Mathf.Sin(angle) * tileDistance);

        Vector3Int spawnCell =
            centerCell + new Vector3Int(dx, dy, 0);

        // フォールバック：中心セル
        if (!tilemap.HasTile(spawnCell))
        {
            spawnCell = centerCell;
        }

        Vector3 spawnWorldPos =
            tilemap.CellToWorld(spawnCell) + tilemap.tileAnchor;

        GameObject enemyObj =
            Instantiate(enemy, spawnWorldPos, rotate);

        enemyObj.transform.parent =
            ALL_SystemManager.Instance.enemySpawnBASE
                .Seve_TileMap_C.EnemysObject.transform;

        ALL_SystemManager.Instance.systemManager
            .AllEnemy.Add(enemyObj);

        return enemyObj;
    }
    public GameObject SpawnNoListEnemy(GameObject enemy,Vector3 targetPos,Quaternion rotate) 
    {

        GameObject CL_enemy = Instantiate(enemy, targetPos, rotate);
        CL_enemy.transform.parent = ALL_SystemManager.Instance.enemySpawnBASE.Seve_TileMap_C.EnemysObject.transform;
        return CL_enemy;
    }
    public void AttackNext() 
    {
        AttackCount = 0;      
        A_Phase++;         
    }
    public void AttackFinish() 
    { 
        A_Phase = 0;
        enemyBase.moveType = EnemyBase.MoveType.Move;
        enemyBase.AttackCount = 0;
        AttackCount = 0;
    }
    public virtual void SpecialPlay()
    {

    }

    public virtual void EnemyAttackPlay() 
    { 
    
    }
    public virtual void EnemyMovePlay()
    {

    }
    public virtual void EnemyConfusionPlay() 
    { 
    
    }
    public virtual void EnemyStanPlay()
    { 
    
    }
}
