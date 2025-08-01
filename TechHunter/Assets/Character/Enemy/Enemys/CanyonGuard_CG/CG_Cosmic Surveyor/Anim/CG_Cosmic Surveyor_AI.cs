using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CG_CosmicSurveyor_AI : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;
    public override void EnemyAttackPlay()
    {
        enemyBase.AttackCount += Time.deltaTime;
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            enemyBase.rb.velocity = Vector2.zero;
            enemyBase.audioManager.isPlaySE(enemyBase.AttackStayAudio);
            enemyBase.mode = EnemyBase.ModeType.M2;
            enemyBase.animator.SetInteger("Anim", 2);
            enemyBase.AttackCount = 0;
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {
            if (enemyBase.AttackCount > 1.5f)
            {
                enemyBase.audioManager.isPlaySE(enemyBase.AttackAudio);
                enemyBase.animator.SetInteger("Anim", 3);
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;
                enemyBase.isPlayerLookBase();
                shotBullet();
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            if (enemyBase.AttackCount > 1)
            {
                enemyBase.animator.SetInteger("Anim", 0);
                enemyBase.mode = EnemyBase.ModeType.M4;
                enemyBase.AttackCount = 0;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {
            if (enemyBase.AttackCount > 2)
            {
                enemyBase.moveType = EnemyBase.MoveType.Move;
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M1;
                enemyBase.animator.SetInteger("Anim", 1);
                //Debug.Log("A");
                enemyBase.PlayerDistance = 1000;
            }
        }

        //base.EnemyAttackPlay();
    }

    void shotBullet()
    {
        //Vector2 ShotPos = enemyBase.Player.transform.position - enemyBase.AttackSpawnPoint.transform.position;

        GameObject CL_Arrow = Instantiate(Arrow, enemyBase.transform.position, Quaternion.identity);
        
        CL_Arrow.transform.up = enemyBase.PlayerDirection;
        CL_Arrow.transform.Rotate(0, 0, 180);

        Destroy(CL_Arrow, enemyBase.AttackDellTimer);

    }
}