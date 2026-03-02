using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Boss_CG_MasterMechanic_AI : Boss_MoveBase
{
    [SerializeField] List<GameObject> ShotPoint;
    [SerializeField] List<GameObject> HoudanShotPoint;
    [SerializeField] GameObject Arrow;
    [SerializeField] GameObject AsaltAttackBody;
    [SerializeField] float ArrowDestroy = 20;
    [SerializeField] float AsaltSpeed = 10;
    [SerializeField] GameObject Moter;
    Vector2 SaveDirection = Vector2.zero;
    [SerializeField] float ArrowSpeed = 2;
    public GameObject S_AttackArea;

    [SerializeField] List<GameObject> EnemySpawnPoint = new List<GameObject>();
    [SerializeField] GameObject StarongKnight;
    [SerializeField] GameObject StorongSpier;
    [SerializeField] GameObject Tank;
    [SerializeField] int SpawnCount = 0;

    int MaxSpawnEnemyCount = 20;
    int SpawnEnemyCount = 0;

    bool flag = false;

    [SerializeField] List<AudioClip> audioClips = new List<AudioClip>();

    [SerializeField] float MoterRandomRange = 4;

    public override void M_Con()
    {
        AllAttackNumber = 0;
        AttackWaitTime = 99;
        AttackNumber = 0;
        AsaltAttackBody.SetActive(false);
    }
    public override void M_Stan()
    {
        AllAttackNumber = 0;
        AttackWaitTime = 99;
        AttackNumber = 0;
        AsaltAttackBody.SetActive(false);
    }

    public override void M_Die()
    {

    }
    public override void M_AI()
    {

        if (!flag)
        {
            if (enemyBase.MAXHP / 2 > enemyBase.HP)
            {
                flag = true;
                for (int i = 0; i < 50; i++)
                    StartCoroutine(LateSpawnEnemy((float)i / 15, StarongKnight, EnemySpawnPoint[Random.Range(0, EnemySpawnPoint.Count)].transform.position));

                for (int i = 0; i < 1; i++)
                    EnemySpawn(Tank, EnemySpawnPoint[Random.Range(0, EnemySpawnPoint.Count)].transform.position);
            }
        }

        if (enemyBase.moveType != EnemyBase.MoveType.Attack)
        {
            enemyBase.rb.velocity = (enemyBase.playerController.transform.position - transform.position).normalized * enemyBase.SPEED;
            enemyBase.animator.SetInteger("Anim", 0);
            AttackWaitTime += Time.deltaTime;

            enemyBase.rb.velocity = Vector3.zero;

            if (phase == Phase.P1)
            {


                if (AttackWaitTime > 7)
                {
                    SpawnCount++;
                    int random = 0;
                    if (SpawnCount > 2)
                    {

                        SpawnCount = 0;
                        random = 4;
                        if (SpawnEnemyCount < MaxSpawnEnemyCount)
                        {
                            SpawnEnemyCount++;
                            for (int i = 0; i < 25; i++)
                                StartCoroutine(LateSpawnEnemy((float)i / 10, StarongKnight, EnemySpawnPoint[Random.Range(0, EnemySpawnPoint.Count)].transform.position));

                            for (int i = 0; i < 1; i++)
                                EnemySpawn(StorongSpier, EnemySpawnPoint[Random.Range(0, EnemySpawnPoint.Count)].transform.position);
                        }
                    }
                    else 
                    { 
                    random = Random.RandomRange(1, 4);
                    }
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;

                   

                    if (random == 1)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A1;//8
                    }
                    if (random == 2)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A2;
                    }
                    if (random == 3)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A4;
                    }
                    if (random == 4)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A3;
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
    void ChoiseNextPhase(int Number)
    {
        AttackPhase = Number;
        enemyBase.AttackCount = 0;
    }
    void Finish()
    {
        AttackPhase = 0;
        AttackNumber = 0;
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
        if (AttackPhase == 0)
        {
            enemyBase.rb.velocity = Vector2.zero;
            if (attackcount() > 2)
            {
                NextPhase();
            }
        }
        if (AttackPhase == 1)
        {
            for (int i = 0; i < 50; i++)
            {
                StartCoroutine(ShotArrow((float)i / 15));

            }
            NextPhase();
        }
        if (AttackPhase == 2)
        {
            if (attackcount() > 6)
            {
                Finish();
            }
        }
    }
    void A2()
    {
        if (AttackPhase == 0)
        {
            enemyBase.rb.velocity = Vector2.zero;
            if (attackcount() > 4)
            {
                NextPhase();
            }
        }
        if (AttackPhase == 1)
        {
            for (int i = 0; i < 50; i++)
            {
                StartCoroutine(ShotArrow((float)i / 5));
            }
            for (int i = 0; i <5; i++)
            {
                StartCoroutine(ShotMoter((float)i / 2));
            }
            NextPhase();
        }
        if (AttackPhase == 2)
        {
            if (attackcount() > 1)
            {
                Finish();
            }
        }
    }
    void A3()
    {
        if (AttackPhase == 0)
        {
            GameObject denger = DengerAreaSpawn(DengerObjects[0], transform.position, 2);

            SaveDirection = enemyBase.playerController.transform.position - transform.position;
            denger.transform.up = SaveDirection;
            
            NextPhase();
        }
        if (AttackPhase == 1)
        {
            if (attackcount() > 2)
            {
                AsaltAttackBody.SetActive(true);
                NextPhase();
            }
        }
        if (AttackPhase == 2)
        {
            enemyBase.rb.velocity = SaveDirection.normalized * AsaltSpeed;
            if (attackcount() > 2)
            {
                Finish();
                AsaltAttackBody.SetActive(false);
            }
        }
    }
    void A4()
    {
        if (AttackPhase == 0)
        {
            enemyBase.rb.velocity = Vector2.zero;
            if (attackcount() > 2)
            {
                NextPhase();
            }
        }
        if (AttackPhase == 1)
        {
            for (int i = 0; i < 12; i++)
            {
                StartCoroutine(ShotMoter((float)i / 3));
            }
            NextPhase();
        }
        if (AttackPhase == 2)
        {
            if (attackcount() > 4)
            {
                Finish();
            }
        }
    }
    void A5()
    {

    }

    IEnumerator ShotArrow(float WaitTime)
    {
        yield return new WaitForSeconds(WaitTime);

        enemyBase.audioManager.PlaySE(audioClips[0]);

        Vector2 shotPos = ShotPoint[Random.Range(0, ShotPoint.Count)].transform.position;
        GameObject CL_Arrow = Instantiate(
            Arrow
            , shotPos
            , Quaternion.identity
            );

        Rigidbody2D rb = CL_Arrow.GetComponent<Rigidbody2D>();
        Vector2 direction = (Vector2)enemyBase.playerController.transform.position - shotPos;
        CL_Arrow.transform.up = direction;
        CL_Arrow.transform.Rotate(0, 0, 90);
        rb.velocity = direction.normalized * ArrowSpeed;

        Destroy(CL_Arrow, ArrowDestroy);
    }
    IEnumerator ShotMoter(float WaitTime)
    {
        yield return new WaitForSeconds(WaitTime);
        enemyBase.audioManager.PlaySE(audioClips[1]);
        Vector2 shotPos = HoudanShotPoint[Random.Range(0, ShotPoint.Count)].transform.position;
        GameObject CL_Arrow = Instantiate(
            Moter
            , shotPos
            , Quaternion.identity
            );

        Mortar_Controller mortar = CL_Arrow.GetComponent<Mortar_Controller>();
        mortar.ChangePos(enemyBase.Player.transform.position + new Vector3(Random.Range(-MoterRandomRange, MoterRandomRange), Random.Range(-MoterRandomRange, MoterRandomRange), 0));
        mortar.enemyBase = enemyBase;
        mortar.AttackArea = S_AttackArea;

    }
    IEnumerator LateSpawnEnemy(float WaitTime,GameObject Enemy,Vector2 pos)
    {
        yield return new WaitForSeconds(WaitTime);
        EnemySpawn(Enemy,pos);
    }
}
