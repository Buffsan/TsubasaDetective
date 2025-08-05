using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class Boss_MoveBase : MonoBehaviour
{
    public string EnemyName;
    public EnemyBase enemyBase;

    public int AttackNumber = 0;
    public float AttackWaitTime = 0;
    public int AllAttackNumber = 0;

   public int AttackPhase =0;

    public float SaveVolume;

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
