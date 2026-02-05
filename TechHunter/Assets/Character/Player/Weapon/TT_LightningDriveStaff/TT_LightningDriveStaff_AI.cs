using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TT_LightningDriveStaff_AI : PlayerSkillBase
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
            GameObject CL_Weapon = Instantiate(SkillWeapon,transform.position, Quaternion.identity);
            Att = ATT.A2;
            CL_Weapon.transform.parent = playerController.transform;
        }
        else if (Att == ATT.A2)
        {
            if (skillcount > 0.4)
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
