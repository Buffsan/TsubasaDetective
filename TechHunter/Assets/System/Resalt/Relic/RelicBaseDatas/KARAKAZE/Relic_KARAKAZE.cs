using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic_KARAKAZE : RelicBase_AI
{
    [SerializeField] List<GameObject> KARAKAZE = new List<GameObject>();
    [SerializeField] List<SkillCardData> skillCards = new List<SkillCardData>();
    GameObject SaveNomalSkill = null;
    [SerializeField] GameObject Bourei;
    [SerializeField] ParticleSystem particleSystem;
    bool Stop = false;

    [SerializeField] float Max_Caurse_Count = 15;
    [SerializeField] List<float> Carse_Level = new List<float>();
    ALL_SystemManager aLL_System => ALL_SystemManager.Instance;

    [SerializeField]float CerseCount = 0;

    // Start is called before the first frame update
    

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
    private void OnSkill(PlayerSkillBase skillBase,SkillCardData AI) 
    {
        
        ///Debug.Log("スキル検知");
        int i = 0;
        foreach (var cards in skillCards)
        {
            //Debug.Log("通常攻撃探索");
            if (cards == AI)
            {
                Debug.Log("通常攻撃検知");
                if (CerseCount < Max_Caurse_Count)
                {
                    if (i == 0) CerseCount += 3;
                    if (i == 1) CerseCount += 3.5f;
                    if (i == 2) CerseCount += 5;
                }
                if (CerseCount > Carse_Level[1])
                {
                    Instantiate(KARAKAZE[1], transform.position, Quaternion.identity);
                }
                else if (CerseCount > Carse_Level[0])
                {
                    Instantiate(KARAKAZE[0], transform.position, Quaternion.identity);
                }
                GameObject CL_Bourei = Instantiate(Bourei, transform.position, Quaternion.identity);
                Destroy(CL_Bourei, 3);
                i++;
            }
        }
    }
    public override void BaseAction()
    {
        if (aLL_System.systemManager.gameMode == SystemManager.GameMode.Goal) 
        {
            CerseCount = 0;
        }
        if (CerseCount > 0) 
        {
            CerseCount -= Time.fixedDeltaTime;
        }
        if (CerseCount > 10)
        {
            particleSystem.emissionRate = 40;
        }
        else 
        {
            particleSystem.emissionRate = 0;
        }
        
    }
}
