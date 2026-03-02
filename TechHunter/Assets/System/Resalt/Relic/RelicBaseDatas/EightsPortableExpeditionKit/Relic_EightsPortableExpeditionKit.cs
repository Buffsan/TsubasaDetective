using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic_EightsPortableExpeditionKit : RelicBase_AI
{
    [SerializeField] List<SkillCardData> skillCards = new List<SkillCardData>();
    GameObject SaveNomalSkill = null;
    
    bool Stop = false;

    [SerializeField] float Max_Caurse_Count = 15;
    [SerializeField] List<float> Cerse_Level = new List<float>();

    int MAX_CONBO = 4;

    public List<int> weaponConbos = new List<int>();
    [SerializeField] List<float>weaponCounts = new List<float>();
    ALL_SystemManager aLL_System => ALL_SystemManager.Instance;

    [SerializeField] float CerseCount = 0;

    // Start is called before the first frame update

    public void AddConbo(int ID) 
    {
        weaponConbos[ID]++;
        weaponCounts[ID] = 0 ;
        if (MAX_CONBO <= weaponConbos[ID]) weaponConbos[ID] = 0;
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
        /*
        ///Debug.Log("スキル検知");
        int i = 0;
        foreach (var cards in skillCards)
        {
            //Debug.Log("通常攻撃探索");
            if (cards == AI)
            {
                Debug.Log("通常攻撃検知");
                weaponConbos[i]++;
                if (MAX_CONBO < weaponConbos[i]) weaponConbos[i] = 0;
                i++;
            }
        }*/
    }
    public override void BaseAction()
    {
    }
}
