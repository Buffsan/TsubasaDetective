using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusMenu : MonoBehaviour
{
    [SerializeField] Vector2 SetPosition;
    [SerializeField] TextMeshProUGUI TextTemplate;

    [Serializable]
    public class StatusRow
    {
        public string Label;
        [Min(0)] public int ValueIndex;
        public bool Percentage;
        [Min(0)] public float ExtraSpacingAfter;
    }

    [Header("Status rows (order = display order)")]
    [SerializeField] List<StatusRow> StatusRows = new List<StatusRow>();

    [Header("Viewport (panel local units)")]
    [SerializeField, Min(0)] float ViewportHorizontalPadding = 10f;
    [SerializeField, Min(0)] float ViewportTopPadding = 168f;
    [SerializeField, Min(0)] float ViewportBottomPadding = 24f;
    [SerializeField, Min(1)] float ScrollSensitivity = 72f;

    [Header("Left scrollbar")]
    [SerializeField, Min(4)] float ScrollbarWidth = 16f;
    [SerializeField, Min(0)] float ScrollbarGap = 6f;
    [SerializeField] Color ScrollbarTrackColor = new Color(0.25f, 0.25f, 0.25f, 0.65f);
    [SerializeField] Color ScrollbarHandleColor = new Color(0.95f, 0.95f, 0.95f, 1f);

    [Header("Row layout (X from viewport left)")]
    [SerializeField, Min(0)] float LabelX = 0f;
    [SerializeField, Min(0)] float ValueX = 245f;
    [SerializeField, Min(0)] float DeltaX = 245f;
    [SerializeField, Min(1)] float RowHeight = 64f;
    [SerializeField, Min(0)] float RowSpacing = 8f;
    [SerializeField, Min(1)] float DeltaFontSize = 22f;

    [Header("Card preview")]
    [SerializeField] Color PositiveColor = new Color(0.25f, 0.9f, 0.45f);
    [SerializeField] Color NegativeColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField, Min(1)] float PreviewFontMultiplier = 1.2f;

    const int MaxHpIndex = 12;
    readonly List<RowView> rows = new List<RowView>();
    StatusUP previewCard;
    RectTransform rect;
    ScrollRect scroll;
    bool listBuilt;

    class RowView
    {
        public StatusRow Definition;
        public TextMeshProUGUI Value;
        public TextMeshProUGUI Delta;
    }

    public enum Move { Move, Wait, Run }
    public Move move = Move.Move;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void FixedUpdate()
    {
        switch (move)
        {
            case Move.Move: isMove(); break;
            case Move.Run:
                if (scroll != null)
                {
                    if (scroll.verticalScrollbar != null)
                        scroll.verticalScrollbar.interactable = false;
                    scroll.enabled = false;
                }
                isRun();
                break;
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

    // Construct once per panel instance, after SkillController has parented it to
    // ResultCanvas. Updating card previews never recreates the rows or resets scrolling.
    void BuildList()
    {
        if (listBuilt || TextTemplate == null || rect == null) return;
        listBuilt = true;
        TextTemplate.gameObject.SetActive(false);

        RectTransform viewport = CreateRect("StatusViewport", rect);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        // Reserve a separate gutter for the scrollbar; rows never overlap it.
        float scrollbarWidth = Mathf.Max(4f, ScrollbarWidth);
        viewport.offsetMin = new Vector2(ViewportHorizontalPadding + scrollbarWidth + ScrollbarGap,
            ViewportBottomPadding);
        viewport.offsetMax = new Vector2(-ViewportHorizontalPadding, -ViewportTopPadding);
        viewport.gameObject.AddComponent<RectMask2D>();

        RectTransform content = CreateRect("Content", viewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        float width = Mathf.Max(1f, viewport.rect.width);
        float labelWidth = Mathf.Max(1f, Mathf.Min(ValueX, width) - LabelX - 8f);
        float valueWidth = Mathf.Max(1f, width - ValueX);
        float deltaWidth = Mathf.Max(1f, width - DeltaX);
        float height = Mathf.Max(1f, RowHeight);
        float valueHeight = height * 0.62f;
        float y = 0f;

        foreach (StatusRow definition in StatusRows)
        {
            if (definition == null) continue;
            RectTransform row = CreateRect(definition.Label, content);
            row.anchorMin = new Vector2(0f, 1f);
            row.anchorMax = new Vector2(1f, 1f);
            row.pivot = new Vector2(0f, 1f);
            row.anchoredPosition = new Vector2(0f, -y);
            row.sizeDelta = new Vector2(0f, height);

            TextMeshProUGUI label = CreateText("Label", row, LabelX, 0f, labelWidth,
                valueHeight, TextAlignmentOptions.MidlineLeft, TextTemplate.fontSize);
            label.text = definition.Label;

            TextMeshProUGUI value = CreateText("Value", row, ValueX, 0f, valueWidth,
                valueHeight, TextAlignmentOptions.MidlineRight, TextTemplate.fontSize);
            TextMeshProUGUI delta = CreateText("Delta", row, DeltaX, valueHeight, deltaWidth,
                height - valueHeight, TextAlignmentOptions.MidlineRight, DeltaFontSize);

            rows.Add(new RowView { Definition = definition, Value = value, Delta = delta });
            y += height + RowSpacing + definition.ExtraSpacingAfter;
        }

        if (rows.Count > 0)
            y -= RowSpacing + rows[rows.Count - 1].Definition.ExtraSpacingAfter;
        content.sizeDelta = new Vector2(0f, Mathf.Max(viewport.rect.height, y));

        // The existing background Image receives pointer events anywhere on the
        // panel. ScrollRect uses EventSystem (wheel / touch), not global mouse input.
        scroll = gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.inertia = false;
        scroll.scrollSensitivity = ScrollSensitivity;
        scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        scroll.verticalScrollbar = CreateScrollbar(scrollbarWidth);
        scroll.verticalScrollbar.size = Mathf.Clamp01(viewport.rect.height / Mathf.Max(1f, content.rect.height));
        scroll.verticalNormalizedPosition = 1f;
        content.anchoredPosition = Vector2.zero;
    }

    Scrollbar CreateScrollbar(float width)
    {
        RectTransform track = CreateRect("VerticalScrollbar", rect);
        track.anchorMin = new Vector2(0f, 0f);
        track.anchorMax = new Vector2(0f, 1f);
        track.pivot = new Vector2(0f, 0.5f);
        track.offsetMin = new Vector2(ViewportHorizontalPadding, ViewportBottomPadding);
        track.offsetMax = new Vector2(ViewportHorizontalPadding + width, -ViewportTopPadding);
        Image trackImage = track.gameObject.AddComponent<Image>();
        trackImage.color = ScrollbarTrackColor;
        trackImage.raycastTarget = true;

        RectTransform slidingArea = CreateRect("SlidingArea", track);
        slidingArea.anchorMin = Vector2.zero;
        slidingArea.anchorMax = Vector2.one;
        slidingArea.offsetMin = new Vector2(2f, 2f);
        slidingArea.offsetMax = new Vector2(-2f, -2f);

        RectTransform handle = CreateRect("Handle", slidingArea);
        handle.anchorMin = Vector2.zero;
        handle.anchorMax = Vector2.one;
        handle.offsetMin = Vector2.zero;
        handle.offsetMax = Vector2.zero;
        Image handleImage = handle.gameObject.AddComponent<Image>();
        handleImage.color = ScrollbarHandleColor;
        handleImage.raycastTarget = true;

        Scrollbar scrollbar = track.gameObject.AddComponent<StatusMenuScrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.handleRect = handle;
        scrollbar.targetGraphic = handleImage;
        scrollbar.navigation = new Navigation { mode = Navigation.Mode.None };
        scrollbar.value = 1f;
        return scrollbar;
    }

    RectTransform CreateRect(string objectName, Transform parent)
    {
        GameObject obj = new GameObject(objectName, typeof(RectTransform));
        obj.layer = gameObject.layer;
        RectTransform result = (RectTransform)obj.transform;
        result.SetParent(parent, false);
        return result;
    }

    TextMeshProUGUI CreateText(string objectName, RectTransform parent,
        float x, float y, float width, float height, TextAlignmentOptions alignment, float fontSize)
    {
        TextMeshProUGUI text = Instantiate(TextTemplate, parent, false);
        text.name = objectName;
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(0f, 1f);
        textRect.pivot = new Vector2(0f, 1f);
        textRect.anchoredPosition = new Vector2(x, -y);
        textRect.sizeDelta = new Vector2(width, height);
        textRect.localScale = Vector3.one;
        text.text = string.Empty;
        text.alignment = alignment;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        text.maskable = true;
        text.overflowMode = TextOverflowModes.Truncate;
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Min(14f, fontSize);
        text.fontSizeMax = fontSize;
        text.fontSize = fontSize;
        text.gameObject.SetActive(true);
        return text;
    }

    void Render()
    {
        BuildList();
        var player = PlayerController.Instance;
        if (player == null || !listBuilt) return;

        float[] current = StatusCardEffects.GetValues(player, null);
        float[] next = previewCard != null ? StatusCardEffects.GetValues(player, previewCard) : current;

        foreach (RowView row in rows)
        {
            int index = row.Definition.ValueIndex;
            if (index < 0 || index >= current.Length || index >= next.Length)
            {
                row.Value.text = "-";
                row.Delta.text = string.Empty;
                continue;
            }

            bool hp = index == 0 && next.Length > MaxHpIndex && current.Length > MaxHpIndex;
            float delta = hp ? next[MaxHpIndex] - current[MaxHpIndex] : next[index] - current[index];
            // Retain existing behavior: when max HP is unchanged, show healing.
            if (hp && Mathf.Approximately(delta, 0f))
                delta = next[0] - current[0];

            bool changed = previewCard != null && !Mathf.Approximately(delta, 0f);
            Color color = delta >= 0f ? PositiveColor : NegativeColor;
            string unit = row.Definition.Percentage ? "%" : string.Empty;
            row.Value.text = hp
                ? next[0].ToString("F1") + " / " + next[MaxHpIndex].ToString("F1")
                : next[index].ToString("F1") + unit;
            row.Value.color = changed ? color : TextTemplate.color;
            row.Value.fontSizeMax = TextTemplate.fontSize * (changed ? PreviewFontMultiplier : 1f);
            row.Delta.text = changed ? (delta > 0f ? "+" : "") + delta.ToString("F1") + unit : "";
            row.Delta.color = color;
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
