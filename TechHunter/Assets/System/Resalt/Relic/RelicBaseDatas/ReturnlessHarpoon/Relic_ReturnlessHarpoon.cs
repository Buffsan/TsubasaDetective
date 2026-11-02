using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class Relic_ReturnlessHarpoon : RelicBase_AI
{
    [SerializeField] float orbitRadius = 1.25f, orbitSpeed = 1.4f;
    [SerializeField] float windup = 1f, dashTime = 0.5f, dashDistance = 15f, returnTime = 4f;
    [SerializeField] float stun = 4f, confusion = 1f;
    [SerializeField] float spinPower = 2000f;

    enum Mode { Orbit, Spin, Dash, Return }
    Mode mode;
    float time, charge;
    Vector2 dashOrigin, dashDirection;
    readonly HashSet<EnemyBase> targets = new HashSet<EnemyBase>();
    public bool CanCharge => isActiveAndEnabled && mode == Mode.Orbit;

    void Awake()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null) box = gameObject.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(0.6f, 1.6f);
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
                float angle = Time.time * orbitSpeed;
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
                }
                break;
            case Mode.Dash:
                time += dt;
                float progress = Mathf.Clamp01(time / Mathf.Max(dashTime, 0.01f));
                transform.position = dashOrigin + dashDirection * (dashDistance * progress);
                if (progress >= 1f) { time = 0f; mode = Mode.Return; }
                break;
            case Mode.Return:
                time += dt;
                Vector2 destination = player.transform.position;
                Vector2 displacement = destination - (Vector2)transform.position;
                if (displacement.sqrMagnitude > 0.001f) transform.up = displacement.normalized;
                float remaining = Mathf.Max(0.01f, returnTime - time);
                float speed = Mathf.Max(Mathf.Lerp(2f, 16f, Mathf.Clamp01(time / returnTime)),
                    displacement.magnitude / remaining);
                transform.position = Vector2.MoveTowards(transform.position, destination, speed * dt);
                if (((Vector2)transform.position - destination).sqrMagnitude <= 0.0625f ||
                    time >= returnTime)
                {
                    charge = 0f;
                    time = 0f;
                    targets.Clear();
                    mode = Mode.Orbit;
                }
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (mode != Mode.Dash || charge <= 0f || !other.CompareTag("EnemyArea")) return;
        charaDamage area = other.GetComponent<charaDamage>();
        if (area == null) return;
        EnemyBase enemy = area.OwnerEnemyBase;
        if (enemy == null || enemy.HP <= 0f || enemy.IsCharmed ||
            enemy.status == EnemyBase.Status.Die || !targets.Add(enemy)) return;
        float bonus = playerController != null ? playerController.AddConfusionDamagePercent : 0f;
        area.DamageFromPlayer(charge, stun, confusion, false, bonus);
    }
}
