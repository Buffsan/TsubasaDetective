using UnityEngine;

// 弾丸PrefabにRigidbody2D/Collider2Dを設定する。
// EnemyAttackによる被ダメージは既存システムに任せる。
[RequireComponent(typeof(Rigidbody2D))]
public class ImperialAssaultTrooper_Bullet : MonoBehaviour
{
    Rigidbody2D body;
    bool launched;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
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
        if (body.velocity.sqrMagnitude > 0.0001f)
            transform.right = body.velocity.normalized;
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
