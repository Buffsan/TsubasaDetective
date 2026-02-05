using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class None_Skill_AI : PlayerSkillBase
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
        playerController.mode = PlayerController.ModeType.M1;
        playerController.movetype = PlayerController.MoveType.Nomal;

        //Debug.Log("AAAAA");
        Destroy(this.gameObject);
    }
}
