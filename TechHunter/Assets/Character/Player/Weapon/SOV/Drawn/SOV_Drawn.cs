using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SOV_Drawn : MonoBehaviour
{
    [SerializeField] GameObject Slash;
    [SerializeField] GameObject AttackArea;
    [SerializeField] GameObject Bullet;
    [SerializeField] float BulletSpeed = 20;

    [SerializeField]Rigidbody2D rb;

    [SerializeField] float ShotTime = 3;

    bool Right = false;
    float AttackCount = 0;
    float MoveCount = 0;
    int Phase = 0;
    PlayerController controller => PlayerController.Instance;
    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;
    void Start()
    {
  
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        MoveCount += Time.deltaTime;
        if (Phase == 0) 
        { 
            rb.velocity = controller.CursorDirection.normalized*30;
            Phase++;
        }
        if (Phase == 1)
        {
            if (MoveCount < ShotTime)
            {

                rb.velocity = new Vector2(rb.velocity.x * 0.9f, rb.velocity.y * 0.9f);

                if (AttackCount > 0.1)
                {
                    if (Right)
                    {
                        GameObject CL_Slash = Instantiate(Slash, transform.position, Quaternion.identity);
                        CL_Slash.transform.localScale = new Vector2(4, 4);
                        Destroy(CL_Slash, 0.5f);
                        Right = false;

                        GameObject CL_AttackArea = Instantiate(AttackArea, new Vector2(transform.position.x + 0.9f, transform.position.y), Quaternion.identity);
                        Destroy(CL_AttackArea, 0.05f);
                    }
                    else
                    {
                        GameObject CL_Slash = Instantiate(Slash, transform.position, Quaternion.identity);
                        CL_Slash.transform.localScale = new Vector2(-4, -4);
                        Destroy(CL_Slash, 0.5f);
                        Right = true;
                        GameObject CL_AttackArea = Instantiate(AttackArea, new Vector2(transform.position.x - 0.9f, transform.position.y), Quaternion.identity);
                        Destroy(CL_AttackArea, 0.05f);
                    }
                    AttackCount = 0;
                }
                else
                {
                    AttackCount += Time.deltaTime;
                }
            }
            else
            {

                Phase++;
                MoveCount = 0;
            }
        }
        if (Phase == 2) 
        {
            Vector2 Direction = controller.transform.position - transform.position;
            float Distance = Vector2.Distance(controller.transform.position, transform.position);
            float Speed = MoveCount / 1.2f;
            if (Speed >= 3.5) 
            {
                Speed = 3.5f;
            }
            rb.velocity = Direction.normalized * Speed;
            if (Distance < 1 && !controller.SaveSkillBase) 
            {
                Phase++;
                MoveCount = 0;
            }

            if (AttackCount > 0.6)
            {
               Shot();
                if (Right)
                { 
                    GameObject CL_Slash = Instantiate(Slash, transform.position, Quaternion.identity);
                    CL_Slash.transform.localScale = new Vector2(4, 4);
                    Destroy(CL_Slash, 0.5f);
                    Right = false;

                    GameObject CL_AttackArea = Instantiate(AttackArea, new Vector2(transform.position.x + 0.9f, transform.position.y), Quaternion.identity);
                    Destroy(CL_AttackArea, 0.05f);
                }
                else
                {
                    GameObject CL_Slash = Instantiate(Slash, transform.position, Quaternion.identity);
                    CL_Slash.transform.localScale = new Vector2(-4, -4);
                    Destroy(CL_Slash, 0.5f);
                    Right = true;
                    GameObject CL_AttackArea = Instantiate(AttackArea, new Vector2(transform.position.x - 0.9f, transform.position.y), Quaternion.identity);
                    Destroy(CL_AttackArea, 0.05f);
                }
                AttackCount = 0;
            }
            else
            {
                AttackCount += Time.deltaTime;
            }
        }
        if (Phase == 3) 
        { 
            rb.velocity = new Vector2(0, rb.velocity.y + 3);
            if (MoveCount > 1)
            {
                Destroy(gameObject);
            }
        }
    }

    void Shot() 
    {
        List<GameObject> enemys = new List<GameObject>();

        foreach (var allenemy in ALL_System.systemManager.AllEnemy)
        {
            if (allenemy) enemys.Add(allenemy);
        }
        if (enemys.Count <= 0) return;

        GameObject TargetEnemy;
        TargetEnemy = enemys[Random.Range(0, enemys.Count)];

        Vector2 targetDirection = TargetEnemy.transform.position - transform.position;


        GameObject CL_Bullet = Instantiate(Bullet, transform.position, Quaternion.identity);
        Rigidbody2D rb = CL_Bullet.GetComponent<Rigidbody2D>();
        rb.velocity = targetDirection.normalized * BulletSpeed;
        Destroy(CL_Bullet, 5);

    }
    
}
