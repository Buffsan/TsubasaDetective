using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class Boss_MoveBase : MonoBehaviour
{
    public string EnemyName;
    public EnemyBase enemyBase;

    // Boss_Controller側のCurrentTargetを参照する。従来のボスはPlayerOnly。
    protected Transform CurrentTarget
    {
        get
        {
            Boss_Controller controller = enemyBase as Boss_Controller;
            if (controller != null)
            {
                if (controller.CurrentTarget != null) return controller.CurrentTarget;
                if (controller.TargetMode == EnemyContoroller.TargetType.PlayerOnly)
                    return enemyBase.Player != null ? enemyBase.Player.transform : null;
                return null;
            }
            return enemyBase != null && enemyBase.Player != null
                ? enemyBase.Player.transform : null;
        }
    }

    // 置換前のTransform.positionと同様、Vector3を返す。
    protected Vector3 TargetPosition =>
        CurrentTarget != null ? CurrentTarget.position :
        (enemyBase != null ? enemyBase.transform.position : Vector3.zero);


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


    ALL_SystemManager aLL_System =>ALL_SystemManager.Instance;
    public List<EnemyContoroller> enemyList = new List<EnemyContoroller>();
    public List<GameObject> AllEnemy = new List<GameObject>();
    public List<GameObject> DengerObjects = new List<GameObject>();
    public List<GameObject> AttackObjects = new List<GameObject>();
    [HideInInspector]public List<GameObject> Clone_DengerObjects = new List<GameObject>();
    public ALL_SystemManager all_System => ALL_SystemManager.Instance;

    public int AttackNumber = 0;
    public float AttackWaitTime = 0;
    public int AllAttackNumber = 0;

   public int AttackPhase =0;

    public float SaveVolume;
    private void Start()
    {
        AttackWaitTime = 9999;
    }
    public enum Phase 
    { 
    
        P1,P2,P3,P4,P5,P6,P7,P8,P9,P10,P11,P12,P13,P14,P15,P16,P17,P18,P19,P20,P21,P22,P23,P24,P25,P26,P27,P28,P29,
    
    }
    public Phase phase = Phase.P1;

    public virtual void M_BaseMove() 
    { 
    
    }

    public virtual void M_Attack() 
    { 
    
    }

    public virtual void M_AI() 
    { 
    
    }
    public virtual void M_Stan()
    {

    }
    public virtual void M_Con()
    {

    }
    public virtual void M_Die()
    {

    }
    public void AllDestroyDengerArea() 
    {
        if (Clone_DengerObjects.Count == 0) return;
        foreach (var denger in Clone_DengerObjects) 
        {
            Destroy(denger);
        }
        Clone_DengerObjects.Clear();
    }
    public GameObject EnemySpawn(GameObject enemyprefab, Vector2 SpawnPos) 
    {
        
        GameObject CL_Enemy  = Instantiate(enemyprefab,SpawnPos,Quaternion.identity);
        AllEnemy.Add(CL_Enemy);
        aLL_System.systemManager.AllEnemy.Add(CL_Enemy);
        return CL_Enemy;
    }
    public void SpawnObjectNow(GameObject SpawnObject, Vector2 Target, float DestroyTime)
    {
        GameObject CL_Object = Instantiate(SpawnObject, Target, Quaternion.identity);
        Destroy(CL_Object, DestroyTime);
    }
    public GameObject SpawnObjectNow_Input(GameObject SpawnObject, Vector2 Target, float DestroyTime)
    {
        GameObject CL_Object = Instantiate(SpawnObject, Target, Quaternion.identity);
        Destroy(CL_Object, DestroyTime);
        return CL_Object;
    }
    public GameObject DengerAreaSpawn(GameObject Effect,Vector2 Pos,float AnimationSpeed) 
    {

        GameObject CL_dengerArea = Instantiate(Effect,Pos,Quaternion.identity);
        Clone_DengerObjects.Add(CL_dengerArea);
        SaveComponent saveComponent = CL_dengerArea.GetComponent<SaveComponent>();
        saveComponent.animator.speed = 1.0f / AnimationSpeed;
        Destroy(CL_dengerArea, AnimationSpeed + 0.2f);
        return CL_dengerArea;
    }

    public void AttackFinish() 
    {

        enemyBase.mode = EnemyBase.ModeType.M1;
        enemyBase.moveType = EnemyBase.MoveType.Move;
        AttackNumber = 0;
        AttackPhase = 0;
    }
    public void Next() 
    {
        //AttackNumber = 0;
        AttackPhase++;
    }
}
