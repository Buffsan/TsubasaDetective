using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HP Sliderの既存Fillを利用して、魅了による侵食領域をピンクで重ねる。
/// コンポーネントと画像は初回魅了ダメージ時に実行時生成するため、
/// 敵PrefabやVariantへの参照設定・Overrideは不要。
/// </summary>
[DisallowMultipleComponent]
public sealed class CharmHPBarVisual : MonoBehaviour
{
    static readonly Color32 CharmPink = new Color32(192, 68, 136, 255);

    Slider slider;
    EnemyBase enemy;
    Image sourceImage;
    Image charmImage;
    RectTransform charmRect;

    /// <summary>同じSliderへの重複生成を防止しつつ、表示対象を登録する。</summary>
    public void Initialize(Slider hpSlider, EnemyBase target)
    {
        if (hpSlider == null || target == null || hpSlider.fillRect == null) return;

        slider = hpSlider;
        enemy = target;
        if (charmRect == null)
            CreateCharmFill();

        Refresh();
    }

    void CreateCharmFill()
    {
        RectTransform sourceRect = slider.fillRect;
        sourceImage = sourceRect.GetComponent<Image>();
        if (sourceImage == null)
        {
            // 一部の古いPrefabはFillRectの子にImageを持つ。
            sourceImage = sourceRect.GetComponentInChildren<Image>();
            if (sourceImage != null)
                sourceRect = sourceImage.rectTransform;
        }

        if (sourceImage == null)
        {
            Debug.LogWarning("魅了ゲージを表示できません: HP_SliderのFillにImageがありません。", slider);
            return;
        }

        Transform existing = sourceRect.Find("CharmFill_Runtime");
        if (existing != null)
        {
            charmImage = existing.GetComponent<Image>();
            charmRect = existing as RectTransform;
            if (charmImage != null && charmRect != null) return;
        }

        GameObject go = new GameObject("CharmFill_Runtime", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(sourceRect, false);
        charmRect = (RectTransform)go.transform;
        charmRect.localScale = Vector3.one;
        charmRect.localRotation = Quaternion.identity;
        charmRect.anchoredPosition = Vector2.zero;
        charmRect.offsetMin = Vector2.zero;
        charmRect.offsetMax = Vector2.zero;

        charmImage = go.GetComponent<Image>();
        charmImage.sprite = sourceImage.overrideSprite != null ? sourceImage.overrideSprite : sourceImage.sprite;
        charmImage.material = sourceImage.material;
        // SliderのFilled画像は全体幅で表示されるため、ピンクは部分領域を持つSimple画像にする。
        charmImage.type = sourceImage.type == Image.Type.Sliced ? Image.Type.Sliced : Image.Type.Simple;
        charmImage.preserveAspect = false;
        charmImage.fillCenter = sourceImage.fillCenter;
        charmImage.pixelsPerUnitMultiplier = sourceImage.pixelsPerUnitMultiplier;
        charmImage.color = new Color(CharmPink.r / 255f, CharmPink.g / 255f,
            CharmPink.b / 255f, sourceImage.color.a);
        charmImage.raycastTarget = false;
        charmImage.maskable = sourceImage.maskable;
        charmRect.SetAsLastSibling();
    }

    void LateUpdate()
    {
        Refresh();
    }

    void Refresh()
    {
        if (enemy == null || slider == null || charmRect == null || charmImage == null)
            return;

        float maxHp = Mathf.Max(0.001f, enemy.MAXHP);
        float hp = Mathf.Clamp(enemy.HP, 0f, maxHp);
        float covered = enemy.IsCharmed ? hp : Mathf.Clamp(enemy.CharmValue, 0f, hp);

        // 既存のFillが Filled の場合、SliderはRect幅でなくImage.fillAmountを変える。
        // Filledでない場合はSliderがFillRect自体を現在HPの長さに縮める。
        bool filledMode = sourceImage != null && sourceImage.type == Image.Type.Filled;
        float start = filledMode
            ? (hp - covered) / maxHp
            : (hp > 0f ? 1f - covered / hp : 1f);
        float end = filledMode ? hp / maxHp : 1f;

        Vector2 min = Vector2.zero;
        Vector2 max = Vector2.one;
        switch (slider.direction)
        {
            case Slider.Direction.RightToLeft:
                min.x = 1f - end;
                max.x = 1f - start;
                break;
            case Slider.Direction.BottomToTop:
                min.y = start;
                max.y = end;
                break;
            case Slider.Direction.TopToBottom:
                min.y = 1f - end;
                max.y = 1f - start;
                break;
            default:
                min.x = start;
                max.x = end;
                break;
        }

        charmRect.anchorMin = min;
        charmRect.anchorMax = max;
        charmRect.offsetMin = Vector2.zero;
        charmRect.offsetMax = Vector2.zero;
        charmImage.enabled = hp > 0f && covered > 0f;
    }
}
