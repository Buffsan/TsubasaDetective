using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Each enemy accumulates post-defense HP damage during confusion.
public sealed class Relic_ColdAnchor : RelicBase_AI
{
    [SerializeField] float fallSeconds = 0.35f, aboveHead = 0.7f;

    [Header("Anchor Appearance")]
    [SerializeField] Sprite AnchorSprite;
    [SerializeField, Min(0.01f)] float AnchorSpriteScale = 1f;
    class Anchor { public float damage; public charaDamage receiver; public GameObject visual; }
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
            else Follow(anchor.visual, enemy);
        }
        foreach (EnemyBase enemy in finished) anchors.Remove(enemy);
    }
    void Follow(GameObject visual, EnemyBase enemy)
    {
        if (visual == null || enemy == null) return;
        float y = enemy.spriteRenderer != null
            ? enemy.spriteRenderer.bounds.max.y : enemy.transform.position.y + 1.5f;
        visual.transform.position = new Vector3(enemy.transform.position.x,
            y + aboveHead, enemy.transform.position.z);
    }
    IEnumerator Drop(EnemyBase enemy, Anchor anchor)
    {
        GameObject visual = anchor.visual;
        Vector3 start = visual != null ? visual.transform.position : Vector3.zero;
        float duration = Mathf.Max(0.05f, fallSeconds);
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            if (enemy == null || enemy.HP <= 0f || enemy.status == EnemyBase.Status.Die)
            {
                Dispose(visual);
                yield break;
            }
            if (visual != null)
                visual.transform.position = Vector3.Lerp(start, enemy.transform.position,
                    Mathf.Clamp01(t / duration));
            yield return null;
        }
        if (enemy != null && enemy.HP > 0f && enemy.status != EnemyBase.Status.Die &&
            anchor.receiver != null && anchor.damage > 0f)
            anchor.receiver.DamageFixed(anchor.damage);
        Dispose(visual);
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
