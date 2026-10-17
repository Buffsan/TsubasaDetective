using System.Collections.Generic;
using UnityEngine;

// 杭投げ・近接突進に共通する攻撃判定。
// Trigger Collider2Dと同じGameObjectに付ける。衝突しても弾は破棄しない。
[DisallowMultipleComponent]
public class DK_PilgrimOfTheMoon_Hitbox : MonoBehaviour
{
    EnemyBase attacker;
    float playerDamage;
    float enemyDamage;
    readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    public void Initialize(EnemyBase owner)
    {
        attacker = owner;
        playerDamage = owner != null ? owner.ATK : 0f;
        // 既存 EnemyAttack と同じ敵対敵ダメージ倍率を使う。
        enemyDamage = owner != null
            ? owner.ATK * Mathf.Max(1f, owner.MAXHP / 100f)
            : 0f;
        hitTargets.Clear();

        Collider2D hitCollider = GetComponent<Collider2D>();
        if (hitCollider == null)
            Debug.LogError("月の巡礼者の攻撃PrefabにはCollider2Dが必要です。", this);
        else if (!hitCollider.isTrigger)
            Debug.LogWarning("月の巡礼者の攻撃Collider2DはIs Triggerにしてください。", this);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerArea"))
        {
            IDamageable player = other.GetComponent<IDamageable>();
            if (player != null && hitTargets.Add(player))
                player.Damage(playerDamage, 0f, 0f, false);
            return;
        }

        if (!other.CompareTag("EnemyArea")) return;
        charaDamage damageable = other.GetComponent<charaDamage>();
        if (damageable == null) return;

        EnemyBase victim = damageable.OwnerEnemyBase;
        if (victim == null || victim == attacker ||
            victim.status == EnemyBase.Status.Die || victim.HP <= 0f)
            return;

        // 自軍以外すべて攻撃。Diana を含む複合派閥は対象外。
        if ((victim.Factions & Faction.Diana) != Faction.None) return;

        // 一つの攻撃判定から同じ敵への重複ヒットを防止。
        if (hitTargets.Add(damageable))
            damageable.Damage(enemyDamage, 0f, 0f, false);
    }

    void OnDisable()
    {
        hitTargets.Clear();
    }
}
