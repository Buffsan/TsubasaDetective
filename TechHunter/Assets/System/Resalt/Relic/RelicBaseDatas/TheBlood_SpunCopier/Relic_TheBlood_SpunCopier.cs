using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic_TheBlood_SpunCopier : RelicBase_AI
{
    [SerializeField] GameObject Ghost;
    [SerializeField] int SPAWN_NUMBER = 0;
    [SerializeField] RelicData relicData;

    private void Start()
    {
        foreach (var myRelic in playerController.relicDatas) 
        {
            if (relicData.RelicName == myRelic.RelicName) 
            { 
            SPAWN_NUMBER++;
            }
        }
    }
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
        ghost.WaitTime = 5 - AI.CoolTime / 10 ;
        if (ghost.WaitTime <= 0) ghost.WaitTime = 0.3f;
        ghost.WaitTime += SPAWN_NUMBER * 1f;
        ghost.playerskillBase = AI.SkillData;
        ghost.skillCardData = AI;
    }
    
}
