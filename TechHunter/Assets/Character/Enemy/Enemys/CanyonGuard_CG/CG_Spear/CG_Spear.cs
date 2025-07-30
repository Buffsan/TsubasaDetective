using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CG_Spear : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;
    Vector2 PlayerDirSave;
    
    
    
    public override void EnemyAttackPlay()
    {
        enemyBase.AttackCount += Time.deltaTime;
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {

             enemyBase.animator.SetInteger("Anim", 2);
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M2;
            enemyBase.rb.velocity = Vector2.zero;
            enemyBase.audioManager.isPlaySE(enemyBase.AttackStayAudio);
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {
            if (enemyBase.AttackCount > 1)
            {
                enemyBase.animator.SetInteger("Anim", 5);
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M3;
                PlayerDirSave = enemyBase.PlayerDirection;

            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3) 
        {
            enemyBase.isPlayerLookBase();
            if (enemyBase.AttackCount > 1) 
            {
                enemyBase.animator.SetInteger("Anim", 3);
            enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M4;
                PlayerDirSave = enemyBase.PlayerDirection;
                enemyBase.audioManager.isPlaySE(enemyBase.AttackAudio);

                GameObject CL_Attack = Instantiate(enemyBase.AttackArea, enemyBase.gameObject.transform.position, Quaternion.identity);
                CL_Attack.transform.parent = enemyBase.EnemyAnimBody.transform;
                EnemyAttack enemyAttack = CL_Attack.GetComponent<EnemyAttack>();
                Rigidbody2D rb = CL_Attack.GetComponent <Rigidbody2D>();
                Destroy(rb);
                enemyAttack.enemyBase = enemyBase;
                
                Destroy(CL_Attack, 2f);
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {
            enemyBase.StanHP = 100;
            enemyBase.rb.velocity = PlayerDirSave.normalized * (enemyBase.SPEED * 3f);

            if (enemyBase.AttackCount > 2)
            {
                
                
                float playerDistance = Vector2.Distance(enemyBase.Player.transform.position, enemyBase.transform.position);
                if (playerDistance < 4)
                {
                    Debug.Log("Äs“®");
                    enemyBase.rb.velocity = enemyBase.rb.velocity * 0.5f;
                    enemyBase.animator.SetInteger("Anim", 5);
                    enemyBase.AttackCount = 0;
                    enemyBase.mode = EnemyBase.ModeType.M3;
                    enemyBase.StanHP = enemyBase.charadata.StanHP;

                }
                else 
                {
                    Debug.Log(playerDistance);
                enemyBase.animator.SetInteger("Anim", 4);
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M5;
                }

            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M5)
        {

            enemyBase.rb.velocity = Vector2.zero ;
            enemyBase.StanHP = enemyBase.charadata.StanHP;

            if (enemyBase.AttackCount > 1)
            {
                

                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M1;
                enemyBase.moveType = EnemyBase.MoveType.Move;
                enemyBase.PlayerDistance = 1000;
            }
        }
        
    }
}
