using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SS_StarMagician_AI : EnemyMoveBase
{

    [SerializeField] GameObject Star;
    [SerializeField] GameObject StarEffect;

    int AttackNumber = 0;
    public override void EnemyAttackPlay()
    {
        enemyBase.AttackCount += Time.deltaTime;
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            enemyBase.rb.velocity = Vector3.zero;
            enemyBase.audioManager.PlaySE(enemyBase.AttackStayAudio);
            enemyBase.animator.SetInteger("Anim", 2);
            enemyBase.mode = EnemyBase.ModeType.M2;

            GameObject CL_StartEffect = Instantiate(StarEffect, enemyBase.gameObject.transform.position, Quaternion.identity);
            Destroy(CL_StartEffect, 5);
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {

            if (enemyBase.AttackCount > 1.5)
            {
                enemyBase.animator.SetInteger("Anim", 3);
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;
                enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3) 
        {
            enemyBase.mode = EnemyBase.ModeType.M4;
            enemyBase.AttackCount = 0;
            for (int i = 0; i < 8; i++)
            {

                GameObject CL_Star = Instantiate(Star, enemyBase.gameObject.transform.position, Quaternion.identity);
                EnemyAttack enemyAttack = CL_Star.GetComponent<EnemyAttack>();

                enemyAttack.enemyBase = enemyBase;
                Rigidbody2D CL_rb = CL_Star.GetComponent<Rigidbody2D>();

                float T_Angle = ((360 / 8) * i) * Mathf.Deg2Rad + AttackNumber * 20;

                Vector3 T_velocity = new Vector3(Mathf.Cos(T_Angle), Mathf.Sin(T_Angle), 0) * 2.5f;
                CL_rb.velocity = T_velocity;

                Destroy(CL_Star, 10);

            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {

            if (enemyBase.AttackCount > 0.4+AttackNumber/15)
            {
                enemyBase.animator.SetInteger("Anim", 0);
                enemyBase.AttackCount = 0;

                if (AttackNumber > 5)
                {
                    enemyBase.mode = EnemyBase.ModeType.M5;
                    AttackNumber = 0;
                }
                else 
                {
                    enemyBase.mode = EnemyBase.ModeType.M3;
                    AttackNumber++;
                }
                
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M5)
        {

            if (enemyBase.AttackCount > 8)
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
