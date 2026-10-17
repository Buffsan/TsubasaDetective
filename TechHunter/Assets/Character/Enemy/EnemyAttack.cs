using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public EnemyBase enemyBase;
    public charadata charadata;

    [Header("攻撃対象設定")]
    [SerializeField] Faction attackableFactions = Faction.None;
    [SerializeField] bool attackPlayer = true; // 既存PrefabのPlayerArea攻撃を維持
    [SerializeField] bool canAttackSelf = false;

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

        // 子オブジェクトの被弾判定でも、EnemyBaseで攻撃者本人を特定する。
        bool isSelf = enemyBase != null && target.OwnerEnemyBase == enemyBase;
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
            float damage = enemyBase.ATK;
            if (isEnemyTarget)
                damage *= Mathf.Max(1f, enemyBase.MAXHP / 100f);
            target.Damage(damage, 0, 0, false);
        }
        else if (charadata != null)
            target.Damage(charadata.ATK, 0, 0, false);
    }

    public void DestroyArea()
    {
        Destroy(gameObject);
    }
}
