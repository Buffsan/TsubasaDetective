using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GS_Katana : PlayerSkillBase
{
    PlayerSkillBase MYskillBase;
    DK_Needle needle;

    public GameObject SaveTarget;
    float SmallDistance=0;

    GameObject[] enemyObject;
    List<GameObject> enemyList;

    [SerializeField] GameObject NeedleOBJ;

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
    // Update is called once per frame
    public override void SkillPlay()
    {
        skillcount += Time.deltaTime;
        if (Att == ATT.A1) 
        {
            enemyObject = GameObject.FindGameObjectsWithTag("Enemy");
            enemyList = new List<GameObject>(enemyObject);

            foreach (GameObject enemy in enemyObject) 
            {

                float Distance = Vector2.Distance(enemy.transform.position , playerController.transform.position);
                //Debug.Log(Distance);
                if (Distance > SmallDistance) 
                {
                    SmallDistance = Distance;
                    SaveTarget = enemy;
                }
            
            }

            GameObject CL_Needle = Instantiate(NeedleOBJ, playerController.transform.position, Quaternion.identity);
            Rigidbody2D CL_rb = CL_Needle.GetComponent<Rigidbody2D>();

            needle = CL_Needle.GetComponent<DK_Needle>();

            needle.Target = SaveTarget;
            //needle.Player = playerController.gameObject;

            //Vector2 velo = SaveTarget.transform.position - playerController.transform.position;

            //CL_Needle.transform.up = velo;
            //CL_Needle.transform.Rotate(0, 0, 90);
            //CL_rb.velocity = velo.normalized *15;

            Att = ATT.A2;
        }
        if (Att == ATT.A2)
        {
            playerController.isMove(0.8f);
            if (needle.Hit) 
            {
                //needle.DieObject();
                Att = ATT.A3;
            }

            
            
            
            
        }
        if (Att == ATT.A3)
        {
            playerController.mode = PlayerController.ModeType.M1;
            playerController.movetype = PlayerController.MoveType.Nomal;

            playerController.transform.position = SaveTarget.transform.position;
            Destroy(this.gameObject);
        }
    }
}
