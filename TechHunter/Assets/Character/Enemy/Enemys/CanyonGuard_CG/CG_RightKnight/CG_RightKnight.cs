using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CG_RightKnight : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;

    int Phase = 0;

    float PhaseCount = 0;
    float PhaseCount1 = 0;
    Vector2 SaveAngle =Vector2.zero;
    Vector2 SavePosPhase= Vector2.zero;

    public override void EnemyMovePlay()
    {
        enemyBase.PlayerDistance = Vector2.Distance(enemyBase.Player.transform.position, enemyBase.gameObject.transform.position);
        
        if (enemyBase.PlayerDistance < 5)
        {//enemyBase.isPlayerLookBase();
            enemyBase.isPlayerLookBase();

            enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * enemyBase.SPEED;
            enemyBase.animator.SetInteger("Anim", 1);
        }
        else 
        {
            PhaseCount += Time.deltaTime;
            if (Phase == 0)
            {
                enemyBase.animator.SetInteger("Anim", 0);
                
                
                

                if (PhaseCount > 1)
                {
                    PhaseCount = 0;
                    Phase = 1;
                    
                }
            }
            if (Phase == 1) 
            {
                SaveAngle = new Vector2(Random.Range(-1f,1f), Random.Range(-1f, 1f)).normalized;
                Phase = 2;
                if (SaveAngle.x > 0)
                {
                   
                    enemyBase.EnemyAnimBody.transform.localScale = new Vector2(2.5f, 2.5f);
                }
                else 
                {
                   
                    enemyBase.EnemyAnimBody.transform.localScale = new Vector2(-2.5f, 2.5f);
                }

                SavePosPhase = enemyBase.gameObject.transform.position;
            }
            if (Phase==2) 
            {
                enemyBase.animator.SetInteger("Anim", 1);
                enemyBase.rb.velocity = SaveAngle * enemyBase.SPEED;

                PhaseCount1 += Time.deltaTime;
                if (PhaseCount1 > 1)
                {
                    
                    PhaseCount1 = 0;

                    float SaveDistance = Vector2.Distance(SavePosPhase, enemyBase.gameObject.transform.position);
                    if (SaveDistance < 0.5)
                    {
                        enemyBase.rb.velocity = Vector2.zero;
                        //Debug.Log("ˆÚ“®Œo˜HÄŒŸõ" + SaveDistance);
                        PhaseCount = 1;
                        Phase = 0;
                        Vector2 SavePosPhase = new Vector2(999, 999);
                    }
                }

                    if (PhaseCount > 4)
                {
                    enemyBase.rb.velocity = Vector2.zero;
                    PhaseCount = 0;
                    Phase = 0;
                }
            }
        }
    }
    public override void EnemyAttackPlay()
    {
        enemyBase.AttackCount += Time.deltaTime;
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
            //enemyBase.isPlayerLookBase();
            enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * 1.2f;
            if (enemyBase.AttackCount > 0.5f)
            {
                enemyBase.mode = EnemyBase.ModeType.M4;
                enemyBase.AttackCount = 0;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {
            enemyBase.rb.velocity = new Vector2(enemyBase.rb.velocity.x * 0.5f, enemyBase.rb.velocity.y * 0.5f);
            if (enemyBase.AttackCount > 0.3)
            {
                enemyBase.mode = EnemyBase.ModeType.M5;
                enemyBase.AttackCount = 0;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M5)
        {
            GameObject CL_Attack = Instantiate(enemyBase.AttackArea, enemyBase.transform.position, Quaternion.identity);
            EnemyAttack enemyAttack = CL_Attack.GetComponent<EnemyAttack>();
            enemyAttack.enemyBase = enemyBase;
            Destroy(CL_Attack, 0.01f);

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
}
