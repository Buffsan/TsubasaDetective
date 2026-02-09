using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class KINGYO_LeaderKoaka_AI : EnemyMoveBase
{
    [SerializeField] GameObject Koaka;
    [SerializeField] int SpawnEnemyNumber = 16;

    int Phase = 0;

    float PhaseCount = 0;
    float PhaseCount1 = 0;
    Vector2 SaveAngle = Vector2.zero;
    //Vector2 SavePos = Vector2.zero;

    bool one = false;
    Vector2 Direction;
    bool trune = false;
    public float RandomTime;

    public float WaitCunt = 0;
    public override void EnemyMovePlay()
    {
        if (!one)
        {
            one = true;
            RandomTime = Random.Range(2, 5f);
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
            RandomTime = Random.Range(5f, 10f);
        }

        enemyBase.PlayerDistance = Vector2.Distance(enemyBase.Player.transform.position, enemyBase.gameObject.transform.position);
        enemyBase.animator.SetInteger("Anim", 1);
        enemyBase.PlayerDirection = enemyBase.Player.transform.position - enemyBase.transform.position;
        float angle = Mathf.Atan2(enemyBase.PlayerDirection.y, enemyBase.PlayerDirection.x) * Mathf.Rad2Deg;
        float offset = 100f / (enemyBase.PlayerDistance + 1f) * 3f; // 距離が遠いほど小さく

        if (trune)
        {
            angle += offset;
        }
        else
        {
            angle -= offset;
        }
        
        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

        if (direction.y < 0)
        {
            enemyBase.EnemyAnimBody.transform.localScale = new Vector2(3, 3);
        }
        else
        {
            enemyBase.EnemyAnimBody.transform.localScale = new Vector2(-3, 3);
        }

        enemyBase.rb.velocity = direction * enemyBase.SPEED;
    }
    public override void EnemyAttackPlay()
    {
        enemyBase.AttackCount += Time.deltaTime;
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
            if (enemyBase.AttackCount > 0.1)
            {
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;

                // プレイヤー → 自分 の方向ベクトル
                Vector2 direction = (transform.position - enemyBase.Player.transform.position).normalized;

                // 反対方向に20m（単位）離れた座標を計算
                Vector2 targetPos = (Vector2)transform.position + direction * 10f;
                for (int i = 0; i < SpawnEnemyNumber; i++)
                {
                    Vector2 randomPos = new Vector2(Random.Range(-3f, 3), Random.Range(-2f, 2));
                    GameObject CL_koaka = SpawnNoListEnemy(Koaka, targetPos + randomPos, Quaternion.identity);
                    Destroy(CL_koaka, 30);
                }
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            //enemyBase.isPlayerLookBase();
            enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * enemyBase.SPEED * 1.2f;
            if (enemyBase.AttackCount > 0.1f)
            {
                enemyBase.mode = EnemyBase.ModeType.M4;
                enemyBase.AttackCount = 0;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {
            enemyBase.rb.velocity = new Vector2(enemyBase.rb.velocity.x * 0.5f, enemyBase.rb.velocity.y * 0.5f);
            if (enemyBase.AttackCount > 0.1)
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
            Destroy(CL_Attack, 0.1f);

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