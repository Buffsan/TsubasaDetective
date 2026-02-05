using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OW_CrabAxe_AI : PlayerSkillBase
{
    PlayerSkillBase MYskillBase;

    public GameObject SaveTarget;
    float SmallDistance = 9999999999;

    GameObject[] enemyObject;
    List<GameObject> enemyList;

    Vector2 EnemyDirection;

    [SerializeField] BuffData BUFF;
    [SerializeField] Animator animator;

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
            playerController.playerBuff.SpawnBuff(BUFF);Att = ATT.A2;
            
            enemyObject = GameObject.FindGameObjectsWithTag("Enemy");
            enemyList = new List<GameObject>(enemyObject);

            animator.Play("‘Ò‹@", 0, 0);

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
            

        }
        else if (Att == ATT.A2)
        {
            if (SaveTarget != null)
            {
                float TargetDistance = Vector2.Distance((Vector2)SaveTarget.transform.position, (Vector2)playerController.gameObject.transform.position);
                if (skillcount >= 0.5 || TargetDistance < 1)
                {

                    
                    Att = ATT.A3;
                    animator.Play("UŒ‚", 0, 0);
      
                    skillcount = 0;
                    //Destroy(this.gameObject);
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
        else if (Att == ATT.A3) 
        {
            if (skillcount > 0.2) 
            {
                if (SaveTarget == null)
                {
                    Att = ATT.A1;
                    skillcount = 0;
                }
                else 
                { 
                
                }
            }
        }
    }
}
