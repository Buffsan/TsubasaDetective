using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusMenu : MonoBehaviour
{
    [SerializeField] Vector2 SetPosition;
    [SerializeField] TextMeshProUGUI Statuss;

    // 既存のStatussは残す。新UIを使用する場合は12個ずつ順番に設定する。
    // 順番: HP, 攻撃倍率, 攻撃, 回復, 防御倍率, 防御, CT, 運, 混乱, スタン, 速度倍率, 速度
    [SerializeField] List<TextMeshProUGUI> StatusValues = new List<TextMeshProUGUI>();
    [SerializeField] List<TextMeshProUGUI> StatusDeltas = new List<TextMeshProUGUI>();
    [SerializeField] Color PositiveColor = new Color(0.25f, 0.9f, 0.45f);
    [SerializeField] Color NegativeColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] float PreviewFontMultiplier = 1.2f;

    readonly HashSet<int> lineBreaks = new HashSet<int> { 2, 5, 6, 9 };
    readonly List<Color> defaultColors = new List<Color>();
    readonly List<float> defaultSizes = new List<float>();
    StatusUP previewCard;
    static readonly string[] AdditionalStatusNames =
        { "獲得資金増加", "混乱時ダメージ", "回復量増加", "幸運ダメージ" };
    readonly List<TextMeshProUGUI> extraStatusRows = new List<TextMeshProUGUI>();

    public enum Move { Move, Wait, Run }
    public Move move = Move.Move;
    RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void FixedUpdate()
    {
        switch (move)
        {
            case Move.Move: isMove(); break;
            case Move.Run: isRun(); break;
        }
    }

    public void SetStatus()
    {
        Render();
    }

    public void Preview(StatusUP card)
    {
        previewCard = card;
        Render();
    }

    public void ClearPreview()
    {
        previewCard = null;
        Render();
    }

    void CacheDefaults()
    {
        if (defaultSizes.Count != 0) return;
        foreach (var value in StatusValues)
        {
            defaultSizes.Add(value != null ? value.fontSize : 0f);
            defaultColors.Add(value != null ? value.color : Color.white);
        }
    }

    void Render()
    {
        var player = PlayerController.Instance;
        if (player == null) return;

        float[] current = StatusCardEffects.GetValues(player, null);
        float[] next = previewCard != null ? StatusCardEffects.GetValues(player, previewCard) : current;
        bool split = StatusValues.Count >= 12 && StatusDeltas.Count >= 12;
        CacheDefaults();

        if (split)
        {
            if (Statuss != null) Statuss.gameObject.SetActive(false);
            for (int i = 0; i < 12; i++)
            {
                var value = StatusValues[i];
                var deltaText = StatusDeltas[i];
                if (value == null || deltaText == null) continue;

                float delta = i == 0 ? next[12] - current[12] : next[i] - current[i];
                // HP回復のみの場合は回復量を変化量として表示。
                if (i == 0 && Mathf.Approximately(delta, 0f))
                    delta = next[0] - current[0];
                bool changed = previewCard != null && !Mathf.Approximately(delta, 0f);
                Color color = delta >= 0f ? PositiveColor : NegativeColor;
                value.color = changed ? color : defaultColors[i];
                value.fontSize = changed ? defaultSizes[i] * PreviewFontMultiplier : defaultSizes[i];
                value.text = i == 0
                    ? next[0].ToString("F1") + " / " + next[12].ToString("F1")
                    : next[i].ToString("F1");
                deltaText.text = changed ? (delta > 0f ? "+" : "") + delta.ToString("F1") : "";
                deltaText.color = color;
            }
            RenderAdditionalRows(current, next);
            return;
        }

        // Inspector未設定の間も従来の1テキスト表示を維持する。
        if (Statuss == null) return;
        Statuss.gameObject.SetActive(true);
        string result = "";
        for (int i = 0; i < 12; i++)
        {
            float delta = i == 0 ? next[12] - current[12] : next[i] - current[i];
            if (i == 0 && Mathf.Approximately(delta, 0f))
                delta = next[0] - current[0];
            string value = i == 0
                ? next[0].ToString("F1") + " / " + next[12].ToString("F1")
                : next[i].ToString("F1");
            if (previewCard != null && !Mathf.Approximately(delta, 0f))
            {
                string hex = ColorUtility.ToHtmlStringRGB(delta > 0f ? PositiveColor : NegativeColor);
                value = "<color=#" + hex + "><size=120%>" + value + "</size> <size=75%>"
                    + (delta > 0f ? "+" : "") + delta.ToString("F1") + "</size></color>";
            }
            result += value + "\n";
            if (lineBreaks.Contains(i)) result += "\n";
        }
        // The four additional modifiers have their own labels even when the
        // existing UI has its original fixed set of twelve labels.
        for (int i = 0; i < AdditionalStatusNames.Length; i++)
        {
            int valueIndex = 13 + i; // 12 is the existing hidden max-HP value.
            float delta = next[valueIndex] - current[valueIndex];
            string value = AdditionalStatusNames[i] + ": " + next[valueIndex].ToString("F1") + "%";
            if (previewCard != null && !Mathf.Approximately(delta, 0f))
            {
                string hex = ColorUtility.ToHtmlStringRGB(delta > 0f ? PositiveColor : NegativeColor);
                value = "<color=#" + hex + "><size=120%>" + value
                    + "</size> <size=75%>" + (delta > 0f ? "+" : "")
                    + delta.ToString("F1") + "%</size></color>";
            }
            result += value + "\n";
        }
        Statuss.text = result;
        // Allow four extra rows to be visible in legacy single-TMP menus.
        Statuss.ForceMeshUpdate();
        Statuss.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
            Mathf.Max(Statuss.rectTransform.rect.height, Statuss.preferredHeight + 8f));
    }

    void RenderAdditionalRows(float[] current, float[] next)
    {
        // Prefabs explicitly wired with 16 value/delta rows are supported.
        bool hasNewInspectorRows = StatusValues.Count >= 16 && StatusDeltas.Count >= 16;
        if (!hasNewInspectorRows && extraStatusRows.Count == 0)
        {
            TextMeshProUGUI reference = StatusValues[11];
            if (reference == null) return;
            RectTransform referenceRect = reference.rectTransform;
            float step = Mathf.Max(22f, referenceRect.rect.height + 3f);
            for (int i = 0; i < AdditionalStatusNames.Length; i++)
            {
                TextMeshProUGUI row = Instantiate(reference, reference.transform.parent);
                row.name = "AdditionalStatus_" + i;
                row.raycastTarget = false;
                row.enableWordWrapping = false;
                row.overflowMode = TextOverflowModes.Overflow;
                row.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                    Mathf.Max(300f, referenceRect.rect.width));
                row.rectTransform.anchoredPosition = referenceRect.anchoredPosition
                    + new Vector2(-45f, -(i + 1) * step);
                row.transform.SetAsLastSibling();
                extraStatusRows.Add(row);
            }
        }

        for (int i = 0; i < AdditionalStatusNames.Length; i++)
        {
            int valueIndex = 13 + i;
            float delta = next[valueIndex] - current[valueIndex];
            bool changed = previewCard != null && !Mathf.Approximately(delta, 0f);
            Color color = delta >= 0f ? PositiveColor : NegativeColor;
            TextMeshProUGUI row = hasNewInspectorRows ? StatusValues[12 + i]
                : (i < extraStatusRows.Count ? extraStatusRows[i] : null);
            if (row == null) continue;

            row.gameObject.SetActive(true);
            row.text = AdditionalStatusNames[i] + ": " + next[valueIndex].ToString("F1") + "%";
            if (changed && !hasNewInspectorRows)
                row.text += " (" + (delta > 0f ? "+" : "") + delta.ToString("F1") + "%)";
            row.color = changed ? color : (hasNewInspectorRows ? defaultColors[12 + i] : defaultColors[11]);
            row.fontSize = (hasNewInspectorRows ? defaultSizes[12 + i] : defaultSizes[11] * 0.85f)
                * (changed ? PreviewFontMultiplier : 1f);
            if (hasNewInspectorRows)
            {
                TextMeshProUGUI deltaText = StatusDeltas[12 + i];
                if (deltaText == null) continue;
                deltaText.text = changed
                    ? (delta > 0f ? "+" : "") + delta.ToString("F1") + "%" : "";
                deltaText.color = color;
            }
        }
    }

    void isRun()
    {
        if (rect != null)
            rect.localPosition = new Vector2(rect.localPosition.x - 100f, 0f);
    }

    void isMove()
    {
        if (rect == null) return;
        rect.localPosition = rect.localPosition.x < SetPosition.x
            ? new Vector2(rect.localPosition.x + 200f, 0f)
            : new Vector2(SetPosition.x, 0f);
    }
}
