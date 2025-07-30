using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MO_EarthSite_AI : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;

    int Phase = 0;
    bool trune = false;
    float PhaseCount = 0;
    bool one = false;
    float PhaseCount1 = 0;
    Vector2 SaveAngle = Vector2.zero;
    Vector2 SavePos = Vector2.zero;
    public float RandomTime;
   
    public float WaitCunt =0;
    public override void EnemyMovePlay()
    {
        if (!one) 
        { 
        one = true;
            RandomTime = Random.Range(2f, 8f);
            int boolrandom = Random.Range(0, 2);
            if (boolrandom == 0)
            {
                trune = true;
            }
            else 
            {
                trune = false;
            }
        }
        WaitCunt += Time.deltaTime;
        if (WaitCunt > RandomTime) 
        {
            WaitCunt = 0;
            enemyBase.moveType = EnemyBase.MoveType.Attack;
            RandomTime = Random.Range(2f, 8f);
        }

        enemyBase.PlayerDistance = Vector2.Distance(enemyBase.Player.transform.position, enemyBase.gameObject.transform.position);
        enemyBase.animator.SetInteger("Anim",1);
        enemyBase.PlayerDirection = enemyBase.Player.transform.position - enemyBase.transform.position;
        float angle = Mathf.Atan2(enemyBase.PlayerDirection.y, enemyBase.PlayerDirection.x) * Mathf.Rad2Deg;
        if (trune)
        {
            angle += 100 - enemyBase.PlayerDistance * 12;
        }
        else 
        {
            angle -= 100 - enemyBase.PlayerDistance * 12;
        }
        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

        if (direction.y > 0)
        {
            enemyBase.EnemyAnimBody.transform.localScale = new Vector2(2.5f, 2.5f);
        }
        else 
        {
            enemyBase.EnemyAnimBody.transform.localScale = new Vector2(-2.5f, 2.5f);
        }

        enemyBase.rb.velocity = direction * enemyBase.SPEED;
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
            if (enemyBase.AttackCount > 1)
            {
                enemyBase.animator.SetInteger("Anim", 3);
                SavePos = enemyBase.Player.transform.position;
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            //enemyBase.isPlayerLookBase();
            float SaveDistance = Vector2.Distance(SavePos,enemyBase.transform.position);
            Vector2 SaveDirection = SavePos - (Vector2)enemyBase.transform.position;
            if (SaveDistance > 0.3f)
            {
                enemyBase.rb.velocity = SaveDirection.normalized * enemyBase.SPEED*2;
                CapsuleCollider2D cap = enemyBase.gameObject.GetComponent<CapsuleCollider2D>();
                cap.isTrigger = true;
                enemyBase.StanHP = 20;
            }
            else 
            { 
            enemyBase.mode = EnemyBase.ModeType.M4;
                enemyBase.AttackCount = 0;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {
            enemyBase.rb.velocity = new Vector2(enemyBase.rb.velocity.x * 0.2f, enemyBase.rb.velocity.y * 0.2f);
            if (enemyBase.AttackCount > 0.2)
            {
                enemyBase.StanHP = 0;
                CapsuleCollider2D cap = enemyBase.gameObject.GetComponent<CapsuleCollider2D>();
                cap.isTrigger = false;
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
            enemyBase.animator.SetInteger("Anim", 4);
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
