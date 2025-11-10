using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kingyo_Koaka : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;
    [SerializeField] GameObject Target;
    [SerializeField] EnemyContoroller enemyContoroller;
    bool TargetMode = false;

    PlayerController controller => PlayerController.Instance;

    bool one = false;
    Vector2 Direction;
    public override void SpecialPlay()
    {
        base.SpecialPlay();
    }
    public override void EnemyMovePlay()
    {
        if(Target)TargetMode = true;
        if (TargetMode)
        {
            Vector2 Direction = Target.transform.position - transform.position;
            enemyBase.rb.velocity = Direction.normalized * enemyBase.SPEED;
            enemyBase.isLookDistance();
            if (!Target) enemyContoroller.Choice_MoveToBase = EnemyContoroller.MoveToBase.Out;
        }
        else 
        { 
        if (!one) {
            Vector2 randomPos = new Vector2 (Random.Range(-1f,1f), Random.Range(-1f, 1f));
            Direction = ((Vector2)controller.transform.position + randomPos) - (Vector2)transform.position;
            one = true;
        }
        
        enemyBase.rb.velocity = Direction.normalized * enemyBase.SPEED;
        enemyBase.isLookDistance();
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
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {
            enemyBase.isPlayerLookBase();
            if (enemyBase.AttackCount > 0.1)
            {
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;
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

            enemyBase.audioManager.isPlaySE(enemyBase.AttackAudio);
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
