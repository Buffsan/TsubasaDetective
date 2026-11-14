using System.Collections.Generic;
using UnityEngine;

// Five shield bearers march independently; only enemies directly ahead interrupt their route.
public class LunarPurgeSquad_Shield_AI : EnemyMoveBase
{
    [Header("行軍")]
    [SerializeField, Min(0.1f)] float marchSpeed = 4f;
    [SerializeField, Min(0.1f)] float laneRecoverySpeed = 2.5f;
    [SerializeField, Min(0f)] float laneRecoveryGain = 5f;
    [SerializeField, Min(5f)] float maxLifetime = 60f;
    [Header("前方攻撃")]
    [SerializeField, Min(0.1f)] float attackReach = 1.7f;
    [SerializeField, Min(0.1f)] float attackLaneWidth = 1.25f;
    [SerializeField, Min(0f)] float windupSeconds = 0.4f;
    [SerializeField, Min(0f)] float swingSeconds = 0.28f;
    [SerializeField, Min(0f)] float attackCooldown = 0.4f;
    [SerializeField, Min(0f)] float attackPowerPercent = 100f;

    enum Phase { March, Windup, Swing, Cooldown }
    Phase phase = Phase.March;
    readonly Collider2D[] hitBuffer = new Collider2D[64];
    readonly HashSet<EnemyBase> hitThisSwing = new HashSet<EnemyBase>();
    EnemyContoroller controller;
    float laneY, exitX, bornAt, phaseSeconds;
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
        SweepShieldRoute route = enemyBase.GetComponent<SweepShieldRoute>();
        laneY = route != null && route.Initialized ? route.LaneY : enemyBase.transform.position.y;
        exitX = route != null && route.Initialized ? route.ExitX : enemyBase.transform.position.x + 40f;
        bornAt = Time.time;
        controller.Choice_MoveToBase = EnemyContoroller.MoveToBase.Special;
        enemyBase.LookPlayerDistance = -1f; // No automatic proximity attacks.
        enemyBase.moveType = EnemyBase.MoveType.Move;
        enemyBase.animator.SetInteger("Anim", 1);
        initialized = true;
    }

    public override void EnemyMovePlay() { March(); }
    public override void EnemyFriendlyPlay() { March(); } // Advance even with no target in EnemyOnly mode.

    void March()
    {
        if (!initialized || enemyBase.status != EnemyBase.Status.Nomal) return;
        if (CheckExit()) return;
        enemyBase.animator.SetInteger("Anim", 1);
        enemyBase.isTargetLook(enemyBase.transform.position + Vector3.right);
        if (HasTargetAhead())
        {
            enemyBase.rb.velocity = Vector2.zero;
            enemyBase.moveType = EnemyBase.MoveType.Attack;
            enemyBase.animator.SetInteger("Anim", 2);
            phase = Phase.Windup;
            phaseSeconds = 0f;
            return;
        }
        // Preserve the original Y lane while allowing natural sideways knockback during stun.
        float ySpeed = Mathf.Clamp((laneY - enemyBase.rb.position.y) * laneRecoveryGain,
            -laneRecoverySpeed, laneRecoverySpeed);
        if (Mathf.Abs(laneY - enemyBase.rb.position.y) < 0.03f) ySpeed = 0f;
        enemyBase.rb.velocity = new Vector2(marchSpeed, ySpeed);
    }

    bool HasTargetAhead()
    {
        int n = ScanAhead();
        for (int i = 0; i < n; i++)
            if (IsValidVictim(hitBuffer[i], out _)) return true;
        return false;
    }

    int ScanAhead()
    {
        return Physics2D.OverlapBoxNonAlloc(
            (Vector2)enemyBase.transform.position + new Vector2(attackReach * 0.5f, 0f),
            new Vector2(attackReach, attackLaneWidth), 0f, hitBuffer);
    }

    bool IsValidVictim(Collider2D hit, out EnemyBase victim)
    {
        victim = null;
        if (hit == null || !hit.CompareTag("EnemyArea")) return false;
        charaDamage damage = hit.GetComponent<charaDamage>();
        victim = damage != null ? damage.OwnerEnemyBase : null;
        if (victim == null || victim == enemyBase ||
            victim.status == EnemyBase.Status.Die || victim.HP <= 0f || victim.IsCharmed)
            return false;
        // Other sweep soldiers must not block one another or receive friendly fire.
        return victim.gameObject.layer != LayerMask.NameToLayer("SweepEnemy");
    }

    public override void EnemyAttackPlay()
    {
        if (!initialized || enemyBase.status != EnemyBase.Status.Nomal) return;
        if (CheckExit()) return;
        enemyBase.rb.velocity = Vector2.zero;
        phaseSeconds += Time.fixedDeltaTime;
        switch (phase)
        {
            case Phase.Windup:
                if (phaseSeconds >= windupSeconds)
                {
                    phase = Phase.Swing;
                    phaseSeconds = 0f;
                    enemyBase.animator.SetInteger("Anim", 3);
                    HitInFront();
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
                    enemyBase.animator.SetInteger("Anim", 1);
                }
                break;
            default:
                phase = Phase.March;
                enemyBase.moveType = EnemyBase.MoveType.Move;
                break;
        }
    }

    void HitInFront()
    {
        hitThisSwing.Clear();
        int n = ScanAhead();
        GameObject temporary = new GameObject("ShieldSweep_Impact");
        temporary.transform.position = enemyBase.transform.position;
        EnemyAttack attack = temporary.AddComponent<EnemyAttack>();
        attack.ConfigureAttack(enemyBase, attackPowerPercent);
        controller.ConfigureAttackTarget(attack);
        for (int i = 0; i < n; i++)
        {
            Collider2D hit = hitBuffer[i];
            if (IsValidVictim(hit, out EnemyBase victim) && hitThisSwing.Add(victim))
                attack.CheckHitNow(hit);
        }
        Destroy(temporary);
        if (enemyBase.AttackAudio != null && enemyBase.audioManager != null)
            enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
    }

    bool CheckExit()
    {
        if (enemyBase.transform.position.x < exitX && Time.time - bornAt < maxLifetime)
            return false;
        enemyBase.rb.velocity = Vector2.zero;
        Destroy(enemyBase.gameObject);
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
        Gizmos.DrawWireCube(enemyBase.transform.position + Vector3.right * attackReach * 0.5f,
            new Vector3(attackReach, attackLaneWidth, 0f));
    }
}
