using UnityEngine;

// 専用の加速弾。ダメージ・防御・魅了・敵対勢力判定は既存EnemyAttackへ委譲。
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(EnemyAttack))]
public sealed class LunarPurgeSquad_axe_Wave : MonoBehaviour
{
    [SerializeField] Transform spriteVisual;
    [SerializeField] float spriteRotationOffsetDegrees;

    readonly RaycastHit2D[] castHits = new RaycastHit2D[64];
    readonly Collider2D[] overlaps = new Collider2D[64];
    Rigidbody2D body;
    BoxCollider2D hitbox;
    EnemyAttack damage;
    Vector2 direction;
    float range, travelled, speed, acceleration, maxSpeed, angle;
    bool initialized, finished;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<BoxCollider2D>();
        damage = GetComponent<EnemyAttack>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        hitbox.isTrigger = true;
    }

    public void Initialize(Vector2 aim, float maxTravel, Vector2 size,
        float initialSpeed, float accelerationPerSecond, float maximumSpeed)
    {
        direction = aim.sqrMagnitude > 0.0001f ? aim.normalized : Vector2.right;
        range = Mathf.Max(0.01f, maxTravel);
        travelled = 0f;
        speed = Mathf.Max(0.01f, initialSpeed);
        acceleration = Mathf.Max(0f, accelerationPerSecond);
        maxSpeed = Mathf.Max(speed, maximumSpeed);
        finished = false;
        angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        if (spriteVisual != null)
            spriteVisual.localRotation = Quaternion.Euler(0f, 0f, spriteRotationOffsetDegrees);
        hitbox.size = new Vector2(Mathf.Max(0.05f, size.x), Mathf.Max(0.05f, size.y));
        damage.BeginManualHitWindow();
        initialized = true;
    }

    void FixedUpdate()
    {
        if (!initialized) return;
        if (finished) { Destroy(gameObject); return; }

        speed = Mathf.Min(maxSpeed, speed + acceleration * Time.fixedDeltaTime);
        float move = Mathf.Min(speed * Time.fixedDeltaTime, range - travelled);
        Vector2 start = body.position;

        int overlapCount = Physics2D.OverlapBoxNonAlloc(start, hitbox.size, angle, overlaps);
        for (int i = 0; i < overlapCount; i++)
            damage.CheckHitNow(overlaps[i]);

        if (move > 0f)
        {
            int hits = Physics2D.BoxCastNonAlloc(start, hitbox.size, angle,
                direction, castHits, move);
            for (int i = 0; i < hits; i++)
                damage.CheckHitNow(castHits[i].collider);
            body.MovePosition(start + direction * move);
            travelled += move;
        }
        if (travelled >= range - 0.0001f) finished = true;
    }

    void OnDisable()
    {
        if (damage != null) damage.EndManualHitWindow();
    }
}
