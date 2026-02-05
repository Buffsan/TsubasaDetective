using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DK_Zweihander_AI : PlayerSkillBase
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
    public override void SkillPlay()
    {
        skillcount += Time.deltaTime;
        if (Att == ATT.A1)
        {
            GameObject CL_Weapon = SkillSpawn(SkillWeapon, TargetObject);
            Att = ATT.A2;
        }
        if (Att == ATT.A2) 
        {
            if (skillcount > 0.4)
            {
                end();
            }
            else 
            {
                PlayerMove(0.5f);
            }
        }
    }
}
