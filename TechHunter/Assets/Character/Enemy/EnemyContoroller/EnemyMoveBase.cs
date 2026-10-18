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

    // 個別AI向けのターゲット参照。PlayerOnlyではこれまでどおりプレイヤー。
    protected Transform CurrentTarget
    {
        get
        {
            EnemyContoroller controller = enemyBase as EnemyContoroller;
            if (controller != null)
            {
                if (controller.CurrentTarget != null) return controller.CurrentTarget;
                if (controller.TargetMode == EnemyContoroller.TargetType.PlayerOnly)
                    return enemyBase.Player != null ? enemyBase.Player.transform : null;
                return null;
            }
            Boss_Controller boss = enemyBase as Boss_Controller;
            if (boss != null) return boss.CurrentTarget;
            return enemyBase != null && enemyBase.Player != null
                ? enemyBase.Player.transform : null;
        }
    }

    protected Vector3 TargetPosition
    {
        get
        {
            Transform target = CurrentTarget;
            return target != null ? target.position :
                (enemyBase != null ? enemyBase.transform.position : Vector3.zero);
        }
    }


    // PlayerOnlyでは旧AIのPlayerDirection/PlayerDistanceの保存値をそのまま返す。
    // 既存の PlayerDistance = 1000 による攻撃抑制も維持する。
    protected bool UsesLegacyPlayerTarget
    {
        get
        {
            if (enemyBase == null) return true;
            EnemyContoroller controller = enemyBase as EnemyContoroller;
            if (controller != null)
                return controller.TargetMode == EnemyContoroller.TargetType.PlayerOnly;
            Boss_Controller boss = enemyBase as Boss_Controller;
            if (boss != null)
                return boss.TargetMode == EnemyContoroller.TargetType.PlayerOnly;
            return true;
        }
    }

    protected Vector2 TargetDirection
    {
        get
        {
            if (enemyBase == null) return Vector2.zero;
            if (UsesLegacyPlayerTarget) return enemyBase.PlayerDirection;
            return CurrentTarget != null
                ? (Vector2)TargetPosition - (Vector2)enemyBase.transform.position : Vector2.zero;
        }
    }

    protected float TargetDistance
    {
        get
        {
            if (enemyBase == null) return float.PositiveInfinity;
            if (UsesLegacyPlayerTarget) return enemyBase.PlayerDistance;
            return CurrentTarget != null
                ? Vector2.Distance(TargetPosition, enemyBase.transform.position)
                : float.PositiveInfinity;
        }
    }

    protected void RefreshTargetInfo()
    {
        if (enemyBase == null || CurrentTarget == null) return;
        if (UsesLegacyPlayerTarget)
        {
            Vector2 direction = (Vector2)TargetPosition - (Vector2)enemyBase.transform.position;
            enemyBase.PlayerDirection = direction;
            enemyBase.PlayerDistance = direction.magnitude;
        }
    }

    protected void LookAtCurrentTarget()
    {
        if (enemyBase == null || CurrentTarget == null) return;
        RefreshTargetInfo();
        enemyBase.isTargetLook(TargetPosition);
    }

    // warningType: 0=四角 / 1=円 / 2=半円。
    // 共通リスト未設定なら既存Prefabへフォールバックし、旧敵の表示を維持する。
    public GameObject SpawnAttackWarning(int warningType, GameObject fallbackPrefab,
        Vector3 position, Quaternion rotation, float duration, Transform parent = null,
        float extraLifetime = 0.2f, bool syncAnimation = true,
        Vector3? localScale = null)
    {
        EnemyContoroller controller = enemyBase as EnemyContoroller;
        if (controller != null)
        {
            if (controller.HasAttackWarningPrefab(warningType))
                return controller.SpawnAttackWarning(warningType, position, rotation,
                    duration, parent, extraLifetime, syncAnimation, localScale);

            return controller.SpawnAttackWarningPrefab(fallbackPrefab, position, rotation,
                duration, parent, extraLifetime, syncAnimation, localScale);
        }

        // EnemyContoroller以外の旧AIも従来どおり生成可能。
        if (fallbackPrefab == null) return null;
        GameObject instance = Instantiate(fallbackPrefab, position, rotation, parent);
        if (localScale.HasValue) instance.transform.localScale = localScale.Value;
        if (syncAnimation && duration > 0f)
        {
            SaveComponent save = instance.GetComponentInChildren<SaveComponent>(true);
            if (save != null && save.animator != null)
                save.animator.speed = 1f / duration;
        }
        Destroy(instance, Mathf.Max(0.01f, duration + extraLifetime));
        return instance;
    }

    // 既存のEnemyMoveBase.DengerAreaSpawn呼び出しを残す。
    public GameObject DengerAreaSpawn(GameObject Effect, Vector2 Pos, float AnimationSpeed)
    {
        GameObject warning = SpawnAttackWarning(EnemyContoroller.WarningRectangle,
            Effect, Pos, Quaternion.identity, AnimationSpeed);
        if (warning != null) Clone_DengerObjects.Add(warning);
        return warning;
    }

    // 新しい敵はPrefabを指定せず、共通リストの警告タイプだけで呼べる。
    public GameObject DengerAreaSpawn(int warningType, Vector2 pos, float duration)
    {
        GameObject warning = SpawnAttackWarning(warningType, null,
            pos, Quaternion.identity, duration);
        if (warning != null) Clone_DengerObjects.Add(warning);
        return warning;
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
    // EnemyOnlyで攻撃対象がいなくなった際、1回だけ呼ばれる。
    public virtual void EnemyFriendlyEnter()
    {
    }

    // EnemyOnlyで攻撃対象がいない間、毎FixedUpdate呼ばれる。
    public virtual void EnemyFriendlyPlay()
    {
    }

    // 敵対勢力が現れ、通常戦闘に戻る際、1回だけ呼ばれる。
    public virtual void EnemyFriendlyExit()
    {
    }

    public virtual void EnemyConfusionPlay() 
    { 
    
    }
    public virtual void EnemyStanPlay()
    { 
    
    }
}
