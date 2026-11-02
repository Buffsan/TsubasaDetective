using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Each enemy accumulates post-defense HP damage during confusion.
public sealed class Relic_ColdAnchor : RelicBase_AI
{
    [SerializeField] float fallSeconds = 0.28f, aboveHead = 0.7f;

    [Header("Anchor Appearance")]
    [SerializeField] Sprite AnchorSprite;
    [SerializeField, Min(0.01f)] float AnchorSpriteScale = 1f;

    [Header("Summon Animation")]
    [SerializeField, Min(0.01f)] float summonSeconds = 0.6f;
    [SerializeField] float summonSpinDegrees = 540f;
    [SerializeField, Range(0.01f, 1f)] float summonStartScale = 0.05f;

    [Header("Slam Animation")]
    [SerializeField, Min(0.01f)] float riseSeconds = 0.16f;
    [SerializeField, Min(0f)] float riseHeight = 0.85f;

    [Header("Impact Effect")]
    [SerializeField] GameObject ImpactEffectPrefab;
    [SerializeField, Min(0.01f)] float impactEffectScale = 0.35f;
    [SerializeField, Min(0.05f)] float impactEffectLifetime = 3f;

    class Anchor
    {
        public float damage;
        public charaDamage receiver;
        public GameObject visual;
        public Vector3 finalScale;
        public float summonElapsed;
    }
    readonly Dictionary<EnemyBase, Anchor> anchors = new Dictionary<EnemyBase, Anchor>();
    readonly List<EnemyBase> finished = new List<EnemyBase>();
    readonly HashSet<GameObject> visuals = new HashSet<GameObject>();
    static Sprite pixel;

    void OnEnable() { GlobalEvents.OnEnemyDamagedWhileConfused += Accumulate; }
    void OnDisable()
    {
        GlobalEvents.OnEnemyDamagedWhileConfused -= Accumulate;
        StopAllCoroutines();
        foreach (GameObject obj in visuals) if (obj != null) Destroy(obj);
        visuals.Clear();
        anchors.Clear();
        finished.Clear();
    }
    void Accumulate(charaDamage receiver, float amount)
    {
        if (receiver == null || amount <= 0f) return;
        EnemyBase enemy = receiver.OwnerEnemyBase;
        if (enemy == null || enemy.HP <= 0f ||
            enemy.status != EnemyBase.Status.ConfusionResistance) return;
        Anchor anchor;
        if (!anchors.TryGetValue(enemy, out anchor))
        {
            anchor = new Anchor { receiver = receiver, visual = CreateVisual() };
            if (anchor.visual != null)
            {
                anchor.finalScale = anchor.visual.transform.localScale;
                anchor.visual.transform.localScale = anchor.finalScale * summonStartScale;
                anchor.visual.transform.rotation = Quaternion.Euler(0f, 0f, summonSpinDegrees);
            }
            anchors.Add(enemy, anchor);
        }
        anchor.damage += amount;
        Follow(anchor.visual, enemy);
    }
    void Update()
    {
        finished.Clear();
        foreach (var pair in anchors)
        {
            EnemyBase enemy = pair.Key;
            Anchor anchor = pair.Value;
            if (enemy == null || enemy.HP <= 0f || enemy.status == EnemyBase.Status.Die)
            {
                Dispose(anchor.visual);
                finished.Add(enemy);
            }
            else if (enemy.status != EnemyBase.Status.ConfusionResistance)
            {
                finished.Add(enemy);
                StartCoroutine(Drop(enemy, anchor));
            }
            else
            {
                Follow(anchor.visual, enemy);
                AnimateSummoning(anchor);
            }
        }
        foreach (EnemyBase enemy in finished) anchors.Remove(enemy);
    }
    Vector3 AboveEnemy(EnemyBase enemy)
    {
        float top = enemy.spriteRenderer != null
            ? enemy.spriteRenderer.bounds.max.y : enemy.transform.position.y + 1.5f;
        return new Vector3(enemy.transform.position.x, top + aboveHead,
            enemy.transform.position.z);
    }

    void Follow(GameObject visual, EnemyBase enemy)
    {
        if (visual != null && enemy != null)
            visual.transform.position = AboveEnemy(enemy);
    }

    void AnimateSummoning(Anchor anchor)
    {
        if (anchor.visual == null) return;
        float duration = Mathf.Max(0.01f, summonSeconds);
        anchor.summonElapsed = Mathf.Min(duration, anchor.summonElapsed + Time.deltaTime);
        float progress = Mathf.SmoothStep(0f, 1f, anchor.summonElapsed / duration);

        // Spin through the requested number of degrees while growing, then
        // settle perfectly upright (zero local Z rotation).
        anchor.visual.transform.localScale = anchor.finalScale *
            Mathf.Lerp(summonStartScale, 1f, progress);
        anchor.visual.transform.rotation = Quaternion.Euler(0f, 0f,
            summonSpinDegrees * (1f - progress));
    }

