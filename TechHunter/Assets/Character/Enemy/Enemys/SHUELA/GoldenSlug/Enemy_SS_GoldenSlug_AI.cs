using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_SS_GoldenSlug_AI : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;
    [SerializeField] float rotateSpeed = 1;

    GameObject saveArrow;
    public GameObject S_AttackArea;
    public override void EnemyMovePlay()
    {
        if(saveArrow)Destroy(saveArrow);
    }
    public override void EnemyStanPlay()
    {
        if (saveArrow) Destroy(saveArrow);
    }
    public override void EnemyAttackPlay()
    {
        enemyBase.AttackCount += Time.deltaTime;
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            enemyBase.rb.velocity = Vector3.zero;
            enemyBase.audioManager.PlaySE(enemyBase.AttackStayAudio);
            enemyBase.animator.SetInteger("Anim", 2);
            enemyBase.mode = EnemyBase.ModeType.M2;

        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {

            if (enemyBase.AttackCount > 2)
            {
                enemyBase.animator.SetInteger("Anim", 3);
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;
                enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
                saveArrow = SpawnObjectNow_Input(Arrow, transform.position, 60);
                saveArrow.transform.parent = transform;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            if (saveArrow) saveArrow.transform.Rotate(0, 0, rotateSpeed);
            if (enemyBase.AttackCount > 20)
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
                enemyBase.animator.SetInteger("Anim", 0);
                enemyBase.mode = EnemyBase.ModeType.M1;
                enemyBase.AttackCount = 0;
                enemyBase.moveType = EnemyBase.MoveType.Move;
                enemyBase.PlayerDistance = 1000;
            }
        }
    }
}
