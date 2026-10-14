using UnityEngine;

// ステータスカードの確定処理をUI・画面遷移から分離する。
// StatusUP の既存フィールド名と適用順序を維持する。
public static class StatusCardEffects
{
    public static float CalculateMaxHpBonus(PlayerController player, StatusUP card)
    {
        return player.charadata.MAXHP + player.AddHP + card.HP < 0f
            ? 1f - player.charadata.MAXHP
            : player.AddHP + card.HP;
    }

    public static int CalculateCoins(int coins, float percentage)
    {
        return Mathf.RoundToInt(coins * Mathf.Max((100f + percentage) / 100f, 0f));
    }

    public static void Apply(PlayerController player, StatusUP card)
    {
        if (player == null || card == null) return;

        player.AddATK += card.Attack;
        player.AddAtkDF += card.Defense;
        player.Add_AdditiveATK += card.AdditiveATK;
        player.AddHP = CalculateMaxHpBonus(player, card);
        player.AddRegen += card.Regen;
        player.isRecoveryHP(card.RecoveryHP);
        player.AddCritical += card.Critical;
        player.AddRange += card.Range;
        player.AddSpeed += card.Speed;
        player.AddConfusionTime += card.ConfusionTime;
        player.AllCoins = CalculateCoins(player.AllCoins, card.Coin);

        if (card.RandomLevelUp > 0)
            player.RandomLevelUp(card.RandomLevelUp);

        player.isAddStatus();
        player.playerDamage.HP_SliderChange();
    }

    // 表示順は StatusMenu の既存12項目と一致する。
    public static float[] GetValues(PlayerController player, StatusUP card)
    {
        float maxHp = player.MaxHP;
        float hp = player.HP;
        float attackMultiplier = player.AddATK;
        float attackBonus = player.Add_AdditiveATK;
        float regen = player.Regen;
        float defense = player.ATKDF;
        float cooldown = player.Range;
        float luck = player.Critical;
        float confusion = player.ConfusionTime;
        float speedBonus = player.AddSpeed;
        float speed = player.SPEED;

        if (card != null)
        {
            maxHp = player.charadata.MAXHP + CalculateMaxHpBonus(player, card);
            // Apply() と同じく回復は最大HP変更前の上限で計算する。
            hp = Mathf.Clamp(hp + card.RecoveryHP, 0f, player.MaxHP);
            attackMultiplier += card.Attack;
            attackBonus += card.AdditiveATK;
            regen += card.Regen;
            defense += card.Defense;
            cooldown += card.Range;
            luck += card.Critical;
            confusion += card.ConfusionTime;
            speedBonus += card.Speed;
            speed = player.charadata.SPEED * ((100f + speedBonus) / 100f);
        }

        return new float[]
        {
            hp, attackMultiplier, attackBonus, regen, 0f, defense,
            cooldown, luck, confusion, 0f, speedBonus, speed, maxHp
        };
    }
}