    static bool CanSlam(EnemyBase enemy)
    {
        return enemy != null && enemy.HP > 0f &&
            enemy.status != EnemyBase.Status.Die;
    }

    IEnumerator Drop(EnemyBase enemy, Anchor anchor)
    {
        GameObject visual = anchor.visual;

        // Finish the summoning pose before the upward anticipation.
        if (visual != null)
        {
            visual.transform.localScale = anchor.finalScale;
            visual.transform.rotation = Quaternion.identity;
        }

        // Anticipation: briefly rise above the enemy before striking down.
        float liftTime = Mathf.Max(0.01f, riseSeconds);
        for (float elapsed = 0f; elapsed < liftTime;)
        {
            if (!CanSlam(enemy))
            {
                Dispose(visual);
                yield break;
            }
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / liftTime));
            if (visual != null)
                visual.transform.position = AboveEnemy(enemy) + Vector3.up * (riseHeight * t);
            yield return null;
        }

        Vector3 start = visual != null ? visual.transform.position : AboveEnemy(enemy);
        float duration = Mathf.Max(0.01f, fallSeconds);
        for (float elapsed = 0f; elapsed < duration;)
        {
            if (!CanSlam(enemy))
            {
                Dispose(visual);
                yield break;
            }
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Cubic easing: almost motionless at first, accelerating into impact.
            float slamProgress = t * t * t;
            if (visual != null)
                visual.transform.position = Vector3.Lerp(start,
                    enemy.transform.position, slamProgress);
            yield return null;
        }

        if (CanSlam(enemy))
        {
            Vector3 impactPosition = enemy.transform.position;
            if (visual != null) visual.transform.position = impactPosition;

            SpawnImpactEffect(impactPosition);
            if (anchor.receiver != null && anchor.damage > 0f)
                anchor.receiver.DamageFixed(anchor.damage);
        }
        Dispose(visual);
    }

    void SpawnImpactEffect(Vector3 impactPosition)
    {
        if (ImpactEffectPrefab == null) return;

        // The HOUKOU animation scales its own root Transform.
        // Scale a wrapper instead so the Inspector size is respected.
        GameObject wrapper = new GameObject("ColdAnchor_ImpactEffect");
        wrapper.transform.position = impactPosition;
        wrapper.transform.localScale = Vector3.one * Mathf.Max(0.01f, impactEffectScale);
        GameObject roar = Instantiate(ImpactEffectPrefab, wrapper.transform);
        roar.transform.localPosition = Vector3.zero;
        roar.transform.localRotation = Quaternion.identity;
        Destroy(wrapper, Mathf.Max(0.05f, impactEffectLifetime));
    }
    GameObject CreateVisual()
    {
        GameObject root = new GameObject("ColdAnchor_Visual");
        visuals.Add(root);

        // Keep the same GameObject and movement logic; swap only its appearance.
        if (AnchorSprite != null)
        {
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = AnchorSprite;
            renderer.sortingOrder = 201;
            root.transform.localScale = Vector3.one * Mathf.Max(0.01f, AnchorSpriteScale);
        }
        else
        {
            // Preserve the original temporary visual when no Sprite is assigned.
            Part(root.transform, "Stem", 0f, 0f, 0.12f, 0.85f);
            Part(root.transform, "Stock", 0f, 0.32f, 0.7f, 0.12f);
            Part(root.transform, "Flukes", 0f, -0.39f, 0.75f, 0.12f);
            Part(root.transform, "Left", -0.37f, -0.27f, 0.12f, 0.36f);
            Part(root.transform, "Right", 0.37f, -0.27f, 0.12f, 0.36f);
        }
        return root;
    }
    void Part(Transform parent, string name, float x, float y, float w, float h)
    {
        if (pixel == null) pixel = Sprite.Create(Texture2D.whiteTexture,
            new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
            new Vector2(0.5f, 0.5f), 1f);
        GameObject part = new GameObject(name);
        part.transform.SetParent(parent, false);
        part.transform.localPosition = new Vector3(x, y, 0f);
        part.transform.localScale = new Vector3(w, h, 1f);
        SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
        renderer.sprite = pixel;
        renderer.color = new Color(0.5f, 0.85f, 1f);
        renderer.sortingOrder = 201;
    }
    void Dispose(GameObject visual)
    {
        visuals.Remove(visual);
        if (visual != null) Destroy(visual);
    }
}
