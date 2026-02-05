using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CG_FarScout_AI : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;
    [SerializeField] EnemyContoroller enemyContoroller;

    int Phase = 0;

    float PhaseCount = 0;
    float PhaseCount1 = 0;
    Vector2 SaveAngle = Vector2.zero;
    Vector2 SavePosPhase = Vector2.zero;
    Vector2 Direction;
    Vector2 AttackDirection;
    [SerializeField]GameObject DengerArea;
    GameObject SaveTarget;

    public override void EnemyMovePlay()
    {
        enemyBase.PlayerDistance = Vector2.Distance(enemyBase.Player.transform.position, enemyBase.gameObject.transform.position);
        enemyBase.isPlayerLookBase();
        enemyBase.rb.velocity = enemyBase.PlayerDirection;
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
            if (enemyBase.AttackCount > 0.2)
            {
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;
                Direction = PlayerController.Instance.transform.position - gameObject.transform.position;
                AttackDirection = Direction;
                int radom = Random.Range(0, 3);
                switch (radom) 
                { 
                
                    case 0:
                        Direction = new Vector2(-Direction.x, Direction.y);
                        break;
                    case 1:
                        Direction = new Vector2(Direction.x, -Direction.y);
                        break;
                    case 2:
                        Direction = new Vector2(-Direction.x, -Direction.y);
                        break;

                }
                DengerSpawn(AttackDirection);
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            //enemyBase.isPlayerLookBase();
            enemyBase.rb.velocity = Direction.normalized * enemyBase.SPEED * 1.2f;
            if (enemyBase.AttackCount > 0.5f)
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
           
            Shot(AttackDirection);

            enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
            enemyBase.animator.Play("UŒ‚");
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
                enemyBase.animator.Play("‘Ò‹@");
                enemyBase.animator.SetInteger("Anim", 0);
            }
        }
    }
    void Shot( Vector2 targetDirection) 
    {
        

        float angleStep = 15;
        float angle = -angleStep;
        
        for (int i = 0; i < 3; i++)
        {//Debug.Log("“Š‚°‚é");

            GameObject bullet = Instantiate(Arrow, gameObject.transform.position, Quaternion.identity);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            Destroy(bullet, 1.5f);

            Vector2 bulletDirection = Quaternion.Euler(0, 0, angle) * targetDirection.normalized;
            rb.velocity = bulletDirection * 12;
            bullet.transform.up = bulletDirection;
            angle += angleStep;

        }
    }
    void DengerSpawn(Vector2 targetDirection) 
    {
        float angleStep = 15;
        float angle = -angleStep;
        for (int i = 0; i < 3; i++)
        {//Debug.Log("“Š‚°‚é");

            GameObject denger = DengerAreaSpawn(DengerArea, transform.position, 1);
            denger.transform.parent = transform;
            denger.transform.up= Quaternion.Euler(0, 0, angle) * targetDirection.normalized;
            angle += angleStep;
        }
    }
}
