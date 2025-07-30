using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CG_AutomaticCrossbow_AI : PlayerSkillBase
{

    PlayerSkillBase MYskillBase;

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

            GameObject CL_Skill = Instantiate(SkillWeapon, playerController.transform.position, Quaternion.identity);
            CL_Skill.transform.parent = playerController.transform;
            Att = ATT.A2;
        
        }
        if (Att == ATT.A2) 
        {
            if (skillcount > 4)
            {
                end();
            }
            else 
            {
                playerController.isMove(0.5f);
            }
        }


    }
}
