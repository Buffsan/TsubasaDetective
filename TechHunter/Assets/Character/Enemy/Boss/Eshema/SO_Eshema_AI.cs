using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SO_Eshema_AI : Boss_MoveBase
{
    [SerializeField] GameObject ARM;
    int AttackPhase = 0;
    int AttackNumber = 0;
    Vector2 targetPos = Vector2.zero;
    Vector2 targetDirection = Vector2.zero;

    [SerializeField] GameObject slashAttack;
    [SerializeField] GameObject asaltAttackArea;
    [SerializeField] GameObject Bullet;

    [SerializeField] GameObject Denger_AttackArea_Slash;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void M_AI()
    {

        if (enemyBase.moveType != EnemyBase.MoveType.Attack)
        {

            AttackWaitTime += Time.deltaTime;

            enemyBase.rb.velocity = Vector3.zero;

            if (phase == Phase.P1)
            {

                if (enemyBase.HP < enemyBase.MAXHP / 2)
                {
                    phase = Phase.P2;
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.attackType = EnemyBase.AttackType.A7;
                }

                if (AttackWaitTime > 1.2)
                {
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;

                    float random = Random.RandomRange(1, 6);
                    random = 3;
                    if (random == 1)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A1;
                    }
                    if (random == 2)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A2;
                    }
                    if (random == 3)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A3;
                    }
                    if (random == 4)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A4;
                    }
                    if (random == 5)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A3;
                    }
                    if (random == 6)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A2;
                    }

                }

            }
            if (phase == Phase.P2)
            {


                if (AttackWaitTime > 2)
                {
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;

                    if (AllAttackNumber > 3)
                    {
                        AllAttackNumber = 0;
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A1;
                    }
                    else
                    {
                        float random = Random.RandomRange(1, 6);
                        AllAttackNumber++;
                        if (random == 1)
                        {
                            enemyBase.moveType = EnemyBase.MoveType.Attack;
                            enemyBase.attackType = EnemyBase.AttackType.A8;
                        }
                        if (random == 2)
                        {
                            enemyBase.moveType = EnemyBase.MoveType.Attack;
                            enemyBase.attackType = EnemyBase.AttackType.A6;
                        }
                        if (random == 3)
                        {
                            enemyBase.moveType = EnemyBase.MoveType.Attack;
                            enemyBase.attackType = EnemyBase.AttackType.A9;
                        }
                        if (random == 4)
                        {
                            enemyBase.moveType = EnemyBase.MoveType.Attack;
                            enemyBase.attackType = EnemyBase.AttackType.A4;
                        }
                        if (random == 5)
                        {
                            enemyBase.moveType = EnemyBase.MoveType.Attack;
                            enemyBase.attackType = EnemyBase.AttackType.A3;
                        }
                        if (random == 6)
                        {
                            enemyBase.moveType = EnemyBase.MoveType.Attack;
                            enemyBase.attackType = EnemyBase.AttackType.A2;
                        }

                    }

                }
            }
        }

    }
    public override void M_Attack()
    {

        enemyBase.AttackCount += Time.deltaTime;
        switch (enemyBase.attackType)
        {

            case EnemyBase.AttackType.A1:
                A1();
                break;
            case EnemyBase.AttackType.A2:
                A2();
                break;
            case EnemyBase.AttackType.A3:
                A3();
                break;
            case EnemyBase.AttackType.A4:
                //A4();
                break;
            case EnemyBase.AttackType.A5:
                //A5();
                break;
            case EnemyBase.AttackType.A6:
                //A6();
                break;
            case EnemyBase.AttackType.A7:
                //A7();
                break;
            case EnemyBase.AttackType.A8:
                //A8();
                break;
            case EnemyBase.AttackType.A9:
                //A9();
                break;
            case EnemyBase.AttackType.A10:
                //A10();
                break;

        }

    }
    public override void M_BaseMove()
    {

    }
    void NextPhase() 
    {
        AttackPhase++;
        enemyBase.AttackCount = 0;
    }
    void Finish() 
    {
        AttackPhase=0;
        AttackNumber = 0;
        enemyBase.AttackCount = 0;
        enemyBase.moveType = EnemyBase.MoveType.Move;
        ARM.SetActive (false);
    }
    //í«ê’Ç∆éaåÇ 
    void A1() 
    {
        if (AttackPhase == 0) 
        {
            bool FindPlayerPos = false;
            while (!FindPlayerPos) 
            { 
                Vector2 TargetFindPlayerPos = new Vector2 (Random.Range(-2f,2f) + enemyBase.Player.transform.position.x, Random.Range(-2f, 2f) + enemyBase.Player.transform.position.y);
                FindPlayerPos = IsTagAtPosition(TargetFindPlayerPos,"Stage_NotFloor");
                    if (FindPlayerPos) 
                    { 
                        targetPos = TargetFindPlayerPos;
                    targetDirection = targetPos - (Vector2)transform.position;
                    enemyBase.isTargetLook(targetPos);
                }
            }
            enemyBase.animator.Play("ï‡çs",0,0);
            
            NextPhase();
        }
        if (AttackPhase == 1) 
        {
            float TargetDistanse = Vector2.Distance(targetPos,transform.position);
            
            if (TargetDistanse > 1)
            {
                enemyBase.rb.velocity = targetDirection.normalized * enemyBase.SPEED;
            }
            else 
            {
                NextPhase();
                enemyBase.animator.Play("çUåÇë“ã@",0,0);
                enemyBase.isPlayerLookBase();
            }
        }
        if (AttackPhase == 2) 
        {
            enemyBase.rb.velocity *= 0.8f;
            if (enemyBase.AttackCount > 0.5) 
            {
                NextPhase();
                targetPos = enemyBase.Player.transform.position;
                targetDirection =  targetPos -(Vector2)transform.position;
                enemyBase.animator.Play("çUåÇ", 0, 0);
            }
        }
        if (AttackPhase == 3) 
        {
             

            if (enemyBase.AttackCount > 0.3) 
            {

                NextPhase();
            }
            enemyBase.rb.velocity = targetDirection.normalized * enemyBase.SPEED * 2;

        }
        if (AttackPhase == 4) 
        {
            if (enemyBase.AttackCount > 0.2)
            {
                int randomRe = Random.Range(0,4);
                if (randomRe != 1 && AttackNumber < 2)
                {
                    enemyBase.AttackCount = 0;
                    AttackPhase = 0;
                    AttackNumber ++;
                }
                else
                {
                    Finish();
                    enemyBase.animator.Play("ë“ã@", 0, 0);
                }
            }
            else 
            {
                enemyBase.rb.velocity *= 0.2f;
            }
        }
    }
    //éaåÇîÚÇŒÇµ
    void A2() 
    {
        if (AttackPhase == 0)
        {
            
            NextPhase();
             enemyBase.isPlayerLookBase();

            enemyBase.animator.Play("çUåÇë“ã@", 0, 0);

            for (int i = 0; i < 5; i++)
            {
                GameObject CL_AttackPPP = Instantiate(Denger_AttackArea_Slash, enemyBase.transform.position, Quaternion.identity);
                CL_AttackPPP.transform.up = enemyBase.PlayerDirection.normalized;
                Vector2 NewLookDirection = Quaternion.Euler(0, 0, -80 + 40 * i) * enemyBase.PlayerDirection.normalized;
                CL_AttackPPP.transform.up = NewLookDirection.normalized;
                Destroy(CL_AttackPPP, 1);
            }
        }
        if (AttackPhase == 1) 
        {
            if (enemyBase.AttackCount > 0.3) 
            {
                NextPhase();
            }
          
        }
        if (AttackPhase == 2) 
        {
            if (enemyBase.AttackCount > 0.2)
            {
                enemyBase.animator.Play("çUåÇ", 0, 0);
               
                for (int i = 0; i < 5; i++)
                {
                    GameObject CL_Slash = Instantiate(slashAttack, enemyBase.transform.position, Quaternion.identity);
                    Vector2 NewLookDirection = Quaternion.Euler(0, 0, -80 + 40 * i) * enemyBase.PlayerDirection.normalized;
                    if (enemyBase.PlayerDirection.x > 0)
                    {
                        CL_Slash.transform.localScale = new Vector2(CL_Slash.transform.localScale.x, CL_Slash.transform.localScale.y);
                    }else{
                        CL_Slash.transform.localScale = new Vector2(-CL_Slash.transform.localScale.x, CL_Slash.transform.localScale.y);}
                    Rigidbody2D rb = CL_Slash.GetComponent<Rigidbody2D>();
                    rb.velocity = NewLookDirection.normalized * 8;
                    Destroy(CL_Slash, 10);
                }
                if (AttackNumber >= 2)
                {
                    enemyBase.AttackCount = 0;
                    enemyBase.attackType = EnemyBase.AttackType.A1;
                    AttackPhase = 0;
                }
                else 
                { 
                    enemyBase.AttackCount = 0;
                    AttackPhase = 0;AttackNumber++;
                }
                
                
            }
        }
        if (AttackPhase == 2) 
        { 
        
        }
    }
    //éÀåÇ
    void A3() 
    {
        if (AttackPhase == 0) 
        {
            enemyBase.animator.Play("èeç\Ç¶",0,0);
            NextPhase();
        }
        if (AttackPhase == 1) 
        {
            if (enemyBase.AttackCount > 0.2) 
            {
                NextPhase();
            }
        }
        if (AttackPhase == 2) 
        {
            enemyBase.animator.Play("èeçUåÇë“ã@", 0, 0);
            ARM.SetActive(true);
            NextPhase();
        }
        if (AttackPhase==3) 
        {
            enemyBase.isPlayerLookBase();
            ARM.transform.up = enemyBase.PlayerDirection;
            ARM.transform.Rotate(0,0,90);
            if (enemyBase.AttackCount > 0.25)
            {
                BulletShot();
                NextPhase();
            }
        }
        if (AttackPhase == 4) 
        {
            Finish();
        }
    }
    void BulletShot() 
    {

        GameObject CL_Bullet = Instantiate(Bullet, ARM.transform.position, Quaternion.identity);
        CL_Bullet.transform.up = enemyBase.PlayerDirection.normalized;

        Rigidbody2D CL_rb = CL_Bullet.GetComponent<Rigidbody2D>();
        CL_rb.velocity = enemyBase.PlayerDirection.normalized * 20;
        Destroy(CL_Bullet, 1);
    
    }

    bool IsTagAtPosition(Vector2 position, string tag)
    {
        Collider2D hit = Physics2D.OverlapPoint(position);

        if (hit != null && hit.CompareTag(tag))
        {
            return true;
        }

        return false;
    }
}

