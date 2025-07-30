using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class AI_CGAstralObserverMage : EnemyMoveBase
{

    [SerializeField] GameObject Star;
    [SerializeField] GameObject StarEffect;
    public override void EnemyAttackPlay() 
    {
        enemyBase.AttackCount += Time.deltaTime;
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            enemyBase.rb.velocity = Vector3.zero;
            enemyBase.audioManager.isPlaySE(enemyBase.AttackStayAudio);
            enemyBase.animator.SetInteger("Anim", 2);
            enemyBase.mode = EnemyBase.ModeType.M2;

            GameObject CL_StartEffect = Instantiate(StarEffect,enemyBase.gameObject.transform.position,Quaternion.identity);
            Destroy(CL_StartEffect, 5);
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {

            if (enemyBase.AttackCount > 2)
            {
                enemyBase.animator.SetInteger("Anim", 3);
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;
                enemyBase.audioManager.isPlaySE(enemyBase.AttackAudio);

                for (int i = 0; i < 4; i++) 
                {
                    
                    GameObject CL_Star = Instantiate(Star,enemyBase.gameObject.transform.position,Quaternion.identity);
                    EnemyAttack enemyAttack = CL_Star.GetComponent<EnemyAttack>();

                    enemyAttack.enemyBase = enemyBase;
                    Rigidbody2D CL_rb = CL_Star.GetComponent<Rigidbody2D>();

                    float T_Angle = ((360 / 4) * i) * Mathf.Deg2Rad;

                    Vector3 T_velocity = new Vector3(Mathf.Cos(T_Angle), Mathf.Sin(T_Angle), 0) * 2.5f;
                    CL_rb.velocity = T_velocity;

                    Destroy(CL_Star,10);

                }
                
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {

            if (enemyBase.AttackCount > 2)
            {
                enemyBase.animator.SetInteger("Anim", 0);
                enemyBase.mode = EnemyBase.ModeType.M4;
                enemyBase.AttackCount = 0;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {

            if (enemyBase.AttackCount > 3)
            {
                enemyBase.animator.SetInteger("Anim", 0);
                enemyBase.mode = EnemyBase.ModeType.M1;
                enemyBase.AttackCount = 0;
                enemyBase.moveType = EnemyBase.MoveType.Move;
                enemyBase.PlayerDistance = 1000;
            }
        }
    }
}
