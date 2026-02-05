using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class Boss_MoveBase : MonoBehaviour
{
    public string EnemyName;
    public EnemyBase enemyBase;

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
