using System.Collections.Generic;
using UnityEngine;

// Sweep soldiers march toward their exit, turning to engage nearby enemies or the player.
public class LunarPurgeSquad_Shield_AI : EnemyMoveBase
{
    [Header("行軍")]
    [SerializeField, Min(0.1f)] float laneRecoverySpeed = 2.5f;
    [SerializeField, Min(0f)] float laneRecoveryGain = 5f;
    [SerializeField, Min(5f)] float maxLifetime = 60f;
    [Header("周囲360度の攻撃索敵")]
    [SerializeField, Min(0.1f), Tooltip("全方向に索敵・攻撃する半径（ワールド単位）")]
    float attackReach = 1.7f;
    [SerializeField, Min(0f)] float windupSeconds = 0.4f;
    [SerializeField, Min(0f)] float swingSeconds = 0.28f;
    [SerializeField, Min(0f)] float attackCooldown = 0.4f;
    [SerializeField, Min(0f)] float attackPowerPercent = 100f;

    [Header("デバッグ")]
    [SerializeField] bool logCombatDiagnostics = false;

    enum Phase { March, Windup, Swing, Cooldown }
    Phase phase = Phase.March;
    readonly Collider2D[] hitBuffer = new Collider2D[64];
    readonly HashSet<EnemyBase> hitThisSwing = new HashSet<EnemyBase>();
    EnemyContoroller controller;
    float laneY, exitX, bornAt, phaseSeconds;
    SweepShieldRoute route;
    bool initialized;

    void Start()
    {
        controller = enemyBase as EnemyContoroller;
        if (controller == null || enemyBase.rb == null || enemyBase.animator == null)
        {
            Debug.LogError("LunarPurgeSquad_Shield: EnemyContoroller/Rigidbody2D/Animator is missing.", this);
            enabled = false;
            return;
        }
        route = enemyBase.GetComponent<SweepShieldRoute>();
        laneY = route != null && route.Initialized ? route.LaneY : enemyBase.transform.position.y;
        exitX = route != null && route.Initialized ? route.ExitX : enemyBase.transform.position.x + 40f;
        bornAt = Time.time;
        controller.Choice_MoveToBase = EnemyContoroller.MoveToBase.Special;
        enemyBase.LookPlayerDistance = -1f; // No automatic proximity attacks.
        enemyBase.moveType = EnemyBase.MoveType.Move;
        PlayAnimation("歩行", 1);
        initialized = true;
    }

    void Update()
    {
        // Exit/time limit must work even during stun or confusion.
        if (initialized) CheckExit();
    }

    public override void EnemyMovePlay() { March(); }
    public override void EnemyFriendlyPlay() { March(); } // Advance even with no target in EnemyOnly mode.

    void March()
    {
        if (!initialized || enemyBase.status != EnemyBase.Status.Nomal) return;
        if (CheckExit()) return;
        PlayAnimation("歩行", 1);
        enemyBase.isTargetLook(enemyBase.transform.position + Vector3.right);
        if (TryFindNearestTarget(out Vector2 targetPosition))
        {
            enemyBase.isTargetLook(targetPosition);
            enemyBase.rb.velocity = Vector2.zero;
            enemyBase.moveType = EnemyBase.MoveType.Attack;
            if (logCombatDiagnostics)
                Debug.Log("LunarPurgeSquad_Shield: 近距離の攻撃対象を検知 " +
                    targetPosition, enemyBase);
            PlayAnimation("攻撃待機", 2);
            phase = Phase.Windup;
            phaseSeconds = 0f;
            return;
        }
        // Preserve the original Y lane while allowing natural sideways knockback during stun.
        float ySpeed = Mathf.Clamp((laneY - enemyBase.rb.position.y) * laneRecoveryGain,
            -laneRecoverySpeed, laneRecoverySpeed);
        if (Mathf.Abs(laneY - enemyBase.rb.position.y) < 0.03f) ySpeed = 0f;
        enemyBase.rb.velocity = new Vector2(Mathf.Max(0f, enemyBase.SPEED), ySpeed);
    }

