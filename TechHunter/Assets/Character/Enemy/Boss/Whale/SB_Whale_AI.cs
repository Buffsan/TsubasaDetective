using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SB_Whale_AI : Boss_MoveBase
{

    private IEnumerator myRunningCoroutine; // 実行中のコルーチンを保持するための変数
    public float duration = 1;
    public float arcHeight = 30;
    float AttackCount1 = 0;
    float saveAngle = 0;
    bool StandOk = false;
    bool isSerious = false;

    [SerializeField] GameObject ARM;
    [SerializeField] GameObject ArmAttackPoint;
    [SerializeField] Animator ARManim;
    [SerializeField] List<GameObject> GunLaser = new List<GameObject>();
    [SerializeField] AudioClip BossBGM;

    [SerializeField] List<AudioClip> audioclips = new List<AudioClip>();

    Vector2 targetPos = Vector2.zero;
    Vector2 targetDirection = Vector2.zero;

    [SerializeField] GameObject slashAttack;
    [SerializeField] GameObject asaltAttackArea;
    [SerializeField] GameObject Bullet;
    [SerializeField] GameObject RockNeedle;
    [SerializeField] GameObject SlashLit;
    [SerializeField] GameObject HalfBullet;
    [SerializeField] GameObject DamageArea;
    [SerializeField] GameObject HoukouAttack;
    [SerializeField] GameObject AttackArea4;

    [SerializeField] List<GameObject> AttackAreas = new List<GameObject>();
    [SerializeField] GameObject Denger_AttackArea_Slash;
    [SerializeField] GameObject Denger_AttackArea_Stand;
    [SerializeField] GameObject Denger_HoukouAttackArea;
    [SerializeField] GameObject Denger_4AttackArea;

    [SerializeField] List<GameObject> Effects = new List<GameObject>();

    AudioManager audioManager => AudioManager.instance;

    // Start is called before the first frame update
    void Start()
    {

    }
    public override void M_Stan()
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
            enemyBase.animator.Play("移動");
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
                    enemyBase.attackType = EnemyBase.AttackType.A6;
                    DamageArea.SetActive(false);
                    isSerious = true;
                }
                
                if (AttackWaitTime > 3)
                {
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.rb.velocity = Vector2.zero;
                    float random = Random.RandomRange(1, 5);
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
                        enemyBase.attackType = EnemyBase.AttackType.A2;
                    }

                }

            }
            if (phase == Phase.P2)
            {


                if (AttackWaitTime > 1.5)
                {
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.rb.velocity = Vector2.zero;
                    float random = Random.RandomRange(1, 6);
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
                        enemyBase.attackType = EnemyBase.AttackType.A2;
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
    
    void A1()
    {
        if (AttackPhase == 0)
        {
            enemyBase.animator.Play("攻撃待機", 0, 0);
            DengerAreaSpawn(Denger_HoukouAttackArea, transform.position, 2);
            
            NextPhase();
        }
        if(AttackPhase == 1) 
        {
            if (attackcount() > 2) 
            {
                enemyBase.animator.Play("攻撃", 0, 0);
                SpawnObjectNow(HoukouAttack, transform.position, 0.1f);
                SpawnObjectNow(Effects[0], transform.position, 3);
                all_System.camera_Controller.Shake(0.2f, 0.3f);
                audioManager.isPlaySE(audioclips[0]);
                NextPhase();
                DengerAreaSpawn(Denger_HoukouAttackArea, transform.position, 0.5f);
            }
        }
        if (AttackPhase == 2) 
        {
            if (attackcount() > 0.5) 
            {
                DengerAreaSpawn(Denger_HoukouAttackArea, transform.position, 0.5f);
                SpawnObjectNow(HoukouAttack, transform.position, 0.1f);
                all_System.camera_Controller.Shake(0.2f, 0.3f);
                SpawnObjectNow(Effects[0], transform.position, 3);
                AttackNumber++;
                if (AttackNumber < 3)
                {
                    enemyBase.AttackCount = 0;
                }
                else 
                {
                    if (isSerious)
                    {
                        for (int i = 0; i < 50; i++)
                        {
                            for (int j = 0; j < 6; j++)
                            {
                                Vector2 randomPos = Vector2.zero;
                                bool ok = false;
                                float range = 12f;
                                float minrange = 5.5f;
                                while (!ok)
                                {
                                    randomPos = new Vector2(Random.Range(-range, range), Random.Range(-range, range));
                                    if (Mathf.Abs(randomPos.x) > minrange || Mathf.Abs(randomPos.y) > minrange) ok = true;
                                }
                                StartCoroutine(LiftSpawnLing((float)i / 30, (Vector2)transform.position + randomPos));
                            }
                        }
                        for (int i = 0; i < 50; i++)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                Vector2 randomPos = Vector2.zero;
                                bool ok = false;
                                float range = 20;
                                float minrange = 12f;
                                while (!ok)
                                {
                                    randomPos = new Vector2(Random.Range(-range, range), Random.Range(-range, range));
                                    if (Mathf.Abs(randomPos.x) > minrange || Mathf.Abs(randomPos.y) > minrange) ok = true;
                                }
                                StartCoroutine(LiftSpawnLing((float)i / 30 + 2, (Vector2)transform.position + randomPos));
                            }
                        }
                    }
                    NextPhase();
                }
            }
        }
        if (AttackPhase == 3) 
        {
            if (attackcount() > 0.5) 
            {
                AttackFinish();
            }
        }
    }
   
    void A2()
    {
        if (AttackPhase == 0)
        {
            enemyBase.animator.Play("攻撃");
            saveAngle = 0;
            if (attackcount() > 1) 
            {
                NextPhase();
            }
        }
        if (AttackPhase == 1) 
        {
            if (attackcount() > 0.1) 
            {
                for (int i = 0; i < 4; i++) 
                {
                    GameObject CL_Effect = DengerAreaSpawn(Denger_4AttackArea, transform.position, 1f);

                    float dirAngle = saveAngle + 90 * i;
                    CL_Effect.transform.Rotate(0, 0, dirAngle);
                    
                    
                }
                NextPhase();
            }
        }
        if (AttackPhase == 2) 
        {
            if (attackcount() > 1.1f)
            {
                for (int i = 0; i < 4; i++)
                {
                    GameObject CL_Effect = SpawnObjectNow_Input(AttackArea4, transform.position, 0.1f);
                    CL_Effect.transform.Rotate(0, 0, saveAngle + 90 * i);
                }
                saveAngle += 45;
                enemyBase.animator.Play("攻撃待機");

                audioManager.isPlaySE(audioclips[1]);
                all_System.camera_Controller.Shake(0.3f, 0.2f);
                SpawnObjectNow(Effects[0], transform.position, 3);

                NextPhase();

                if (isSerious)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            Vector2 randomPos = Vector2.zero;
                            float range = 12f;
                            randomPos = new Vector2(Random.Range(-range, range), Random.Range(-range, range));

                            StartCoroutine(LiftSpawnLing((float)i / 30, (Vector2)transform.position + randomPos));
                        }
                    }
                }
            }
            
        }
        if (AttackPhase == 3) 
        {
            if (attackcount() > 0.1f) 
            {
                AttackNumber++;
                if (AttackNumber > 4)
                {
                    NextPhase();
                }
                else
                {
                    ChoiseNextPhase(1);
                    enemyBase.animator.Play("攻撃");
                }
            }
        }
        if (AttackPhase == 4) 
        {
            AttackFinish();
        }
    }
    void A3()
    {
        if (AttackPhase == 0)
        {
            enemyBase.isPlayerLookBase();
            enemyBase.animator.Play("攻撃待機", 0, 0);

            GameObject CL_AttackPPP = Instantiate(Denger_4AttackArea, enemyBase.transform.position, Quaternion.identity);
            CL_AttackPPP.transform.up = enemyBase.PlayerDirection.normalized;
            Destroy(CL_AttackPPP, 1);

            NextPhase();
        }
        if (AttackPhase == 1)
        {
            if (enemyBase.AttackCount > 0.8)
            {
                asaltAttackArea.SetActive(true);
                enemyBase.animator.Play("歩行", 0, 0);
                NextPhase();
            }
        }
        if (AttackPhase == 2)
        {
            enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * enemyBase.SPEED * 8;
            if (enemyBase.AttackCount > 2 * (1 + AttackNumber * 0.5))
            {
                enemyBase.animator.Play("攻撃待機", 0, 0);
                asaltAttackArea.SetActive(false);
                NextPhase();
            }
            AttackCount1 += Time.fixedDeltaTime;
            if (AttackCount1 > 0.2)
            {
                AttackCount1 = 0;
                if (isSerious)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        // 垂直方向（左右）ベクトル
                        Vector2 perp = new Vector2(-enemyBase.PlayerDirection.normalized.y, enemyBase.PlayerDirection.normalized.x);

                        // 攻撃判定を左右に生成する位置
                        Vector2 leftPos = (Vector2)transform.position + perp * 0.5f * (i + 3);
                        Vector2 rightPos = (Vector2)transform.position - perp * 0.5f * (i + 3);

                        // 攻撃判定オブジェクトを左右に生成
                        GameObject leftAttack = Instantiate(RockNeedle, leftPos, Quaternion.identity);
                        GameObject rightAttack = Instantiate(RockNeedle, rightPos, Quaternion.identity);
                        Destroy(leftAttack, 5);
                        Destroy(rightAttack, 5);
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        // 垂直方向（左右）ベクトル
                        Vector2 perp = new Vector2(-enemyBase.PlayerDirection.normalized.y, enemyBase.PlayerDirection.normalized.x);

                        // 攻撃判定を左右に生成する位置
                        Vector2 leftPos = (Vector2)transform.position + perp * 0.5f * (i + 3);
                        Vector2 rightPos = (Vector2)transform.position - perp * 0.5f * (i + 3);

                        // 攻撃判定オブジェクトを左右に生成
                        GameObject leftAttack = Instantiate(RockNeedle, leftPos, Quaternion.identity);
                        GameObject rightAttack = Instantiate(RockNeedle, rightPos, Quaternion.identity);
                        Destroy(leftAttack, 5);
                        Destroy(rightAttack, 5);
                    }
                }
            }


        }
        if (AttackPhase == 3)
        {
            enemyBase.rb.velocity = Vector2.zero;
            int randomValue = Random.Range(0, 2);
            if (AttackNumber == 0)
            {
                AttackNumber++;
                if (isSerious)
                {
                    ChoiseNextPhase(10);
                }
                else 
                {
                    int randomnumberAttack = Random.Range(0, 2);
                    if (randomnumberAttack == 0)
                    {
                        ChoiseNextPhase(0);
                    }
                    else
                    {
                        ChoiseNextPhase(0);
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A1;
                    }
                }
            }
            else
            {
                Finish();
            }


        }
        if (AttackPhase == 10) 
        {
            if (attackcount() > 0.2) 
            {
                enemyBase.animator.Play("ワープ", 0, 0);
                DamageArea.SetActive(false);
                audioManager.isPlaySE(audioclips[2]);
                NextPhase();
            }
        }
        if (AttackPhase == 11) 
        {
            if (attackcount() > 1) 
            {
                audioManager.isPlaySE(audioclips[2]);

                bool FindPlayerPos = false;
                int safeCount = 0; // 安全カウンタ

                while (!FindPlayerPos && safeCount < 100)
                {
                    safeCount++;

                    Vector2 TargetFindPlayerPos = new Vector2(
                        Random.Range(-4f, 4f) + enemyBase.Player.transform.position.x,
                        Random.Range(-2f, 2f) + enemyBase.Player.transform.position.y
                    );

                    FindPlayerPos = IsTagAtPosition(TargetFindPlayerPos, "Stage_Floor");

                    if (FindPlayerPos)
                    {
                        targetPos = TargetFindPlayerPos;
                        targetDirection = targetPos - (Vector2)transform.position;
                        enemyBase.isTargetLook(targetPos);
                    }
                }

                // 見つからなかったときの代替処理
                if (!FindPlayerPos)
                {
                    // 代替位置を設定（例：現在位置のまま or デフォルト座標）
                    targetPos = new Vector2 (enemyBase.Player.transform.position.x, enemyBase.Player.transform.position.y+2);
                    
                    Debug.LogWarning("Floor not found. Using player position as fallback.");
                }


                enemyBase.transform.position = targetPos;
                enemyBase.animator.Play("出現", 0, 0);
                NextPhase();

                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        Vector2 randomPos = Vector2.zero;
                        float range = 6f;
                        randomPos = new Vector2(Random.Range(-range, range), Random.Range(-range, range));

                        StartCoroutine(LiftSpawnLing((float)i / 30, (Vector2)transform.position + randomPos));
                    }
                }
            }
        }
        if (AttackPhase == 12) 
        {
            if (attackcount() > 1) 
            {
                DamageArea.SetActive(true);
                NextPhase();
            }
        }
        if (AttackPhase == 13)
        {
            if (attackcount() > 0.5)
            {
                int randomnumberAttack = Random.Range(0, 2);
                if (randomnumberAttack == 0)
                {
                    ChoiseNextPhase(0);
                }
                else 
                {
                    ChoiseNextPhase(0);
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.attackType = EnemyBase.AttackType.A1;
                }
                
            }
        }
    }
    void A4()
    {
        if (AttackPhase == 0) 
        {
            enemyBase.isPlayerLookBase();
            enemyBase.animator.Play("攻撃待機");
            GameObject CL_denger = DengerAreaSpawn(Denger_4AttackArea, transform.position, 1);
            CL_denger.transform.up = enemyBase.PlayerDirection.normalized;
            targetPos = enemyBase.Player.transform.position;
            NextPhase();
        }
        if (AttackPhase == 1)
        {
            if (attackcount() > 1)
            {
                GameObject CL_Attack = SpawnObjectNow_Input(AttackArea4, transform.position, 0.1f);
                CL_Attack.transform.up = enemyBase.PlayerDirection.normalized;
                NextPhase();
                audioManager.isPlaySE(audioclips[1]);

                all_System.camera_Controller.Shake(0.3f, 0.2f);
                SpawnObjectNow(Effects[0], transform.position, 3);

                if (isSerious)
                {
                    for (int i = 0; i < 50; i++)
                    {
                        StartCoroutine(Ougi(i, targetPos, (float)i / 40));
                    }
                }
            }
        }
        if (AttackPhase == 2) 
        {
            if (attackcount() > 1) 
            {
                AttackNumber++;
                if (AttackNumber > 2)
                {
                    AttackFinish();
                }
                else 
                {
                    ChoiseNextPhase(0);
                }
            }
        }
    }
    void A5()
    {
        if (AttackPhase == 0)
        {
            if (attackcount() > 0.2)
            {
                enemyBase.animator.Play("ワープ", 0, 0);
                DamageArea.SetActive(false);
                audioManager.isPlaySE(audioclips[2]);
                NextPhase();
            }
        }
        if (AttackPhase == 1)
        {
            if (attackcount() > 1)
            {
                audioManager.isPlaySE(audioclips[2]);

                bool FindPlayerPos = false;
                int safeCount = 0; // 安全カウンタ

                while (!FindPlayerPos && safeCount < 100)
                {
                    safeCount++;

                    Vector2 TargetFindPlayerPos = new Vector2(
                        Random.Range(-4f, 4f) + enemyBase.Player.transform.position.x,
                        Random.Range(-2f, 2f) + enemyBase.Player.transform.position.y
                    );

                    FindPlayerPos = IsTagAtPosition(TargetFindPlayerPos, "Stage_Floor");

                    if (FindPlayerPos)
                    {
                        targetPos = TargetFindPlayerPos;
                        targetDirection = targetPos - (Vector2)transform.position;
                        enemyBase.isTargetLook(targetPos);
                    }
                }

                // 見つからなかったときの代替処理
                if (!FindPlayerPos)
                {
                    // 代替位置を設定（例：現在位置のまま or デフォルト座標）
                    targetPos = new Vector2(enemyBase.Player.transform.position.x, enemyBase.Player.transform.position.y + 2);
                    Debug.LogWarning("Floor not found. Using player position as fallback.");
                }


                enemyBase.transform.position = targetPos;
                enemyBase.animator.Play("出現", 0, 0);
                NextPhase();

                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        Vector2 randomPos = Vector2.zero;
                        float range = 6f;
                        randomPos = new Vector2(Random.Range(-range, range), Random.Range(-range, range));

                        StartCoroutine(LiftSpawnLing((float)i / 30, (Vector2)transform.position + randomPos));
                    }
                }
            }
        }
        if (AttackPhase == 2)
        {
            if (attackcount() > 1)
            {
                DamageArea.SetActive(true);
                NextPhase();
            }
        }
        if (AttackPhase == 3)
        {
            if (attackcount() > 1)
            {
                NextPhase();
            }
        }
        if (AttackPhase == 4)
        {
            enemyBase.isPlayerLookBase();
            enemyBase.animator.Play("攻撃");
            GameObject CL_denger = DengerAreaSpawn(Denger_4AttackArea, transform.position, 1);
            CL_denger.transform.up = enemyBase.PlayerDirection.normalized;
            targetPos = enemyBase.Player.transform.position;
            NextPhase();
        }
        if (AttackPhase == 5)
        {
            if (attackcount() > 1)
            {
                enemyBase.animator.Play("攻撃待機");
                GameObject CL_Attack = SpawnObjectNow_Input(AttackArea4, transform.position, 0.1f);
                CL_Attack.transform.up = enemyBase.PlayerDirection.normalized;
                NextPhase();
                audioManager.isPlaySE(audioclips[1]);

                all_System.camera_Controller.Shake(0.3f, 0.2f);
                SpawnObjectNow(Effects[0], transform.position, 3);

                for (int i = 0; i < 50; i++)
                {
                    StartCoroutine(Ougi(i, targetPos, (float)i / 40));
                }

                
            }
        }
        if(AttackPhase == 6) 
        {
            if (attackcount() > 1) 
            { 
            AttackNumber++;
                if (AttackNumber > 3)
                {
                    AttackFinish();
                }
                else 
                {
                    ChoiseNextPhase(0);
                }
            }
        }
    }

    void A6() 
    {
        if (AttackPhase == 0) 
        {
            NextPhase();

            all_System.camera_Controller.Shake(1, 0.3f);
            audioManager.isPlaySE(audioclips[0]);
            enemyBase.animator.Play("被弾");
        }
        if (AttackPhase == 1) 
        {
            enemyBase.isPlayerLookBase();
            enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * -enemyBase.SPEED;
            if (attackcount() > 1) 
            {
                NextPhase();
                enemyBase.animator.Play("攻撃");
            }
        }
        if (AttackPhase == 2) 
        {
            enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * -enemyBase.SPEED *0.5f;
            if (attackcount() > 0.5)
            {
                enemyBase.rb.velocity = Vector2.zero;
                
                audioManager.isPlaySE(audioclips[2]);
                enemyBase.animator.Play("消える");
                NextPhase();
            }
        }
        if (AttackPhase == 3)
        {
            if (attackcount() > 5)
            {
                targetPos = Vector2.zero;
                NextPhase();
            }
        }
        if (AttackPhase == 4)
        {
            if (attackcount() > 1)
            {
                audioManager.isPlaySE(audioclips[2]);
                audioManager.BgmSource.Play();


                enemyBase.transform.position = targetPos;
                enemyBase.animator.Play("出現", 0, 0);
                DamageArea.SetActive(true);
                audioManager.BgmSource.Play();

                NextPhase();
                
                for (int i = 0; i < 30; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        Vector2 randomPos = Vector2.zero;
                        float range = 15f;
                        randomPos = new Vector2(Random.Range(-range, range), Random.Range(-range, range));

                        StartCoroutine(LiftSpawnLing((float)i / 7, (Vector2)transform.position + randomPos));
                    }
                }
            }
        }
        if (AttackPhase == 5)
        {
            if (attackcount() > 1)
            {
                ChoiseNextPhase(0);
                enemyBase.moveType = EnemyBase.MoveType.Attack;
                enemyBase.attackType = EnemyBase.AttackType.A1;

            }
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

        GameObject CL_AttackPPP = Instantiate(Denger_AttackArea_Stand, endPos, Quaternion.identity);

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
        transform.position = targetPos;
        Debug.Log("目的地に到着！" + targetPos);
    }
    float attackcount() 
    {
        float count = enemyBase.AttackCount;
        return count;

    }

    void BulletShot()
    {
        audioManager.isPlaySE(audioclips[0]);
        Vector2 ARMdirection = enemyBase.Player.transform.position - ArmAttackPoint.transform.position;
        GameObject CL_Bullet = Instantiate(Bullet, ArmAttackPoint.transform.position, Quaternion.identity);
        CL_Bullet.transform.up = ARMdirection.normalized;

        Rigidbody2D CL_rb = CL_Bullet.GetComponent<Rigidbody2D>();
        CL_rb.velocity = ARMdirection.normalized * 25;
        Destroy(CL_Bullet, 1);

    }
    void DoubleBulletShot()
    {
        audioManager.isPlaySE(audioclips[0]);
        Vector2 ARMdirection = enemyBase.Player.transform.position - ArmAttackPoint.transform.position;
        for (int i = 0; i < 2; i++)
        {
            GameObject CL_Slash = Instantiate(HalfBullet, ArmAttackPoint.transform.position, Quaternion.identity);
            Vector2 NewLookDirection = Quaternion.Euler(0, 0, -6 + 12 * i) * ARMdirection.normalized;
            CL_Slash.transform.up = NewLookDirection.normalized;

            if (enemyBase.PlayerDirection.x > 0)
            {
                CL_Slash.transform.localScale = new Vector2(CL_Slash.transform.localScale.x, CL_Slash.transform.localScale.y);
            }
            else
            {
                CL_Slash.transform.localScale = new Vector2(-CL_Slash.transform.localScale.x, CL_Slash.transform.localScale.y);
            }
            Rigidbody2D rb = CL_Slash.GetComponent<Rigidbody2D>();
            rb.velocity = NewLookDirection.normalized * 25;
            Destroy(CL_Slash, 1);
        }

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

            NeedleSpawn(newPosition);

        }
    }
    void NeedleSpawn(Vector2 Ptransform)
    {
        GameObject CL_Needle = Instantiate(RockNeedle, Ptransform, Quaternion.identity);

        Destroy(CL_Needle, 5);
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
    private IEnumerator LiftSpawnLing(float delayTime, Vector2 Pos)
    {
        //Debug.Log("コルーチンが開始されました。今から " + delayTime + " 秒待ちます...");

        // ここで指定した秒数だけ待機する
        yield return new WaitForSeconds(delayTime);
        SpawnObjectNow(SlashLit, Pos, 5);
    }
    IEnumerator Ougi(int number, Vector2 pos ,float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        //number++;

        float AttackDistance = number / 2.5f;

        // ランダムな角度を生成 (0 ~ 360度)
        float randomAngle = Random.Range(-80f, 80f);

        // ランダムな角度をラジアンに変換
        float radians = randomAngle * Mathf.Deg2Rad;

       
        Quaternion rotation = Quaternion.AngleAxis(randomAngle, Vector3.forward);
        Vector2 newDirection = (pos - (Vector2)transform.position).normalized;

       
        Vector2 rotatedDirection = rotation * newDirection; // SavePos方向を基準に角度を回転
        Vector2 newPosition = (Vector2)transform.position + rotatedDirection * AttackDistance; // 回転方向で新しい位置を計算

        SpawnObjectNow(SlashLit,newPosition,2);
    }
}

