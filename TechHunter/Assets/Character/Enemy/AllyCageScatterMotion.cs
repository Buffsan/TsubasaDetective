using System.Collections;
using UnityEngine;

/// <summary>
/// すでに選ばれたStandFloor上の位置で解放演出を再生する。
/// 壁を直線移動で通過することを避けるため、座標は一切補間しない。
/// </summary>
[DisallowMultipleComponent]
public sealed class AllyCageScatterMotion : MonoBehaviour
{
    EnemyContoroller regularAI;
    Boss_Controller bossAI;
    CG_Knght legacyKnightAI;
    Rigidbody2D body;

    bool regularWasEnabled;
    bool bossWasEnabled;
    bool knightWasEnabled;
    bool wasSimulated;
    bool initialized;
    bool restored;
    Vector3 originalScale;

    public void Begin(float duration)
    {
        if (initialized) return;
        initialized = true;
        originalScale = transform.localScale;

        regularAI = GetComponent<EnemyContoroller>();
        bossAI = GetComponent<Boss_Controller>();
        legacyKnightAI = GetComponent<CG_Knght>();
        body = GetComponent<Rigidbody2D>();

        if (regularAI != null)
        {
            regularWasEnabled = regularAI.enabled;
            regularAI.enabled = false;
        }
        if (bossAI != null)
        {
            bossWasEnabled = bossAI.enabled;
            bossAI.enabled = false;
        }
        if (legacyKnightAI != null)
        {
            knightWasEnabled = legacyKnightAI.enabled;
            legacyKnightAI.enabled = false;
        }
        if (body != null)
        {
            wasSimulated = body.simulated;
            body.velocity = Vector2.zero;
            body.simulated = false;
        }

        StartCoroutine(PopIn(Mathf.Max(0.01f, duration)));
    }

    IEnumerator PopIn(float duration)
    {
        Vector3 fromScale = originalScale * 0.65f;
        transform.localScale = fromScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = t * t * (3f - 2f * t);
            transform.localScale = Vector3.LerpUnclamped(fromScale, originalScale, eased);
            yield return null;
        }

        RestoreState();
        Destroy(this);
    }

    void RestoreState()
    {
        if (!initialized || restored) return;
        restored = true;
        transform.localScale = originalScale;

        if (body != null)
        {
            body.simulated = wasSimulated;
            if (wasSimulated) body.velocity = Vector2.zero;
        }
        if (regularAI != null) regularAI.enabled = regularWasEnabled;
        if (bossAI != null) bossAI.enabled = bossWasEnabled;
        if (legacyKnightAI != null) legacyKnightAI.enabled = knightWasEnabled;
    }

    void OnDestroy()
    {
        // 演出が途中で中止されても、残ったNPCの状態を元に戻す。
        RestoreState();
    }
}
