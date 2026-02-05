using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic_ComponentsoftheRegenerativeReactor : RelicBase_AI
{
    [SerializeField] int RecoveryNumber = 50;
    [SerializeField] int Add_RecoveryNumber = 1;
    [SerializeField] int Add_Criticul_RecoveryNumber = 5;
    [SerializeField] float RecoveryHP = 1.0f;
    [SerializeField] int RecoveryCount = 0;

    [SerializeField] AudioClip audioClip;
    [SerializeField] GameObject Effect;
    AudioManager audioManager => AudioManager.instance;

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
        switch (c) 
        {
            case true:Add(Add_Criticul_RecoveryNumber);
                break;

            case false:Add(Add_RecoveryNumber);
                break;
        }

        if (RecoveryCount < RecoveryNumber) return;
        RecoveryCount = 0;
        playerController.isRecoveryHP(RecoveryHP);
        SpawnObjectNow(Effect, transform.position, 2);
        audioManager.PlaySE(audioClip);
        
    }
    void Add(int count) 
    {
        RecoveryCount += count;
    }
}
