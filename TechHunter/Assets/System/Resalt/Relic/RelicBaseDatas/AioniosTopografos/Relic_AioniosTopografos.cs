using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic_AioniosTopografos : RelicBase_AI
{
    PlayerController playerController => PlayerController.Instance;
    [SerializeField] BuffData buffData;
    private void OnEnable()
    {
        // クリティカルイベントを購読
        GlobalEvents.OnPlayerAttack += OnCritical;
    }

    private void OnDisable()
    {
        // イベント購読を解除（忘れるとバグの原因）
        GlobalEvents.OnPlayerAttack -= OnCritical;
    }

    // クリティカルが発生したときに呼ばれる関数
    private void OnCritical(GameObject target, float damage, float criticalRate, bool c) 
    {
        if (!c) return;
        playerController.playerBuff.SpawnBuff(buffData);
    }
}
