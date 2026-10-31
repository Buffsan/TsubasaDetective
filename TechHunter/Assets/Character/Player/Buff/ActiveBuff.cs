using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActiveBuff
{
    public BuffData data;
    public StatusEffectAttack statuseffectdata;
    public float remainTime;
    public float tickTimer = 0;
    // 魅了などの蓄積型の状態異常値。SOではなく個体ごとに保持する。
    public float accumulatedValue = 0f;
    public GameObject effectObject;
    public List<AudioClip> audioClips;
    public ActiveBuff(StatusEffectAttack buffData)
    {
        statuseffectdata = buffData;
        remainTime = statuseffectdata.Duration;
    }
    public ActiveBuff(BuffData buffData)
    {
        data = buffData;
        remainTime = buffData.BuffTime;
    }
    public void Tick(float deltaTime)
    {
        if (statuseffectdata != null && statuseffectdata.IsPermanent) return;
        remainTime -= deltaTime;
    }
    public bool IsExpired()
    {
        return (statuseffectdata == null || !statuseffectdata.IsPermanent) && remainTime <= 0;
    }
}