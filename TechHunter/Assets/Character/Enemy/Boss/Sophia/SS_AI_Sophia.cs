using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;

using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SS_AI_Sophia : Boss_MoveBase
{
    [SerializeField] GameObject Motar;
    [SerializeField] GameObject Laser;
    [SerializeField] GameObject Star;
    [SerializeField] GameObject Slash;

    [SerializeField] GameObject AttackStartEffect;

    [SerializeField] AudioClip StarClip;
    [SerializeField] AudioClip AttackStartClip;

    float StarRAMU = 0;

    Vector2 SavePos;

    float ChangeBloom = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (enemyBase.volume.profile.TryGet<Bloom>(out enemyBase.bloom))
        {
            Debug.Log("BloomÉGÉtÉFÉNÉgÇ™å©Ç¬Ç©ÇËÇ‹ÇµÇΩÅI");
            //enemyBase.bloom.scatter.value = 0.5f; // ScatterílÇê›íË
        }
        else
        {
            Debug.LogWarning("ÉvÉçÉtÉ@ÉCÉãÇ…BloomÇ™å©Ç¬Ç©ÇËÇ‹ÇπÇÒÅI");
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

                if (enemyBase.HP < enemyBase.MAXHP / 2) 
                { 
                phase = Phase.P2;
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.attackType = EnemyBase.AttackType.A7;
                }

                if (AttackWaitTime > 3.5)
                {
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;

                    float random = Random.RandomRange(1, 6);

                    if (random == 1)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A10;//8
                    }
                    if (random == 2)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A6;
                    }
                    if (random == 3)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A5;
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
        

        /*
        if (Input.GetKey(KeyCode.H)) 
        {

            enemyBase.moveType = EnemyBase.MoveType.Attack;
            enemyBase.attackType = EnemyBase.AttackType.A1;
            Debug.Log("úaêØåQ");

        }
        if (Input.GetKey(KeyCode.J))
        {

            enemyBase.moveType = EnemyBase.MoveType.Attack;
            enemyBase.attackType = EnemyBase.AttackType.A5;
            Debug.Log("êØâ_");

        }
        if (Input.GetKey(KeyCode.K))
        {

            enemyBase.moveType = EnemyBase.MoveType.Attack;
            enemyBase.attackType = EnemyBase.AttackType.A3;
            Debug.Log("êØ‚I");

        }
        if (Input.GetKey(KeyCode.L))
        {

            enemyBase.moveType = EnemyBase.MoveType.Attack;
            enemyBase.attackType = EnemyBase.AttackType.A8;
            Debug.Log("ÉVÉÖÉGÉâÇÃãÛåÑ");

        }
        if (Input.GetKey(KeyCode.G))
        {

            enemyBase.moveType = EnemyBase.MoveType.Attack;
            enemyBase.attackType = EnemyBase.AttackType.A6;
            Debug.Log("úaêØÇ∆êØâ_");

        }*/
    }
    public override void M_Attack()
    {
    
        enemyBase.AttackCount+= Time.deltaTime;
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
                case EnemyBase .AttackType.A5:
                A5();
                break;
                case EnemyBase .AttackType.A6:
                A6();
                break;
                case EnemyBase .AttackType.A7:
                A7();
                break;
                case EnemyBase .AttackType.A8:
                A8();
                break;
                case EnemyBase .AttackType.A9:
                A9();
                break;
                case EnemyBase .AttackType.A10:
                A10();
                break;

        }

    }
    public override void M_BaseMove()
    {
      
    }
    void A10() 
    {
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            if (enemyBase.AttackCount > 0.5)
            {
                SavePos = new Vector2(enemyBase.Player.transform.position.x + Random.RandomRange(-1f, 1f), enemyBase.Player.transform.position.y + Random.RandomRange(-1f, 1f));
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M2;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2) 
        {

            Vector2 SaveDirection = SavePos - (Vector2)enemyBase.gameObject.transform.position;
            float SaveDistance = Vector2.Distance(SavePos, (Vector2)enemyBase.gameObject.transform.position);

            enemyBase.rb.velocity = SaveDirection.normalized * 10;
            if (SaveDistance < 1 || enemyBase.AttackCount > 1.5) 
            {
                
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M3;
            }                    
        }

        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            enemyBase.rb.velocity = enemyBase.rb.velocity * 0.9f;
            enemyBase.animator.SetInteger("Anim", 2);

            if (enemyBase.AttackCount > 0.4)
            {
                enemyBase.animator.SetInteger("Anim", 3);
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M4;

                GameObject CL_Laser = Instantiate(Laser, enemyBase.transform.position, Quaternion.identity);
                enemyBase.isPlayerLookBase();
                CL_Laser.transform.parent = enemyBase.EnemyAnimBody.transform;
                SophiaLaser sophiaLaser = CL_Laser.GetComponent<SophiaLaser>();
                sophiaLaser.enemyAttack.enemyBase = enemyBase;
                sophiaLaser.AttackNumaber = 2;

                CL_Laser.transform.up = enemyBase.PlayerDirection;
                CL_Laser.transform.Rotate(0, 0, 180);
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {
            if (enemyBase.AttackCount > 0.1)
            {
                enemyBase.animator.SetInteger("Anim", 0);

                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M5;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M5)
        {


            
                enemyBase.mode = EnemyBase.ModeType.M6;
                enemyBase.AttackCount = 0;

        }
        if (enemyBase.mode == EnemyBase.ModeType.M6)
        {
            if (enemyBase.AttackCount > 1)
            {
                enemyBase.mode = EnemyBase.ModeType.M7;
                isStarShot(10, AttackNumber * 15);
                
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M7) 
        {
            if (enemyBase.AttackCount > 1.8)
            {
                AttackNumber++;

                if (AttackNumber < 3)
                {
                    enemyBase.mode = EnemyBase.ModeType.M1;
                }
                else
                {
                    enemyBase.mode = EnemyBase.ModeType.M1;
                    enemyBase.moveType = EnemyBase.MoveType.Move;
                    AttackNumber = 0;
                }
                enemyBase.AttackCount = 0;
            }
        }
    }

    void A9() 
    {
        //êØâ_
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            enemyBase.animator.SetInteger("Anim", 2);
            if (enemyBase.AttackCount > 1)
            {
                enemyBase.animator.SetInteger("Anim", 3);
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M2;

                GameObject CL_Laser = Instantiate(Laser, enemyBase.transform.position, Quaternion.identity);
                enemyBase.isPlayerLookBase();

                SophiaLaser sophiaLaser = CL_Laser.GetComponent<SophiaLaser>();
                sophiaLaser.enemyAttack.enemyBase = enemyBase;

                CL_Laser.transform.up = enemyBase.PlayerDirection;
                CL_Laser.transform.Rotate(0, 0, 180);
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {
            if (enemyBase.AttackCount > 0.7)
            {
                enemyBase.animator.SetInteger("Anim", 0);
                
                AttackNumber++;

                if (AttackNumber < 3)
                {
                    enemyBase.mode = EnemyBase.ModeType.M1;
                    isStarShot(15,5 * AttackNumber);
                }
                else 
                {
                    enemyBase.mode = EnemyBase.ModeType.M3;
                    enemyBase.AttackCount = 0;
                    AttackNumber = 0;
                
                }
                
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            if (enemyBase.AttackCount > 0.8)
            {
                isStarShot(10, AttackNumber * 15);
                AttackNumber++;

                if (AttackNumber < 3)
                {
                    enemyBase.mode = EnemyBase.ModeType.M3;
                    
                }
                else
                {
                    enemyBase.mode = EnemyBase.ModeType.M1;
                    enemyBase.moveType = EnemyBase.MoveType.Move;
                    AttackNumber = 0;
                }
                enemyBase.AttackCount = 0;
            }
        }
    }
    void A8() 
    {
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            GameObject CL_StartEffect = Instantiate(AttackStartEffect, enemyBase.gameObject.transform.position, Quaternion.identity);
            enemyBase.audioManager.isPlaySE(AttackStartClip);
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;
            enemyBase.animator.SetInteger("Anim", 2);
            

        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            if (enemyBase.AttackCount > 1.5)
            {
                enemyBase.mode = EnemyBase.ModeType.M2;
                enemyBase.AttackCount = 0;
                enemyBase.animator.SetInteger("Anim", 3);
            }

        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {

            if (enemyBase.AttackCount > 0.14)
            {
                isStarRandomShot(80);
                enemyBase.AttackCount = 0;
            AttackNumber++;

                enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * 0.5f;

                if (AttackNumber % 10 == 0 && phase == Phase.P2) 
                {
                    isStarShot(16, AttackNumber);
                }

                if (AttackNumber > 60)
                {enemyBase.rb.velocity = Vector2.zero;
                    enemyBase.mode = EnemyBase.ModeType.M1;
                    enemyBase.moveType = EnemyBase.MoveType.Move;
                    enemyBase.AttackCount = 0;
                    enemyBase.animator.SetInteger("Anim", 0);
                    AttackNumber = 0;
                }
                else 
                {
                    enemyBase.audioManager.isPlaySE(StarClip);
                    enemyBase.mode = EnemyBase.ModeType.M2;
                }
            }
        }
    }
    void A7() 
    {
        
        //úaêØÇ∆ëÂêØâ_
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            float Distance = Vector2.Distance(new Vector2(0, 0), enemyBase.transform.position);


            //ChangeBloom = enemyBase.bloom.scatter.value;
            if (Distance < 0.5)
            {

                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M2;
                enemyBase.rb.velocity = Vector2.zero;

            }
            else 
            {
                Vector2 Cpos = new Vector2(0, 0);
                Vector2 direction = Cpos - (Vector2)enemyBase.gameObject.transform.position;
                enemyBase.rb.velocity = direction.normalized *6;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {

            for (int i = 0; i < 15; i++)
            {

                Vector2 TargetPoint = new Vector2(Random.Range(-10f, 10f), Random.Range(-2f, 9f));

                GameObject CL_Moter = Instantiate(Motar, enemyBase.transform.position, Quaternion.identity);
                Mortar_Controller mortar_ = CL_Moter.GetComponent<Mortar_Controller>();
                mortar_.enemyBase = enemyBase;
                mortar_.ChangePos(TargetPoint);
            }
            enemyBase.AttackCount = 0;
            enemyBase.mode = EnemyBase.ModeType.M3;
        }

        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            
            
           

            enemyBase.animator.SetInteger("Anim", 2);



            if (enemyBase.AttackCount > 2)
            {

                //enemyBase.bloom.scatter.value = Mathf.PingPong(Time.time, 1);
                enemyBase.bloom.scatter.value = 0.5f;
                enemyBase.bloom.intensity.value = 15;

                enemyBase.animator.SetInteger("Anim", 3);
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M4;

                

                for (int i = 0; i < 4; i++) 
                {
                    GameObject CL_Laser = Instantiate(Laser, enemyBase.transform.position, Quaternion.identity);
                    enemyBase.isPlayerLookBase();

                    SophiaLaser sophiaLaser = CL_Laser.GetComponent<SophiaLaser>();
                    sophiaLaser.enemyAttack.enemyBase = enemyBase;
                    sophiaLaser.AttackNumaber = 1;

                    CL_Laser.transform.Rotate(0, 0, 90 * i);
                
                }
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {

            StarRAMU += Time.deltaTime;
            if (StarRAMU > 1) 
            {
                StarRAMU = 0;
                AttackNumber++;
                isStarShot(13,-8 * AttackNumber);

            }

            if (enemyBase.AttackCount > 15)
            {
                AttackNumber = 0;
                enemyBase.animator.SetInteger("Anim", 0);
                StarRAMU = 0;
                //enemyBase.moveType = EnemyBase.MoveType.Move;

                enemyBase.attackType = EnemyBase.AttackType.A1;
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M1;
                AttackNumber = 0;
            }
        }
    }
    void A6() 
    {
        //úaêØÇ∆êØâ_
        
        if (enemyBase.mode == EnemyBase.ModeType.M1) 
            {

            if (phase == Phase.P2)
            {
                enemyBase.isPlayerLookBase();

                enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * -2.5f;
            }

            enemyBase.animator.SetInteger("Anim", 2);
                if (enemyBase.AttackCount > 1)
                { 
                    enemyBase.mode = EnemyBase.ModeType.M2;
                    enemyBase.AttackCount = 0;
                }
            }
            if (enemyBase.mode == EnemyBase.ModeType.M2)
            {

            if (phase == Phase.P2)
            {
                enemyBase.isPlayerLookBase();

                enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * -3f;
            }

            enemyBase.animator.SetInteger("Anim", 2);
                if (enemyBase.AttackCount > 0.4)
                {
                    enemyBase.mode = EnemyBase.ModeType.M3;
                    enemyBase.AttackCount = 0;
                }
            }
            if (enemyBase.mode == EnemyBase.ModeType.M3)
            {


            if (phase == Phase.P2)
            {
                enemyBase.isPlayerLookBase();

                enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * -0.2f;
            }

            enemyBase.animator.SetInteger("Anim", 3);
                if (enemyBase.AttackCount > 0.2)
                {

                    AttackNumber++;

                    if (AttackNumber < 8)
                    {
                        enemyBase.mode = EnemyBase.ModeType.M2;
                    }
                    else
                    {
                        enemyBase.mode = EnemyBase.ModeType.M4;
                    }
                    GameObject CL_Motar = Instantiate(Motar,enemyBase.transform.position,Quaternion.identity);
                    Mortar_Controller CL_mortar_Controller = CL_Motar.GetComponent<Mortar_Controller>();

                    CL_mortar_Controller.ChangePos(enemyBase.Player.transform.position);
                CL_mortar_Controller.enemyBase = enemyBase;
                    enemyBase.AttackCount = 0;
                    enemyBase.animator.SetInteger("Anim", 0);


                }
            }
            if (enemyBase.mode == EnemyBase.ModeType.M4) 
            {
                if (enemyBase.AttackCount > 0.2) 
                {
                if (phase == Phase.P1)
                {
                    enemyBase.mode = EnemyBase.ModeType.M1;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.attackType = EnemyBase.AttackType.A5;
                    enemyBase.AttackCount = 0;
                    AttackNumber = 0;
                }
                else 
                {
                    enemyBase.mode = EnemyBase.ModeType.M1;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.attackType = EnemyBase.AttackType.A10;
                    enemyBase.AttackCount = 0;
                    AttackNumber = 0;
                }
                
                }
            }
        
    }
    void A5() 
    {
        //êØâ_
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            enemyBase.animator.SetInteger("Anim", 2);
            if (enemyBase.AttackCount > 0.2)
            {
                enemyBase.animator.SetInteger("Anim", 3);
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M2;

                GameObject CL_Laser = Instantiate(Laser, enemyBase.transform.position, Quaternion.identity);
                enemyBase.isPlayerLookBase();

                SophiaLaser sophiaLaser = CL_Laser.GetComponent<SophiaLaser>();
                sophiaLaser.enemyAttack.enemyBase = enemyBase;

                CL_Laser.transform.up = enemyBase.PlayerDirection;
                CL_Laser.transform.Rotate(0, 0, 180);
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {
            if (enemyBase.AttackCount > 0.1)
            {
                enemyBase.animator.SetInteger("Anim", 0);
                
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M3;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {


            if (enemyBase.AttackCount > 0.01)
            {

                enemyBase.mode = EnemyBase.ModeType.M4;
                enemyBase.AttackCount = 0;

            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {
            if (enemyBase.AttackCount > 1)
            {
                isStarShot(10, AttackNumber*15);
                AttackNumber++;

                if (AttackNumber < 3)
                {
                    enemyBase.mode = EnemyBase.ModeType.M4;
                }
                else 
                { 
                enemyBase.mode =EnemyBase.ModeType.M1;
                    enemyBase.moveType = EnemyBase.MoveType.Move;
                    AttackNumber = 0;
                }
                enemyBase.AttackCount = 0;
            }
        }
        


    }
    void A4() 
    {
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {enemyBase.isPlayerLookBase();

            if (enemyBase.PlayerDistance > 2 && AttackNumber ==0)
            {
                
                enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * 10;
            }
            else 
            {
                enemyBase.animator.SetInteger("Anim", 2);
                enemyBase.mode = EnemyBase.ModeType.M2;
                enemyBase.AttackCount = 0;
                
                
            }

            
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {
            enemyBase.rb.velocity = enemyBase.rb.velocity * 0.8f;
            if (enemyBase.AttackCount > 0.2)
            {
                enemyBase.rb.velocity = Vector2.zero;
                enemyBase.animator.SetInteger("Anim", 3);
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;


                GameObject CL_Sword = Instantiate(Slash, enemyBase.transform.position, Quaternion.identity);

                SophiaSword sword = CL_Sword.GetComponent<SophiaSword>();

                if (AttackNumber == 0) 
                {
                    sword.AttackNumaber = 0;
                }else if (AttackNumber == 1) 
                {
                    sword.AttackNumaber = 1;
                }

                sword.EnemyBase = enemyBase;
                enemyBase.isPlayerLookBase();

                CL_Sword.transform.up = enemyBase.PlayerDirection;
                CL_Sword.transform.Rotate(0, 0, 90);

                sword.EnemyBase = enemyBase;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            enemyBase.isPlayerLookBase();
            if (enemyBase.AttackCount > 0.5)
            {
                if (phase == Phase.P2 && AttackNumber ==0)
                {
                    AttackNumber = 1;
                    enemyBase.mode = EnemyBase.ModeType.M1;
                    enemyBase.AttackCount = 0;
                }
                else 
                { 
                enemyBase.moveType = EnemyBase.MoveType.Move;
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M1;
                    AttackNumber = 0;
                }
                
            }
        }
    }

    void isStarShot(int value,int Addvalue) 
    {
        for (int i = 0; i < value; i++)
        {
            GameObject CL_Star = Instantiate(Star, enemyBase.gameObject.transform.position, Quaternion.identity);
            Rigidbody2D CL_rb = CL_Star.GetComponent<Rigidbody2D>();
            EnemyAttack CL_EnemyAttack= CL_Star.GetComponent<EnemyAttack>();
            
            CL_EnemyAttack.enemyBase = enemyBase;

            float T_Angle = (((360 / value)* i)+Addvalue ) * Mathf.Deg2Rad;

            Vector3 T_velocity = new Vector3(Mathf.Cos(T_Angle), Mathf.Sin(T_Angle), 0) * 2.5f;
            CL_rb.velocity = T_velocity;

            Destroy(CL_Star, 10);
        }
    }
    void isStarRandomShot(int value)
    {

        enemyBase.isPlayerLookBase();

            GameObject CL_Star = Instantiate(Star, enemyBase.gameObject.transform.position, Quaternion.identity);
            Rigidbody2D CL_rb = CL_Star.GetComponent<Rigidbody2D>();
            EnemyAttack CL_EnemyAttack = CL_Star.GetComponent<EnemyAttack>();

            CL_EnemyAttack.enemyBase = enemyBase;

            float T_Angle = (Random.RandomRange(-value, value)) * Mathf.Deg2Rad;


        Vector2 direction = enemyBase.PlayerDirection.normalized;
        Vector3 T_velocity = new Vector3(
            Mathf.Cos(T_Angle) * direction.x - Mathf.Sin(T_Angle) * direction.y,
            Mathf.Sin(T_Angle) * direction.x + Mathf.Cos(T_Angle) * direction.y,
            0
        );

        CL_rb.velocity = T_velocity.normalized * 2.3f;

            Destroy(CL_Star, 10);
        
    }
    private void A3()
    {
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {

            GameObject CL_StartEffect = Instantiate(AttackStartEffect, enemyBase.gameObject.transform.position, Quaternion.identity);
            enemyBase.audioManager.isPlaySE(AttackStartClip);
            
            enemyBase.animator.SetInteger("Anim", 2);

            enemyBase.mode = EnemyBase.ModeType.M4;
                enemyBase.AttackCount = 0;

        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {


            if (enemyBase.AttackCount > 1) 
            {

                enemyBase.mode = EnemyBase.ModeType.M2;
                enemyBase.AttackCount = 0;

            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {

            if (phase == Phase.P2) 
            {
                enemyBase.isPlayerLookBase();

                enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized* 0.3f;
            }

            if (enemyBase.AttackCount > 0.8)
            {
                if (AttackNumber % 3 == 0 && phase == Phase.P2)
                {
                    isStarShot(36, 20 * AttackNumber);
                }
                else 
                { 
                 isStarShot(12,20*AttackNumber);
                }
               

                    AttackNumber++;

                if (AttackNumber < 9)
                {
                    enemyBase.mode = EnemyBase.ModeType.M2;
                }
                else 
                { 
                enemyBase.mode = EnemyBase.ModeType.M3;
                }
                
                enemyBase.AttackCount = 0;
            }

            
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            if (enemyBase.AttackCount > 1.5)
            {
                
                    isStarShot(36, 0);
                
                
             
                
                AttackNumber = 0;
                enemyBase.animator.SetInteger("Anim", 0);
                enemyBase.moveType = EnemyBase.MoveType.Move;
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M1;
            }
        }
    }
    void A2()
    {
        //êØâ_
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            enemyBase.animator.SetInteger("Anim", 2);
            if (enemyBase.AttackCount > 0.2)
            {
                enemyBase.animator.SetInteger("Anim", 3);
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M2;

                GameObject CL_Laser = Instantiate(Laser, enemyBase.transform.position, Quaternion.identity);
                enemyBase.isPlayerLookBase();

                SophiaLaser sophiaLaser = CL_Laser.GetComponent<SophiaLaser>();
                sophiaLaser.enemyAttack.enemyBase = enemyBase;

                CL_Laser.transform.up = enemyBase.PlayerDirection;
                CL_Laser.transform.Rotate(0,0,180);
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {
            if (enemyBase.AttackCount > 0.3)
            {
                enemyBase.animator.SetInteger("Anim", 0);
                enemyBase.moveType = EnemyBase.MoveType.Move;
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M1;
            }
        }
    }

    void A1() 
    {

        if (enemyBase.mode == EnemyBase.ModeType.M1) 
        {
            if (enemyBase.AttackCount > 0.2) 
            {

                enemyBase.AttackCount = 0;
               enemyBase.mode = EnemyBase.ModeType.M2;

            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2) 
        {

            for (int i = 0; i < 20; i++) 
            {

                Vector2 TargetPoint = new Vector2(Random.Range(-10f,10f), Random.Range(-2f, 9f));
            
                GameObject CL_Moter = Instantiate(Motar,enemyBase.transform.position,Quaternion.identity);
                Mortar_Controller mortar_ = CL_Moter.GetComponent<Mortar_Controller>();
                mortar_.enemyBase = enemyBase;
                mortar_.ChangePos(TargetPoint);
            }
            enemyBase.AttackCount = 0;
            enemyBase.mode = EnemyBase.ModeType.M3;
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            enemyBase.moveType = EnemyBase.MoveType.Move;
            enemyBase.AttackCount = 0;
            enemyBase.mode = EnemyBase.ModeType.M1;
        }
    }

}
