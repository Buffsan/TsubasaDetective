using System.Collections;
using System.Collections.Generic;
using Unity.Android.Types;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ParticipantInThemeal_AI : Boss_MoveBase
{

    private IEnumerator myRunningCoroutine; // 実行中のコルーチンを保持するための変数
    // Inspectorから目標地点のTransformを設定
    public Transform targetPos;
    bool StandOk = false;
    // 移動にかかる時間
    public float duration = 1;

    // 山なりの高さ
    public float arcHeight = 30;

    [SerializeField] float PlayerDistanceNow = 0;

    public List<GameObject> AttackAreas = new List<GameObject>();
    public List<EnemyAttack> enemyAttacks = new List<EnemyAttack>();

    [SerializeField] GameObject CarseSlash;
    [SerializeField] GameObject BigCarseSlash;
    [SerializeField] GameObject PPPAttack;
    [SerializeField] GameObject PPPAttack1;
    [SerializeField] GameObject RockNeedle;

    int AttackNumber2 = 0;

    // Start is called before the first frame update
    void Start()
    {/*
        int i=0;
        foreach (var a in AttackAreas) 
        {
            enemyAttacks.Add(a.GetComponent<EnemyAttack>());
            i++;
        }*/
        enemyBase.DefoScale = 3;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerDistanceNow = Vector2.Distance(enemyBase.transform.position,enemyBase.Player.transform.position);

        if (enemyBase.status == EnemyBase.Status.ConfusionResistance) 
        {
            AttackPhase = 0;
            StandOk = false;
            AttackNumber = 0;
            AttackNumber2 = 0;

            if (myRunningCoroutine != null)
            {
                StopCoroutine(myRunningCoroutine); // 特定のコルーチンを停止
                Debug.Log("コルーチンを停止しました。");
                myRunningCoroutine = null; // 参照をクリア
            }
        }
    }
    public override void M_AI() 
    {

        if (enemyBase.moveType != EnemyBase.MoveType.Attack)
        {

            AttackWaitTime += Time.deltaTime;

            enemyBase.rb.velocity = Vector3.zero;



            if (phase == Phase.P1)
            {


                if (AttackWaitTime > 4.5)
                {
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;

                    float random = Random.RandomRange(3, 7);
                    //float random = 6;
                    //int random = 0;

                    if (PlayerDistanceNow > 8) 
                    {
                        random = 7;
                    }

                    if (random == 0)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A1;//8
                    }
                    if (random == 1)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A2;//8
                    }
                    if (random == 2)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A3;
                    }
                    if (random == 3)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A4;
                    }
                    if (random == 4)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A5;
                    }
                    if (random == 5)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A6;
                    }
                    if (random == 6)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A7;
                    }
                    if (random == 7)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A8;
                    }

                }

            }
            if (phase == Phase.P2)
            {


                if (AttackWaitTime > 3)
                {
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;

                    float random = Random.RandomRange(0, 4);
                    //int random = 0;

                    if (random == 0)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A1;//8
                    }
                    if (random == 1)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A2;//8
                    }
                    if (random == 2)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A3;
                    }
                    if (random == 3)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A4;
                    }
                    if (random == 4)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A5;
                    }
                    if (random == 5)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A6;
                    }
                    if (random == 6)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A7;
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
                A4();
                break;
            case EnemyBase.AttackType.A5:
                A5();
                break;
            case EnemyBase.AttackType.A6:
                A6();
                break;
            case EnemyBase.AttackType.A7:
                A7();
                break;
            case EnemyBase.AttackType.A8:
                A8();
                break;
            case EnemyBase.AttackType.A9:
                //A9();
                break;
            case EnemyBase.AttackType.A10:
                //A10();
                break;

        }
    }
    public void AttackSpawnBase(GameObject CL_BASE) 
    {
        EnemyAttack enemyattack = CL_BASE.GetComponent<EnemyAttack>();
        enemyattack.enemyBase = enemyBase;
    }

    void A1() 
    {
        if (AttackPhase == 0) 
        {
            enemyBase.animator.Play("跳躍準備");
            if (enemyBase.AttackCount > 0.8) 
            {
                targetPos = enemyBase.Player.transform;
                StopAllCoroutines();
                StartCoroutine(MoveInArc());

                NEXT();
            }
        }
        if (AttackPhase == 1) 
        {
            if (StandOk) 
            {
                StandOk = false;
                NEXT();
            }
        }
        if (AttackPhase == 2) 
        {
            GameObject CL_Attack = Instantiate(AttackAreas[0], enemyBase.transform.position, Quaternion.identity);
            AttackSpawnBase(CL_Attack);
            Destroy(CL_Attack,0.2f);
            enemyBase.animator.Play("待機");


            for (int j = 0; j < 15; j++)
            {
                StartCoroutine(NeedleSpawnLing(0.1f * j, j * 0.5f, 0));
            }

            NEXT();
        }
            if (AttackPhase == 3) 
        {

            if (enemyBase.AttackCount > 0.2) 
            {
                
                ATTCKFINISH();
               
            }
        }
    }
    void A2() 
    {

        if (AttackPhase == 0) 
        {
            enemyBase.isPlayerLookBase();
            enemyBase.animator.Play("攻撃待機",0,0);
            if (AttackNumber == 3) 
            {
                for (int i = 0; i < 3; i++)
                {

                    GameObject CL_AttackPPP = Instantiate(PPPAttack, enemyBase.transform.position, Quaternion.identity);
                    CL_AttackPPP.transform.up = enemyBase.PlayerDirection.normalized;

                    Vector2 NewLookDirection = Quaternion.Euler(0, 0, -30 + 30 * i) * enemyBase.PlayerDirection.normalized;
                    CL_AttackPPP.transform.up = NewLookDirection.normalized;

                    Destroy(CL_AttackPPP, 1);
                }
            }
            else
            {
                GameObject CL_AttackPPP = Instantiate(PPPAttack, enemyBase.transform.position, Quaternion.identity);
                CL_AttackPPP.transform.up = enemyBase.PlayerDirection.normalized;
                //Animator animator = CL_AttackPPP.GetComponent<Animator>();
                //animator.speed = 1.2f;
                Destroy(CL_AttackPPP, 1);
            }

            NEXT();
        }
        if (AttackPhase == 1) 
        {
            if (enemyBase.AttackCount > 0.6) 
            {
                enemyBase.animator.Play("攻撃");
                NEXT();
            }
        }
        if (AttackPhase == 2) 
        {
            if (AttackNumber == 3) 
            {
                for (int i = 0; i < 3; i++)
                {

                    GameObject CL_Slash = Instantiate(CarseSlash, enemyBase.transform.position, Quaternion.identity);

                    Vector2 NewLookDirection = Quaternion.Euler(0, 0, -30 + 30 * i) * enemyBase.PlayerDirection.normalized;

                    if (enemyBase.PlayerDirection.x > 0)
                    {
                        CL_Slash.transform.localScale = new Vector2(CL_Slash.transform.localScale.x, CL_Slash.transform.localScale.y);
                    }
                    else
                    {
                        CL_Slash.transform.localScale = new Vector2(-CL_Slash.transform.localScale.x, CL_Slash.transform.localScale.y);
                    }

                    Rigidbody2D rb = CL_Slash.GetComponent<Rigidbody2D>();
                    rb.velocity = NewLookDirection.normalized * 10;
                    Destroy(CL_Slash, 10);

                    AttackSpawnBase(CL_Slash);
                }
            }
            else
            {
                GameObject CL_Slash = Instantiate(CarseSlash, enemyBase.transform.position, Quaternion.identity);
                if (enemyBase.PlayerDirection.x > 0)
                {
                    CL_Slash.transform.localScale = new Vector2(CL_Slash.transform.localScale.x, CL_Slash.transform.localScale.y);
                }
                else
                {
                    CL_Slash.transform.localScale = new Vector2(-CL_Slash.transform.localScale.x, CL_Slash.transform.localScale.y);
                }

                Rigidbody2D rb = CL_Slash.GetComponent<Rigidbody2D>();
                rb.velocity = enemyBase.PlayerDirection.normalized * 10;
                Destroy(CL_Slash, 10);

                AttackSpawnBase(CL_Slash);
            }

            NEXT();
        }
        if (AttackPhase == 3)
        {

            if (enemyBase.AttackCount > 0.2)
            {
                enemyBase.animator.Play("待機");
                AttackNumber++;
                if (AttackNumber > 3)
                {
                    NEXT();
                }
                else 
                {
                    AttackPhase = 0;
                    enemyBase.AttackCount = 0;
                }
            }
        }
        //------------------------------------跳躍
        if (AttackPhase == 4)
        {
            enemyBase.animator.Play("跳躍準備");
            if (enemyBase.AttackCount > 1.5)
            {
                targetPos = enemyBase.Player.transform;
                StopAllCoroutines();
                StartCoroutine(MoveInArc());

                NEXT();
            }
        }
        if (AttackPhase == 5)
        {
            if (StandOk)
            {
                StandOk = false;
                NEXT();
            }
        }
        if (AttackPhase == 6)
        {
            GameObject CL_Attack = Instantiate(AttackAreas[0], enemyBase.transform.position, Quaternion.identity);
            AttackSpawnBase(CL_Attack);
            Destroy(CL_Attack, 0.2f);
            enemyBase.animator.Play("待機");

            for (int j = 0; j < 10; j++)
            {
                StartCoroutine(NeedleSpawnLing(0.1f*j, j*0.6f,0));
            }

            NEXT();
        }
        if (AttackPhase == 7)
        {

            if (enemyBase.AttackCount > 0.2)
            {

                ATTCKFINISH();

            }
        }

    }
    void A3() 
    {

        if (AttackPhase == 0)
        {
            enemyBase.isPlayerLookBase();
            enemyBase.animator.Play("攻撃待機", 0, 0);
            NEXT();
        }
        if (AttackPhase == 1)
        {
            if (enemyBase.AttackCount > 1)
            {
                enemyBase.animator.Play("攻撃");
                NEXT();
            }
        }
        if (AttackPhase == 2)
        {
            GameObject CL_Slash = Instantiate(CarseSlash, enemyBase.transform.position, Quaternion.identity);
            if (enemyBase.PlayerDirection.x > 0)
            {
                CL_Slash.transform.localScale = new Vector2(CL_Slash.transform.localScale.x, CL_Slash.transform.localScale.y);
            }
            else
            {
                CL_Slash.transform.localScale = new Vector2(-CL_Slash.transform.localScale.x, CL_Slash.transform.localScale.y);
            }

            Rigidbody2D rb = CL_Slash.GetComponent<Rigidbody2D>();
            rb.velocity = enemyBase.PlayerDirection.normalized * 10;
            Destroy(CL_Slash, 10);

            AttackSpawnBase(CL_Slash);

            NEXT();
        }
        if (AttackPhase == 3)
        {

            if (enemyBase.AttackCount > 0.4)
            {
                enemyBase.animator.Play("待機");
                ATTCKFINISH();

            }
        }

    }
    void A4() 
    {
        if (AttackPhase == 0)
        {
            enemyBase.isPlayerLookBase();
            enemyBase.animator.Play("攻撃待機", 0, 0);

            GameObject CL_AttackPPP = Instantiate(PPPAttack, enemyBase.transform.position, Quaternion.identity);
            CL_AttackPPP.transform.up = enemyBase.PlayerDirection.normalized;
            //Animator animator = CL_AttackPPP.GetComponent<Animator>();
            //animator.speed = 1.2f;
            Destroy(CL_AttackPPP, 1);

            NEXT();
        }
        if (AttackPhase == 1)
        {
            if (enemyBase.AttackCount > 0.6)
            {
                enemyBase.animator.Play("攻撃");
                NEXT();
            }
        }
        if (AttackPhase == 2)
        {
            GameObject CL_Slash = Instantiate(CarseSlash, enemyBase.transform.position, Quaternion.identity);
            if (enemyBase.PlayerDirection.x > 0)
            {
                CL_Slash.transform.localScale = new Vector2(CL_Slash.transform.localScale.x, CL_Slash.transform.localScale.y);
            }
            else
            {
                CL_Slash.transform.localScale = new Vector2(-CL_Slash.transform.localScale.x, CL_Slash.transform.localScale.y);
            }

            Rigidbody2D rb = CL_Slash.GetComponent<Rigidbody2D>();
            rb.velocity = enemyBase.PlayerDirection.normalized * 10;
            Destroy(CL_Slash, 10);

            AttackSpawnBase(CL_Slash);

            NEXT();
        }
        if (AttackPhase == 3)
        {

            if (enemyBase.AttackCount > 0.2)
            {
                enemyBase.animator.Play("待機");
                AttackNumber++;
                if (AttackNumber > 2)
                {
                    NEXT();
                }
                else
                {
                    AttackPhase = 0;
                    enemyBase.AttackCount = 0;
                }
            }
        }
        //------------------------------------跳躍
        if (AttackPhase == 4)
        {
            enemyBase.animator.Play("跳躍準備");
            if (enemyBase.AttackCount > 0.8)
            {
                targetPos = enemyBase.Player.transform;
                StopAllCoroutines();
                StartCoroutine(MoveInArc());

                NEXT();
            }
        }
        if (AttackPhase == 5)
        {
            if (StandOk)
            {
                StandOk = false;
                NEXT();
            }
        }
        if (AttackPhase == 6)
        {
            GameObject CL_Attack = Instantiate(AttackAreas[0], enemyBase.transform.position, Quaternion.identity);
            AttackSpawnBase(CL_Attack);
            Destroy(CL_Attack, 0.2f);
            enemyBase.animator.Play("待機");

            NEXT();
        }
        if (AttackPhase == 7)
        {

            if (enemyBase.AttackCount > 0.2)
            {

                if (AttackNumber2 > 2)
                {
                    ATTCKFINISH();
                }
                else 
                {
                    AttackPhase = 0;
                    enemyBase.AttackCount = 0;
                    AttackNumber2++;
                }

            }
        }
    }
    void A5() 
    {
        if (AttackPhase == 0)
        {
            enemyBase.isPlayerLookBase();
            enemyBase.animator.Play("攻撃待機横", 0, 0);
            NEXT();
            GameObject CL_AttackPPP = Instantiate(PPPAttack, enemyBase.transform.position, Quaternion.identity);

            CL_AttackPPP.transform.up = enemyBase.PlayerDirection.normalized;
            CL_AttackPPP.transform.localScale = new Vector2(4, 2);
            Destroy(CL_AttackPPP, 1);

        }
        if (AttackPhase == 1)
        {
            if (enemyBase.AttackCount > 1)
            {
                enemyBase.animator.Play("攻撃横");
                NEXT();
            }
        }
        if (AttackPhase == 2)
        {
            GameObject CL_Slash = Instantiate(BigCarseSlash, enemyBase.transform.position, Quaternion.identity);
            CL_Slash.transform.up = enemyBase.PlayerDirection.normalized;
            Rigidbody2D rb = CL_Slash.GetComponent<Rigidbody2D>();
            rb.velocity = enemyBase.PlayerDirection.normalized *0.5f;
            Destroy(CL_Slash, 30);

            AttackSpawnBase(CL_Slash);

            NEXT();
        }
        if (AttackPhase == 3)
        {

            if (enemyBase.AttackCount > 0.4)
            {
                enemyBase.animator.Play("待機");
                //ATTCKFINISH();
                AttackNumber2 = 0;
                AttackNumber =0;
                enemyBase.AttackCount = 0;
                int randomint = Random.Range(0,3);
                if (randomint == 0)
                {
                    enemyBase.attackType = EnemyBase.AttackType.A2;
                }
                else if(randomint ==1)
                {
                    enemyBase.attackType = EnemyBase.AttackType.A4;
                }
                else
                {
                    enemyBase.attackType = EnemyBase.AttackType.A5;
                }

            }
        }
        
    }

    void A6() 
    {
        if (AttackPhase == 0)
        {
            enemyBase.isPlayerLookBase();
            enemyBase.animator.Play("攻撃待機横", 0, 0);
            NEXT();
            GameObject CL_AttackPPP = Instantiate(PPPAttack, enemyBase.transform.position, Quaternion.identity);

            CL_AttackPPP.transform.up = enemyBase.PlayerDirection.normalized;
            CL_AttackPPP.transform.localScale = new Vector2(4, 20);
            
            Destroy(CL_AttackPPP, 1);

        }
        if (AttackPhase == 1)
        {
            if (enemyBase.AttackCount > 1)
            {
                enemyBase.animator.Play("攻撃横");
                NEXT();
            }
        }
        if (AttackPhase == 2)
        {
            GameObject CL_Slash = Instantiate(BigCarseSlash, enemyBase.transform.position, Quaternion.identity);
            CL_Slash.transform.up = enemyBase.PlayerDirection.normalized;
            Rigidbody2D rb = CL_Slash.GetComponent<Rigidbody2D>();
            rb.velocity = enemyBase.PlayerDirection.normalized * 14f;
            Destroy(CL_Slash, 30);

            AttackSpawnBase(CL_Slash);

            NEXT();
        }
        if (AttackPhase == 3)
        {

            if (enemyBase.AttackCount > 0.7)
            {
                enemyBase.animator.Play("待機");
                NEXT();

            }
        }
        if (AttackPhase == 4)
        {
            enemyBase.isPlayerLookBase();
            enemyBase.animator.Play("攻撃待機", 0, 0);

            for (int i = 0; i < 3; i++) 
            {

                GameObject CL_AttackPPP = Instantiate(PPPAttack, enemyBase.transform.position, Quaternion.identity);
                 CL_AttackPPP.transform.up = enemyBase.PlayerDirection.normalized;

                Vector2 NewLookDirection = Quaternion.Euler(0, 0, -30 + 30*i) * enemyBase.PlayerDirection.normalized;
                CL_AttackPPP.transform.up = NewLookDirection.normalized;

                Destroy(CL_AttackPPP,1);
            }

            NEXT();
        }
        if (AttackPhase == 5)
        {
            if (enemyBase.AttackCount > 1)
            {
                enemyBase.animator.Play("攻撃");
                NEXT();
            }
        }
        if (AttackPhase == 6)
        {
            for (int i = 0; i < 3; i++)
            {

                GameObject CL_Slash = Instantiate(CarseSlash, enemyBase.transform.position, Quaternion.identity);

                Vector2 NewLookDirection = Quaternion.Euler(0, 0, -30 + 30 * i) * enemyBase.PlayerDirection.normalized;
                
                if (enemyBase.PlayerDirection.x > 0)
                {
                    CL_Slash.transform.localScale = new Vector2(CL_Slash.transform.localScale.x, CL_Slash.transform.localScale.y);
                }
                else
                {
                    CL_Slash.transform.localScale = new Vector2(-CL_Slash.transform.localScale.x, CL_Slash.transform.localScale.y);
                }

                Rigidbody2D rb = CL_Slash.GetComponent<Rigidbody2D>();
                rb.velocity = NewLookDirection.normalized * 10;
                Destroy(CL_Slash, 10);

                AttackSpawnBase(CL_Slash);
            }
            
            

            NEXT();
        }
        if (AttackPhase == 7)
        {

            if (enemyBase.AttackCount > 0.4)
            {
                enemyBase.animator.Play("待機");
                if (AttackNumber > 2)
                {
                    //ATTCKFINISH();
                    enemyBase.attackType = EnemyBase.AttackType.A1;
                    enemyBase.AttackCount = 0;
                    AttackPhase = 0;
                }
                else 
                {
                    AttackNumber++;
                    enemyBase.AttackCount = 0;
                    AttackPhase = 3;
                }
                

            }
        }
    }
    void A7() 
    {
        if (AttackPhase == 0)
        {
            enemyBase.animator.Play("跳躍準備");
            if (enemyBase.AttackCount > 0.8)
            {
                targetPos = enemyBase.Player.transform;
                StopAllCoroutines();
                StartCoroutine(MoveInArc());

                NEXT();
            }
        }
        if (AttackPhase == 1)
        {
            if (StandOk)
            {
                StandOk = false;
                NEXT();
            }
        }
        if (AttackPhase == 2)
        {
            GameObject CL_Attack = Instantiate(AttackAreas[0], enemyBase.transform.position, Quaternion.identity);
            AttackSpawnBase(CL_Attack);
            Destroy(CL_Attack, 0.2f);
            enemyBase.animator.Play("待機");


            for (int j = 0; j < 15; j++)
            {
                StartCoroutine(NeedleSpawnLing(0.1f * j, j * 0.5f, 0));
            }

            NEXT();
        }
        if (AttackPhase == 3)
        {

            if (enemyBase.AttackCount > 1f)
            {
                
                if (AttackNumber > 2)
                {
                    ATTCKFINISH();
                }
                else 
                { 
                    AttackNumber++;
                    enemyBase.AttackCount = 0;
                    AttackPhase = 0;
                }
           }
        }
    }
    void A8() 
    {
        
        if (AttackPhase == 0)
        {
            enemyBase.isPlayerLookBase();
            enemyBase.animator.Play("攻撃待機", 0, 0);

            for (int i = 0; i < 5; i++)
            {

                GameObject CL_AttackPPP = Instantiate(PPPAttack, enemyBase.transform.position, Quaternion.identity);
                CL_AttackPPP.transform.up = enemyBase.PlayerDirection.normalized;
                CL_AttackPPP.transform.localScale = new Vector2(4, 20);
                Vector2 NewLookDirection = Quaternion.Euler(0, 0, -40 + 20 * i) * enemyBase.PlayerDirection.normalized;
                CL_AttackPPP.transform.up = NewLookDirection.normalized;

                Destroy(CL_AttackPPP, 2);
            }

            NEXT();
        }
        if (AttackPhase == 1)
        {
            if (enemyBase.AttackCount > 2)
            {
                enemyBase.animator.Play("攻撃");
                NEXT();
            }
        }
        if (AttackPhase == 2)
        {
            for (int i = 0; i < 5; i++)
            {

                GameObject CL_Slash = Instantiate(BigCarseSlash, enemyBase.transform.position, Quaternion.identity);

                Vector2 NewLookDirection = Quaternion.Euler(0, 0, -40 + 20 * i) * enemyBase.PlayerDirection.normalized;

                CL_Slash.transform.up = NewLookDirection.normalized;
                Rigidbody2D rb = CL_Slash.GetComponent<Rigidbody2D>();
                rb.velocity = NewLookDirection.normalized * 10;
                Destroy(CL_Slash, 10);

                AttackSpawnBase(CL_Slash);
            }

            for (int i = 0; i < 3; i++)
            {

                GameObject CL_Slash = Instantiate(BigCarseSlash, enemyBase.transform.position, Quaternion.identity);

                Vector2 NewLookDirection = Quaternion.Euler(0, 0, -40 + 40 * i) * enemyBase.PlayerDirection.normalized;

                CL_Slash.transform.up = NewLookDirection.normalized;
                Rigidbody2D rb = CL_Slash.GetComponent<Rigidbody2D>();
                rb.velocity = NewLookDirection.normalized * 0.4f;
                Destroy(CL_Slash, 40);

                AttackSpawnBase(CL_Slash);
            }


            enemyBase.attackType = EnemyBase.AttackType.A1;
            enemyBase.AttackCount = 0;
            AttackPhase = 0;
        }
    }
    void A9() 
    {
        if (AttackPhase == 0)
        {
            enemyBase.animator.Play("跳躍準備");
            if (enemyBase.AttackCount > 0.8)
            {
                targetPos = enemyBase.Player.transform;
                StopAllCoroutines();
                StartCoroutine(MoveInArc());

                NEXT();
            }
        }
        if (AttackPhase == 1)
        {
            if (StandOk)
            {
                StandOk = false;
                NEXT();
            }
        }
        if (AttackPhase == 2)
        {
            GameObject CL_Attack = Instantiate(AttackAreas[0], enemyBase.transform.position, Quaternion.identity);
            AttackSpawnBase(CL_Attack);
            Destroy(CL_Attack, 0.2f);
            enemyBase.animator.Play("待機");


            for (int j = 0; j < 20; j++)
            {
                StartCoroutine(NeedleSpawnLing(0.1f * j, j * 0.5f, 0));
            }

            NEXT();
        }
        if (AttackPhase == 3)
        {

            if (enemyBase.AttackCount > 0.2)
            {

                NEXT();

            }
        }
        if (AttackPhase == 4)
        {
            enemyBase.isPlayerLookBase();
            enemyBase.animator.Play("攻撃待機", 0, 0);
            if (AttackNumber == 3)
            {
                for (int i = 0; i < 10; i++)
                {

                    GameObject CL_AttackPPP = Instantiate(PPPAttack, enemyBase.transform.position, Quaternion.identity);
                    CL_AttackPPP.transform.up = enemyBase.PlayerDirection.normalized;

                    float angle = 360 / 10;
                    Vector3 NewLookDirection = Quaternion.Euler(0, 0, angle * i)*Vector2.zero;
                    CL_AttackPPP.transform.up = NewLookDirection.normalized;

                    Destroy(CL_AttackPPP, 1);
                }
            }
            else
            {
                GameObject CL_AttackPPP = Instantiate(PPPAttack, enemyBase.transform.position, Quaternion.identity);
                CL_AttackPPP.transform.up = enemyBase.PlayerDirection.normalized;
                //Animator animator = CL_AttackPPP.GetComponent<Animator>();
                //animator.speed = 1.2f;
                Destroy(CL_AttackPPP, 1);
            }

            NEXT();
        }
        if (AttackPhase == 5)
        {
            if (enemyBase.AttackCount > 0.6)
            {
                enemyBase.animator.Play("攻撃");
                NEXT();
            }
        }
        if (AttackPhase == 6)
        {
            if (AttackNumber == 3)
            {
                for (int i = 0; i < 10; i++)
                {

                    GameObject CL_Slash = Instantiate(CarseSlash, enemyBase.transform.position, Quaternion.identity);
                    CL_Slash.transform.up = enemyBase.PlayerDirection.normalized;

                    float angle = 360 / 10;
                    Vector3 NewLookDirection = Quaternion.Euler(0, 0, angle * i) * Vector2.zero;
                    CL_Slash.transform.up = NewLookDirection.normalized;

                    Rigidbody2D rb = CL_Slash.GetComponent<Rigidbody2D>();
                    rb.velocity = NewLookDirection.normalized * 10;
                    Destroy(CL_Slash, 10);

                    AttackSpawnBase(CL_Slash);
                }
            }
            else
            {
                GameObject CL_Slash = Instantiate(CarseSlash, enemyBase.transform.position, Quaternion.identity);
                if (enemyBase.PlayerDirection.x > 0)
                {
                    CL_Slash.transform.localScale = new Vector2(CL_Slash.transform.localScale.x, CL_Slash.transform.localScale.y);
                }
                else
                {
                    CL_Slash.transform.localScale = new Vector2(-CL_Slash.transform.localScale.x, CL_Slash.transform.localScale.y);
                }

                Rigidbody2D rb = CL_Slash.GetComponent<Rigidbody2D>();
                rb.velocity = enemyBase.PlayerDirection.normalized * 10;
                Destroy(CL_Slash, 10);

                AttackSpawnBase(CL_Slash);
            }

            NEXT();
        }
        if (AttackPhase == 3)
        {

            if (enemyBase.AttackCount > 0.2)
            {
                enemyBase.animator.Play("待機");
                AttackNumber++;
                if (AttackNumber > 3)
                {
                    NEXT();
                }
                else
                {
                    AttackPhase = 4;
                    enemyBase.AttackCount = 0;
                }
            }
        }
    }
    public void ATTCKFINISH() 
    {
        AttackNumber2 = 0;
        enemyBase.AttackCount = 0;
    AttackFinish();
    }
    public void NEXT() 
    {
        enemyBase.AttackCount = 0;
        Next();
    }

    private IEnumerator MoveInArc()
    {
        myRunningCoroutine = MoveInArc();
        // 移動開始前のBodyTypeを保存しておく
        var originalBodyType = enemyBase.rb.bodyType;
        // 移動中はKinematicにして、物理演算の影響をなくす
        enemyBase.rb.bodyType = RigidbodyType2D.Kinematic;

        Vector2 startPos = enemyBase.transform.position;
        Vector2 endPos = targetPos.position;

        GameObject CL_AttackPPP = Instantiate(PPPAttack1, endPos, Quaternion.identity);
        
        Destroy(CL_AttackPPP,1.5f);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration); // 0から1の範囲に収める

            // XとYの直線的な移動を計算
            Vector2 linearPos = Vector2.Lerp(startPos, endPos, progress);

            // 高さを計算 (Sinカーブ)
            float height = Mathf.Sin(progress * Mathf.PI) * arcHeight;

            // Rigidbody2Dを使って位置を更新する
            // 2Dなので、Y座標に高さを加える
            enemyBase.rb.MovePosition(new Vector2(linearPos.x, linearPos.y + height));

            //yield return new WaitForFixedUpdate(); // 物理演算の更新タイミングで待つ
            yield return new WaitForEndOfFrame();
        }

        // 念のため、最終的にきっちり目標地点に合わせる
        transform.position = endPos;
        // BodyTypeを元に戻す
        enemyBase.rb.bodyType = originalBodyType;


        StandOk = true;
        // 念のため、最終的にきっちり目標地点に合わせる
        transform.position = targetPos.position;
        Debug.Log("目的地に到着！" + targetPos.position);
    }
    private IEnumerator ChoiceMoveInArc(Vector2 Target)
    {
        myRunningCoroutine = MoveInArc();
        // 移動開始前のBodyTypeを保存しておく
        var originalBodyType = enemyBase.rb.bodyType;
        // 移動中はKinematicにして、物理演算の影響をなくす
        enemyBase.rb.bodyType = RigidbodyType2D.Kinematic;

        Vector2 startPos = enemyBase.transform.position;
        Vector2 endPos = Target;

        GameObject CL_AttackPPP = Instantiate(PPPAttack1, endPos, Quaternion.identity);

        Destroy(CL_AttackPPP, 1.5f);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration); // 0から1の範囲に収める

            // XとYの直線的な移動を計算
            Vector2 linearPos = Vector2.Lerp(startPos, endPos, progress);

            // 高さを計算 (Sinカーブ)
            float height = Mathf.Sin(progress * Mathf.PI) * arcHeight;

            // Rigidbody2Dを使って位置を更新する
            // 2Dなので、Y座標に高さを加える
            enemyBase.rb.MovePosition(new Vector2(linearPos.x, linearPos.y + height));

            //yield return new WaitForFixedUpdate(); // 物理演算の更新タイミングで待つ
            yield return new WaitForEndOfFrame();
        }

        // 念のため、最終的にきっちり目標地点に合わせる
        transform.position = endPos;
        // BodyTypeを元に戻す
        enemyBase.rb.bodyType = originalBodyType;


        StandOk = true;
        // 念のため、最終的にきっちり目標地点に合わせる
        transform.position = targetPos.position;
        Debug.Log("目的地に到着！" + targetPos.position);
    }
    void NeedleSpawn(Vector2 Ptransform)
    {


        GameObject CL_Needle = Instantiate(RockNeedle, Ptransform, Quaternion.identity);
        CG_Puppeteer_Needle puppeteer_Needle = CL_Needle.GetComponent<CG_Puppeteer_Needle>();
        puppeteer_Needle.enemyAttack.enemyBase = enemyBase;

        Destroy(CL_Needle, 5);
    }
    private IEnumerator NeedleSpawnLing(float delayTime,float Lange,int Plass)
    {
        //Debug.Log("コルーチンが開始されました。今から " + delayTime + " 秒待ちます...");

        // ここで指定した秒数だけ待機する
        yield return new WaitForSeconds(delayTime);
        for (int i = 0; i < 10+ Plass; i++)
        {

            float angle = (360 / 10+ Plass) * i;
            float angleRad = angle * Mathf.Deg2Rad;

            float newX = enemyBase.transform.position.x + 2 * Lange * Mathf.Cos(angleRad);
            float newY = enemyBase.transform.position.y + 2 * Lange * Mathf.Sin(angleRad);

            // 新しい位置にクローンを作成
            Vector2 newPosition = new Vector2(newX, newY);

            NeedleSpawn(newPosition);

        }
    }
}
