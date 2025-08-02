using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TF_FishHanter_Shoygun_AI : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;
    [SerializeField] GameObject SartchArrow;
    [SerializeField] GameObject Effect;

    int AttackNumber = 0;

    public override void EnemyAttackPlay()
    {
        enemyBase.AttackCount += Time.deltaTime;
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {   
            enemyBase.isPlayerLookBase();
            enemyBase.rb.velocity = Vector2.zero;
            enemyBase.audioManager.isPlaySE(enemyBase.AttackStayAudio);
            enemyBase.mode = EnemyBase.ModeType.M2;
            enemyBase.animator.SetInteger("Anim", 2);
            enemyBase.AttackCount = 0;
            shotBullet(SartchArrow,1);
            
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {
            if (enemyBase.AttackCount > 1f)
            {
                enemyBase.audioManager.isPlaySE(enemyBase.AttackAudio);
                enemyBase.animator.SetInteger("Anim", 3);
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;
                enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * -10f;
                shotBullet(Arrow,0.1f);shotBullet(Effect, 1);
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {

            enemyBase.rb.velocity = enemyBase.rb.velocity * 0.8f;
            if (enemyBase.AttackCount > 0.15f)
            {
                enemyBase.rb.velocity = Vector2.zero;
                if (AttackNumber >= 1)
                {
                    enemyBase.animator.SetInteger("Anim", 0);
                    enemyBase.mode = EnemyBase.ModeType.M4;
                    enemyBase.AttackCount = 0;
                }
                else
                {
                    AttackNumber ++;
                    enemyBase.mode = EnemyBase.ModeType.M1;
                    enemyBase.AttackCount = 0;
                }
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {
            if (enemyBase.AttackCount > 1)
            {
                enemyBase.moveType = EnemyBase.MoveType.Move;
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M1;
                enemyBase.animator.SetInteger("Anim", 1);
                //Debug.Log("A");
                enemyBase.PlayerDistance = 1000;
                AttackNumber = 0 ;
            }
        }

        //base.EnemyAttackPlay();
    }

    void shotBullet(GameObject Ar,float dell)
    {
        //Vector2 ShotPos = enemyBase.Player.transform.position - enemyBase.AttackSpawnPoint.transform.position;

        GameObject CL_Arrow = Instantiate(Ar, enemyBase.transform.position, Quaternion.identity);

        CL_Arrow.transform.up = enemyBase.PlayerDirection;
        CL_Arrow.transform.Rotate(0, 0, 180);

        Destroy(CL_Arrow, dell);

    }
}
