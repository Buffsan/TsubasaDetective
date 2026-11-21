using UnityEngine;

// ステップ移動中のSpriteRendererを1フレーム切り取る残像。
// 生成後は敵本体から独立したワールド座標で静止し、0.4秒等で透明になって消える。
public sealed class LunarPurgeSquad_axe_Afterimage : MonoBehaviour
{
    SpriteRenderer ghostRenderer;
    Color originalColor;
    float fadeDuration;
    float elapsed;

    public static void Spawn(SpriteRenderer original, float opacity, float fadeSeconds)
    {
        if (original == null || original.sprite == null || opacity <= 0f) return;

        GameObject ghost = new GameObject("LunarPurgeSquad_axe_Afterimage");
        Transform source = original.transform;
        ghost.transform.SetPositionAndRotation(source.position, source.rotation);
        // 敵の向きに応じた負のスケールもスナップショットへ反映する。
        ghost.transform.localScale = source.lossyScale;

        SpriteRenderer image = ghost.AddComponent<SpriteRenderer>();
        image.sprite = original.sprite; // 現在の歩行フレームを静止画として複写
        image.sharedMaterial = original.sharedMaterial;
        image.flipX = original.flipX;
        image.flipY = original.flipY;
        image.drawMode = original.drawMode;
        if (original.drawMode != SpriteDrawMode.Simple)
            image.size = original.size;
        image.sortingLayerID = original.sortingLayerID;
        image.sortingOrder = original.sortingOrder - 1;
        image.spriteSortPoint = original.spriteSortPoint;
        image.maskInteraction = original.maskInteraction;

        Color color = original.color;
        color.a = Mathf.Clamp01(color.a * opacity);
        image.color = color;

        LunarPurgeSquad_axe_Afterimage fade =
            ghost.AddComponent<LunarPurgeSquad_axe_Afterimage>();
        fade.ghostRenderer = image;
        fade.originalColor = color;
        fade.fadeDuration = Mathf.Max(0.01f, fadeSeconds);
    }

    void Update()
    {
        if (ghostRenderer == null)
        {
            Destroy(gameObject);
            return;
        }

        elapsed += Time.deltaTime;
        float remaining = 1f - Mathf.Clamp01(elapsed / fadeDuration);
        Color color = originalColor;
        color.a *= remaining;
        ghostRenderer.color = color;
        if (elapsed >= fadeDuration)
            Destroy(gameObject);
    }
}
