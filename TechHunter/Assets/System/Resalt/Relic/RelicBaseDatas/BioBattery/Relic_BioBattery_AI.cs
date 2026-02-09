using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Relic_BioBattery_AI : RelicBase_AI
{

    [SerializeField] GameObject Thender;
    [SerializeField] float DestroyTime = 0.2f;
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
        int ThenderNumber = (int)AI.CoolTime / 10;
        float ThenderSize = AI.CoolTime / 20;
        if (ThenderNumber <= 0) return;
        if (AI.CoolTime >= 30)
        {
            ThenderSize *= 2;
            ThenderNumber *= 2;
        }
        StartCoroutine(shotThender(1.5f * SPAWN_NUMBER-1, ThenderSize, ThenderNumber));
    }

    IEnumerator shotThender(float WaitTime ,float size ,float number) 
    {

        yield return new WaitForSeconds(WaitTime);

        
        for (int i = 0; i < number; i++)
        {
            GameObject CL_Thender = Instantiate(Thender, transform.position, Quaternion.identity);
            CL_Thender.transform.localScale = Vector2.one * size;
            CL_Thender.transform.Rotate(0, 0, Random.Range(0, 360));
            Destroy(CL_Thender, DestroyTime);
        }

    }
}
