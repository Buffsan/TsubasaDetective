using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class GlobalEvents
{
    public static event Action<int> OnNextSpot;
    public static event Action<float,bool> OnPlayerDamage;
    public static event Action<PlayerSkillBase,GameObject> OnSkillUse;
    // Critical発生時のイベント
    // 引数：攻撃対象, ダメージ量, クリティカル倍率など
    public static event Action<GameObject, float, float> OnCriticalHit;

    public static void InvokeOnNextSpot(int spotNumber) 
    { 
    OnNextSpot?.Invoke(spotNumber);
    }
    public static void InvokeOnPlayerDamage(float damage,bool Invincible) 
    { 
        OnPlayerDamage?.Invoke(damage, Invincible);
    }
    public static void InvokeSkillUse(PlayerSkillBase skill , GameObject Skill_AI)
    {
     OnSkillUse?.Invoke(skill,Skill_AI);
    }
    // 呼び出し用
    public static void InvokeCritical(GameObject target, float damage, float multiplier)
    {
        OnCriticalHit?.Invoke(target, damage, multiplier);
    }
}
