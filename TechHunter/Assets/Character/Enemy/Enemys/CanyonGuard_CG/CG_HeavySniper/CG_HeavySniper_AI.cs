using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class CG_HeavySniper_AI : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;
    [SerializeField] AudioClip ShotAudio;
    [SerializeField] GameObject CG_Sniper;
    PlayerController playerController => PlayerController.Instance;
    SystemManager systemManager => SystemManager.Instance;
    int ArrowCount = 0;
    float walkCount = 0;

    int AttackCount = 0;
    int AttackType = 0;

    Vector2 SavePos=Vector2.zero;

    public override void EnemyMovePlay()
    {
        enemyBase.PlayerDistance = Vector2.Distance(enemyBase.Player.transform.position, enemyBase.gameObject.transform.position);

        enemyBase.isPlayerLookBase();

        if (walkCount > 3 || walkCount > 1 && enemyBase.PlayerDistance < 8)
        {
            Debug.Log(AttackCount +"  "+enemyBase.PlayerDistance);
            if (enemyBase.PlayerDistance < 3 && AttackCount > 1)
            {
                //Debug.Log("AAA");
                enemyBase.moveType = EnemyBase.MoveType.Attack;
                AttackType = 1;
                AttackCount = 0;
            }
            else 
            { 
            AttackType = 0;
            enemyBase.moveType = EnemyBase.MoveType.Attack;
            AttackCount++;
            }
        } 
        else 
        {
            if (enemyBase.PlayerDistance < 8) 
            {
                enemyBase.animator.SetInteger("Anim", 0);
                enemyBase.rb.velocity = Vector2.zero;
            }
            else
            {
                enemyBase.animator.SetInteger("Anim", 1);
                enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * enemyBase.SPEED;
                
            }walkCount += Time.deltaTime;
        }
    
    }

    public override void EnemyAttackPlay()
    {
        enemyBase.AttackCount += Time.deltaTime;

        if (AttackType == 0)
        {
            if (enemyBase.mode == EnemyBase.ModeType.M1)
            {
                SavePos = enemyBase.Player.transform.position;
                
                enemyBase.audioManager.isPlaySE(enemyBase.AttackStayAudio);
                enemyBase.mode = EnemyBase.ModeType.M2;
                enemyBase.animator.SetInteger("Anim", 2);
                enemyBase.AttackCount = 0;
            }
            if (enemyBase.mode == EnemyBase.ModeType.M2)
            {
                enemyBase.isPlayerLookBase();
                enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * -1f;
                if (enemyBase.AttackCount > 1)
                {
                    enemyBase.rb.velocity = Vector2.zero;
                    enemyBase.audioManager.isPlaySE(enemyBase.AttackAudio);
                    enemyBase.animator.SetInteger("Anim", 3);
                    enemyBase.mode = EnemyBase.ModeType.M3;
                    enemyBase.AttackCount = 0;

                }
            }
            if (enemyBase.mode == EnemyBase.ModeType.M3)
            {

                if (ArrowCount < 15)
                {
                    if (enemyBase.AttackCount > 0.2)
                    {
                        enemyBase.AttackCount = 0;
                        ArrowCount++;
                        shotBullet();
                    }
                }
                else
                {
                    ArrowCount = 0;
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
        }
        if (AttackType == 1) 
        {
            if (enemyBase.mode == EnemyBase.ModeType.M1)
            {
                enemyBase.rb.velocity = Vector2.zero;
                enemyBase.audioManager.isPlaySE(enemyBase.AttackStayAudio);
                enemyBase.mode = EnemyBase.ModeType.M2;
                enemyBase.animator.SetInteger("Anim", 0);
                enemyBase.AttackCount = 0;
            }
            if (enemyBase.mode == EnemyBase.ModeType.M2)
            {
                if (enemyBase.AttackCount > 0.1)
                {
                     GameObject CL_HeaveySniper_Sniper=Instantiate(CG_Sniper, enemyBase.transform.position, Quaternion.identity);

                    systemManager.AllEnemy.Add(CL_HeaveySniper_Sniper);
                    enemyBase.audioManager.isPlaySE(enemyBase.AttackAudio);
                    enemyBase.animator.SetInteger("Anim", 1);
                    enemyBase.mode = EnemyBase.ModeType.M3;
                    enemyBase.AttackCount = 0;
                    CapsuleCollider2D cap = enemyBase.gameObject.GetComponent<CapsuleCollider2D>();
                    cap.isTrigger = true;
                }
            }
            if (enemyBase.mode == EnemyBase.ModeType.M3)
            {
                float SaveDistance = Vector2.Distance(SavePos , (Vector2)enemyBase.gameObject.transform.position);
                Vector2 SaveDirection = SavePos - (Vector2)enemyBase.transform.position;

                enemyBase.rb.velocity = SaveDirection.normalized * enemyBase.SPEED * 2;

                if (SaveDistance < 0.5)
                {
                    enemyBase.rb.velocity = Vector2.zero;
                    
                    enemyBase.animator.SetInteger("Anim", 0);
                    enemyBase.mode = EnemyBase.ModeType.M1;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    AttackType = 0;
                    enemyBase.AttackCount = 0;
                    CapsuleCollider2D cap = enemyBase.gameObject.GetComponent<CapsuleCollider2D>();
                    cap.isTrigger = false;

                    
                }
            }
        }

        //base.EnemyAttackPlay();
    }

    void shotBullet()
    {
        GameObject CL_Arrow = Instantiate(Arrow, enemyBase.transform.position, Quaternion.identity);
        Rigidbody2D rb = CL_Arrow.GetComponent<Rigidbody2D>();

        float A_Random = Random.Range(-40, 40) * Mathf.Deg2Rad;

        EnemyAttack enemyAttack = CL_Arrow.GetComponent<EnemyAttack>();
        enemyAttack.enemyBase = enemyBase;

        Vector2 PlayerLookDirecton = enemyBase.Player.transform.position - enemyBase.transform.position;

        Vector2 RadomDirection = new Vector2(
        Mathf.Cos(A_Random) * PlayerLookDirecton.x - Mathf.Sin(A_Random) * PlayerLookDirecton.y,
        Mathf.Sin(A_Random) * PlayerLookDirecton.x + Mathf.Cos(A_Random) * PlayerLookDirecton.y);

        rb.velocity = RadomDirection.normalized * 2;
        CL_Arrow.transform.up = RadomDirection;
        CL_Arrow.transform.Rotate(0, 0, 90);

        enemyBase.audioManager.isPlaySE(ShotAudio);
        Destroy(CL_Arrow, 5f);

    }
}
