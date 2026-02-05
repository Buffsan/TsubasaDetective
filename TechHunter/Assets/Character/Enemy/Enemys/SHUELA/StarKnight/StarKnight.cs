using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class StarKnight : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;
    [SerializeField] GameObject Slash;
    [SerializeField] GameObject Effect;

    int Phase = 0;

    [SerializeField]float PhaseCount = 0;
    float PhaseCount1 = 0;

    float PlayerDistanceAutCount = 0;

    int AttackType = 0;
    int Attack1Count = 0;

    Vector2 SaveAngle = Vector2.zero;
    Vector2 SavePosPhase = Vector2.zero;

    public override void EnemyMovePlay()
    {
        enemyBase.isPlayerLookBase();
        enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * enemyBase.SPEED;
        enemyBase.animator.SetInteger("Anim",1);
        PhaseCount += Time.deltaTime;
        if (PhaseCount > 1)
        {
            PhaseCount = 0;
            AttackType = 1;
            enemyBase.moveType = EnemyBase.MoveType.Attack;
        }
        else 
        {
            AttackType = 0;
        }
    }
    public override void EnemyAttackPlay()
    {
        enemyBase.AttackCount += Time.deltaTime;
        if (AttackType == 0)
        {
            if (enemyBase.mode == EnemyBase.ModeType.M1)
            {
                enemyBase.audioManager.PlaySE(enemyBase.AttackStayAudio);
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
                {                   
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

                enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
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
                enemyBase.audioManager.PlaySE(enemyBase.AttackStayAudio);
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

                    GameObject CL_Effect = Instantiate(Effect, transform.position, Quaternion.identity);
                    Destroy(CL_Effect ,2);
                }
            }
            if (enemyBase.mode == EnemyBase.ModeType.M4)
            {
                enemyBase.rb.velocity = new Vector2(enemyBase.rb.velocity.x * 0.1f, enemyBase.rb.velocity.y * 0.1f);
                if (enemyBase.AttackCount > 1)
                {
                    enemyBase.mode = EnemyBase.ModeType.M5;
                    enemyBase.AttackCount = 0;
                }
            }
            if (enemyBase.mode == EnemyBase.ModeType.M5)
            {
                GameObject CL_Moter = Instantiate(Slash, enemyBase.transform.position, Quaternion.identity);
                Mortar_Controller mortar_ = CL_Moter.GetComponent<Mortar_Controller>();
                mortar_.enemyBase = enemyBase;
                mortar_.ChangePos(enemyBase.Player.transform.position);
                Destroy(CL_Moter, 10);

                enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
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
        else if (AttackType == 2)
        {
            if (enemyBase.mode == EnemyBase.ModeType.M1)
            {
                enemyBase.audioManager.PlaySE(enemyBase.AttackStayAudio);
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
                enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
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
                            Vector2 newPosition = new Vector2(newX, newY);

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