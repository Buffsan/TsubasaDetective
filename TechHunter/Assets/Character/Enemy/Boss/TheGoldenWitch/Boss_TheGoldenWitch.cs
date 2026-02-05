using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_TheGoldenWitch : Boss_MoveBase
{

    private IEnumerator myRunningCoroutine; // 実行中のコルーチンを保持するための変数
    public float duration = 1;
    public float arcHeight = 30;
    float AttackCount1 = 0;
    float saveAngle = 0;
    bool StandOk = false;
    bool isSerious = false;

    [SerializeField] AudioClip BossBGM;

    [SerializeField] List<AudioClip> audioclips = new List<AudioClip>();

    Vector2 targetPos = Vector2.zero;
    Vector2 targetDirection = Vector2.zero;

    [SerializeField] GameObject DamageArea;
    [SerializeField] GameObject asaltAttackArea;

    [SerializeField] GameObject RockNeedle;

    [SerializeField] GameObject Shougekihaa;

    [SerializeField] GameObject SaveDengerArea;
    [SerializeField] GameObject GoaldTurret;
    [SerializeField] List<GameObject> Effects = new List<GameObject>();
    [SerializeField] List<Transform> EnemySpawnPoint = new List<Transform>();
    [SerializeField] List<GameObject> SpawnEnemy = new List<GameObject>();

    AudioManager audioManager => AudioManager.instance;
    Vector2 saveDirection = Vector2.zero;

    int HissatuNumber = 0;

    // Start is called before the first frame update
    void Start()
    {

    }
    public override void M_Stan()
    {
        Finish(); AttackWaitTime = 0; StandOk = false; asaltAttackArea.SetActive(false);
        /*
        if (myRunningCoroutine != null)
        {
            AttackCount1 = 0;
            asaltAttackArea.SetActive(false);
            StopCoroutine(myRunningCoroutine); // 特定のコルーチンを停止
            Debug.Log("コルーチンを停止しました。");
            myRunningCoroutine = null; // 参照をクリア
        }*/
    }
    public override void M_Con()
    {
        Finish(); AttackWaitTime = 0; StandOk = false; asaltAttackArea.SetActive(false);
        if (myRunningCoroutine != null)
        {
            AttackCount1 = 0;
            asaltAttackArea.SetActive(false);
            StopCoroutine(myRunningCoroutine); // 特定のコルーチンを停止
            Debug.Log("コルーチンを停止しました。");
            myRunningCoroutine = null; // 参照をクリア
        }
    }
    public override void M_AI()
    {

        if (enemyBase.moveType != EnemyBase.MoveType.Attack)
        {
            enemyBase.animator.Play("歩行");
            AttackWaitTime += Time.deltaTime;

            enemyBase.isPlayerLookBase();
            enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * enemyBase.SPEED;

            if (phase == Phase.P1)
            {

                if (enemyBase.HP < enemyBase.MAXHP * 0.65f)
                {
                    audioManager.BgmSource.clip = BossBGM;

                    phase = Phase.P2;
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.attackType = EnemyBase.AttackType.A10;
                    DamageArea.SetActive(false);
                    isSerious = true;
                }

                if (AttackWaitTime > 3)
                {
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.rb.velocity = Vector2.zero;
                    float random = Random.RandomRange(1, 6);
                    //random = 5;
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
                        enemyBase.attackType = EnemyBase.AttackType.A5;
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


                if (AttackWaitTime > 1.5)
                {
                    for (int i = 0; i < 8; i++)
                    {

                        StartCoroutine(LateSpawnEnemy(i / 2, SpawnEnemy[Random.Range(0, SpawnEnemy.Count - 1)]));

                    }
                    HissatuNumber++;
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.rb.velocity = Vector2.zero;
                    float random = Random.RandomRange(1, 6);
                    if (HissatuNumber > 3) 
                    { 
                        random = 11;
                        HissatuNumber = 0;
                    } 
                    //float random = 5;
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
                        enemyBase.attackType = EnemyBase.AttackType.A5;
                    }
                    if (random == 6)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A6;
                    }
                    if (random == 11) 
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A11;
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
                //A7();
                break;
            case EnemyBase.AttackType.A8:
                //A8();
                break;
            case EnemyBase.AttackType.A9:
                //A9();
                break;
            case EnemyBase.AttackType.A10:
                A10();
                break;
            case EnemyBase.AttackType.A11:
                A11();
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
    void ChoiseNextPhase(int Number)
    {
        AttackPhase = Number;
        enemyBase.AttackCount = 0;
    }
    void Finish()
    {
        AttackPhase = 0;
        AttackNumber = 0;
        AttackCount1 = 0;
        enemyBase.AttackCount = 0;
        enemyBase.moveType = EnemyBase.MoveType.Move;
    }
    float attackcount()
    {
        float count = enemyBase.AttackCount;
        return count;

    }
    void A1() 
    {
        StartAttackMotion(0);
        if (AttackPhase == 1) 
        {
            enemyBase.isPlayerLookBase();
            enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * -10;
            if (attackcount() > 0.5f) 
            {
                NextPhase();
                enemyBase.animator.Play("縦ぶり移動");
                audioManager.PlaySE(audioclips[1]);

                SaveDengerArea = DengerAreaSpawn(DengerObjects[0],transform.position, 2f);
                
            }
        }
        if (AttackPhase == 2) 
        {
            enemyBase.isPlayerLookBase();
            SaveDengerArea.transform.position = transform.position;
            SaveDengerArea.transform.transform.up = enemyBase.PlayerDirection.normalized;
            enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * enemyBase.SPEED*0.5f;
            if(attackcount() > 1) 
            {
                saveDirection = enemyBase.PlayerDirection.normalized;
                NextPhase();
                enemyBase.rb.velocity = Vector2.zero;
            }
        }
        if (AttackPhase == 3) 
        {
            if (attackcount() > 1) 
            {
                GameObject CL_Attack  = SpawnObjectNow_Input(AttackObjects[0], transform.position, 0.1f);
                CL_Attack.transform.up = saveDirection;
                GameObject CL_Effect = SpawnObjectNow_Input(Effects[0], transform.position, 1);
                CL_Effect.transform.up = saveDirection;
                enemyBase.animator.Play("縦攻撃");
                audioManager.PlaySE(audioclips[2]);
                NextPhase();
            }
        }
        if (AttackPhase == 4) 
        {
            if (attackcount() > 1) 
            {
                Finish();
            }
        }
    }
    void A2()
    {
        StartAttackMotion(0);
        if (AttackPhase == 1) 
        {
            DengerAreaSpawn(DengerObjects[1], transform.position, 2);
            NextPhase();
        }
        if (AttackPhase == 2) 
        {
            if (attackcount() > 2f) 
            {
                enemyBase.animator.Play("詠唱");
                GameObject CL_Attack = SpawnObjectNow_Input(AttackObjects[1], transform.position, 0.1f);
                GameObject CL_Effect = SpawnObjectNow_Input(Effects[1], transform.position, 1);
                SpawnObjectNow(GoaldTurret, transform.position, 300);
                audioManager.PlaySE(audioclips[2]);
                NextPhase();
            }
        }
        if (AttackPhase == 3) 
        {
            if (attackcount() > 1.5f) 
            {
                Finish();
            }
        }
    }
    void A3()
    {
        StartAttackMotion(0);
        if (AttackPhase == 1)
        {
            bool FindPlayerPos = false;
            int safeCount = 0;
            while (!FindPlayerPos)
            {
                Vector2 TargetFindPlayerPos = new Vector2(Random.Range(-2f, 2f) + enemyBase.Player.transform.position.x, Random.Range(-2f, 2f) + enemyBase.Player.transform.position.y);
                FindPlayerPos = IsTagAtPosition(TargetFindPlayerPos, "Stage_Floor");
                safeCount++;
                if (safeCount > 100) { Debug.Log("目的地が見つからない"); FindPlayerPos = true; }
                if (FindPlayerPos)
                {
                    targetPos = TargetFindPlayerPos;
                    targetDirection = targetPos - (Vector2)transform.position;
                    enemyBase.isTargetLook(targetPos);
                }
            }
            enemyBase.animator.Play("加速", 0, 0);

            NextPhase();
        }
        if (AttackPhase == 2)
        {
            float TargetDistanse = Vector2.Distance(targetPos, transform.position);

            if (TargetDistanse > 1 && AttackCount1 < 2.5)
            {
                AttackCount1 += Time.fixedDeltaTime;
                enemyBase.rb.velocity = targetDirection.normalized * enemyBase.SPEED * 10;
            }
            else
            {
                AttackCount1 = 0;
                NextPhase();
                enemyBase.animator.Play("攻撃待機", 0, 0);
                audioManager.PlaySE(audioclips[2]);
                enemyBase.isPlayerLookBase();
                GameObject CL_Denger = DengerAreaSpawn(DengerObjects[2], transform.position, 1);
                saveDirection = enemyBase.PlayerDirection.normalized;
                CL_Denger.transform.up = saveDirection;
                CL_Denger.transform.parent = transform;
            }
        }
        if (AttackPhase == 3)
        {
            enemyBase.rb.velocity *= 0.8f;
            if (enemyBase.AttackCount > 1)
            {
                //asaltAttackArea.SetActive(true);
                NextPhase();
                GameObject CL_Attack = SpawnObjectNow_Input(AttackObjects[2], transform.position, 0.1f);
                GameObject CL_Denger = SpawnObjectNow_Input(Effects[2], transform.position, 1);
                CL_Denger.transform.up = saveDirection;
                CL_Attack.transform.up = saveDirection;
                
                targetPos = enemyBase.Player.transform.position;
                targetDirection = targetPos - (Vector2)transform.position;
                enemyBase.animator.Play("攻撃", 0, 0);
                audioManager.PlaySE(audioclips[2]);

                for (int i = 0; i < 2; i++)
                {
                    // 垂直方向（左右）ベクトル
                    Vector2 perp = new Vector2(-saveDirection.y, saveDirection.x);
                    float Lenght = 3;
                    // 攻撃判定を左右に生成する位置
                    Vector2 leftPos = (Vector2)transform.position + perp * Lenght * (i + 1);
                    Vector2 rightPos = (Vector2)transform.position - perp * Lenght * (i + 1);

                    // 攻撃判定オブジェクトを左右に生成
                    StartCoroutine(DengerAreaSpawn_RateTime(i, 1, DengerObjects[2], leftPos, 1, saveDirection, AttackObjects[2], Effects[2]));
                    StartCoroutine(DengerAreaSpawn_RateTime(i, 1, DengerObjects[2], rightPos, 1, saveDirection, AttackObjects[2], Effects[2]));
                }
            
            }
        }
        if (AttackPhase == 4)
        {


            if (enemyBase.AttackCount > 0.3)
            {

                asaltAttackArea.SetActive(false);
                NextPhase();
            }
            enemyBase.rb.velocity = saveDirection * enemyBase.SPEED * 2;

        }
        if (AttackPhase == 5)
        {
            if (enemyBase.AttackCount > 0.2)
            {
                int randomRe = Random.Range(0, 4);
                if (randomRe != 1 && AttackNumber < 2)
                {
                    enemyBase.AttackCount = 0;
                    AttackPhase = 0;
                    AttackNumber++;
                }
                else
                {
                    Finish();
                    enemyBase.animator.Play("待機", 0, 0);
                }
            }
            else
            {
                enemyBase.rb.velocity *= 0.2f;
            }
        }
    }
    void A4()
    {
        StartAttackMotion(0);
        if (AttackPhase == 1)
        {
            if (enemyBase.AttackCount > 0.3)
            {
                targetPos = (Vector2)enemyBase.Player.transform.position;
                StartCoroutine(MoveInArc());
                enemyBase.animator.Play("加速", 0, 0);

                NextPhase();
            }
        }
        if (AttackPhase == 2)
        {
            if (StandOk)
            {
                GameObject CL_Denger = SpawnObjectNow_Input(Effects[1], transform.position, 1);
                enemyBase.animator.Play("縦攻撃", 0, 0);
                StandOk = false;
                NextPhase();
            }
        }
        if (AttackPhase == 3) 
        {
            for (int i = 0; i < 20; i++) 
            {
                float randomValue = 1;
                Vector2 randomPos = new Vector2(Random.Range(-randomValue, randomValue), Random.Range(-randomValue, randomValue));
                StartCoroutine(DengerAreaSpawn_RateTime((float)i/5, 1, DengerObjects[2], transform.position, 1, randomPos, AttackObjects[2], Effects[2]));
            } 
            NextPhase();
        }
        if (AttackPhase == 4) 
        {
            if (attackcount() > 3) 
            {
                Finish();
            }
        }
    }
    void A5()
    {
        StartAttackMotion(0);

        if (AttackPhase == 1)
        {
            if (attackcount() > 1f)
            {
                enemyBase.animator.Play("詠唱");
                StartCoroutine(DengerAreaSpawn_RateTime(0, 1.5f, DengerObjects[1],transform.position, 1.5f,Vector2.zero, AttackObjects[1], Effects[1]));

                NextPhase();
            }
        }
        if (AttackPhase == 2) 
        {
            if (attackcount() > 1)
            {
                for (int i = 0; i < 30; i++)
                {
                    StartCoroutine(DengerAreaSpawn_RateTime((float)i / 20, 1.5f, DengerObjects[3], new Vector2(-20 + i*3, 0), 1.5f, new Vector2(0,1), AttackObjects[3], Effects[3]));
                }
                NextPhase() ;
            }
        }
        if (AttackPhase == 3)
        {
            if (attackcount() > 1)
            {

                for (int i = 0; i < 30; i++)
                {
                    StartCoroutine(DengerAreaSpawn_RateTime((float)i / 20, 1.5f, DengerObjects[3], new Vector2(0, -20 + i * 3), 1.5f, new Vector2(1, 0), AttackObjects[3], Effects[3]));
                }
                NextPhase();
            }
        }
        if (AttackPhase == 4) 
        {
            if (attackcount() > 2) 
            {
                enemyBase.attackType = EnemyBase.AttackType.A3;
                AttackPhase = 0;
                AttackNumber = 0;
                AttackCount1 = 0;
                enemyBase.AttackCount = 0;
                //Finish();
            }
        }
    }
    void A6()
    {
        if (AttackPhase == 0)
        {
            enemyBase.animator.Play("変身待機");
            StartCoroutine(DengerAreaSpawn_RateTime(0, 1, DengerObjects[1], transform.position, 1, Vector2.zero, AttackObjects[1], Effects[1]));
            NextPhase();
        }
        if (AttackPhase == 1)
        {
            if (attackcount() > 1)
            {
                audioManager.BgmSource.clip = BossBGM;
                DamageArea.SetActive(true);
                enemyBase.animator.Play("形態変化");
                GameObject CL_Attack = SpawnObjectNow_Input(AttackObjects[1], transform.position, 0.1f);
                GameObject CL_Effect = SpawnObjectNow_Input(Effects[1], transform.position, 1);
                NextPhase();
            }
        }
        if (AttackPhase == 2) 
        {
            if (attackcount() > 0.5f) 
            {
                for (int i = 0; i < 35; i++) 
                {

                    StartCoroutine(LateSpawnEnemy(i/10,SpawnEnemy[Random.Range(0, SpawnEnemy.Count - 1)]));
                
                }
                GameObject CL_Denger = DengerAreaSpawn(DengerObjects[2], transform.position, 1);
                saveDirection = enemyBase.PlayerDirection.normalized;
                CL_Denger.transform.up = saveDirection;
                asaltAttackArea.SetActive(true);
                enemyBase.animator.Play("形態変化＿跳躍");
                NextPhase();
            }
        }
        if (AttackPhase == 3) 
        {
            enemyBase.rb.velocity = saveDirection * 20;

            if (attackcount() > 0.5f) 
            {
                enemyBase.rb.velocity = Vector2.zero;
                asaltAttackArea.SetActive(false);
                NextPhase();
            }
        }
        if (AttackPhase == 4) 
        {
            if (attackcount() > 1) 
            {
                Finish();
            }
        }
    }
    
    void A10() 
    {
        if (AttackPhase == 0)
        {        
                enemyBase.animator.Play("変身待機");
            enemyBase.rb.velocity = Vector2.zero;
                StartCoroutine(DengerAreaSpawn_RateTime(0, 3, DengerObjects[1], transform.position, 3, Vector2.zero, AttackObjects[1], Effects[1]));
                NextPhase();
        }
        if (AttackPhase == 1) 
        {
            if (attackcount() > 3) 
            {
                audioManager.BgmSource.clip = BossBGM;
                audioManager.BgmSource.Play();
                DamageArea.SetActive(true);
                enemyBase.animator.Play("形態変化");
                GameObject CL_Attack = SpawnObjectNow_Input(AttackObjects[1], transform.position, 0.1f);
                GameObject CL_Effect = SpawnObjectNow_Input(Effects[1], transform.position, 1);
                NextPhase();
            }
        }
        if (AttackPhase == 2)
        {
            if (attackcount() > 1)
            {

                for (int i = 0; i < 30; i++)
                {
                    StartCoroutine(DengerAreaSpawn_RateTime((float)i / 20, 1.5f, DengerObjects[3], new Vector2(-30 + i * 3, 0), 1.5f, new Vector2(0, 1), AttackObjects[3], Effects[3]));
                }
                for (int i = 0; i < 30; i++)
                {
                    StartCoroutine(DengerAreaSpawn_RateTime((float)i / 20, 1.5f, DengerObjects[3], new Vector2(0, -30 + i * 3), 1.5f, new Vector2(1, 0), AttackObjects[3], Effects[3]));
                }
                
                NextPhase();
            }
        }
        if (AttackPhase == 3)
        {
            if (attackcount() > 1)
            {
                for (int i = 0; i < 30; i++)
                {
                    StartCoroutine(DengerAreaSpawn_RateTime((float)i / 20, 1.5f, DengerObjects[3], new Vector2(-20 + i * 3, 0), 1.5f, new Vector2(0, 1), AttackObjects[3], Effects[3]));
                }
                for (int i = 0; i < 30; i++)
                {
                    StartCoroutine(DengerAreaSpawn_RateTime((float)i / 20, 1.5f, DengerObjects[3], new Vector2(0, -20 + i * 3), 1.5f, new Vector2(1, 0), AttackObjects[3], Effects[3]));
                }

                NextPhase();
            }
        }
        if (AttackPhase == 4) 
        {
            if (attackcount() > 1) 
            {
                enemyBase.attackType = EnemyBase.AttackType.A6;
                AttackPhase = 2;
                AttackNumber = 0;
                AttackCount1 = 0;
                enemyBase.AttackCount = 0;
            }
        }
    }
    void A11()
    {
        if (AttackPhase == 0)
        {
            enemyBase.animator.Play("変身待機");
            enemyBase.rb.velocity = Vector2.zero;
            StartCoroutine(DengerAreaSpawn_RateTime(0, 3, DengerObjects[1], transform.position, 1.5f, Vector2.zero, AttackObjects[1], Effects[1]));
            DengerAreaSpawn(DengerObjects[0], transform.position, 2);
            NextPhase();
        }
        if (AttackPhase == 1)
        {
            if (attackcount() > 1.5f)
            {
 
                DamageArea.SetActive(true);
                enemyBase.animator.Play("形態変化");
                GameObject CL_Attack = SpawnObjectNow_Input(AttackObjects[1], transform.position, 0.1f);
                GameObject CL_Effect = SpawnObjectNow_Input(Effects[1], transform.position, 1);
                NextPhase();
            }
        }
        if (AttackPhase == 1)
        {
            if (attackcount() > 2)
            {
                enemyBase.animator.Play("形態変化＿たたきつける", 0, 0);
                SpawnObjectNow(AttackObjects[0], transform.position, 0.1f);
                SpawnObjectNow(Effects[0], transform.position, 3);
                all_System.camera_Controller.Shake(0.2f, 0.3f);
                audioManager.PlaySE(audioclips[0]);
                NextPhase();
                DengerAreaSpawn(DengerObjects[1], transform.position, 0.5f);
            }
        }
        if (AttackPhase == 2)
        {
            if (attackcount() > 1)
            {
                DengerAreaSpawn(DengerObjects[1], transform.position, 0.5f);
                SpawnObjectNow(Shougekihaa, transform.position, 0.1f);
                all_System.camera_Controller.Shake(0.2f, 0.3f);
                SpawnObjectNow(Effects[1], transform.position, 3);
                AttackNumber++;
                if (AttackNumber < 3)
                {
                    enemyBase.AttackCount = 0;
                    for (int i = 0; i < 10; i++)
                    {
                        float randomValue = 1;
                        Vector2 randomPos = new Vector2(Random.Range(-randomValue, randomValue), Random.Range(-randomValue, randomValue));
                        StartCoroutine(DengerAreaSpawn_RateTime((float)i / 5, 1, DengerObjects[2], transform.position, 1, randomPos, AttackObjects[2], Effects[2]));
                    }
                }
                else
                {
                    for (int i = 0; i < 20; i++)
                    {

                        StartCoroutine(LateSpawnEnemy(i / 5, SpawnEnemy[Random.Range(0, SpawnEnemy.Count - 1)]));

                    }
                    StartCoroutine(LateSpawnEnemy(0, SpawnEnemy[5]));
                    NextPhase();
                }
            }
        }
        if (AttackPhase == 3)
        {
            if (attackcount() > 1)
            {
                Finish();
            }
        }
    }

    void StartAttackMotion(int attackPhase) 
    {
        if (AttackPhase == attackPhase)
        {
            enemyBase.animator.Play("攻撃待機");
            audioManager.PlaySE(audioclips[0]);

            NextPhase();
        }
    }
    private IEnumerator MoveInArc()
    {
        myRunningCoroutine = MoveInArc();
        // 移動開始前のBodyTypeを保存しておく
        var originalBodyType = enemyBase.rb.bodyType;
        // 移動中はKinematicにして、物理演算の影響をなくす
        enemyBase.rb.bodyType = RigidbodyType2D.Kinematic;

        Vector2 startPos = enemyBase.transform.position;
        Vector2 endPos = targetPos;

        GameObject CL_AttackPPP = DengerAreaSpawn(DengerObjects[1],endPos,1);

       
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

        SpawnObjectNow(AttackObjects[1], endPos, 0.1f);
        transform.position = targetPos;
        Debug.Log("目的地に到着！" + targetPos);
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

    IEnumerator DengerAreaSpawn_RateTime(float startdelayTime, float delayTime, GameObject DengerObject, Vector2 pos, float destroyTime, Vector2 RotatePos , GameObject attackarea,GameObject effect)
    {
        yield return new WaitForSeconds(startdelayTime);
        GameObject CL_Denger = DengerAreaSpawn(DengerObject, pos, destroyTime);
        CL_Denger.transform.up = RotatePos;
        yield return new WaitForSeconds(delayTime);
        GameObject CL_Effect = SpawnObjectNow_Input(effect, pos, 1);
        GameObject CL_Attack = SpawnObjectNow_Input(attackarea, pos, 0.1f);
        CL_Effect.transform.up = RotatePos;
        CL_Attack.transform.up = RotatePos;

    }

    IEnumerator LateSpawnEnemy(float delayTime,GameObject Enemy) 
    {
        yield return new WaitForSeconds(delayTime);
        EnemySpawn(Enemy, EnemySpawnPoint[Random.Range(0, EnemySpawnPoint.Count)].position);
    }
    private IEnumerator NeedleSpawnLing(float delayTime, float Lange, int Plass)
    {
        //Debug.Log("コルーチンが開始されました。今から " + delayTime + " 秒待ちます...");

        // ここで指定した秒数だけ待機する
        yield return new WaitForSeconds(delayTime);
        for (int i = 0; i < 10 + Plass; i++)
        {

            float angle = (360 / 10 + Plass) * i;
            float angleRad = angle * Mathf.Deg2Rad;

            float newX = enemyBase.transform.position.x + 2 * Lange * Mathf.Cos(angleRad);
            float newY = enemyBase.transform.position.y + 2 * Lange * Mathf.Sin(angleRad);

            // 新しい位置にクローンを作成
            Vector2 newPosition = new Vector2(newX, newY);

            //NeedleSpawn(newPosition);

        }
    }


}
