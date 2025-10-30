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
        GlobalEvents.OnCriticalHit += OnCritical;
    }

    private void OnDisable()
    {
        // イベント購読を解除（忘れるとバグの原因）
        GlobalEvents.OnCriticalHit -= OnCritical;
    }

    // クリティカルが発生したときに呼ばれる関数
    private void OnCritical(GameObject target, float damage, float criticalRate) 
    {
        playerController.playerBuff.SpawnBuff(buffData);
    }
}
