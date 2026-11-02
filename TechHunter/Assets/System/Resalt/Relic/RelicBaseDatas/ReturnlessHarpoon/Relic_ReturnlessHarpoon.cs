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
    [SerializeField, Min(0.1f)] float returnStartSpeed = 2f;
    [SerializeField, Min(0.1f)] float returnMaxSpeed = 17f;
    [SerializeField, Min(0.1f)] float returnAcceleration = 24f;
    [SerializeField, Min(0.1f)] float returnCatchRadius = 0.85f;

    [Header("Hitbox")]
    [SerializeField, Min(1f)] float spriteHitboxPadding = 1.15f;

    enum Mode { Orbit, Spin, Dash, Return }
    Mode mode;
    float time, charge;
    Vector2 dashOrigin, dashDirection;
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
                    // The returning harpoon has inertia rather than snapping toward the player.
                    returnVelocity = dashDirection * returnStartSpeed;
                    mode = Mode.Return;
                }
                break;
            case Mode.Return:
                time += dt;
                Vector2 destination = player.transform.position;
                Vector2 displacement = destination - (Vector2)transform.position;
                float distance = displacement.magnitude;
                if (distance <= returnCatchRadius)
                {
                    ResetAfterReturn(player);
                    break;
                }

                // Gradually accelerate and steer toward the moving player.
                // Never force the harpoon to arrive on a timer: that caused screen-pinned motion.
                float speedLimit = Mathf.Lerp(returnStartSpeed, returnMaxSpeed,
                    Mathf.Clamp01(time / Mathf.Max(0.1f, returnTime)));
                float brakingSpeed = Mathf.Sqrt(2f * returnAcceleration * distance);
                Vector2 desiredVelocity = displacement.normalized *
                    Mathf.Min(speedLimit, brakingSpeed);
                returnVelocity = Vector2.MoveTowards(returnVelocity, desiredVelocity,
                    returnAcceleration * dt);
                transform.position = (Vector2)transform.position + returnVelocity * dt;
                if (returnVelocity.sqrMagnitude > 0.001f)
                    transform.up = returnVelocity.normalized;

                if (((Vector2)transform.position - destination).sqrMagnitude <=
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
