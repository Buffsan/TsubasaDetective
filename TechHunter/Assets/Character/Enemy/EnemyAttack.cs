using System.Collections.Generic;
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

    // 高速移動・瞬間生成された攻撃だけで利用する明示的な衝突チェック。
    // 通常のEnemyAttackは従来どおりOnTriggerEnter2Dのみで動作する。
    bool useManualCollisionChecks;
    readonly HashSet<Collider2D> checkedColliders = new HashSet<Collider2D>();

    public void BeginManualHitWindow()
    {
        checkedColliders.Clear();
        useManualCollisionChecks = true;
    }

    public void EndManualHitWindow()
    {
        useManualCollisionChecks = false;
        checkedColliders.Clear();
    }

    public void CheckHitNow(Collider2D other)
    {
        if (other != null) ProcessTriggerHit(other);
    }

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
        ProcessTriggerHit(other);
    }

    void ProcessTriggerHit(Collider2D other)
    {
        if (other == null) return;
        // 手動スイープと通常の物理イベントが同じColliderに当たっても二重処理しない。
        if (useManualCollisionChecks && !checkedColliders.Add(other)) return;
        // ボスの飛び道具などはInstantiate後にenemyBaseが設定されることがある。
        // 敵対モードのボスだけ、攻撃命中時に所属・対象派閥を反映する。
        Boss_Controller boss = enemyBase as Boss_Controller;
        if (boss != null && boss.TargetMode != EnemyContoroller.TargetType.PlayerOnly)
            boss.ConfigureAttackTarget(this);

        if (other.CompareTag("PlayerArea"))
        {
            // PlayerControllerにはEnemyBaseがないため、従来のPlayerArea判定を維持。
            if (!attackPlayer || (enemyBase != null && enemyBase.IsCharmed)) return;
            ApplyDamage(other.GetComponent<IDamageable>());
            return;
        }

        if (!other.CompareTag("EnemyArea")) return;

        charaDamage target = other.GetComponent<charaDamage>();
        if (target == null) return;

        EnemyBase victim = target.OwnerEnemyBase;
        if (victim == null || victim.status == EnemyBase.Status.Die || victim.HP <= 0f) return;

        // AllEnemyの位置は動的な個体ID。リスト不在の場合は実体参照で自己判定。
        bool isSelf;
        SystemManager manager = SystemManager.Instance;
        if (enemyBase != null)
        {
            int attackerId = manager != null && manager.AllEnemy != null
                ? manager.AllEnemy.IndexOf(enemyBase.gameObject) : -1;
            int victimId = manager != null && manager.AllEnemy != null
                ? manager.AllEnemy.IndexOf(victim.gameObject) : -1;
            isSelf = attackerId >= 0 && victimId >= 0
                ? attackerId == victimId : enemyBase == victim;
        }
        else
        {
            // 所有者を識別できない旧Prefabだけ従来のScriptableObject照合。
            isSelf = charadata != null && charadata == victim.charadata;
        }
        if (isSelf && !canAttackSelf) return;

        // 味方同士の攻撃を防ぎ、魅了された敵は非魅了の敵のみを攻撃する。
        if (enemyBase != null && enemyBase.IsCharmed)
        {
            if (!victim.IsCharmed) ApplyDamage(target, true);
            return;
        }
        if (victim.IsCharmed || (attackableFactions & target.MyFactions) != Faction.None)
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
