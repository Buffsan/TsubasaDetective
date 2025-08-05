using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eaterofleftovers_AI : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;

    int Phase = 0;
    bool trune = false; bool Strate = false;
    float PhaseCount = 0;
    bool one = false;
    float PhaseCount1 = 0;
    Vector2 SaveAngle = Vector2.zero;
    Vector2 SavePos = Vector2.zero;
    public float RandomTime;

    [SerializeField] GameObject AttackBefore;
    [SerializeField] float MinDashCount = 0.1f;
    [SerializeField] float MaxDashCount = 0.4f;
    [SerializeField] int DashCountLimit = 5;
    [SerializeField] GameObject AttackPoint;
    [SerializeField] float DashSpeed = 5;
    float AttackMeeleLimit = 0;

    bool DashWait = false;

    public float WaitCunt = 0;
    public float LestTime = 0;
    float LestCount = 0;

    public override void EnemyConfusionPlay()
    {
        enemyBase.DamageBody.SetActive(false);
    }
    public override void EnemyStanPlay()
    {
        enemyBase.DamageBody.SetActive(false); Debug.Log("¬—");
    }
    public override void EnemyMovePlay()
    {
        enemyBase.DamageBody.SetActive(false);
        if (WaitCunt > RandomTime && !Strate || WaitCunt > RandomTime * 2 && Strate)
        {
            WaitCunt = 0;
            one = false;
            DashWait = true;
        }

        if (!DashWait)
        {
            if (!one)
            {
                one = true;
                RandomTime = Random.Range(MinDashCount, MaxDashCount);
                int boolrandom = Random.Range(0, 3);
                if (boolrandom == 0)
                {
                    trune = true;
                    Strate = false;
                }
                else if (boolrandom == 1)
                {
                    trune = false;
                    Strate = false;
                }
                else 
                {
                    Strate = true;
                }
            }
            WaitCunt += Time.deltaTime;

            enemyBase.PlayerDistance = Vector2.Distance(enemyBase.Player.transform.position, enemyBase.gameObject.transform.position);
            enemyBase.animator.SetInteger("Anim", 1);
            enemyBase.PlayerDirection = enemyBase.Player.transform.position - enemyBase.transform.position;

            if (Strate)
            {
                enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * enemyBase.SPEED;
            }
            else
            {
                float angle = Mathf.Atan2(enemyBase.PlayerDirection.y, enemyBase.PlayerDirection.x) * Mathf.Rad2Deg;
                if (trune)
                {
                    angle += 150 - enemyBase.PlayerDistance * 12;
                }
                else
                {
                    angle -= 150 - enemyBase.PlayerDistance * 12;
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
        }
        else 
        {
            LestCount += Time.deltaTime;
            enemyBase.rb.velocity = enemyBase.rb.velocity * 0.3f;
            if (LestCount > LestTime ) 
            {
                LestCount = 0;
                DashWait = false;
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
            
            enemyBase.isPlayerLookBase();
            GameObject CL_Before = Instantiate(AttackBefore, enemyBase.AttackSpawnPoint.transform.position,Quaternion.identity);
            CL_Before.transform.up = enemyBase.PlayerDirection.normalized;
            Destroy(CL_Before, 1);

            
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {
            
            if (enemyBase.AttackCount > 1)
            {enemyBase.DamageBody.SetActive(true);
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;
                enemyBase.animator.SetInteger("Anim", 3);
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            
            enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * DashSpeed;
            if (enemyBase.AttackCount > 0.5f)
            {
                enemyBase.DamageBody.SetActive(false);
                enemyBase.mode = EnemyBase.ModeType.M4;
                enemyBase.AttackCount = 0;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {
            enemyBase.animator.SetInteger("Anim", 0);
            enemyBase.rb.velocity = new Vector2(enemyBase.rb.velocity.x * 0.5f, enemyBase.rb.velocity.y * 0.5f);
            if (enemyBase.AttackCount > 0.3)
            {
                enemyBase.mode = EnemyBase.ModeType.M5;
                enemyBase.AttackCount = 0;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M5)
        {
            /*
            GameObject CL_Attack = Instantiate(enemyBase.AttackArea, enemyBase.transform.position, Quaternion.identity);
            EnemyAttack enemyAttack = CL_Attack.GetComponent<EnemyAttack>();
            enemyAttack.enemyBase = enemyBase;
            Destroy(CL_Attack, 0.01f);*/

            enemyBase.audioManager.isPlaySE(enemyBase.AttackAudio);
            
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