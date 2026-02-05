using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SophiaSword : MonoBehaviour
{

    [SerializeField] GameObject AttackArea;
    [SerializeField] GameObject AttackPoint;
    [SerializeField] Animator animator;

    [SerializeField] GameObject P1;
    [SerializeField] GameObject P2;
    [SerializeField] ParticleSystem particleSystem;

    public int AttackNumaber = 0;
    public EnemyAttack enemyAttack;
    public EnemyBase EnemyBase;
    public enum MoveMode 
    { 
    
        M1, M2,M3, M4, M5, M6, M7, M8,  M9, M10, M11, M12, M13, M14, M15

    }
    MoveMode moveMode = MoveMode.M1;

    float AttackTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    [System.Obsolete]
    private void FixedUpdate()
    {
        AttackTime += Time.deltaTime;

        

        if (moveMode == MoveMode.M1) 
        {



            if (AttackTime > 1.5) 
            {
                P1.SetActive(false);
                P2.SetActive(true);

                if (AttackNumaber == 0)
                {
                    animator.SetInteger("Anim", 1);
                }
                else if (AttackNumaber == 1) 
                {
                    animator.SetInteger("Anim", 2);
                }
                AttackTime = 0;
                moveMode = MoveMode.M2;

                enemyAttack.enemyBase = EnemyBase;


                //Quaternion newRotation = transform.rotation * Quaternion.Euler(0, 0, 90);
                //GameObject CL_AttackArea = Instantiate(AttackArea,AttackPoint.transform.position, Quaternion.identity);

                
               

                //CL_AttackArea.transform.up = EnemyBase.PlayerDirection;
                //CL_AttackArea.transform.Rotate(0,0,90);

                //EnemyAttack enemyAttack = CL_AttackArea.GetComponent<EnemyAttack>();

              //  Destroy(CL_AttackArea,0.1f);
               //enemyAttack.enemyBase = EnemyBase;
            }
        
        }
        if (moveMode == MoveMode.M2)
        {
            if (AttackTime > 0.1) 
            { 
            enemyAttack.enabled = true;
            }


            if (AttackTime > 0.4 && AttackNumaber ==0 || AttackTime > 1.4 && AttackNumaber == 1)
            {
                particleSystem.emissionRate = 0;
                /*
                P1.SetActive(true);
                P2.SetActive(false);

                
                
                
               */moveMode = MoveMode.M3;AttackTime = 0;
            }

        }
        if (moveMode == MoveMode.M3)
        {



            if (AttackTime > 1)
            {
                Destroy(gameObject);
            }

        }

    }

    public void ChangeAttackArea(bool value) 
    { 
    
        enemyAttack.enabled = value;
    
    }
}
