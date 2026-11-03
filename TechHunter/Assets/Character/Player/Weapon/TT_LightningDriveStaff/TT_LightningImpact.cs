using System.Collections.Generic;
using UnityEngine;

// Attached to both the normal and Bio Battery lightning projectiles at spawn time.
public sealed class TT_LightningImpact : MonoBehaviour
{
    private StatusEffectAttack electricShock;
    private float fieldRadius;
    private float knockbackImpulse;
    private bool impacted;

    public void Configure(StatusEffectAttack status, float radius, float impulse)
    {
        electricShock = status;
        fieldRadius = Mathf.Max(0.1f, radius);
        knockbackImpulse = Mathf.Max(0f, impulse);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (impacted || !other.CompareTag("EnemyArea")) return;
        charaDamage hit = other.GetComponent<charaDamage>();
        if (hit == null || hit.OwnerEnemyBase == null ||
            hit.OwnerEnemyBase.status == EnemyBase.Status.Die ||
            hit.OwnerEnemyBase.HP <= 0f || hit.OwnerEnemyBase.IsCharmed) return;

        impacted = true;

        if (knockbackImpulse > 0f)
        {
            Rigidbody2D targetBody = hit.OwnerEnemyBase.GetComponent<Rigidbody2D>();
            if (targetBody != null && targetBody.bodyType == RigidbodyType2D.Dynamic)
            {
                Vector2 direction = targetBody.position - (Vector2)transform.position;
                if (direction.sqrMagnitude < 0.001f)
                    direction = transform.up;
                targetBody.AddForce(direction.normalized * knockbackImpulse, ForceMode2D.Impulse);
            }
        }

        GameObject fieldObject = new GameObject("LightningImpactField");
        fieldObject.transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        TT_LightningShockField field = fieldObject.AddComponent<TT_LightningShockField>();
        field.Initialize(electricShock, fieldRadius);
        // Prevent this projectile from producing additional impacts after its first hit.
        Collider2D projectileCollider = GetComponent<Collider2D>();
        if (projectileCollider != null) projectileCollider.enabled = false;
        Destroy(gameObject);
    }
}

// Visual-only area: damage is always zero. Each enemy receives this field's
// status at most once, even if it owns several EnemyArea colliders.
public sealed class TT_LightningShockField : MonoBehaviour
{
    private const float Lifetime = 1f;
    private const float FadeTime = 0.18f;
    private static Sprite circleSprite;
    private readonly HashSet<GameObject> affectedEnemies = new HashSet<GameObject>();
    private StatusEffectAttack electricShock;
    private SpriteRenderer spriteRenderer;
    private float radius;
    private float age;

    public void Initialize(StatusEffectAttack status, float fieldRadius)
    {
        electricShock = status;
        radius = Mathf.Max(0.1f, fieldRadius);
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetCircleSprite();
        spriteRenderer.sortingOrder = 5;
        transform.localScale = Vector3.one * radius * 2f;
        UpdateAppearance();
    }

    private void Update()
    {
        age += Time.deltaTime;
        UpdateAppearance();
        if (age >= Lifetime) Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (electricShock == null) return;
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D collider in overlaps)
        {
            if (!collider.CompareTag("EnemyArea")) continue;
            charaDamage damage = collider.GetComponent<charaDamage>();
            if (damage == null || damage.OwnerEnemyBase == null ||
                damage.OwnerEnemyBase.status == EnemyBase.Status.Die ||
                damage.OwnerEnemyBase.HP <= 0f || damage.OwnerEnemyBase.IsCharmed) continue;

            GameObject enemy = damage.OwnerEnemyBase.gameObject;
            if (!affectedEnemies.Add(enemy)) continue;
            damage.SpawnBuff(electricShock);
        }
    }

    private void UpdateAppearance()
    {
        if (spriteRenderer == null) return;
        float fadeIn = Mathf.Clamp01(age / FadeTime);
        float fadeOut = Mathf.Clamp01((Lifetime - age) / FadeTime);
        float alpha = 0.6f * Mathf.Min(fadeIn, fadeOut);
        spriteRenderer.color = new Color(0.30f, 0.80f, 1.0f, alpha);
    }

    private static Sprite GetCircleSprite()
    {
        if (circleSprite != null) return circleSprite;
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = (x + 0.5f - size * 0.5f) / (size * 0.5f);
            float dy = (y + 0.5f - size * 0.5f) / (size * 0.5f);
            pixels[y * size + x] = new Color(1f, 1f, 1f,
                Mathf.Clamp01((1f - Mathf.Sqrt(dx * dx + dy * dy)) * 24f));
        }
        texture.SetPixels(pixels);
        texture.Apply();
        circleSprite = Sprite.Create(texture, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), size);
        return circleSprite;
    }
}
