using UnityEngine;
public class StatusEffect : ScriptableObject
{
    public enum StatusEffectType //状態異常
    {

        None,//付与なし。ステータス異常のみ適用
        Poison,//毒
        ElectricShock,//感電
        Charm,//魅了（既存のEnum IDを維持するため末尾に追加）
    }
    public StatusEffectType effecttype = StatusEffectType.None;

    public GameObject PoisonEffectPrefab;
}
