using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GS_YARI_AI : PlayerSkillBase
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
            playerController.isCursorDirection();

            CL_Weapon.transform.up = playerController.CursorDirection;
            

            Destroy(CL_Weapon, 3);
            Att = ATT.A2;
        }
        if (Att == ATT.A2)
        {
            PlayerMove(0.6f);
            if (skillcount > 0.8)
            {
                end();
            }

        }
    }
}
