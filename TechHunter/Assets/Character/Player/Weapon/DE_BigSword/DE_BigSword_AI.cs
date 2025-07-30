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
            GameObject CL_Weapon = Instantiate(SkillWeapon, playerController.transform.position, Quaternion.identity);
            Att = ATT.A2;
            CL_Weapon.transform.parent = playerController.MainAnimBody.transform;
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
                playerController.isMove(0.1f);
            }

        }


    }
}
