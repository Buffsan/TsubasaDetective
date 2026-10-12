using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActiveBuff
{
    public BuffData data;
    public StatusEffectAttack statuseffectdata;
    public float remainTime;
    public float tickTimer = 0;
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
        remainTime -= deltaTime;
    }
    public bool IsExpired()
    {
        return remainTime <= 0;
    }
}