using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mortar_Controller : MonoBehaviour
{
    public GameObject AttackArea;
    [SerializeField] GameObject Bullet;
    [SerializeField] GameObject BulletAttack;
    [SerializeField] float RagiSize = 1;

    [SerializeField] GameObject particle;
    [SerializeField] GameObject NewSpase;
    [SerializeField] ParticleSystem particlesystem;
    public EnemyBase enemyBase;

    public enum BulletType 
    { 
    
    None,
    Explosion,
    Fire
    
    }
    public BulletType bullettype = BulletType.None;

    public Transform TargetPoint;

    public float speed = 5f;
    public float arcHeight = 2f;

    Vector3 StartPos;
    float journeyLength;
    float startTime;
    float journeyTime;

    [SerializeField] float BulletSize = 4;

    void Start()
    {
        StartPos = Bullet.transform.position;
        journeyLength = Vector3.Distance(StartPos, TargetPoint.position);
        journeyTime = journeyLength / speed;

        if (TargetPoint.position.x > transform.position.x)
        {
            Bullet.transform.localScale = new Vector3(BulletSize, BulletSize, BulletSize);
        }
        else 
        {
            Bullet.transform.localScale = new Vector3(-BulletSize, BulletSize, BulletSize);
        }

        startTime = Time.time;
    }

    private void FixedUpdate()
    {
        float distCovered = (Time.time - startTime)*speed;
        //float fractionjourney = (Time.time - startTime) * speed / journeyLength;

        Vector3 nextPos = Vector3.Lerp(StartPos, TargetPoint.position, distCovered);
           
        nextPos.y += Mathf.Sin(distCovered * Mathf.PI) * arcHeight;
        Bullet.transform.position = nextPos;
        Bullet.transform.up = nextPos;

        if (distCovered >= 1f) 
        {

            if (bullettype != BulletType.None)
            {
                GameObject CL_AttackBullet = Instantiate(BulletAttack, Bullet.transform.position, Quaternion.identity);

                EnemyAttack enemyAttack = CL_AttackBullet.GetComponent<EnemyAttack>();
                enemyAttack.enemyBase = enemyBase;


                GameObject CL_Spase = Instantiate(NewSpase, Bullet.transform.position, Quaternion.identity);
                particle.transform.parent = CL_Spase.transform;

                //particlesystem.Pause();
                Destroy(CL_Spase, 0.5f);
                //particlesystem.emission.rateOverTime = 0f;

            }
            else 
            { 
            
            GameObject CL_AttackArea = Instantiate(AttackArea, Bullet.transform.position, Quaternion.identity);
            CircleCollider2D circle = CL_AttackArea.GetComponent<CircleCollider2D>();
            EnemyAttack enemyAttack = CL_AttackArea.GetComponent<EnemyAttack>();
            enemyAttack.enemyBase = enemyBase;
            circle.radius = RagiSize;
            Destroy(CL_AttackArea,0.05f);
            }

            Destroy(gameObject);
        }

    }
    public void ChangePos(Vector3 value) 
    { 
    TargetPoint.gameObject.transform.position = value;
    }
}
