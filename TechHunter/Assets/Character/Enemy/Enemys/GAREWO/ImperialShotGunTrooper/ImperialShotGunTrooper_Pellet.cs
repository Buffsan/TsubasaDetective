using System.Collections.Generic;
using UnityEngine;

// 帝国散弾銃士専用の実弾。ダメージ・派閥は既存 EnemyAttack に任せる。
[RequireComponent(typeof(Rigidbody2D), typeof(EnemyAttack), typeof(CircleCollider2D))]
public sealed class ImperialShotGunTrooper_Pellet : MonoBehaviour
{
    // Unityの物理更新はメインスレッド上で順次実行される。大量の散弾で配列を毎回確保しない。
    static readonly RaycastHit2D[] wallHits = new RaycastHit2D[64];
    static readonly RaycastHit2D[] damageHits = new RaycastHit2D[64];
    Rigidbody2D body;
    EnemyAttack damage;
    Vector2 direction;
    float speed, remaining, radius;
    bool launched, finishNextStep;

    public static float RadiusOf(GameObject prefab)
    {
        CircleCollider2D circle = prefab != null ? prefab.GetComponent<CircleCollider2D>() : null;
        if (circle == null) return 0.04f;
        Vector3 scale = circle.transform.lossyScale;
        return Mathf.Max(0.005f, circle.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y)));
    }

    // 警告表示と実弾が共通で使う遮蔽物判定。弾の半径も考慮する。
    public static float WallDistance(Vector2 origin, Vector2 dir, float range, float radius)
    {
        if (range <= 0f) return 0f;
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        int count = Physics2D.CircleCast(origin, radius, dir, filter, wallHits, range);
        RaycastHit2D[] hits = wallHits;
        // コライダーが密集して配列が埋まった場合は壁を取りこぼさない。
        if (count == wallHits.Length)
        {
            var overflow = new List<RaycastHit2D>(wallHits.Length * 2);
            Physics2D.CircleCast(origin, radius, dir, filter, overflow, range);
            hits = overflow.ToArray();
            count = hits.Length;
        }
        float length = range;
        for (int i = 0; i < count; ++i)
            if (hits[i].collider != null && hits[i].collider.CompareTag("Stage_Wall"))
                length = Mathf.Min(length, Mathf.Max(0f, hits[i].distance));
        return length;
    }

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        damage = GetComponent<EnemyAttack>();
        body.gravityScale = 0f;
        body.drag = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Launch(Vector2 shotDirection, float shotSpeed, float range)
    {
        direction = shotDirection.normalized;
        speed = Mathf.Max(0.1f, shotSpeed);
        remaining = Mathf.Max(0f, range);
        radius = RadiusOf(gameObject);
        transform.right = direction;
        // 最初の壁スイープを行ってから動かす（生成フレームで壁を抜けない）。
        body.velocity = Vector2.zero;
        damage.BeginManualHitWindow();
        launched = true;
        Destroy(gameObject, remaining / speed + 1f);
    }

    void FixedUpdate()
    {
        if (!launched) return;
        if (finishNextStep || remaining <= 0.0001f) { End(); return; }

        float step = Mathf.Min(remaining, speed * Time.fixedDeltaTime);
        float wall = WallDistance(body.position, direction, step + 0.001f, radius);
        bool blocked = wall <= step;
        float travel = blocked ? Mathf.Max(0f, wall - 0.002f) : step;
        if (travel <= 0.0001f) { End(); return; }

        // 高速弾でも判定を飛び越えない。壁より先は絶対に手動チェックしない。
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        int count = Physics2D.CircleCast(body.position, radius, direction, filter, damageHits, travel);
        RaycastHit2D[] hits = damageHits;
        if (count == damageHits.Length)
        {
            var overflow = new List<RaycastHit2D>(damageHits.Length * 2);
            Physics2D.CircleCast(body.position, radius, direction, filter, overflow, travel);
            hits = overflow.ToArray();
            count = hits.Length;
        }
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = hits[i].collider;
            if (hit == null) continue;
            if (hit.CompareTag("PlayerArea") || hit.CompareTag("EnemyArea"))
                damage.CheckHitNow(hit);
        }

        body.velocity = direction * (travel / Time.fixedDeltaTime);
        remaining -= travel;
        finishNextStep = blocked || remaining <= 0.0001f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (launched && other.CompareTag("Stage_Wall")) End();
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (launched && collision.collider.CompareTag("Stage_Wall")) End();
    }

    void End()
    {
        if (!launched) return;
        launched = false;
        body.velocity = Vector2.zero;
        // Destroyはフレーム末尾なので即座に判定を無効化する。
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
