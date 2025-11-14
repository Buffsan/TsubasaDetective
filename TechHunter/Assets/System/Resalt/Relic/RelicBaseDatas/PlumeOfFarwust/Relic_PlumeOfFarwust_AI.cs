using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic_PlumeOfFarwust_AI : RelicBase_AI
{
    [SerializeField] float AttackCount = 0;
    [SerializeField] float AttackTime = 1;
    [SerializeField] float SpawnAttackNumber = 10;
    [SerializeField] GameObject Effect;

    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;

    public override void BaseAction()
    {
        if (playerController.SaveSkillBase)
        {
            float speedMultiplier = Mathf.Abs(playerController.NowSkillMove);
            if (speedMultiplier < 1) speedMultiplier *= 0.3f;
            AttackCount += Time.fixedDeltaTime * speedMultiplier;
        }
        if (AttackCount < AttackTime) return;

        if (!playerController.SaveSkillBase)
        {
            float i = 0;
            while (AttackCount > AttackTime)
            {
                i++;
                AttackCount -= AttackTime;
                if (i >=2) break;
            }
            AttackCount =0;
            GameObject CL_Effect = SpawnObjectNow_Input(Effect, transform.position, 1);
            GameObject CL_Attack = SpawnObjectNow_Input(AttackObject, transform.position, 1f);

            CL_Effect.transform.localScale = new Vector2(i, i);
            CL_Attack.transform.localScale = new Vector2(i*1.4f, i*1.4f);
        }
    }

    private IEnumerator SpawnSlash(float delayTime,Vector2 targetpos)
    {
        
        yield return new WaitForSeconds(delayTime);
        GameObject CL_Effect = SpawnObjectNow_Input(Effect, targetpos, 1);
        GameObject CL_Attack =  SpawnObjectNow_Input(AttackObject, targetpos, 0.1f);

    }
}
