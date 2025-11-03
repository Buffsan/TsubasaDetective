using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OW_Harpoon_AI : PlayerSkillBase
{
    PlayerSkillBase MYskillBase;

    public GameObject SaveTarget;
    float SmallDistance = 9999999999;

    GameObject[] enemyObject;
    List<GameObject> enemyList;

    Vector2 EnemyDirection;

    [SerializeField] BuffData BUFF;

    void Start()
    {
        MYskillBase = GetComponent<PlayerSkillBase>();
    }
    private void FixedUpdate()
    {
        if (playerController.SaveSkillBase != MYskillBase)
        {
            Destroy(this.gameObject);
        }
    }
    public override void SkillPlay() 
    {
        skillcount += Time.deltaTime;
        if (Att == ATT.A1)
        {
            
            playerController.playerBuff.SpawnBuff(BUFF);
            GameObject CL_Weapon = SkillSpawn(SkillWeapon, TargetObject);
            Att = ATT.A2;
            

            enemyObject = GameObject.FindGameObjectsWithTag("Enemy");
            enemyList = new List<GameObject>(enemyObject);
            if (enemyList.Count == 0) return;
            foreach (GameObject enemy in enemyObject)
            {
                float Distance =0;
                if (TargetObject)
                {
                     Distance = Vector2.Distance(enemy.transform.position, TargetObject.transform.position);
                }
                else 
                { 
                     Distance = Vector2.Distance(enemy.transform.position, playerController.transform.position);
                }
                
                //Debug.Log(Distance);
                if (Distance < SmallDistance)
                {
                    SmallDistance = Distance;
                    SaveTarget = enemy;
                }
            }
            if (TargetObject)
            {
                EnemyDirection = (Vector2)SaveTarget.transform.position - (Vector2)TargetObject.transform.position;
            }
            else 
            { 
             EnemyDirection = (Vector2)SaveTarget.transform.position - (Vector2)playerController.gameObject.transform.position;
            }

               
            CL_Weapon.transform.up = EnemyDirection.normalized;
            CL_Weapon.transform.Rotate(0, 0, 90);
            
        }
        else if (Att == ATT.A2) 
        {
            if (SaveTarget != null)
            {
                float TargetDistance =0;
                if (TargetObject)
                {
                     TargetDistance = Vector2.Distance((Vector2)SaveTarget.transform.position, (Vector2)playerController.gameObject.transform.position);
                }
                else 
                { 
                     TargetDistance = Vector2.Distance((Vector2)SaveTarget.transform.position, (Vector2)playerController.gameObject.transform.position);
                }
                    
                if (skillcount >= 0.25 || TargetDistance < 1)
                {
                    end();

                    //Debug.Log("AAAAA");
                    Destroy(this.gameObject);
                }
                else
                {
                    PlayerVelocityMove(10, EnemyDirection);
                }
            }
            else 
            {
                end ();

                //Debug.Log("AAAAA");
                Destroy(this.gameObject);
            }
        }
    }
}
