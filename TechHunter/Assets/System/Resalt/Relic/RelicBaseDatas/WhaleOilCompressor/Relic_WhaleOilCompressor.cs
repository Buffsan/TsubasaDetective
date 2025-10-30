using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic_WhaleOilCompressor : RelicBase_AI
{
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

        Instantiate(AttackObject, target.transform.position, Quaternion.identity);
        //Debug.Log("Critical!");
        // 追加情報を見たい場合はこちら
        //Debug.Log($"対象: {target.name}, ダメージ: {damage}, 倍率: {criticalRate}");
    }
}
