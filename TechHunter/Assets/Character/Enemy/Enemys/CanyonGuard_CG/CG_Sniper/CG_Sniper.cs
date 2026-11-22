using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CG_Sniper : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;
    [SerializeField] float BulletSpeed = 1.8f;
    Vector2 lockedShotDirection;
    public override void EnemyAttackPlay()
    {
        enemyBase.AttackCount += Time.deltaTime;
        if (enemyBase.mode == EnemyBase.ModeType.M1) 
        {
            enemyBase.rb.velocity = Vector2.zero;
            LookAtCurrentTarget();
            // プレイヤー戦と同じタイミングで照準を保存する。
            lockedShotDirection = TargetDirection;
            enemyBase.audioManager.PlaySE(enemyBase.AttackStayAudio);
            enemyBase.mode = EnemyBase.ModeType.M2;
            enemyBase.animator.SetInteger("Anim",2);
            enemyBase.AttackCount = 0;
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2) 
        {
            if (enemyBase.AttackCount > 1) 
            {
                enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
                enemyBase.animator.SetInteger("Anim", 3);
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;
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
            if (enemyBase.AttackCount > 1)
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
        // 従来は構え時に更新されたPlayerDirectionを発射時にも使用していた。
        // 敵相手にも同じ構え時点の方向を使う（発射直前の再照準はしない）。
        Vector2 shotDirection = lockedShotDirection;
        if (shotDirection.sqrMagnitude < 0.0001f) shotDirection = TargetDirection;

        GameObject CL_Arrow = Instantiate(Arrow, enemyBase.transform.position, Quaternion.identity);
        Rigidbody2D rb = CL_Arrow.GetComponent<Rigidbody2D>();
        rb.velocity = shotDirection.normalized * BulletSpeed;

        ConfigureSummonedAttack(CL_Arrow);
        EnemyAttack enemyAttack = CL_Arrow.GetComponent<EnemyAttack>();
        enemyAttack.enemyBase = enemyBase;

        CL_Arrow.transform.up = shotDirection;
        CL_Arrow.transform.Rotate(0, 0, 90);

        Destroy( CL_Arrow ,enemyBase.AttackDellTimer);

    }
}
