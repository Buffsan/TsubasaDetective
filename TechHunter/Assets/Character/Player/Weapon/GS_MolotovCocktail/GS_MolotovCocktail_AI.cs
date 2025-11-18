using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GS_MolotovCocktail_AI : PlayerSkillBase
{
    PlayerSkillBase MYskillBase;

    void Start()
    {
        MYskillBase = GetComponent<PlayerSkillBase>();

        GameObject CL_Skill = SkillSpawn_Add(SkillWeapon, TargetObject,TargetObject);
        GS_MolotovCocktail gS_Molotov = CL_Skill.GetComponent<GS_MolotovCocktail>();
        gS_Molotov.endPos = playerController.cursorController.MousePos;
        end();
    }
    private void FixedUpdate()
    {
        if (playerController.SaveSkillBase != MYskillBase)
        {
            Destroy(this.gameObject);
        }
    }
}
