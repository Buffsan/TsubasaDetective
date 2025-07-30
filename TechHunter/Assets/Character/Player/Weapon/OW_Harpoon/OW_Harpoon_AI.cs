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
            GameObject CL_Weapon = Instantiate(SkillWeapon, playerController.transform.position, Quaternion.identity);
            Att = ATT.A2;
            CL_Weapon.transform.parent = playerController.MainAnimBody.transform;

            enemyObject = GameObject.FindGameObjectsWithTag("Enemy");
            enemyList = new List<GameObject>(enemyObject);

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
            EnemyDirection = (Vector2)SaveTarget.transform.position - (Vector2)playerController.gameObject.transform.position;
            CL_Weapon.transform.up = EnemyDirection.normalized;
            CL_Weapon.transform.Rotate(0, 0, 90);
            
        }
        else if (Att == ATT.A2) 
        {
            if (SaveTarget != null)
            {
                float TargetDistance = Vector2.Distance((Vector2)SaveTarget.transform.position, (Vector2)playerController.gameObject.transform.position);
                if (skillcount >= 0.25 || TargetDistance < 1)
                {
                    playerController.mode = PlayerController.ModeType.M1;
                    playerController.movetype = PlayerController.MoveType.Nomal;

                    //Debug.Log("AAAAA");
                    Destroy(this.gameObject);
                }
                else
                {

                    playerController.rb.velocity = EnemyDirection.normalized * 10;

                }
            }
            else 
            {
                playerController.mode = PlayerController.ModeType.M1;
                playerController.movetype = PlayerController.MoveType.Nomal;

                //Debug.Log("AAAAA");
                Destroy(this.gameObject);
            }
        }
    }
}
