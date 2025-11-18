using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic_TheNecklaceOfTheEndlessHollowShell : RelicBase_AI
{
    [SerializeField] List<PlayerSkillBase> skillCards = new List<PlayerSkillBase>();
    [SerializeField] GameObject Effect;
    [SerializeField] GameObject Attack;
    [SerializeField] bool Doge = false;

    [SerializeField] GameObject Effect1;
    [SerializeField] GameObject AttackArea1;

    ALL_SystemManager aLL_System => ALL_SystemManager.Instance;

    [SerializeField] float CerseCount = 0;

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
    private void OnSkill(PlayerSkillBase skillBase, SkillCardData AI)
    {

        ///Debug.Log("スキル検知");
        int i = 0;
        bool isfindNomalSkill = false;
        foreach (var cards in skillCards)
        {
            //Debug.Log("通常攻撃探索");
            if (cards.SkillWeapon == skillBase.SkillWeapon)
            {
                if (!Doge) return;
                Doge = false;
                GameObject CL_Effect = SpawnObjectNow_Input(Effect, transform.position, 2);
                GameObject CL_AttackAreat = SpawnObjectNow_Input(Attack, transform.position, 0.1f);

                CL_Effect.transform.up = playerController.CursorDirection.normalized;
                CL_AttackAreat.transform.up = playerController.CursorDirection.normalized;
                isfindNomalSkill = true;
            }
        }
        if (!isfindNomalSkill) 
        {
            if (AI.CoolTime <= 1) return;
            GameObject CL_Effet1 = SpawnObjectNow_Input(Effect1, transform.position, 0.8f);
            GameObject CL_Attack1 = SpawnObjectNow_Input(AttackArea1, transform.position, 0.2f);
            int Scale = (int)(AI.CoolTime / 10) + 1;
            CL_Effet1.transform.localScale = Vector2.one * Scale;
            CL_Attack1.transform.localScale = Vector2.one * Scale;
            if (AI.CoolTime / 10 < 3) return;
            GameObject CL_Effet2 = SpawnObjectNow_Input(Effect1, transform.position, 0.8f);
            GameObject CL_Attack2 = SpawnObjectNow_Input(AttackArea1, transform.position, 0.2f);
            Scale = (int)(AI.CoolTime / 15) + 1;
            CL_Effet1.transform.localScale = Vector2.one * Scale;
            CL_Attack1.transform.localScale = Vector2.one * Scale;
        }

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) Doge = true;
    }
    
}

