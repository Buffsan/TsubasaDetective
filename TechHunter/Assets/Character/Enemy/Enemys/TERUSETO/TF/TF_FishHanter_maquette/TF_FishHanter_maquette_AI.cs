using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TF_FishHanter_maquette_AI : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;
    [SerializeField] GameObject Laser;
    public override void EnemyStanPlay()
    {
        Laser.SetActive(false);
    }
    public override void EnemyConfusionPlay()
    {
        Laser.SetActive(false);
    }
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

            Laser.SetActive(true);
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {
            enemyBase.isPlayerLookBase();

                Laser.transform.up = enemyBase.PlayerDirection.normalized;
            if (enemyBase.AttackCount > 1)
            {
                enemyBase.animator.SetInteger("Anim", 2);
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;
                
                
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            if (enemyBase.AttackCount > 0.4)
            {
                enemyBase.audioManager.isPlaySE(enemyBase.AttackAudio);
                shotBullet();
                enemyBase.animator.SetInteger("Anim", 3);
                enemyBase.mode = EnemyBase.ModeType.M4;
                enemyBase.AttackCount = 0;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {
            if (enemyBase.AttackCount > 1)
            {
                enemyBase.moveType = EnemyBase.MoveType.Move;
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M1;
                enemyBase.animator.SetInteger("Anim", 0);
                //Debug.Log("A");
                enemyBase.PlayerDistance = 1000;

                Laser.SetActive(false);
            }
        }

        //base.EnemyAttackPlay();
    }

    void shotBullet()
    {
        //Vector2 ShotPos = enemyBase.Player.transform.position - enemyBase.AttackSpawnPoint.transform.position;

        GameObject CL_Arrow = Instantiate(Arrow, enemyBase.transform.position, Quaternion.identity);
        Rigidbody2D rb = CL_Arrow.GetComponent<Rigidbody2D>();
        rb.velocity = enemyBase.PlayerDirection.normalized * 23;

       

        CL_Arrow.transform.up = enemyBase.PlayerDirection;
        CL_Arrow.transform.Rotate(0, 0, 90);

        Destroy(CL_Arrow, enemyBase.AttackDellTimer);

    }
}
