using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SS_StarShot_AI : PlayerSkillBase
{

    PlayerSkillBase MYskillBase;
    [SerializeField] GameObject Effect;

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
            Instantiate(Effect, playerController.gameObject.transform.position, Quaternion.identity);

            Att = ATT.A2;
        }
        if (Att == ATT.A2)
        {
            playerController.isMove(0.3f);
            if (skillcount > 0.5)
            {
                skillcount = 0;
                Att = ATT.A3;
            }
        }
        if (Att == ATT.A3)
        {

            GameObject CL_Skill = Instantiate(SkillWeapon, playerController.transform.position, Quaternion.identity);
            CL_Skill.transform.parent = playerController.transform;
            Att = ATT.A4;

        }
        if (Att == ATT.A4)
        {
            if (skillcount > 2)
            {
                end();
            }
            else
            {
                playerController.isMove(0.3f);
            }
        }


    }
}
