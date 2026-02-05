using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_OW_Chainsaw_AI : PlayerSkillBase
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
            Skill_OW_Chainsaw _Chainsaw = CL_Weapon.GetComponent<Skill_OW_Chainsaw>();
            _Chainsaw.AI_Object = gameObject;
            Att = ATT.A2;
        }
        if (Att == ATT.A2)
        {
            if (skillcount > 10)
            {
                end();
            }
            else
            {
                PlayerMove(0.2f);
            }
        }
    }
}