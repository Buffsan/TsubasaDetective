using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public EnemyBase enemyBase;
    public charadata charadata;

    [Header("攻撃対象設定")]
    [SerializeField] Faction attackableFactions = Faction.None;
    [SerializeField] bool attackPlayer = true; // 既存PrefabのPlayerArea攻撃を維持
    [SerializeField] bool canAttackSelf = false;
    [Header("攻撃倍率（100 = 元の攻撃力100%）")]
    [SerializeField, Min(0f)] float attackPowerPercent = 100f;

    public float AttackPowerPercent => attackPowerPercent;

    // 通常Prefabの設定を維持したまま、生成時だけ任意に上書きできる。
    public void ConfigureAttack(EnemyBase owner, float powerPercent, Faction targetFactions, bool hitPlayer)
    {
        enemyBase = owner;
        attackPowerPercent = Mathf.Max(0f, powerPercent);
        attackableFactions = targetFactions;
        attackPlayer = hitPlayer;
    }

    // 対象設定を変更せず、攻撃倍率と攻撃者だけ変更する場合。
    public void ConfigureAttack(EnemyBase owner, float powerPercent)
    {
        enemyBase = owner;
        attackPowerPercent = Mathf.Max(0f, powerPercent);
    }

    public enum DellCondition
    {
        none,
        DirectionAttack_enum
    }
    public DellCondition dellCondition = DellCondition.none;

    public AudioManager audioManager => AudioManager.instance;

    private void FixedUpdate()
    {
        if (dellCondition == DellCondition.DirectionAttack_enum &&
            enemyBase != null && enemyBase.moveType != EnemyBase.MoveType.Attack)
        {
            DestroyArea();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ボスの飛び道具などはInstantiate後にenemyBaseが設定されることがある。
        // 敵対モードのボスだけ、攻撃命中時に所属・対象派閥を反映する。
        Boss_Controller boss = enemyBase as Boss_Controller;
        if (boss != null && boss.TargetMode != EnemyContoroller.TargetType.PlayerOnly)
            boss.ConfigureAttackTarget(this);

        if (other.CompareTag("PlayerArea"))
        {
            // PlayerControllerにはEnemyBaseがないため、従来のPlayerArea判定を維持。
            if (!attackPlayer) return;
            ApplyDamage(other.GetComponent<IDamageable>());
            return;
        }

        if (!other.CompareTag("EnemyArea")) return;

        charaDamage target = other.GetComponent<charaDamage>();
        if (target == null) return;

        // 攻撃者と被弾者のcharadataを比較して自己攻撃を判定する。
        charadata attackerData = enemyBase != null ? enemyBase.charadata : charadata;
        charadata targetData = target.OwnerEnemyBase != null ? target.OwnerEnemyBase.charadata : null;
        bool isSelf = attackerData != null && attackerData == targetData;
        if (isSelf)
        {
            if (!canAttackSelf) return;
        }
        else if ((attackableFactions & target.MyFactions) == Faction.None)
        {
            return;
        }

        ApplyDamage(target, true);
    }

    private void ApplyDamage(IDamageable target, bool isEnemyTarget = false)
    {
        if (target == null) return;
        if (enemyBase != null)
        {
            float damage = enemyBase.ATK * (attackPowerPercent / 100f);
            if (isEnemyTarget)
                damage *= Mathf.Max(1f, enemyBase.MAXHP / 100f);
            target.Damage(damage, 0, 0, false);
        }
        else if (charadata != null)
            target.Damage(charadata.ATK * (attackPowerPercent / 100f), 0, 0, false);
    }

    public void DestroyArea()
    {
        Destroy(gameObject);
    }
}
