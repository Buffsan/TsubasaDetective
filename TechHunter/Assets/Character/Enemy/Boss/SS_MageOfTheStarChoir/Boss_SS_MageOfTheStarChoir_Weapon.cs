using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_SS_MageOfTheStarChoir_Weapon : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;
    [SerializeField] GameObject ChargeEffect;
    [SerializeField] float AttackTime = 2;
    [SerializeField] GameObject DengerObject;
    [SerializeField]float attackCount = 0;

    [SerializeField] float AttackSpeed = 5;

    Vector2 SaveDirection;

    [SerializeField] float AttackStartTime = 1.5f;

    public override void EnemyMovePlay()
    {
        attackCount += Time.fixedDeltaTime;
        enemyBase.animator.Play("‘Ò‹@");
        if (attackCount > AttackTime) 
        {
            attackCount = 0;
            enemyBase.moveType = EnemyBase.MoveType.Attack;
        }
    }
    public override void EnemyAttackPlay()
    {
        enemyBase.AttackCount += Time.deltaTime;
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            
            enemyBase.audioManager.PlaySE(enemyBase.AttackStayAudio);
            GameObject CL_Effect = Instantiate(ChargeEffect,transform.position,Quaternion.identity);
            SaveDirection = (enemyBase.playerController.transform.position - transform.position).normalized;
            Destroy(CL_Effect,2);

            if (DengerObject)
            {
                GameObject CL_Denger = Instantiate(DengerObject, transform.position, Quaternion.identity);
                CL_Denger.transform.up = SaveDirection;
                SaveComponent savecomponent = CL_Denger.GetComponent<SaveComponent>();
                savecomponent.animator.speed = 1 / AttackStartTime;
                Destroy(CL_Denger, AttackStartTime * 1.1f);
            }

            enemyBase.mode = EnemyBase.ModeType.M2;
            enemyBase.AttackCount = 0;
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {
            if (enemyBase.AttackCount > AttackStartTime)
            {
                enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
                enemyBase.animator.Play("”ñ•\Ž¦”­ŽË");
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;
                shotBullet();

            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            if (enemyBase.AttackCount > 1)
            {
                enemyBase.moveType = EnemyBase.MoveType.Move;
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M1;
                
                enemyBase.PlayerDistance = 1000;
            }
        }
        
    }

    void shotBullet()
    {
        
        GameObject CL_Arrow = Instantiate(Arrow, enemyBase.transform.position, Quaternion.identity);
        Rigidbody2D rb = CL_Arrow.GetComponent<Rigidbody2D>();
        rb.velocity = SaveDirection * AttackSpeed;

        EnemyAttack enemyAttack = CL_Arrow.GetComponent<EnemyAttack>();
        if(enemyAttack)
        enemyAttack.enemyBase = enemyBase;

        CL_Arrow.transform.up = SaveDirection;
        //CL_Arrow.transform.Rotate(0, 0, 90);

        Destroy(CL_Arrow, enemyBase.AttackDellTimer);

    }
}
