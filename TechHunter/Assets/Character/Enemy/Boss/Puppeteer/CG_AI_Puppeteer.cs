using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;

using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CG_AI_Puppeteer : Boss_MoveBase
{

    [SerializeField] GameObject RockNeedle;
    [SerializeField] GameObject DieEffect;
    [SerializeField] AudioClip Needleclip;

    public List<bool> EnemySpawnBool = new List<bool>();

    public List<GameObject> Enemys = new List<GameObject>();

    [SerializeField] GameObject AddMovePointGameObject;
    Add_Lists_GameObject add_Lists_GameObject;
    [SerializeField]Add_Lists_GameObject EnemySpawnLists_GameObject;

    [SerializeField] GameObject Zako;
    [SerializeField] GameObject AstroZako;

    bool Astrobool = false;
    float AstroCount = 0;


    GameObject LastMovePoint;

    Vector2 SavePos;
    Vector2 StartPos;

    float Near = 0;
    bool NearBool = false;
    float NearP = 0;

    float sabAttackCount = 0;
    int sabAttackNumber = 0;



    public List<GameObject> MovePoints = new List<GameObject>();
   
    void Start()
    {
        StartPos = transform.position;
         AddMovePointGameObject = GameObject.FindGameObjectWithTag("MovePoints");
        add_Lists_GameObject = AddMovePointGameObject.GetComponent<Add_Lists_GameObject>();

        GameObject EnemysSpawnList = GameObject.FindGameObjectWithTag("EnemySpawnPoints");
        EnemySpawnLists_GameObject = EnemysSpawnList.GetComponent<Add_Lists_GameObject>();



        if (enemyBase.volume.profile.TryGet<Bloom>(out enemyBase.bloom))
        {
            Debug.Log("Bloomエフェクトが見つかりました！");
            //enemyBase.bloom.scatter.value = 0.5f; // Scatter値を設定
        }
        else
        {
            Debug.LogWarning("プロファイルにBloomが見つかりません！");
        }
    }
    public void FixedUpdate()
    {
        if (!Astrobool)
        {
            if (AstroCount > 40)
            {
                Astrobool = true;
                NeedSpawnEnemy(AstroZako, StartPos + new Vector2(4, 4));
                NeedSpawnEnemy(AstroZako, StartPos + new Vector2(-4, -4));
                for (int i = 0; i < 2; i++) 
                {
                    NeedSpawnEnemy(Enemys[2], EnemySpawnLists_GameObject.gameObjects[Random.Range(0, EnemySpawnLists_GameObject.gameObjects.Count)].transform.position);
                }
            }
            else
            {
                AstroCount += Time.deltaTime;
            }
        }
    }
    public override void M_AI()
    {
        NearP = Vector2.Distance(enemyBase.Player.transform.position ,transform.position);
        if (NearP < 5)
        {
            if (Near < 10 && !NearBool)
            {
                Near = Time.deltaTime;
            }
            else
            {
                NearBool = true;
                Near = 0;
            }
        }

        if (enemyBase.moveType != EnemyBase.MoveType.Attack)
        {

            AttackWaitTime += Time.deltaTime;

            enemyBase.rb.velocity = Vector3.zero;

            

            if (phase == Phase.P1)
            {
                
                if (enemyBase.HP < enemyBase.MAXHP / 2)
                {
                    //SpawnEnemy();
                    Astrobool = false;
                    AstroCount = 0;
                    SpawnZakoEnemy();

                    phase = Phase.P2;
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.attackType = EnemyBase.AttackType.A7;
                }

                if (AttackWaitTime > 4)
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
                        enemyBase.attackType = EnemyBase.AttackType.A2;
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
                        enemyBase.attackType = EnemyBase.AttackType.A2;
                    }

                }

            }

            if (enemyBase.attackType != EnemyBase.AttackType.A7)
            {
                if (enemyBase.HP < (enemyBase.MAXHP / 4) * 4 && !EnemySpawnBool[0])
                {
                    AttackNumber = 0;
                    sabAttackCount = 0;
                    enemyBase.mode = EnemyBase.ModeType.M1;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.attackType = EnemyBase.AttackType.A4;


                    EnemySpawnBool[0] = true;
                    SpawnEnemy();
                }
                if (enemyBase.HP < (enemyBase.MAXHP / 4) * 3 && !EnemySpawnBool[1])
                {
                    AttackNumber = 0;
                    sabAttackCount = 0;
                    enemyBase.mode = EnemyBase.ModeType.M1;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.attackType = EnemyBase.AttackType.A4;

                    EnemySpawnBool[0] = true;
                    EnemySpawnBool[1] = true;
                    SpawnEnemy();
                }
                if (enemyBase.HP < (enemyBase.MAXHP / 4) * 2 && !EnemySpawnBool[2])
                {/*
                    AttackNumber = 0;
                    sabAttackCount = 0;
                    enemyBase.mode = EnemyBase.ModeType.M1;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.attackType = EnemyBase.AttackType.A4;

                    EnemySpawnBool[0] = true;
                    EnemySpawnBool[1] = true;
                    EnemySpawnBool[2] = true;
                    SpawnEnemy();*/
                }
                if (enemyBase.HP < (enemyBase.MAXHP / 4) * 1 && !EnemySpawnBool[3])
                {
                    AttackNumber = 0;
                    sabAttackCount = 0;
                    enemyBase.mode = EnemyBase.ModeType.M1;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.attackType = EnemyBase.AttackType.A4;

                    EnemySpawnBool[3] = true;
                    SpawnEnemy();
                }
            }

            if (NearBool)
            {

                NearBool = false;
                Near = 0;
                enemyBase.attackType = EnemyBase.AttackType.A5;

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
    void NeedleSpawn(Vector2 Ptransform) 
    {
        

    GameObject CL_Needle = Instantiate(RockNeedle ,Ptransform,Quaternion.identity);
            CG_Puppeteer_Needle puppeteer_Needle = CL_Needle.GetComponent<CG_Puppeteer_Needle>();
            puppeteer_Needle.enemyAttack.enemyBase = enemyBase;

            Destroy(CL_Needle, 5);
    }
    
    void A1() 
    {

        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {



            if (enemyBase.AttackCount > 0.5) 
            {
                enemyBase.animator2.Play("前攻撃", 0, 0);
                enemyBase.mode = EnemyBase.ModeType.M2;
                SavePos = enemyBase.Player.transform.position;
            }

        }
        if (enemyBase.mode == EnemyBase.ModeType.M2) 
        {
            if (enemyBase.AttackCount > 0.1)
            {
                AttackNumber++;

                if (AttackNumber < 30)
                {
                    Vector2 newPos = SavePos + new Vector2(Random.Range((float)-AttackNumber / 3, (float)AttackNumber / 3), Random.Range((float)-AttackNumber / 2, (float)AttackNumber / 2));
                    NeedleSpawn(newPos);
                }
                else 
                { 
                    enemyBase.mode = EnemyBase.ModeType.M1;
                    enemyBase.moveType = EnemyBase.MoveType.Move;
                    AttackNumber = 0;
                }
            }
        
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {

            

        }

    }
    void A2() 
    {
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            enemyBase.animator2.Play("前攻撃", 0, 0);
            SavePos = enemyBase.Player.transform.position;
            enemyBase.mode = EnemyBase.ModeType.M2;
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        { 

        if (enemyBase.AttackCount > 0.05)
            {
                enemyBase.AttackCount = 0;
                AttackNumber++;

                float AttackDistance = AttackNumber / 2.5f;

                // ランダムな角度を生成 (0 ~ 360度)
                float randomAngle = Random.Range(-40f, 40f);

                // ランダムな角度をラジアンに変換
                float radians = randomAngle * Mathf.Deg2Rad;

                // 極座標からデカルト座標へ変換
                float xOffset = AttackDistance * Mathf.Cos(radians);
                float yOffset = AttackDistance * Mathf.Sin(radians);
                Quaternion rotation = Quaternion.AngleAxis(randomAngle, Vector3.forward);
                Vector2 newDirection = (SavePos - (Vector2)transform.position).normalized;

                // 新しい位置を計算
                //Vector2 newPosition = (Vector2)transform.position + newDirection*new Vector2(xOffset, yOffset);

                Vector2 rotatedDirection = rotation * newDirection; // SavePos方向を基準に角度を回転
                Vector2 newPosition = (Vector2)transform.position + rotatedDirection * AttackDistance; // 回転方向で新しい位置を計算

                NeedleSpawn(newPosition);
                if (AttackNumber > 45) 
                {
                    AttackNumber = 0;
                    enemyBase.mode = EnemyBase.ModeType.M1;
                    enemyBase.moveType = EnemyBase.MoveType.Move;
                  
                }
            }
        }
    }

    void A3()
    {
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            enemyBase.animator2.Play("前攻撃", 0, 0);
            SavePos = enemyBase.Player.transform.position;
            enemyBase.mode = EnemyBase.ModeType.M2;
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {

            if (enemyBase.AttackCount > 0.02)
            {
                enemyBase.AttackCount = 0;
                AttackNumber++;

                float AttackDistance = AttackNumber *1.5f;

                

               
                Vector2 newDirection = (SavePos - (Vector2)transform.position).normalized;

                // 新しい位置を計算
                //Vector2 newPosition = (Vector2)transform.position + newDirection*new Vector2(xOffset, yOffset);

                
                Vector2 newPosition = (Vector2)transform.position +newDirection * AttackDistance; // 回転方向で新しい位置を計算

                NeedleSpawn(newPosition);
                if (AttackNumber > 15)
                {
                    sabAttackNumber++;
                    AttackNumber = 0;
                    if (sabAttackNumber > 3)
                    {
                        enemyBase.mode = EnemyBase.ModeType.M1;
                        enemyBase.moveType = EnemyBase.MoveType.Move;
                        sabAttackNumber = 0;
                    }
                    else 
                    {
                        enemyBase.mode = EnemyBase.ModeType.M1;
                    }

                }
            }
        }
    }
    void A4()
    {
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            enemyBase.animator.Play("泥撤退");
            enemyBase.animator2.Play("潜伏");
            if (enemyBase.AttackCount > 2)
            {
                enemyBase.DamageBody.SetActive(false);
                GameObject newChoicePoint = add_Lists_GameObject.gameObjects[Random.Range(0, add_Lists_GameObject.gameObjects.Count)];
                int SafeCount = 0;
                while(newChoicePoint == LastMovePoint)
                {
                    SafeCount++;
                     newChoicePoint = add_Lists_GameObject.gameObjects[Random.Range(0, add_Lists_GameObject.gameObjects.Count)];
                    if (SafeCount > 1000) 
                    {
                        Debug.Log("この世界でトップクラスの悪運か目的地が１つしかないためたどり着けませんでした");  
                        return;
                    }
                }

                LastMovePoint = newChoicePoint;
                SavePos = newChoicePoint.transform.position;
                enemyBase.animator.Play("泥移動");
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M2;



                
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {
            float newDistance = Vector2.Distance(SavePos, transform.position);
            Vector2 newDirection = SavePos - (Vector2)transform.position;

            enemyBase.rb.velocity = newDirection.normalized * enemyBase.SPEED;

            sabAttackCount += Time.deltaTime;
            if (sabAttackCount > 0.08)
            {

                sabAttackCount = 0;
                Vector2 NeedlePos = (Vector2)transform.position + new Vector2(Random.Range(-2f, 2f), Random.Range(-3f, -1f));
                NeedleSpawn(NeedlePos);

            }

            if (newDistance < 0.1f)
            {
                enemyBase.animator.Play("泥出現");
                enemyBase.animator2.Play("出現");
                sabAttackCount = 0;
                enemyBase.rb.velocity = Vector2.zero;
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.DamageBody.SetActive(true);
            }

        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            if (enemyBase.AttackCount > 2)
            {
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M1;
                enemyBase.moveType = EnemyBase.MoveType.Move;
            }
        }
    }
    void A5()
    {

        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {

            if (enemyBase.AttackCount > 0.5)
            {
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M2;
                NeedleSpawn(transform.position);
            }

        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {
            enemyBase.animator2.Play("上攻撃", 0, 0);
            for (int i = 0; i < 10; i++)
            {

                float angle = (360 / 10)*i;
                float angleRad = angle * Mathf.Deg2Rad;

                float newX = transform.position.x + 1 * Mathf.Cos(angleRad);
                float newY = transform.position.y + 1 * Mathf.Sin(angleRad);

                // 新しい位置にクローンを作成
                Vector2 newPosition = new Vector2(newX, newY);

                NeedleSpawn(newPosition);

            }

            enemyBase.AttackCount = 0;
            enemyBase.mode = EnemyBase.ModeType.M3;


        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {

            if (enemyBase.AttackCount > 0.2)
            {
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M4;
            }


        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {
            enemyBase.animator2.Play("上攻撃", 0, 0);
            for (int i = 0; i < 12; i++)
            {

                float angle = (360 / 12) *i;
                float angleRad = angle * Mathf.Deg2Rad;

                float newX = transform.position.x + 2 * Mathf.Cos(angleRad);
                float newY = transform.position.y + 2 * Mathf.Sin(angleRad);

                // 新しい位置にクローンを作成
                Vector2 newPosition = new Vector2(newX, newY);

                NeedleSpawn(newPosition);

            }

            AttackNumber = 0;
            enemyBase.mode = EnemyBase.ModeType.M5;
           


        }
        if (enemyBase.mode == EnemyBase.ModeType.M5)
        {

            if (enemyBase.AttackCount > 0.2)
            {
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M6;
            }


        }
        if (enemyBase.mode == EnemyBase.ModeType.M6)
        {
            enemyBase.animator2.Play("上攻撃", 0, 0);
            for (int i = 0; i < 12; i++)
            {

                float angle = (360 / 12) * i;
                float angleRad = angle * Mathf.Deg2Rad;

                float newX = transform.position.x + 3 * Mathf.Cos(angleRad);
                float newY = transform.position.y + 3 * Mathf.Sin(angleRad);

                // 新しい位置にクローンを作成
                Vector2 newPosition = new Vector2(newX, newY);

                NeedleSpawn(newPosition);

            }

            AttackNumber = 0;
            enemyBase.mode = EnemyBase.ModeType.M1;
            enemyBase.moveType = EnemyBase.MoveType.Move;


        }

    }
    void SpawnEnemy() 
    {
        for (int i = 0; i < 2; i++)
        {

            Vector2 newPos = EnemySpawnLists_GameObject.gameObjects[Random.Range(0, EnemySpawnLists_GameObject.gameObjects.Count)].transform.position;

            Instantiate(Enemys[Random.Range(0, Enemys.Count)], newPos, Quaternion.identity);
            Instantiate(DieEffect, newPos, Quaternion.identity);

        }
        Vector2 newPos2 = EnemySpawnLists_GameObject.gameObjects[Random.Range(0, EnemySpawnLists_GameObject.gameObjects.Count)].transform.position;

        Instantiate(Enemys[1], newPos2, Quaternion.identity);
        Instantiate(DieEffect, newPos2, Quaternion.identity);
        for (int i = 0; i < 7; i++)
        {
            NeedSpawnEnemy(Zako, EnemySpawnLists_GameObject.gameObjects[Random.Range(0, EnemySpawnLists_GameObject.gameObjects.Count)].transform.position);
        }
    
    }
    void NeedSpawnEnemy(GameObject ZakoEnemy,Vector2 ZakoPos)
    {
       
            Instantiate(ZakoEnemy, ZakoPos, Quaternion.identity);
            Instantiate(DieEffect, ZakoPos, Quaternion.identity);

    }
    void SpawnZakoEnemy()
    {
        for (int i = 0; i < 8; i++)
        {

            Vector2 newPos = EnemySpawnLists_GameObject.gameObjects[Random.Range(0, EnemySpawnLists_GameObject.gameObjects.Count)].transform.position;

            Instantiate(Zako, newPos, Quaternion.identity);
            Instantiate(DieEffect, newPos, Quaternion.identity);

        }
        for (int i = 0; i < 2; i++)
        {

            Vector2 newPos = EnemySpawnLists_GameObject.gameObjects[1].transform.position;

            Instantiate(Zako, newPos, Quaternion.identity);
            Instantiate(DieEffect, newPos, Quaternion.identity);

        }
    }
    void A6() 
    {


        for (int i = 0; i < 6; i++) 
        {

            Vector2 newPos = EnemySpawnLists_GameObject.gameObjects[Random.Range(0, EnemySpawnLists_GameObject.gameObjects.Count)].transform.position;

            Instantiate(Enemys[Random.Range(0,Enemys.Count)], newPos, Quaternion.identity);
            Instantiate(DieEffect, newPos, Quaternion.identity);

        }

        AttackNumber = 0;
        enemyBase.mode = EnemyBase.ModeType.M1;
        enemyBase.moveType = EnemyBase.MoveType.Move;

    }

    void A7()
    {
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {

            enemyBase.CharaDamage.AllRecorverConfusionHP();enemyBase.DamageBody.SetActive(false);

            enemyBase.animator.Play("泥撤退");
            enemyBase.animator2.Play("潜伏");
            if (enemyBase.AttackCount > 2)
            {
                
                

                
                SavePos = add_Lists_GameObject.gameObjects[4].transform.position;
                enemyBase.animator.Play("泥移動");
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M2;




            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {
            float newDistance = Vector2.Distance(SavePos, transform.position);
            Vector2 newDirection = SavePos - (Vector2)transform.position;

            enemyBase.rb.velocity = newDirection.normalized * enemyBase.SPEED;

            sabAttackCount += Time.deltaTime;
            if (sabAttackCount > 0.08)
            {

                sabAttackCount = 0;
                Vector2 NeedlePos = (Vector2)transform.position + new Vector2(Random.Range(-2f, 2f), Random.Range(-3f, -1f));
                NeedleSpawn(NeedlePos);

            }

            if (newDistance < 0.1f)
            {
                enemyBase.animator.Play("泥出現");
                enemyBase.animator2.Play("出現");
                sabAttackCount = 0;
                enemyBase.rb.velocity = Vector2.zero;
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.DamageBody.SetActive(true);
            }

        }
       
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {

            if (enemyBase.AttackCount > 0.5)
            {
                enemyBase.animator2.Play("上攻撃", 0, 0);
                AttackNumber++;
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M4;
                NeedleSpawn(transform.position);
            }

        }

        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {
            if (enemyBase.AttackCount > 0.3)
            {
               
                if (AttackNumber < 12)
                {
                    for (int i = 0; i < 10 + AttackNumber; i++)
                    {

                        float angle = (360 / 10 + AttackNumber) * i;
                        float angleRad = angle * Mathf.Deg2Rad;

                        float newX = (transform.position.x + 1 + AttackNumber) * Mathf.Cos(angleRad);
                        float newY = (transform.position.y + 1 + AttackNumber) * Mathf.Sin(angleRad);

                        // 新しい位置にクローンを作成
                        Vector2 newPosition = new Vector2(newX, newY);

                        NeedleSpawn(newPosition);

                    }
                    AttackNumber++;
                    enemyBase.AttackCount = 0;
                    enemyBase.mode = EnemyBase.ModeType.M4;
                }
                else
                {
                    AttackNumber = 0;
                    AttackNumber = 0;
                    enemyBase.mode = EnemyBase.ModeType.M1;
                    //enemyBase.moveType = EnemyBase.MoveType.Move;
                    enemyBase.attackType = EnemyBase.AttackType.A6;
                }
            }


        }
        

    }
}
