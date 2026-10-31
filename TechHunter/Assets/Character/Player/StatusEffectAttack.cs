using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Data/CreateStatusEffectAttack")]
public class StatusEffectAttack : StatusEffect
{


    public float Duration = 1;//効果時間

    // 効果時間が経過しても消えない状態異常（魅了など）
    public bool IsPermanent = false;

    public float number = 1;//効果値　デバフによって値の使い方が変化

    public float TickTime = 1;

    public GameObject EffectObject;

    public List<AudioClip> audioClips = new List<AudioClip>();

    //ステータス異常。バフにもでバフにもなるように設計
    public float AttackBuff = 0;
    public float MultiplyAttackBuff  = 0;

    public float DEFBuff = 0;
    public float MultiplyDEFBuff = 0;

    public float SpeedBuff = 0;
    public float MultiplySpeedBuff = 0;


}
