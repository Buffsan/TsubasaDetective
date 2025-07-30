using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CG_HeavyKnight_AI : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;

    int Phase = 0;

    float PhaseCount = 0;
    float PhaseCount1 = 0;
    Vector2 SaveAngle = Vector2.zero;
    Vector2 SavePosPhase = Vector2.zero;

    float DistanceCount = 0;

    int AttackType = 0;

    bool Dash = false;
    public override void EnemyMovePlay()
    {
        //enemyBase.PlayerDistance = Vector2.Distance(enemyBase.Player.transform.position, enemyBase.gameObject.transform.position);
        enemyBase.isPlayerLookBase();
        if (Dash)
        {
            enemyBase.animator.SetInteger("Anim", 4);
            enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * 3f;

        }
        else 
        {
            AttackType = 0;
            enemyBase.animator.SetInteger("Anim",1);
            
            enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * enemyBase.SPEED;
        }

        if (enemyBase.PlayerDistance > 1.7)
        {
            DistanceCount += Time.deltaTime;
            if (DistanceCount > 2)
            {
                Dash = true;
            }
            if (DistanceCount > 5) 
            { 
            AttackType = 1;
                enemyBase.moveType = EnemyBase.MoveType.Attack;
            }
        }
        else
        {
            DistanceCount = 0;
        }


    }
    public override void EnemyAttackPlay()
    {
        enemyBase.AttackCount += Time.deltaTime;
        Dash = false;
        DistanceCount = 0;
        if (AttackType == 0)
        {
            if (enemyBase.mode == EnemyBase.ModeType.M1)
            {
                
                enemyBase.audioManager.isPlaySE(enemyBase.AttackStayAudio);
                enemyBase.rb.velocity = Vector3.zero;
                enemyBase.mode = EnemyBase.ModeType.M2;
                enemyBase.animator.SetInteger("Anim", 2);
            }
            if (enemyBase.mode == EnemyBase.ModeType.M2)
            {
                enemyBase.isPlayerLookBase();
                if (enemyBase.AttackCount > 0.2)
                {
                    enemyBase.mode = EnemyBase.ModeType.M3;
                    enemyBase.AttackCount = 0;
                }
            }
            if (enemyBase.mode == EnemyBase.ModeType.M3)
            {
                enemyBase.isPlayerLookBase();
                enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * 1.6f;
                if (enemyBase.AttackCount > 1f)
                {
                    enemyBase.mode = EnemyBase.ModeType.M4;
                    enemyBase.AttackCount = 0;
                }
            }
            if (enemyBase.mode == EnemyBase.ModeType.M4)
            {
                enemyBase.rb.velocity = new Vector2(enemyBase.rb.velocity.x * 0.5f, enemyBase.rb.velocity.y * 0.5f);
                if (enemyBase.AttackCount > 0.4)
                {
                    enemyBase.mode = EnemyBase.ModeType.M5;
                    enemyBase.AttackCount = 0;
                }
            }
            if (enemyBase.mode == EnemyBase.ModeType.M5)
            {
                GameObject CL_Attack = Instantiate(enemyBase.AttackArea, enemyBase.AttackSpawnPoint.transform.position, Quaternion.identity);
                BoxCollider2D CL_Box = CL_Attack.GetComponent<BoxCollider2D>();
                CL_Box.size = enemyBase.AttackAreaSize;

                EnemyAttack enemyAttack = CL_Attack.GetComponent<EnemyAttack>();
                enemyAttack.enemyBase = enemyBase;
                Destroy(CL_Attack, 0.02f);

                enemyBase.audioManager.isPlaySE(enemyBase.AttackAudio);
                enemyBase.animator.SetInteger("Anim", 3);
                enemyBase.mode = EnemyBase.ModeType.M6;

            }
            if (enemyBase.mode == EnemyBase.ModeType.M6)
            {

                if (enemyBase.AttackCount > 1)
                {
                    enemyBase.mode = EnemyBase.ModeType.M1;
                    enemyBase.AttackCount = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Move;
                    enemyBase.PlayerDistance = 1000;
                    enemyBase.animator.SetInteger("Anim", 0);
                }
            }
        }
        else if(AttackType ==1)
        {
            if (enemyBase.mode == EnemyBase.ModeType.M1)
            {
                Dash = false;
                DistanceCount = 0;
                enemyBase.audioManager.isPlaySE(enemyBase.AttackStayAudio);
                enemyBase.rb.velocity = Vector3.zero;
                enemyBase.mode = EnemyBase.ModeType.M2;
                enemyBase.animator.SetInteger("Anim", 2);

                CapsuleCollider2D cap = enemyBase.gameObject.GetComponent<CapsuleCollider2D>();
                cap.isTrigger = true;
            }
            if (enemyBase.mode == EnemyBase.ModeType.M2)
            {

                enemyBase.isPlayerLookBase();
                enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * 5f;
                if (enemyBase.PlayerDistance < 1)
                {
                    enemyBase.animator.SetInteger("Anim", 3);
                    enemyBase.mode = EnemyBase.ModeType.M3;
                    enemyBase.AttackCount = 0;

                    CapsuleCollider2D cap = enemyBase.gameObject.GetComponent<CapsuleCollider2D>();
                    cap.isTrigger = false;
                }
            }
            if (enemyBase.mode == EnemyBase.ModeType.M3)
            {
                //enemyBase.isPlayerLookBase();
                enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * 0.2f;
                if (enemyBase.AttackCount > 1f)
                {
                    
                    enemyBase.mode = EnemyBase.ModeType.M1;
                    enemyBase.AttackCount = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Move;
                    enemyBase.PlayerDistance = 1000;
                    enemyBase.animator.SetInteger("Anim", 0);
                }
            }
        }
    }
}

