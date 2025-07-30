using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TG_Maquette_AI : PlayerSkillBase
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
            GameObject CL_Skill = Instantiate(SkillWeapon, playerController.transform.position, Quaternion.identity);
            CL_Skill.transform.parent = playerController.transform;
            Att = ATT.A2;
        }
        if (Att == ATT.A2)
        {
            playerController.isMove(0.3f);
            if (skillcount > 2)
            {
                skillcount = 0;
                Att = ATT.A3;
            }
        }
        if (Att == ATT.A3)
        {

            if (skillcount > 1)
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
