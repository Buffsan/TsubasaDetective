using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CG_Wizard : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;
    public GameObject S_AttackArea;
    public override void EnemyAttackPlay()
    {
        enemyBase.AttackCount += Time.deltaTime;
        if (enemyBase.mode == EnemyBase.ModeType.M1) 
        {
            enemyBase.rb.velocity = Vector3.zero;
            enemyBase.audioManager.isPlaySE(enemyBase.AttackStayAudio);
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
                enemyBase.audioManager.isPlaySE(enemyBase.AttackAudio);
                GameObject CL_Arrow = Instantiate(Arrow, enemyBase.gameObject.transform.position, Quaternion.identity);
                Mortar_Controller mortar = CL_Arrow.GetComponent<Mortar_Controller>();
                mortar.ChangePos(enemyBase.Player.transform.position);
                mortar.enemyBase = enemyBase;
                mortar.AttackArea = S_AttackArea;
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
