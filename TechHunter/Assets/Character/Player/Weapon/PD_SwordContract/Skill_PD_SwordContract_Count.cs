using UnityEngine;

/// <summary>
/// すべてのプレイヤースキル使用イベントを数え、剣継の契約で消費する。
/// スキルのPrefabが破棄されてもカウントを保持する。
/// </summary>
public static class Skill_PD_SwordContract_Count
{
    public static int Count { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        // Domain Reloadを無効化していても二重購読を防ぐ。
        GlobalEvents.OnSkillUse -= OnSkillUse;
        Count = 0;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Subscribe()
    {
        GlobalEvents.OnSkillUse -= OnSkillUse;
        GlobalEvents.OnSkillUse += OnSkillUse;
    }

    private static void OnSkillUse(PlayerSkillBase skillBase, SkillCardData skillData)
    {
        AddCount();
    }

    /// <summary>複製機など、OnSkillUseを経由しないスキル使用時に呼ぶ。</summary>
    public static void AddCount()
    {
        if (Count < int.MaxValue) Count++;
    }

    /// <summary>現在の蓄積カウントを返し、0に戻す。</summary>
    public static int ConsumeCount()
    {
        int result = Count;
        Count = 0;
        return result;
    }

    /// <summary>新規ランなどの開始時に呼ぶ。</summary>
    public static void ResetCount()
    {
        Count = 0;
    }
}
