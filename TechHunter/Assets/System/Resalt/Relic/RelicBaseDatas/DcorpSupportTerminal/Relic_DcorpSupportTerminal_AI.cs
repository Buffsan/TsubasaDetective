using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic_DcorpSupportTerminal_AI : RelicBase_AI
{
    [SerializeField] float BuffStartTime =5;
    [SerializeField] float BuffTime =0.5f;
    [SerializeField] BuffData buffdata;
    [SerializeField] Animator RelicObject;
    float BuffCount = 0;
    int lastVisualState = -1;
    PlayerController playerController => PlayerController.Instance;

    private void OnEnable()
    {
        // クリティカルイベントを購読
        GlobalEvents.OnPlayerAttack += OnCritical;
        lastVisualState = -1;
    }

    private void OnDisable()
    {
        // イベント購読を解除（忘れるとバグの原因）
        GlobalEvents.OnPlayerAttack -= OnCritical;
    }
    private void OnCritical(GameObject target, float damage, float criticalRate, bool c) 
    {
        BuffCount = 0;
    }
    public override void BaseAction()
    {
        BuffCount += Time.fixedDeltaTime;
        if (BuffCount > BuffStartTime)
        {
            if (lastVisualState != 1)
            {
                RelicObject.Play("出現");
                lastVisualState = 1;
            }
            if (BuffCount > BuffStartTime + BuffTime)
            {
                playerController.playerBuff.SpawnBuff(buffdata);
                BuffCount = BuffStartTime;
            }
        }
        else 
        {
            if (lastVisualState != 0)
            {
                RelicObject.Play("消える");
                lastVisualState = 0;
            }
        }
    }
}
