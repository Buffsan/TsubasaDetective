using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic_Dracibless : RelicBase_AI
{

    [SerializeField] bool isSpaceInput = false;
    float WaitCount = 0;
    private void OnEnable()
    {
        // クリティカルイベントを購読
        GlobalEvents.OnSkillUse += OnSkill;
    }

    private void OnDisable()
    {
        // イベント購読を解除（忘れるとバグの原因）
        GlobalEvents.OnSkillUse -= OnSkill;
    }
    private void OnSkill(PlayerSkillBase skillBase, GameObject AI)
    {
        isSpaceInput = false;
    }

    public override void BaseAction()
    {
        if (Input.GetKeyDown(KeyCode.Space))isSpaceInput = true;
        if (Input.GetKeyUp(KeyCode.Space) 
            || playerController.MoveInput != Vector2.zero) isSpaceInput = false;
        if (!isSpaceInput) { WaitCount = 0;  return;}

        WaitCount += Time.fixedDeltaTime;
        if (WaitCount < 1) return;

        SkillCardManager SaveCard = null;
        float MaxCoolTime = 0;
        foreach (var skills in playerController.skillINFO) 
        {
            if (skills.SkillCardManager.mode == SkillCardManager.Mode.CoolTime && skills.SkillCardManager.CoolTime > MaxCoolTime) 
            { 
                MaxCoolTime = skills.SkillCardManager.CoolTime;
                SaveCard = skills.SkillCardManager;
            }
        }
        if (SaveCard != null) 
        {
            Debug.Log(SaveCard.CoolTime);
        }
    }
}
