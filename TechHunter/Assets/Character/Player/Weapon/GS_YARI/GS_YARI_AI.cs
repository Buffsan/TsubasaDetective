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

            GameObject CL_Weapon = Instantiate(SkillWeapon, playerController.gameObject.transform.position, Quaternion.identity);
            playerController.isCursorDirection();

            CL_Weapon.transform.up = playerController.CursorDirection;
            CL_Weapon.transform.parent = playerController.transform;

            Destroy(CL_Weapon, 3);
            Att = ATT.A2;
        }
        if (Att == ATT.A2)
        {
            playerController.isMove(0.6f);
            if (skillcount > 0.8)
            {
                end();
            }

        }
    }
}