    bool TryFindNearestTarget(out Vector2 targetPosition)
    {
        targetPosition = Vector2.zero;
        float nearestSqr = float.PositiveInfinity;
        bool found = false;
        int count = ScanAround();
        Vector2 from = enemyBase.transform.position;
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = hitBuffer[i];
            if (!IsValidTarget(hit, out _)) continue;
            Vector2 position = hit.bounds.center;
            float distanceSqr = (position - from).sqrMagnitude;
            if (distanceSqr >= nearestSqr) continue;
            nearestSqr = distanceSqr;
            targetPosition = position;
            found = true;
        }
        return found;
    }

    int ScanAround()
    {
        return Physics2D.OverlapCircleNonAlloc(
            enemyBase.transform.position, attackReach, hitBuffer);
    }

    bool IsValidTarget(Collider2D hit, out EnemyBase victim)
    {
        victim = null;
        if (hit == null) return false;
        if (hit.CompareTag("PlayerArea")) return true;
        if (!hit.CompareTag("EnemyArea")) return false;

        charaDamage damage = hit.GetComponent<charaDamage>();
        victim = damage != null ? damage.OwnerEnemyBase : null;
        if (victim == null || victim == enemyBase ||
            victim.status == EnemyBase.Status.Die || victim.HP <= 0f)
            return false;

        // 盾兵同士は味方。プレイヤーが召喚した魅了済みの敵は攻撃対象。
        // Earlier code mistakenly skipped IsCharmed here, although EnemyAttack
        // explicitly permits damage to charmed targets.
        if (victim.gameObject.layer == LayerMask.NameToLayer("SweepEnemy") ||
            victim.GetComponent<SweepShieldRoute>() != null)
            return false;

        // 盾兵と同じ元の国/所属で、魅了されていない敵は攻撃しない。
        // 魅了済みなら元の所属に関係なく攻撃する。
        if (!victim.IsCharmed &&
            (victim.Factions & enemyBase.Factions) != Faction.None)
            return false;

        return true;
    }

    void PlayAnimation(string stateName, int anim)
    {
        Animator animator = enemyBase.animator;
        if (animator == null) return;
        animator.SetInteger("Anim", anim);

        // Cross-frame transitions occasionally leave the inherited base Animator in idle.
        // Switch once, then leave the sprite walk clip running without resetting its time.
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer." + stateName))
            animator.Play("Base Layer." + stateName, 0, 0f);
    }

    public override void EnemyAttackPlay()
    {
        if (!initialized || enemyBase.status != EnemyBase.Status.Nomal) return;
        if (CheckExit()) return;
        enemyBase.rb.velocity = Vector2.zero;
        // If the opponent circles around during windup, track its current side.
        if (phase == Phase.Windup && TryFindNearestTarget(out Vector2 targetPosition))
            enemyBase.isTargetLook(targetPosition);
        phaseSeconds += Time.fixedDeltaTime;
        switch (phase)
        {
            case Phase.Windup:
                if (phaseSeconds >= windupSeconds)
                {
                    phase = Phase.Swing;
                    phaseSeconds = 0f;
                    PlayAnimation("攻撃", 3);
                    HitNearbyTargets();
                }
                break;
            case Phase.Swing:
                if (phaseSeconds >= swingSeconds)
                {
                    phase = Phase.Cooldown;
                    phaseSeconds = 0f;
                }
                break;
            case Phase.Cooldown:
                if (phaseSeconds >= attackCooldown)
                {
                    phase = Phase.March;
                    phaseSeconds = 0f;
                    enemyBase.moveType = EnemyBase.MoveType.Move;
                    PlayAnimation("歩行", 1);
                }
                break;
            default:
                phase = Phase.March;
                enemyBase.moveType = EnemyBase.MoveType.Move;
                break;
        }
    }

    void HitNearbyTargets()
    {
        hitThisSwing.Clear();
        int count = ScanAround();
        GameObject temporary = new GameObject("ShieldSweep_Impact");
        temporary.transform.position = enemyBase.transform.position;
        EnemyAttack attack = temporary.AddComponent<EnemyAttack>();
        attack.ConfigureAttack(enemyBase, attackPowerPercent);
        controller.ConfigureAttackTarget(attack);
        attack.BeginManualHitWindow();

        bool playerHit = false;
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = hitBuffer[i];
            if (!IsValidTarget(hit, out EnemyBase victim)) continue;

            if (hit.CompareTag("PlayerArea"))
            {
                if (playerHit) continue;
                playerHit = true;
            }
            else if (!hitThisSwing.Add(victim))
                continue;

            attack.CheckHitNow(hit);
        }
        attack.EndManualHitWindow();
        if (logCombatDiagnostics)
            Debug.Log("LunarPurgeSquad_Shield: 攻撃判定終了。攻撃を試みた対象数: " +
                (hitThisSwing.Count + (playerHit ? 1 : 0)), enemyBase);
        Destroy(temporary);
        if (enemyBase.AttackAudio != null && enemyBase.audioManager != null)
            enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
    }

    bool CheckExit()
    {
        if (enemyBase.transform.position.x < exitX && Time.time - bornAt < maxLifetime)
            return false;
        enemyBase.rb.velocity = Vector2.zero;
        if (route != null && route.LogLifecycle)
            Debug.Log("LunarPurgeSquad_Shield: " +
                (enemyBase.transform.position.x >= exitX ? "目的地到達で消滅" : "最大生存時間で消滅") +
                " (x=" + enemyBase.transform.position.x.ToString("F1") +
                ", exitX=" + exitX.ToString("F1") + ")", enemyBase);
        Destroy(enemyBase.gameObject);
        initialized = false;
        return true;
    }

    void InterruptAttack()
    {
        phase = Phase.March;
        phaseSeconds = 0f;
        hitThisSwing.Clear();
        // The shared charaDamage script owns knockback; avoid resetting rb.velocity here.
        if (enemyBase != null && enemyBase.moveType == EnemyBase.MoveType.Attack)
            enemyBase.moveType = EnemyBase.MoveType.Move;
    }

    public override void EnemyStanPlay() { InterruptAttack(); }
    public override void EnemyConfusionPlay() { InterruptAttack(); }

    void OnDrawGizmosSelected()
    {
        if (enemyBase == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(enemyBase.transform.position, attackReach);
    }
}
