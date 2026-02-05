using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_CG_TANK_AI : EnemyMoveBase
{
    [SerializeField] GameObject Koaka;
    [SerializeField] GameObject RockNeedle;
    [SerializeField] GameObject DengerLenght;
    [SerializeField] GameObject AttackLenght;
    [SerializeField] GameObject AssultAttack;

    [SerializeField] List<GameObject> Taiya = new List<GameObject>();

    int Phase = 0;

    float PhaseCount = 0;
    float PhaseCount1 = 0;
    Vector2 SaveAngle = Vector2.zero;
    //Vector2 SavePos = Vector2.zero;

    bool one = false;
    Vector2 Direction;
    bool trune = false;
    public float RandomTime;
    float AttackCount1 = 0;

    public float WaitCunt = 0;


    private IEnumerator myRunningCoroutine; // 実行中のコルーチンを保持するための変数
    public float duration = 1;
    public float arcHeight = 30;
    bool StandOk = false;
    float waitCount = 0;
    Vector2 targetPos = Vector2.zero;
    [SerializeField] GameObject Denger_AttackArea;
    [SerializeField] GameObject Denger_Attackarea1;
    [SerializeField] GameObject AttackArea;

    public override void EnemyConfusionPlay()
    {
        StandOk = false;
        A_Phase = 0;
        enemyBase.AttackCount = 0;
        AttackArea.SetActive(false);
    }
    public override void EnemyStanPlay()
    {
        StandOk = false;
        A_Phase = 0;
        enemyBase.AttackCount = 0;
        AttackArea.SetActive(false);
    }

    public override void EnemyMovePlay()
    {
        enemyBase.isPlayerLookBase();
        enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * enemyBase.SPEED;
        foreach (var t in Taiya) 
        {
            t.transform.Rotate(0, 0, 10);
        }
        waitCount += Time.fixedDeltaTime;
        if (waitCount < 3) return;
        if (enemyBase.PlayerDistance > 3) return;

        enemyBase.moveType = EnemyBase.MoveType.Attack;
        
    }
    public override void EnemyAttackPlay()
    {
        enemyBase.AttackCount += Time.deltaTime;
        foreach (var t in Taiya)
        {
            t.transform.Rotate(0, 0, 30);
        }
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            enemyBase.audioManager.PlaySE(enemyBase.AttackStayAudio);
            enemyBase.rb.velocity = Vector3.zero;
            enemyBase.mode = EnemyBase.ModeType.M2;
            enemyBase.animator.SetInteger("Anim", 2);
            enemyBase.isPlayerLookBase();

            GameObject CL_Denger = Instantiate(Denger_Attackarea1, transform.position, Quaternion.identity);
            CL_Denger.transform.up = enemyBase.PlayerDirection.normalized;
            Destroy(CL_Denger, 1);
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {

            if (enemyBase.AttackCount > 1)
            {
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;
                AttackArea.SetActive(true);

            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            //enemyBase.isPlayerLookBase();
            AttackCount1 += Time.fixedDeltaTime;
            if (AttackCount1 > 0.05)
            {
                AttackCount1 = 0;
                
            }
            enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * enemyBase.SPEED * 4;
            if (enemyBase.AttackCount > 2f)
            {
                enemyBase.mode = EnemyBase.ModeType.M4;
                enemyBase.AttackCount = 0;
                AttackArea.SetActive(false);
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


            enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
            enemyBase.animator.SetInteger("Anim", 3);
            enemyBase.mode = EnemyBase.ModeType.M6;

        }
        if (enemyBase.mode == EnemyBase.ModeType.M6)
        {

            if (enemyBase.AttackCount > 0.1)
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
