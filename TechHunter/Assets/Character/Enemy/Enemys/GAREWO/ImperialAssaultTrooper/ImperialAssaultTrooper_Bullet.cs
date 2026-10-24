using UnityEngine;

// 弾丸PrefabにRigidbody2D/Collider2Dを設定する。
// EnemyAttackによる被ダメージは既存システムに任せる。
[RequireComponent(typeof(Rigidbody2D))]
public class ImperialAssaultTrooper_Bullet : MonoBehaviour
{
    Rigidbody2D body;
    bool launched;
    readonly RaycastHit2D[] wallSweepHits = new RaycastHit2D[8];

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }
    public void Launch(Vector2 direction, float speed, float lifetime)
    {
        if (body == null) body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.velocity = direction.normalized * speed;
        launched = true;
        Destroy(gameObject, Mathf.Max(0.05f, lifetime));
    }
    void FixedUpdate()
    {
        if (!launched || body == null) return;
        Vector2 velocity = body.velocity;
        if (velocity.sqrMagnitude <= 0.0001f) return;
        Vector2 direction = velocity.normalized;
        transform.right = direction;
        // Trigger Colliderは薄いwallを高速で通過する場合があるため、
        // 次の物理更新で移動する線分もwallタグに対して確認する。
        float travel = velocity.magnitude * Time.fixedDeltaTime;
        int count = Physics2D.RaycastNonAlloc(body.position, direction, wallSweepHits,
            travel + 0.03f);
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = wallSweepHits[i].collider;
            if (hit != null && hit.gameObject != gameObject && hit.CompareTag("wall"))
            {
                Destroy(gameObject);
                return;
            }
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("wall")) Destroy(gameObject);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("wall")) Destroy(gameObject);
    }
}
