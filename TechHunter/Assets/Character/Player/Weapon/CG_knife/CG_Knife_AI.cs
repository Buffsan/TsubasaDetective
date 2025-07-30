using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CG_Knife_AI : PlayerSkillBase
{
    // Start is called before the first frame update

    PlayerSkillBase MYskillBase;

    public GameObject SaveTarget;
    float SmallDistance = 9999999999;

    GameObject[] enemyObject;
    List<GameObject> enemyList;
    [SerializeField] BuffData buff;

    void Start()
    {
        MYskillBase = GetComponent<PlayerSkillBase>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController.SaveSkillBase != MYskillBase)
        {
            Destroy(this.gameObject);
        }
    }
    public override void SkillPlay()
    {
        skillcount += Time.deltaTime;
        if (Att == ATT.A2) 
        {
            playerController.isMove(3f);
            if (skillcount > 0.15)
            {playerController.mode = PlayerController.ModeType.M1;
            playerController.movetype = PlayerController.MoveType.Nomal;
                
            }
        }
        if (Att == ATT.A1) 
        {

            //GameObject CL_Bullet = Instantiate(SlillBullet, playerController.transform.position, Quaternion.identity);
            //Rigidbody2D rb = CL_Bullet.GetComponent<Rigidbody2D>();
            //Destroy(CL_Bullet,5);
            enemyObject = GameObject.FindGameObjectsWithTag("Enemy");
            enemyList = new List<GameObject>(enemyObject);
            //playerController.playerDamage.SetInvincible(0.2f);
            playerController.playerBuff.SpawnBuff(buff);
            

            foreach (GameObject enemy in enemyObject)
            {

                float Distance = Vector2.Distance(enemy.transform.position, playerController.transform.position);
                //Debug.Log(Distance);
                if (Distance < SmallDistance)
                {
                    SmallDistance = Distance;
                    SaveTarget = enemy;
                }

            }
            Vector2 Direction = Vector2.zero;
            if (SaveTarget != null)
            {
                Direction = SaveTarget.transform.position - playerController.gameObject.transform.position;

                float angleStep = 15;
            float angle = -angleStep;
            for (int i = 0; i < 3; i++) 
            {//Debug.Log("“Š‚°‚é");

                GameObject bullet = Instantiate(SlillBullet, playerController.transform.position, Quaternion.identity);
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                Destroy(bullet,5);

                Vector2 bulletDirection = Quaternion.Euler(0,0,angle) * Direction.normalized;
                rb.velocity = bulletDirection * 12;
                bullet.transform.up = bulletDirection;
                angle += angleStep;

            }
            }
            

            
            //rb.velocity = Direction.normalized * 12;
            //CL_Bullet.transform.up = Direction;
            
            

            Att = ATT.A2;
                skillcount = 0;

            //Debug.Log("AAAAA");
            Destroy(this.gameObject);
        }
    }
}
