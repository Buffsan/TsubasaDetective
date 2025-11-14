using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DK_LancethatPiercesHeresy_AI : PlayerSkillBase
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
    [SerializeField] BuffData buffdata;
    public override void SkillPlay()
    {
        skillcount += Time.fixedDeltaTime;
        if (A_Phase == 0) 
        {
            playerController.playerBuff.SpawnBuff(buffdata);
            GameObject CL_Weapon = SkillSpawn(SkillWeapon, TargetObject);
            DK_LancethatPiercesHeresy dK_LancethatPiercesHeresy = CL_Weapon.GetComponent<DK_LancethatPiercesHeresy>();
            dK_LancethatPiercesHeresy.player = TargetObject;
            NextPhase();

        }
        if (A_Phase == 1) 
        {
            PlayerMove(0.12f);
            if (skillcount > 0.5f) 
            {
                NextPhase();
                playerController.NowSkillMove = 10;
            }
        }
        if (A_Phase == 2) 
        {
            if (skillcount > 0.25f) 
            { 
                end();
            }
        }
    }
}
