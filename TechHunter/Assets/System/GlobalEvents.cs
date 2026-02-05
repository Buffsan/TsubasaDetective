using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class GlobalEvents
{
    public static event Action<int> OnNextSpot;
    public static event Action<float,bool> OnPlayerDamage;
    public static event Action<PlayerSkillBase,SkillCardData> OnSkillUse;
    public static event Action<float> OnPlayerDie;
    public static event Action<float> OnDecide;
    // Critical発生時のイベント
    // 引数：攻撃対象, ダメージ量, クリティカル倍率など
    public static event Action<GameObject, float, float,bool> OnPlayerAttack;

    public static void InvokePlayerDie(float damage) 
    {
        OnPlayerDie?.Invoke(damage);
    }
    public static void InvokeOnNextSpot(int spotNumber) 
    { 
    OnNextSpot?.Invoke(spotNumber);
    }
    public static void InvokeOnPlayerDamage(float damage,bool Invincible) 
    { 
        OnPlayerDamage?.Invoke(damage, Invincible);
    }
    public static void InvokeSkillUse(PlayerSkillBase skill , SkillCardData Skill_AI)
    {
     OnSkillUse?.Invoke(skill,Skill_AI);
    }
    // 呼び出し用
    public static void InvokePlayerAttack(GameObject target, float damage, float multiplier,bool critical)
    {
        OnPlayerAttack?.Invoke(target, damage, multiplier, critical);
    }
    public static void InvokeOnDecide(float Push)
    {
        OnDecide?.Invoke(Push);
    }
}
