using UnityEngine;

// 実行中のスキルと、そこから生成された攻撃オブジェクトのスロットを関連付ける。
public sealed class SkillOriginMarker : MonoBehaviour
{
    public int SkillSlotIndex = -1;

    public static void Stamp(GameObject instance, int slot)
    {
        if (instance == null || slot < 0) return;
        SkillOriginMarker marker = instance.GetComponent<SkillOriginMarker>();
        if (marker == null) marker = instance.AddComponent<SkillOriginMarker>();
        marker.SkillSlotIndex = slot;

        foreach (PlayerAttack hitbox in instance.GetComponentsInChildren<PlayerAttack>(true))
            hitbox.SourceSkillSlotIndex = slot;
    }
}
