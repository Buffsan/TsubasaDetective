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
        ghost.HP = (int)AI.CoolTime / 8;
        ghost.playerskillBase = AI.SkillData;
    }
    
}
