using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TD_Houki_AI : PlayerSkillBase
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
            CL_Weapon.transform.Rotate(0, 0, 90);

            Destroy(CL_Weapon , 5);
            Att = ATT.A2;
        }
        if (Att == ATT.A2) 
        {
            PlayerMove(0.4f);
            if (skillcount > 0.5)
            {
                end();
            }
        
        }
    }
}
