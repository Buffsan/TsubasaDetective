using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SS_StarSmog_AI : PlayerSkillBase
{

    PlayerSkillBase MYskillBase;
    [SerializeField] GameObject Effect;

    void Start()
    {
        MYskillBase = GetComponent<PlayerSkillBase>();
    }
    private void FixedUpdate()
    {
        if (playerController.SaveSkillBase != MYskillBase && TargetObject == playerController.gameObject)
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
            Instantiate(Effect, playerController.gameObject.transform.position, Quaternion.identity);
           
            Att = ATT.A2;
        }
        if (Att == ATT.A2)
        {
            PlayerMove(0.1f);
            if (skillcount > 1.5) 
            { 
            skillcount =0;
                Att = ATT.A3;
            }
        }
        if (Att == ATT.A3)
        {

            GameObject CL_Weapon = SkillSpawn(SkillWeapon, TargetObject);
            Att = ATT.A4;

        }
        if (Att == ATT.A4)
        {
            if (skillcount > 2)
            {
                end();
            }
            else
            {
                PlayerMove(0.3f);
            }
        }


    }
}
