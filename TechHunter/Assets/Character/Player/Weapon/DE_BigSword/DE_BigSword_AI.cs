using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DE_BigSword_AI : PlayerSkillBase
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
            GameObject CL_Weapon = SkillSpawn(SkillWeapon, TargetObject);
            Att = ATT.A2;
        }
        else if (Att == ATT.A2)
        {
            if (skillcount > 1.2)
            {
                playerController.mode = PlayerController.ModeType.M1;
                playerController.movetype = PlayerController.MoveType.Nomal;

                //Debug.Log("AAAAA");
                Destroy(this.gameObject);
            }
            else
            {
                PlayerMove(0.1f);
            }

        }


    }
}
