using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class Relic_ReturnlessHarpoon : RelicBase_AI
{
    [SerializeField] float orbitRadius = 1.25f, orbitSpeed = 1.4f;
    [SerializeField] float windup = 1f, dashTime = 0.5f, dashDistance = 15f, returnTime = 4f;
    [SerializeField] float stun = 4f, confusion = 1f;
    [SerializeField] float spinPower = 2000f;

    [Header("Return homing")]
    [Tooltip("Acceleration used to turn toward the moving player while returning.")]
    [SerializeField, Min(0.1f)] float returnSteeringAcceleration = 26f;
    [Tooltip("Extra time after the baseline return duration to catch a moving player.")]
    [SerializeField, Min(0.05f)] float movingTargetGraceSeconds = 0.5f;
    [Tooltip("Additional chase speed for players moving farther from the dash endpoint.")]
    [SerializeField, Min(0f)] float movingTargetCompensation = 2f;
    [SerializeField, Min(0.1f)] float returnCatchRadius = 0.85f;

    [Header("Hitbox")]
    [SerializeField, Min(1f)] float spriteHitboxPadding = 1.15f;

    enum Mode { Orbit, Spin, Dash, Return }
    Mode mode;
    float time, charge;
    Vector2 dashOrigin, dashDirection;
    // The player position when the harpoon was thrown is our four-second baseline.
    Vector2 playerPositionAtThrow, returnStartPosition;
    float baselineReturnDistance;
    Vector2 returnVelocity;
    float orbitPhaseOffset;
    BoxCollider2D hitbox;

    // The dash is only 0.5 seconds; non-allocating sweeps prevent skipped nearby enemies.
    readonly Collider2D[] overlapBuffer = new Collider2D[128];
    readonly RaycastHit2D[] sweepBuffer = new RaycastHit2D[128];
    readonly HashSet<EnemyBase> targets = new HashSet<EnemyBase>();
    public bool CanCharge => isActiveAndEnabled && mode == Mode.Orbit;

    void Awake()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        hitbox = GetComponent<BoxCollider2D>();
        if (hitbox == null)
        {
            // Automatically follow the actual sprite size, including prefabs scaled to 3.
            hitbox = gameObject.AddComponent<BoxCollider2D>();
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer != null && renderer.sprite != null)
            {
                Bounds bounds = renderer.sprite.bounds;
                hitbox.size = (Vector2)bounds.size * spriteHitboxPadding;
                hitbox.offset = bounds.center;
            }
            else
            {
                hitbox.size = new Vector2(0.6f, 1.6f);
            }
        }
        // Keep inspector-authored BoxCollider2D shapes intact.
        hitbox.isTrigger = true;
    }

    public bool ChargeFromPlayer(float damage)
    {
        if (!CanCharge || damage <= 0f) return false;
        charge = damage;
        time = 0f;
        targets.Clear();
        mode = Mode.Spin;
        return true;
    }

    void Update()
    {
        PlayerController player = playerController;
        if (player == null) return;
        float dt = Time.deltaTime;
        switch (mode)
        {
            case Mode.Orbit:
                float angle = Time.time * orbitSpeed + orbitPhaseOffset;
                transform.position = (Vector2)player.transform.position +
                    orbitRadius * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                transform.up = Vector2.up;
                break;
            case Mode.Spin:
                time += dt;
                transform.Rotate(0f, 0f, spinPower * dt);
                if (time >= Mathf.Max(windup, 0.01f))
                {
                    Vector2 cursor = player.cursorController != null
                        ? (Vector2)player.cursorController.MousePos
                        : (Vector2)player.transform.position + player.CursorDirection;
                    dashDirection = (cursor - (Vector2)transform.position).normalized;
                    if (dashDirection.sqrMagnitude < 0.001f) dashDirection = Vector2.up;
                    dashOrigin = transform.position;
                    playerPositionAtThrow = player.transform.position;
                    transform.up = dashDirection;
                    time = 0f;
                    mode = Mode.Dash;
                    // An enemy may already overlap the harpoon before the dash starts.
                    HitEnemiesAlongDash((Vector2)transform.position, (Vector2)transform.position);
                }
                break;
            case Mode.Dash:
                time += dt;
                float progress = Mathf.Clamp01(time / Mathf.Max(dashTime, 0.01f));
                Vector2 previous = transform.position;
                Vector2 next = dashOrigin + dashDirection * (dashDistance * progress);
                HitEnemiesAlongDash(previous, next);
                transform.position = next;
                if (progress >= 1f)
                {
                    time = 0f;
                    // Calibrate the return flight to the player's position at throw time.
                    returnStartPosition = transform.position;
                    baselineReturnDistance = Mathf.Max(0f,
                        Vector2.Distance(returnStartPosition, playerPositionAtThrow) -
                        returnCatchRadius);
                    Vector2 home = playerPositionAtThrow - returnStartPosition;
                    if (home.sqrMagnitude < 0.001f)
                        home = (Vector2)player.transform.position - returnStartPosition;
                    returnVelocity = home.sqrMagnitude > 0.001f
                        ? home.normalized * (baselineReturnDistance /
                            Mathf.Max(0.1f, returnTime) * 0.25f)
                        : Vector2.zero;
                    mode = Mode.Return;
                }
                break;
            case Mode.Return:
                time += dt;
                Vector2 destination = player.transform.position;
                Vector2 displacement = destination - (Vector2)transform.position;
                float distance = displacement.magnitude;

                // Physically meeting the player is always enough to retrieve it,
                // even if only a fraction of the expected return time has passed.
                if (distance <= returnCatchRadius)
                {
                    ResetAfterReturn(player);
                    break;
                }

                float baselineTime = Mathf.Max(0.1f, returnTime);
                float baselineSpeed = baselineReturnDistance / baselineTime;
                float returnProgress = Mathf.Max(0f, time / baselineTime);
                // Speed rises linearly. Its integral over 4 seconds equals the
                // original distance to the player at the moment of throwing.
                float scheduledSpeed = baselineSpeed *
                    (0.25f + 1.5f * Mathf.Min(returnProgress, 1.5f));

                // The player may move closer (early pickup), sideways, or farther
                // away. Add speed only when their new position is farther from
                // the harpoon's return starting point than the original location.
                float extraTravel = Mathf.Max(0f,
                    Vector2.Distance(returnStartPosition, destination) -
                    Vector2.Distance(returnStartPosition, playerPositionAtThrow));
                float chaseSpeed = movingTargetCompensation * extraTravel / baselineTime;
                float desiredSpeed = scheduledSpeed + chaseSpeed;

                // After the baseline ~4 seconds, accelerate further to intercept
                // a moving player within roughly another half second. This is
                // still normal world-space movement: never teleport or force-pickup.
                if (time > baselineTime)
                {
                    float remaining = Mathf.Max(0.04f,
                        baselineTime + movingTargetGraceSeconds - time);
                    desiredSpeed = Mathf.Max(desiredSpeed, distance / remaining);
                }

                Vector2 desiredVelocity = displacement.normalized * desiredSpeed;
                float steering = returnSteeringAcceleration *
                    (1f + Mathf.Max(0f, time - baselineTime) * 6f);
                returnVelocity = Vector2.MoveTowards(returnVelocity,
                    desiredVelocity, steering * dt);
                Vector2 returnStart = transform.position;
                Vector2 returnNextPosition = returnStart + returnVelocity * dt;
                transform.position = returnNextPosition;
                if (returnVelocity.sqrMagnitude > 0.001f)
                    transform.up = returnVelocity.normalized;

                // Pick up even if high-speed pursuit crosses the player between frames.
                Vector2 step = returnNextPosition - returnStart;
                float fraction = step.sqrMagnitude > 0.000001f
                    ? Mathf.Clamp01(Vector2.Dot(destination - returnStart, step) /
                        step.sqrMagnitude)
                    : 0f;
                Vector2 closestPoint = returnStart + step * fraction;
                if ((closestPoint - destination).sqrMagnitude <=
                    returnCatchRadius * returnCatchRadius)
                    ResetAfterReturn(player);
                break;
        }
    }

    void ResetAfterReturn(PlayerController player)
    {
        charge = 0f;
        time = 0f;
        returnVelocity = Vector2.zero;
        targets.Clear();
        // Resume the orbit near the pickup angle instead of jumping to a fixed point.
        Vector2 relative = (Vector2)transform.position - (Vector2)player.transform.position;
        if (relative.sqrMagnitude > 0.001f)
            orbitPhaseOffset = Mathf.Atan2(relative.y, relative.x) - Time.time * orbitSpeed;
        mode = Mode.Orbit;
    }

    void HitEnemiesAlongDash(Vector2 previous, Vector2 next)
    {
        if (mode != Mode.Dash || charge <= 0f || hitbox == null) return;

        Vector3 scale = transform.lossyScale;
        Vector2 size = new Vector2(Mathf.Abs(hitbox.size.x * scale.x),
                                   Mathf.Abs(hitbox.size.y * scale.y));
        float angle = transform.eulerAngles.z;
        Vector2 localOffset = Vector2.Scale(hitbox.offset, new Vector2(scale.x, scale.y));
        Vector2 offset = (Vector2)(Quaternion.Euler(0f, 0f, angle) * localOffset);
        Vector2 start = previous + offset;
        Vector2 end = next + offset;

        // Overlaps at the starting position are important when the enemy is very close.
        HitOverlappingEnemies(start, size, angle);
        Vector2 delta = end - start;
        if (delta.sqrMagnitude > 0.000001f)
        {
            int hitCount = Physics2D.BoxCastNonAlloc(start, size, angle,
                delta.normalized, sweepBuffer, delta.magnitude);
            for (int i = 0; i < hitCount; i++)
                TryHitEnemy(sweepBuffer[i].collider);
        }
        HitOverlappingEnemies(end, size, angle);
    }

    void HitOverlappingEnemies(Vector2 center, Vector2 size, float angle)
    {
        int count = Physics2D.OverlapBoxNonAlloc(center, size, angle, overlapBuffer);
        for (int i = 0; i < count; i++)
            TryHitEnemy(overlapBuffer[i]);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Retain normal trigger hits in addition to swept queries.
        if (mode == Mode.Dash) TryHitEnemy(other);
    }

    void TryHitEnemy(Collider2D other)
    {
        if (other == null || mode != Mode.Dash || charge <= 0f ||
            !other.CompareTag("EnemyArea")) return;
        charaDamage area = other.GetComponent<charaDamage>();
        if (area == null) return;
        EnemyBase enemy = area.OwnerEnemyBase;
        if (enemy == null || enemy.HP <= 0f || enemy.IsCharmed ||
            enemy.status == EnemyBase.Status.Die || !targets.Add(enemy)) return;
        float bonus = playerController != null ? playerController.AddConfusionDamagePercent : 0f;
        area.DamageFromPlayer(charge, stun, confusion, false, bonus);
    }
}
