using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class Relic_ReturnlessHarpoon : RelicBase_AI
{
    [SerializeField] float orbitRadius = 1.25f, orbitSpeed = 1.4f;
    [SerializeField, Min(1f)] float orbitAlignmentSpeed = 720f;
    [SerializeField] float windup = 1f, dashTime = 0.5f, dashDistance = 15f, returnTime = 4f;
    [SerializeField] float stun = 4f, confusion = 1f;
    [SerializeField] float spinPower = 2000f;

    [Header("Return homing")]
    [Tooltip("Acceleration of the returning harpoon, in world units/sec squared.")]
    [SerializeField, Min(0.1f)] float returnSteeringAcceleration = 26f;
    [Tooltip("Maximum turning speed while flying back, in degrees/second.")]
    [SerializeField, Min(1f)] float returnTurnSpeed = 220f;
    [Tooltip("Extra acceleration after the expected return time, with no teleport or deadline.")]
    [SerializeField, Min(0.05f)] float movingTargetGraceSeconds = 0.5f;
    [Tooltip("Additional chase speed when the orbiting slot moves farther away.")]
    [SerializeField, Min(0f)] float movingTargetCompensation = 2f;
    [Tooltip("Pickup distance when the player physically touches the returning harpoon.")]
    [SerializeField, Min(0.1f)] float returnCatchRadius = 0.85f;
    [Tooltip("Distance from its assigned orbit position needed to rejoin the orbit.")]
    [SerializeField, Min(0.05f)] float returnOrbitCatchRadius = 0.32f;
    [Tooltip("Pickup slightly before contact when the player approaches the harpoon.")]
    [SerializeField, Min(0f)] float returnApproachPadding = 0.15f;
    [Header("Orbit rejoin")]
    [SerializeField, Min(1f)] float returnRejoinAngularSpeed = 180f;
    [SerializeField, Min(0.1f)] float returnRejoinRadialSpeed = 3.5f;

    [Header("Hitbox")]
    [SerializeField, Min(1f)] float spriteHitboxPadding = 1.15f;

    enum Mode { Orbit, Spin, Dash, Return }
    Mode mode;
    float time, charge;
    Vector2 dashOrigin, dashDirection;
    // The player position when the harpoon was thrown is our four-second baseline.
    Vector2 playerPositionAtThrow, returnStartPosition, lastPlayerPosition;
    float baselineReturnDistance;
    Vector2 returnVelocity;
    float orbitAngle, currentOrbitRadius;
    bool orbitAngleInitialized, rejoiningOrbit;
    BoxCollider2D hitbox;

    // Every acquired copy gets its own slot, including copies currently
    // spinning, dashing, or returning. Their slots stay reserved until removed.
    static readonly List<Relic_ReturnlessHarpoon> activeHarpoons =
        new List<Relic_ReturnlessHarpoon>();

    // The dash is only 0.5 seconds; non-allocating sweeps prevent skipped nearby enemies.
    readonly Collider2D[] overlapBuffer = new Collider2D[128];
    readonly RaycastHit2D[] sweepBuffer = new RaycastHit2D[128];
    readonly HashSet<EnemyBase> targets = new HashSet<EnemyBase>();
    public bool CanCharge => isActiveAndEnabled && mode == Mode.Orbit;

    void OnEnable()
    {
        if (!activeHarpoons.Contains(this))
            activeHarpoons.Add(this);
    }

    void OnDisable()
    {
        activeHarpoons.Remove(this);
        orbitAngleInitialized = false;
        rejoiningOrbit = false;
    }

    float TargetOrbitAngle()
    {
        int count = Mathf.Max(1, activeHarpoons.Count);
        int index = activeHarpoons.IndexOf(this);
        if (index < 0) index = 0;
        return Time.time * orbitSpeed * Mathf.Rad2Deg + 360f * index / count;
    }

    Vector2 AssignedOrbitPosition(Vector2 playerPosition)
    {
        float angle = TargetOrbitAngle() * Mathf.Deg2Rad;
        return playerPosition + orbitRadius *
            new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

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
                float targetAngle = TargetOrbitAngle();
                if (!orbitAngleInitialized)
                {
                    orbitAngle = targetAngle;
                    currentOrbitRadius = orbitRadius;
                    orbitAngleInitialized = true;
                }

                // Rejoin from the actual arrival point instead of jumping to the circle.
                float alignment = rejoiningOrbit ? returnRejoinAngularSpeed : orbitAlignmentSpeed;
                orbitAngle = Mathf.MoveTowardsAngle(
                    orbitAngle, targetAngle, alignment * dt);
                currentOrbitRadius = Mathf.MoveTowards(
                    currentOrbitRadius, orbitRadius, returnRejoinRadialSpeed * dt);
                if (rejoiningOrbit &&
                    Mathf.Abs(Mathf.DeltaAngle(orbitAngle, targetAngle)) < 1f &&
                    Mathf.Abs(currentOrbitRadius - orbitRadius) < 0.02f)
                    rejoiningOrbit = false;

                float orbitRadians = orbitAngle * Mathf.Deg2Rad;
                transform.position = (Vector2)player.transform.position +
                    currentOrbitRadius * new Vector2(Mathf.Cos(orbitRadians), Mathf.Sin(orbitRadians));
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
                    // Calibrate the ~4-second flight to this harpoon's orbit slot,
                    // using the player's position when the attack was thrown.
                    returnStartPosition = transform.position;
                    Vector2 originalSlot = AssignedOrbitPosition(playerPositionAtThrow);
                    baselineReturnDistance = Mathf.Max(0f,
                        Vector2.Distance(returnStartPosition, originalSlot) -
                        returnOrbitCatchRadius);
                    Vector2 home = originalSlot - returnStartPosition;
                    if (home.sqrMagnitude < 0.001f)
                        home = AssignedOrbitPosition(player.transform.position) - returnStartPosition;
                    float startSpeed = baselineReturnDistance /
                        Mathf.Max(0.1f, returnTime) * 0.25f;
                    returnVelocity = home.sqrMagnitude > 0.001f
                        ? home.normalized * Mathf.Max(0.1f, startSpeed)
                        : Vector2.zero;
                    lastPlayerPosition = player.transform.position;
                    mode = Mode.Return;
                }
                break;
            case Mode.Return:
                time += dt;
                Vector2 playerPosition = player.transform.position;
                Vector2 harpoonPosition = transform.position;
                Vector2 slotPosition = AssignedOrbitPosition(playerPosition);
                Vector2 playerMotion = playerPosition - lastPlayerPosition;
                Vector2 towardHarpoon = harpoonPosition - playerPosition;
                float playerDistance = towardHarpoon.magnitude;

                // Approaching the weapon picks it up: it must not fly away simply
                // because the assigned orbit position shifts with the player.
                bool playerApproaching = playerMotion.sqrMagnitude > 0.000001f &&
                    Vector2.Dot(playerMotion, towardHarpoon) > 0f;
                if (playerDistance <= returnCatchRadius ||
                    (playerApproaching &&
                     playerDistance <= orbitRadius + returnApproachPadding))
                {
                    ResetAfterReturn(player);
                    break;
                }

                float distanceToSlot = Vector2.Distance(harpoonPosition, slotPosition);
                if (distanceToSlot <= returnOrbitCatchRadius)
                {
                    ResetAfterReturn(player);
                    break;
                }

                // The speed grows along a natural flight curve. Unlike the old
                // deadline-based catch-up there is no distance / time-remaining
                // boost that makes the weapon stick to the screen.
                float baselineTime = Mathf.Max(0.1f, returnTime);
                float baselineSpeed = baselineReturnDistance / baselineTime;
                float returnProgress = Mathf.Max(0f, time / baselineTime);
                float scheduledSpeed = baselineSpeed *
                    (0.25f + 1.5f * Mathf.Min(returnProgress, 1f));
                float extraTravel = Mathf.Max(0f,
                    Vector2.Distance(returnStartPosition, slotPosition) -
                    Vector2.Distance(returnStartPosition,
                        AssignedOrbitPosition(playerPositionAtThrow)));
                float chaseSpeed = movingTargetCompensation * extraTravel / baselineTime;
                float desiredSpeed = scheduledSpeed + chaseSpeed;

                // Speed keeps rising after ~4 seconds for a moving target,
                // but retrieval still requires physically reaching the slot.
                if (time > baselineTime)
                {
                    float lateProgress = (time - baselineTime) /
                        Mathf.Max(0.05f, movingTargetGraceSeconds);
                    desiredSpeed += baselineSpeed * 2f * lateProgress;
                }

                Vector2 aim = (slotPosition - harpoonPosition).normalized;
                Vector2 heading = returnVelocity.sqrMagnitude > 0.000001f
                    ? returnVelocity.normalized : aim;
                float headingAngle = Mathf.Atan2(heading.y, heading.x) * Mathf.Rad2Deg;
                float aimAngle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
                float turnAngle = Mathf.MoveTowardsAngle(
                    headingAngle, aimAngle, returnTurnSpeed * dt) * Mathf.Deg2Rad;
                heading = new Vector2(Mathf.Cos(turnAngle), Mathf.Sin(turnAngle));

                // Real acceleration and bounded turning, like a bird flying home.
                float speed = Mathf.MoveTowards(returnVelocity.magnitude,
                    Mathf.Max(0.1f, desiredSpeed), returnSteeringAcceleration * dt);
                returnVelocity = heading * speed;

                Vector2 returnNextPosition = harpoonPosition + returnVelocity * dt;
                transform.position = returnNextPosition;
                if (returnVelocity.sqrMagnitude > 0.000001f)
                    transform.up = returnVelocity.normalized;

                // Catch a fast crossing of the moving orbit slot between frames.
                Vector2 step = returnNextPosition - harpoonPosition;
                float fraction = step.sqrMagnitude > 0.000001f
                    ? Mathf.Clamp01(Vector2.Dot(slotPosition - harpoonPosition, step) /
                        step.sqrMagnitude)
                    : 0f;
                Vector2 closestPoint = harpoonPosition + step * fraction;

                // Also account for the player walking into the harpoon in the
                // same frame, even when the harpoon moves quickly.
                Vector2 playerStep = playerPosition - lastPlayerPosition;
                float playerFraction = playerStep.sqrMagnitude > 0.000001f
                    ? Mathf.Clamp01(Vector2.Dot(harpoonPosition - lastPlayerPosition,
                        playerStep) / playerStep.sqrMagnitude)
                    : 0f;
                Vector2 playerClosest = lastPlayerPosition + playerStep * playerFraction;

                lastPlayerPosition = playerPosition;
                if ((closestPoint - slotPosition).sqrMagnitude <=
                        returnOrbitCatchRadius * returnOrbitCatchRadius ||
                    (returnNextPosition - playerPosition).sqrMagnitude <=
                        returnCatchRadius * returnCatchRadius ||
                    (playerClosest - harpoonPosition).sqrMagnitude <=
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
        // Preserve the actual pickup position for the first orbit frame.
        // Neither radial position nor angle is snapped onto the ideal circle.
        Vector2 relative = (Vector2)transform.position - (Vector2)player.transform.position;
        orbitAngle = relative.sqrMagnitude > 0.001f
            ? Mathf.Atan2(relative.y, relative.x) * Mathf.Rad2Deg
            : TargetOrbitAngle();
        currentOrbitRadius = relative.magnitude;
        orbitAngleInitialized = true;
        rejoiningOrbit = true;
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
