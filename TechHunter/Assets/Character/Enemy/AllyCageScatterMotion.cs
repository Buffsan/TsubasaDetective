using System.Collections;
using UnityEngine;

/// <summary>
/// Briefly throws a released NPC outward before restoring its original enemy AI.
/// The spawned prefab and its normal movement code remain unchanged.
/// </summary>
[DisallowMultipleComponent]
public sealed class AllyCageScatterMotion : MonoBehaviour
{
    EnemyContoroller regularAI;
    Boss_Controller bossAI;
    CG_Knght legacyKnightAI;
    Rigidbody2D body;
    bool wasSimulated;
    bool initialized;

    public void Begin(Vector3 destination, float duration)
    {
        if (initialized) return;
        initialized = true;
        regularAI = GetComponent<EnemyContoroller>();
        bossAI = GetComponent<Boss_Controller>();
        legacyKnightAI = GetComponent<CG_Knght>();
        body = GetComponent<Rigidbody2D>();

        if (regularAI != null) regularAI.enabled = false;
        if (bossAI != null) bossAI.enabled = false;
        if (legacyKnightAI != null) legacyKnightAI.enabled = false;

        if (body != null)
        {
            wasSimulated = body.simulated;
            body.velocity = Vector2.zero;
            body.simulated = false; // 散開中は物理衝突を起こさない。
        }
        StartCoroutine(Scatter(destination, Mathf.Max(0.01f, duration)));
    }

    IEnumerator Scatter(Vector3 target, float duration)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            transform.position = Vector3.LerpUnclamped(start, target, eased);
            yield return null;
        }

        transform.position = target;
        if (body != null)
        {
            body.simulated = wasSimulated;
            if (wasSimulated) body.velocity = Vector2.zero;
        }

        if (regularAI != null) regularAI.enabled = true;
        if (bossAI != null) bossAI.enabled = true;
        if (legacyKnightAI != null) legacyKnightAI.enabled = true;

        Destroy(this);
    }

    void OnDestroy()
    {
        // 中断時（ステージ終了など）も物理を無効なまま残さない。
        if (!initialized || !gameObject.scene.IsValid()) return;
        if (body != null) body.simulated = wasSimulated;
    }
}
