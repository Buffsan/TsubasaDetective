using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMoveBase : MonoBehaviour
{
    public string EnemyName;
    public EnemyBase enemyBase;

    public int A_Phase =0;
    public int attacktype = 0;
    public float AttackCount = 0;

    public AudioManager AUDIO_manager => AudioManager.instance;
    
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
