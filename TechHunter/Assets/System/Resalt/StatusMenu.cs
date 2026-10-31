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
        if (defaultSizes.Count == StatusValues.Count) return;
        defaultSizes.Clear();
        defaultColors.Clear();
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
        bool split = StatusValues.Count >= 16 && StatusDeltas.Count >= 16;
        CacheDefaults();

        if (split)
        {
            if (Statuss != null) Statuss.gameObject.SetActive(false);
            for (int i = 0; i < 16; i++)
            {
                var value = StatusValues[i];
                var deltaText = StatusDeltas[i];
                if (value == null || deltaText == null) continue;

                int index = i < 12 ? i : i + 1; // Index 12 holds MaxHP, shown in HP row.
                float delta = i == 0 ? next[12] - current[12] : next[index] - current[index];
                // HP回復のみの場合は回復量を変化量として表示。
                if (i == 0 && Mathf.Approximately(delta, 0f))
                    delta = next[0] - current[0];
                bool changed = previewCard != null && !Mathf.Approximately(delta, 0f);
                Color color = delta >= 0f ? PositiveColor : NegativeColor;
                value.color = changed ? color : defaultColors[i];
                value.fontSize = changed ? defaultSizes[i] * PreviewFontMultiplier : defaultSizes[i];
                value.text = i == 0
                    ? next[0].ToString("F1") + " / " + next[12].ToString("F1")
                    : (i < 12 ? next[index].ToString("F1")
                        : AdditionalStatusNames[i - 12] + ": " + next[index].ToString("F1") + "%");
                deltaText.text = changed ? (delta > 0f ? "+" : "") + delta.ToString("F1")
                    + (i >= 12 ? "%" : "") : "";
                deltaText.color = color;
            }
            return;
        }

        // A prefab that has not yet been migrated still shows all 16 values in
        // the existing legacy text, without instantiating new UI at runtime.
        if (Statuss == null) return;
        foreach (var value in StatusValues)
            if (value != null) value.gameObject.SetActive(false);
        foreach (var deltaText in StatusDeltas)
            if (deltaText != null) deltaText.gameObject.SetActive(false);
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
