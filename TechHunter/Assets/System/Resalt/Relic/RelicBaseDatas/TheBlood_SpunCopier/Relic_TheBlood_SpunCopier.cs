using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic_TheBlood_SpunCopier : RelicBase_AI
{
    [SerializeField] GameObject Ghost;
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
    private void OnSkill(PlayerSkillBase skillBase, SkillCardData AI)
    {
        
        GameObject CL_Ghost = Instantiate(Ghost, transform.position, Quaternion.identity);
        Relic_TheBlood_SpunCopier_Ghost ghost = CL_Ghost.GetComponent<Relic_TheBlood_SpunCopier_Ghost>();
        ghost.HP = (int)AI.CoolTime / 7 +1;
        ghost.WaitTime = 5 - AI.CoolTime / 10;
        if (ghost.WaitTime <= 0) ghost.WaitTime = 0.3f;
        ghost.playerskillBase = AI.SkillData;
    }
    
}
