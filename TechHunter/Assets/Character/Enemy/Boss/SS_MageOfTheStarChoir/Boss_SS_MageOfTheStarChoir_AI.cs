using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_SS_MageOfTheStarChoir_AI : Boss_MoveBase
{

    [SerializeField] int DefaltDeffence;
    [SerializeField] int PowerDeffence = 140;

    [SerializeField] GameObject Laser;
    [SerializeField] GameObject SaveLaser;
    [SerializeField] GameObject chainEffect;
    [SerializeField] GameObject BrealEffect;
    bool Break = false;

    [SerializeField] List<GameObject> NowStaffs = new List<GameObject>();
    [SerializeField] List<ParticleSystem> magicShieldEffects = new List<ParticleSystem>();
    [SerializeField] float ShieldEmmision = 2;
    [SerializeField] List<GameObject> Staffs = new List<GameObject>();
    [SerializeField] List<GameObject> StaffSpawnPoint = new List<GameObject>();
    [SerializeField] List<GameObject> DengerArea = new List<GameObject>();
    [SerializeField] List<GameObject> AttackArea = new List<GameObject>();
    [SerializeField] List<GameObject> AttackEffect = new List<GameObject>();

    bool one = false;

    public override void M_Con()
    {
        AllAttackNumber = 0;
        AttackWaitTime = 99;
        AttackNumber = 0;
        if (SaveLaser) Destroy(SaveLaser);
    }
    public override void M_Stan()
    {
        if (SaveLaser) Destroy(SaveLaser);
        AllAttackNumber = 0;
        AttackWaitTime = 99;
        AttackNumber = 0;
    }
    private void FixedUpdate()
    {
        if (Break) return;
        int i = 0;
        int shieldBreak = 0;
        foreach (var staff in NowStaffs) 
        {
            if (!staff) 
            {
                shieldBreak ++;
                magicShieldEffects[i].emissionRate = 0;
            }
            i++;
        }
        if (shieldBreak >= 4) 
        {
            Break = true;
            AttackWaitTime = 0;
            AttackPhase = 0;
            enemyBase.moveType = EnemyBase.MoveType.Attack;
            enemyBase.attackType = EnemyBase.AttackType.A3;
            NowStaffs.Clear();
        }
        
    }
    public override void M_Die()
    {
        foreach (var staff in NowStaffs) 
        { 
        Destroy(staff);
        }
        if(SaveLaser)Destroy(SaveLaser);
    }
    public override void M_AI()
    {
        
        if (!one) 
        {
            one = true;
            AttackWaitTime = 0;
            enemyBase.moveType = EnemyBase.MoveType.Attack;
            enemyBase.attackType = EnemyBase.AttackType.A5;
        }
        if (enemyBase.moveType != EnemyBase.MoveType.Attack)
        {
            enemyBase.animator.SetInteger("Anim", 0);
            AttackWaitTime += Time.deltaTime;

            enemyBase.rb.velocity = Vector3.zero;

            if (phase == Phase.P1)
            {
                
                
                if (AttackWaitTime > 5)
                {
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;

                    int random = Random.RandomRange(1, 3);

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
                //A4();
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
            enemyBase.isPlayerLookBase();
            SaveLaser = Instantiate(Laser,transform.position,Quaternion.identity);
            SaveLaser.transform.up = enemyBase.PlayerDirection.normalized;
            enemyBase.animator.SetInteger("Anim", 2);
            NextPhase();
        }
        if (AttackPhase == 1) 
        {
            if (attackcount() > 1.5) 
            {
                SaveLaser.GetComponent<Animator>().SetInteger("Anim",1);
                NextPhase();
            }
        }
        if (AttackPhase == 2)
        {
            if (attackcount() > 7)
            {
                SaveLaser.GetComponent<Animator>().SetInteger("Anim", 2);
                NextPhase();
            }
        }
        if (AttackPhase == 3)
        {
            if (attackcount() > 1)
            {
                Destroy(SaveLaser);
                Finish();
            }
        }
    }
    void A2() 
    {
        if (AttackPhase == 0)
        {
            enemyBase.isPlayerLookBase();
            SaveLaser = Instantiate(Laser, transform.position, Quaternion.identity);
            SaveLaser.transform.up = enemyBase.PlayerDirection.normalized;

            enemyBase.animator.SetInteger("Anim", 2);
            NextPhase();
        }
        if (AttackPhase == 1)
        {
            if (attackcount() > 1.5)
            {
                SaveLaser.GetComponent<Animator>().SetInteger("Anim", 1);
                NextPhase();
            }
        }
        if (AttackPhase == 2)
        {
            SaveLaser.transform.Rotate(0, 0,(8 * Time.fixedDeltaTime));
            if (attackcount() > 5)
            {
                SaveLaser.GetComponent<Animator>().SetInteger("Anim", 2);
                NextPhase();
            }
        }
        if (AttackPhase == 3)
        {
            if (attackcount() > 1)
            {
                Destroy(SaveLaser);
                Finish();
            }
        }
    }
    void A3()
    {
        if (AttackPhase == 0)
        {
            if (SaveLaser) Destroy(SaveLaser);
            BrealEffect.SetActive(true);
            enemyBase.animator.SetBool("Damage", true);
            enemyBase.animator.SetInteger("Anim", 0);
            enemyBase.charaDamage.DF = DefaltDeffence;
            NextPhase();
        }
        if (AttackPhase == 1)
        {
            if (attackcount() > 20)
            {
                enemyBase.animator.SetBool("Damage", false);
                ChoiseNextPhase(0);
                enemyBase.moveType = EnemyBase.MoveType.Attack;
                enemyBase.attackType = EnemyBase.AttackType.A5;
            }
        }
    }
    void A5() 
    {
        if (AttackPhase == 0) 
        {
            
            enemyBase.charaDamage.DF = PowerDeffence;
            DengerAreaSpawn(DengerArea[0], transform.position, 2);
            NextPhase();
            enemyBase.animator.SetInteger("Anim", 2);

            foreach (var shield in magicShieldEffects) 
            {
                shield.emissionRate = ShieldEmmision;
            }
        }
        if (AttackPhase == 1) 
        {
            if (attackcount() > 2) 
            {
                GameObject CL_Attack = Instantiate(AttackArea[0], transform.position, Quaternion.identity);
                Destroy(CL_Attack,0.1f);
                GameObject CL_Effect = Instantiate(AttackEffect[0], transform.position, Quaternion.identity);
                Destroy(CL_Effect, 2);
                enemyBase.animator.SetInteger("Anim", 3);
                NextPhase();
            }
        }
        if (AttackPhase == 2) 
        {
            
            for (int i = 0; i < StaffSpawnPoint.Count; i++) 
            {
                GameObject CL_Staff = Instantiate(Staffs[i], StaffSpawnPoint[i].transform.position, Quaternion.identity);
                NowStaffs.Add(CL_Staff);

                AllEnemy.Add(CL_Staff);

                GameObject CL_Effect = Instantiate(chainEffect, CL_Staff.transform.position, Quaternion.identity);
                Vector2 DistanceChain = transform.position - CL_Staff.transform.position;
                CL_Effect.transform.up = DistanceChain;

                CL_Effect.transform.parent = CL_Staff.transform;
            }
            BrealEffect.SetActive(false);
            Break = false;
            Finish();
            
        }
    }
}
