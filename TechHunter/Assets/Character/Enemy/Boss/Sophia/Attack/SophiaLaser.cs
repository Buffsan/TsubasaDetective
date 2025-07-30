using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SophiaLaser : MonoBehaviour
{
    public EnemyAttack enemyAttack;
    public PlayerAttack playerAttack;
    public Animator animator;
    float LaserAttackTime = 0.25f;
    public bool EnemyAttackbool = true;
    Vector2 SavePos = Vector2.zero;
    public float LaserDieTimer = 2;
    float LaserAttackCount = 0;

    GameObject Laser;

    float LaserCount = 0;

    public int AttackNumaber =0;

    [SerializeField] AudioClip Clip;
    [SerializeField] AudioClip Clip2;
    [SerializeField] GameObject LaserEffect;
    [SerializeField] GameObject AttackArea;
    AudioManager manager => AudioManager.instance;

    PlayerController playerController => PlayerController.Instance;

    public enum MoveType 
    { 
    
        M1,M2, M3, M4, M5, M6, M7, M8, M9, M10, M11, M12, M13, M14, M15, M16, M17, M18, M19, M20, M21, M22, M23, M24, M25,

    }
    MoveType moveType = MoveType.M1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        LaserCount+= Time.deltaTime;

        if (AttackNumaber == 0)
        {
            if (MoveType.M1 == moveType)
            {
                SavePos = playerController.CursorDirection;

                moveType = MoveType.M2;
                LaserCount = 0;
                manager.isPlaySE(Clip);


            }
            if (MoveType.M2 == moveType)
            {

                if (LaserCount > 0.8)
                {

                    GameObject CL_Laser = Instantiate(LaserEffect, transform.position, transform.rotation);
                    Laser = CL_Laser;
                    CL_Laser.transform.Rotate(0, 0, 180);

                    moveType = MoveType.M3;
                    LaserCount = 0;
                    manager.isPlaySE(Clip2);
                }

            }
            if (MoveType.M3 == moveType)
            {
                animator.SetInteger("Anim", 1);
                if (LaserCount > 0.05)
                {
                    moveType = MoveType.M4;
                    LaserCount = 0;
                    if (EnemyAttackbool)
                    {
                        enemyAttack.gameObject.SetActive(true);
                    }
                    else 
                    {
                        //playerAttack.gameObject.SetActive(true);
                    }
                }
            }
            if (MoveType.M4 == moveType)
            {
                if (!EnemyAttackbool) 
                {
                    LaserAttackCount += Time.deltaTime;
                    if (LaserAttackCount > LaserAttackTime) 
                    { 
                    LaserAttackCount = 0;
                        GameObject CL_LasaerAtttack = Instantiate(AttackArea, transform.position,Quaternion.identity);
                        //CL_LasaerAtttack.transform.up = SavePos.normalized;
                        CL_LasaerAtttack.transform.up = playerController.CursorDirection.normalized;
                        Destroy(CL_LasaerAtttack,0.1f);
                    }
                }

                    animator.SetInteger("Anim", 2);
                if (LaserCount > 3)
                {
                    moveType = MoveType.M5;
                    LaserCount = 0;
                }
            }
            if (MoveType.M5 == moveType)
            {
                animator.SetInteger("Anim", 3);
                if (LaserCount > 1)
                {
                    moveType = MoveType.M1;
                    LaserCount = 0;
                    Destroy(gameObject);
                }
            }
        }

        if (AttackNumaber == 1)
        {
            if (MoveType.M1 == moveType)
            {

                transform.localScale = new Vector2(1.2f, 1);
                moveType = MoveType.M2;
                LaserCount = 0;
                manager.isPlaySE(Clip);


            }
            if (MoveType.M2 == moveType)
            {

                if (LaserCount > 1)
                {
                    GameObject CL_Laser = Instantiate(LaserEffect, transform.position, transform.rotation);
                    CL_Laser.transform.Rotate(0, 0, 180);

                    moveType = MoveType.M3;
                    LaserCount = 0;
                    manager.isPlaySE(Clip2);
                }

            }
            if (MoveType.M3 == moveType)
            {
                animator.SetInteger("Anim", 1);
                if (LaserCount > 0.05)
                {
                    moveType = MoveType.M4;
                    LaserCount = 0;
                    if (EnemyAttackbool)
                    {
                        enemyAttack.gameObject.SetActive(true);
                    }
                    else 
                    {
                        //playerAttack.gameObject.SetActive(true);
                    }
                }
            }
            if (MoveType.M4 == moveType)
            {
                animator.SetInteger("Anim", 2);

                transform.Rotate(0, 0, 18*Time.deltaTime);
                if (LaserCount > 15)
                {


                    moveType = MoveType.M5;
                    LaserCount = 0;
                }
            }
            if (MoveType.M5 == moveType)
            {
                animator.SetInteger("Anim", 3);
                if (LaserCount > 1)
                {
                    moveType = MoveType.M1;
                    LaserCount = 0;
                    Destroy(gameObject);
                }
            }
        }
        if (AttackNumaber == 2) 
        {

            if (MoveType.M1 == moveType)
            {


                moveType = MoveType.M2;
                LaserCount = 0;
                manager.isPlaySE(Clip);


            }
            if (MoveType.M2 == moveType)
            {

                if (LaserCount > 0.8)
                {

                    GameObject CL_Laser = Instantiate(LaserEffect, transform.position, transform.rotation);
                    CL_Laser.transform.Rotate(0, 0, 180);

                    moveType = MoveType.M3;
                    LaserCount = 0;
                    manager.isPlaySE(Clip2);
                }

            }
            if (MoveType.M3 == moveType)
            {
                animator.SetInteger("Anim", 1);
                if (LaserCount > 0.05)
                {
                    moveType = MoveType.M4;
                    LaserCount = 0;
                    if (EnemyAttackbool)
                    {
                        enemyAttack.gameObject.SetActive(true);
                    }
                    else 
                    {
                        //playerAttack.gameObject.SetActive(true);
                    }
                }
            }
            if (MoveType.M4 == moveType)
            {
                animator.SetInteger("Anim", 2);
                if (LaserCount > 1)
                {
                    moveType = MoveType.M5;
                    LaserCount = 0;
                }
            }
            if (MoveType.M5 == moveType)
            {
                animator.SetInteger("Anim", 3);
                if (LaserCount > 1)
                {
                    moveType = MoveType.M1;
                    LaserCount = 0;
                    Destroy(gameObject);
                }
            }

        }

    }
}
