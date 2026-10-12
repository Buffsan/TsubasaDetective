using UnityEngine;

/// <summary>
/// 剣継の契約。
/// プレイヤーと複製体の両方が共通カウントを消費する。
/// どちらも発動時点の蓄積カウント + 1本の剣を生成する。
/// </summary>
public class Skill_PD_SwordContract_AI : PlayerSkillBase
{
    public override void SkillPlay()
    {
        if (Att != ATT.A1) return;
        Att = ATT.A2;

        int count = Skill_PD_SwordContract_Count.ConsumeCount();
        int swordCount = count + 1;

        // 複製体も既存仕様どおりプレイヤーのカーソル方向を使用。
        Vector2 direction = playerController.CursorDirection.normalized;
        if (direction == Vector2.zero)
            direction = playerController.LastMoveInput.normalized;
        if (direction == Vector2.zero)
            direction = Vector2.right;

        if (SkillWeapon != null && TargetObject != null)
        {
            GameObject obj = Instantiate(
                SkillWeapon,
                TargetObject.transform.position,
                Quaternion.identity
            );

            Skill_PD_SwordContract_Spawner spawner =
                obj.GetComponent<Skill_PD_SwordContract_Spawner>();

            if (spawner != null)
                spawner.Initialize(direction, swordCount);
            else
            {
                Debug.LogError("Skill_PD_SwordContract_AI: 生成PrefabにSpawnerがありません。");
                Destroy(obj);
            }
        }
        else
            Debug.LogWarning("Skill_PD_SwordContract_AI: SkillWeaponまたはTargetObjectが未設定です。");

        end();
    }
}
