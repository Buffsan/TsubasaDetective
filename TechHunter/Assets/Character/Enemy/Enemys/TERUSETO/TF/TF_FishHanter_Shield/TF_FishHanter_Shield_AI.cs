using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TF_FishHanter_Shield_AI : EnemyMoveBase
{
    [SerializeField] GameObject RockNeedle;
    [SerializeField] float baseDistance = 0.5f;
    [SerializeField] float randomOffset = 0.5f;
    [SerializeField] int RockNumbber = 20;

    
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
            if (enemyBase.AttackCount > 1)
            {
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            enemyBase.isPlayerLookBase();
            enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * 1.3f;
            if (enemyBase.AttackCount > 0.8)
            {
                enemyBase.mode = EnemyBase.ModeType.M4;
                enemyBase.AttackCount = 0;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {
            enemyBase.rb.velocity = new Vector2(enemyBase.rb.velocity.x * 0.5f, enemyBase.rb.velocity.y * 0.5f);
            if (enemyBase.AttackCount > 0.2)
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

            for (int i = 0; i < RockNumbber; i++) 
            {
                StartCoroutine(PlaceObject(i, enemyBase.transform.position.normalized, enemyBase.Player.transform.position.normalized));
            }

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
    private IEnumerator PlaceObject(float index, Vector3 savepos, Vector3 playerpos)
    {
        yield return new WaitForSeconds(index / 30);


        // 距離に応じて前方に配置
        float distance = baseDistance * (index + 5);
        Vector3 basePos = savepos + playerpos * distance;

        // 位置にランダムなズレを追加（円形範囲内で）
        Vector2 randomCircle = Random.insideUnitCircle * randomOffset * index;
        Vector3 finalPos = basePos + new Vector3(randomCircle.x, randomCircle.y, 0);

        // オブジェクトを設置
        GameObject CL_litSlash = Instantiate(RockNeedle, finalPos, Quaternion.identity);
        Destroy( CL_litSlash ,5);
    }
}
