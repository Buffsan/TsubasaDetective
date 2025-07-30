using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CG_Hanter_AI : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;
    [SerializeField] GameObject Slash;

    int Phase = 0;

    float PhaseCount = 0;
    float PhaseCount1 = 0;

    float PlayerDistanceAutCount = 0;

    int AttackType = 0;
    int Attack1Count = 0;

    Vector2 SaveAngle = Vector2.zero;
    Vector2 SavePosPhase = Vector2.zero;

    public override void EnemyMovePlay()
    {
        if (PlayerDistanceAutCount > 1)
        {
            enemyBase.moveType = EnemyBase.MoveType.Attack;
            PlayerDistanceAutCount = 0;
            AttackType = 1;
        }
        else
        {
            if (Attack1Count < 2)
            {
                AttackType = 0;
            }
            else
            {
                Attack1Count = 0;
                AttackType = 2;
            }
        }
        enemyBase.PlayerDistance = Vector2.Distance(enemyBase.Player.transform.position, enemyBase.gameObject.transform.position);
        if (enemyBase.PlayerDistance > 4) 
        {
            PlayerDistanceAutCount += Time.deltaTime;
            
        }
        if (enemyBase.PlayerDistance < 5)
        {//enemyBase.isPlayerLookBase();
            enemyBase.isPlayerLookBase();

            enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * enemyBase.SPEED;
            enemyBase.animator.SetInteger("Anim", 1);
        }
        else
        {
            PhaseCount += Time.deltaTime;
            if (Phase == 0)
            {
                enemyBase.animator.SetInteger("Anim", 0);




                if (PhaseCount > 1)
                {
                    PhaseCount = 0;
                    Phase = 1;

                }
            }
            if (Phase == 1)
            {
                SaveAngle = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                Phase = 2;
                if (SaveAngle.x > 0)
                {

                    enemyBase.EnemyAnimBody.transform.localScale = new Vector2(2.5f, 2.5f);
                }
                else
                {

                    enemyBase.EnemyAnimBody.transform.localScale = new Vector2(-2.5f, 2.5f);
                }

                SavePosPhase = enemyBase.gameObject.transform.position;
            }
            if (Phase == 2)
            {
                enemyBase.animator.SetInteger("Anim", 1);
                enemyBase.rb.velocity = SaveAngle * enemyBase.SPEED;

                PhaseCount1 += Time.deltaTime;
                if (PhaseCount1 > 1)
                {

                    PhaseCount1 = 0;

                    float SaveDistance = Vector2.Distance(SavePosPhase, enemyBase.gameObject.transform.position);
                    if (SaveDistance < 0.5)
                    {
                        enemyBase.rb.velocity = Vector2.zero;
                        //Debug.Log("移動経路再検索" + SaveDistance);
                        PhaseCount = 1;
                        Phase = 0;
                        Vector2 SavePosPhase = new Vector2(999, 999);
                    }
                }

                if (PhaseCount > 4)
                {
                    enemyBase.rb.velocity = Vector2.zero;
                    PhaseCount = 0;
                    Phase = 0;
                }
            }
        }
    }
    public override void EnemyAttackPlay()
    {
        enemyBase.AttackCount += Time.deltaTime;
        if (AttackType == 0)
        {
            if (enemyBase.mode == EnemyBase.ModeType.M1)
            {
                enemyBase.audioManager.isPlaySE(enemyBase.AttackStayAudio);
                enemyBase.rb.velocity = Vector3.zero;
                enemyBase.mode = EnemyBase.ModeType.M2;
                Attack1Count++;
                enemyBase.animator.SetInteger("Anim", 2);
            }
            if (enemyBase.mode == EnemyBase.ModeType.M2)
            {
                enemyBase.isPlayerLookBase();
                if (enemyBase.AttackCount > 0.2)
                {
                    enemyBase.mode = EnemyBase.ModeType.M3;
                    enemyBase.AttackCount = 0;
                }
            }
            if (enemyBase.mode == EnemyBase.ModeType.M3)
            {
                //enemyBase.isPlayerLookBase();
                enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * 6f;
                if (enemyBase.AttackCount > 0.2f)
                {
                    enemyBase.mode = EnemyBase.ModeType.M4;
                    enemyBase.AttackCount = 0;
                }
            }
            if (enemyBase.mode == EnemyBase.ModeType.M4)
            {
                enemyBase.rb.velocity = new Vector2(enemyBase.rb.velocity.x * 0.5f, enemyBase.rb.velocity.y * 0.5f);
                if (enemyBase.AttackCount > 0.3)
                { int Randomr = Random.Range(0,2);
                    if (Randomr == 0)
                    {
                        enemyBase.mode = EnemyBase.ModeType.M5;
                    }
                    else 
                    {
                        enemyBase.mode = EnemyBase.ModeType.M2;
                    }
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
                enemyBase.animator.SetInteger("Anim", 3);
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
        else if (AttackType == 1) 
        {
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
                if (enemyBase.AttackCount > 0.2)
                {
                    enemyBase.mode = EnemyBase.ModeType.M3;
                    enemyBase.AttackCount = 0;
                }
            }
            if (enemyBase.mode == EnemyBase.ModeType.M3)
            {
                //enemyBase.isPlayerLookBase();
                enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * 0.5f;
                if (enemyBase.AttackCount > 0.5f)
                {
                    enemyBase.mode = EnemyBase.ModeType.M4;
                    enemyBase.AttackCount = 0;
                }
            }
            if (enemyBase.mode == EnemyBase.ModeType.M4)
            {
                enemyBase.rb.velocity = new Vector2(enemyBase.rb.velocity.x * 0.1f, enemyBase.rb.velocity.y * 0.1f);
                if (enemyBase.AttackCount > 0.3)
                {
                    enemyBase.mode = EnemyBase.ModeType.M5;
                    enemyBase.AttackCount = 0;
                }
            }
            if (enemyBase.mode == EnemyBase.ModeType.M5)
            {
                GameObject CL_Attack = Instantiate(Slash, enemyBase.transform.position, Quaternion.identity);
                EnemyAttack enemyAttack = CL_Attack.GetComponent<EnemyAttack>();
                enemyAttack.enemyBase = enemyBase;
                enemyBase.isPlayerLookBase();


                Rigidbody2D CL_rb = CL_Attack.GetComponent<Rigidbody2D>();
                CL_rb.velocity = enemyBase.PlayerDirection.normalized * 5;
                CL_Attack.transform.up = enemyBase.PlayerDirection.normalized;
                CL_Attack.transform.Rotate(0, 0, 90);


                Destroy(CL_Attack, 10);

                enemyBase.audioManager.isPlaySE(enemyBase.AttackAudio);
                enemyBase.animator.SetInteger("Anim", 3);
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
        else if (AttackType == 2)
        {
            if (enemyBase.mode == EnemyBase.ModeType.M1)
            {
                enemyBase.audioManager.isPlaySE(enemyBase.AttackStayAudio);
                enemyBase.rb.velocity = Vector3.zero;
                enemyBase.mode = EnemyBase.ModeType.M2;
                Attack1Count++;
                enemyBase.animator.SetInteger("Anim", 2);
            }
            if (enemyBase.mode == EnemyBase.ModeType.M2)
            {
                enemyBase.isPlayerLookBase();
                if (enemyBase.AttackCount > 0.2)
                {
                    enemyBase.mode = EnemyBase.ModeType.M3;
                    enemyBase.AttackCount = 0;
                }
            }
            if (enemyBase.mode == EnemyBase.ModeType.M3)
            {
                //enemyBase.isPlayerLookBase();
                //enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * 5f;
                    enemyBase.mode = EnemyBase.ModeType.M4;
                    enemyBase.AttackCount = 0;
            }
            if (enemyBase.mode == EnemyBase.ModeType.M4)
            {
                //enemyBase.rb.velocity = new Vector2(enemyBase.rb.velocity.x * 0.5f, enemyBase.rb.velocity.y * 0.5f);
                 enemyBase.audioManager.isPlaySE(enemyBase.AttackAudio);
                enemyBase.animator.SetInteger("Anim", 4);
                    enemyBase.mode = EnemyBase.ModeType.M5;
                    enemyBase.AttackCount = 0;
                
            }
            if (enemyBase.mode == EnemyBase.ModeType.M5)
            {
                //GameObject CL_Attack = Instantiate(enemyBase.AttackArea, enemyBase.transform.position, Quaternion.identity);
                // EnemyAttack enemyAttack = CL_Attack.GetComponent<EnemyAttack>();
                //enemyAttack.enemyBase = enemyBase;
                //Destroy(CL_Attack, 0.01f);

                if (enemyBase.AttackCount > 0.4)
                {
                    for (int f = 0; f < 3; f++)
                    {
                        for (int i = 0; i < 12; i++)
                        {

                            float angle = (360 / 12) * i;
                            float angleRad = angle * Mathf.Deg2Rad;

                            float newX = enemyBase.transform.position.x + f * Mathf.Cos(angleRad);
                            float newY = enemyBase.transform.position.y + f * Mathf.Sin(angleRad);

                            // 新しい位置にクローンを作成
                            Vector2 newPosition = new Vector2(newX, newY );

                            GameObject CL_Needle = Instantiate(Arrow, newPosition, Quaternion.identity);
                            CG_Puppeteer_Needle puppeteer_Needle = CL_Needle.GetComponent<CG_Puppeteer_Needle>();
                            puppeteer_Needle.enemyAttack.enemyBase = enemyBase;

                            Destroy(CL_Needle, 5);


                        }
                    }
                    enemyBase.AttackCount = 0;
                    enemyBase.mode = EnemyBase.ModeType.M6;
                }

            }
            if (enemyBase.mode == EnemyBase.ModeType.M6)
            {

                if (enemyBase.AttackCount > 0.4)
                {
                    for (int f = 0; f < 3; f++)
                    {
                        for (int i = 0; i < 12; i++)
                        {

                            float angle = (360 / 12) * i;
                            float angleRad = angle * Mathf.Deg2Rad;

                            float newX = enemyBase.transform.position.x + (f + 3) * Mathf.Cos(angleRad);
                            float newY = enemyBase.transform.position.y + (f + 3) * Mathf.Sin(angleRad);

                            // 新しい位置にクローンを作成
                            Vector2 newPosition = new Vector2(newX, newY);

                            GameObject CL_Needle = Instantiate(Arrow, newPosition, Quaternion.identity);
                            CG_Puppeteer_Needle puppeteer_Needle = CL_Needle.GetComponent<CG_Puppeteer_Needle>();
                            puppeteer_Needle.enemyAttack.enemyBase = enemyBase;

                            Destroy(CL_Needle, 5);


                        }
                    }
                    enemyBase.AttackCount = 0;
                    enemyBase.mode = EnemyBase.ModeType.M7;
                }
            }
            if (enemyBase.mode == EnemyBase.ModeType.M7)
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
}