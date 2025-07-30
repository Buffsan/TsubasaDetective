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
            GameObject CL_SkillWeapon = Instantiate(SkillWeapon, playerController.transform.position, Quaternion.identity);
            CL_SkillWeapon.transform.parent = playerController.MainAnimBody.transform;
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
                playerController.isMove(0.5f);
            }
        }
    }
}
