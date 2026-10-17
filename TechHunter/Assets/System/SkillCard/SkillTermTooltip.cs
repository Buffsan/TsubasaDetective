using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// 説明文の用語部分だけをリンク化する。既存のスキルデータは変更しない。
public class SkillTermTooltip : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI explanation;
    [SerializeField] RectTransform tooltipRoot;
    [SerializeField] TextMeshProUGUI tooltipText;
    [SerializeField] Canvas canvas;
    [SerializeField] Vector2 cursorOffset = new Vector2(18f, -18f);

    static readonly string[] Terms = { "感電", "毒", "氷結", "煙", "無敵", "詠唱", "スタック", "混乱" };
    static readonly string[] Details =
    {
        "0.3秒毎に1ダメージを与えてそこそこの敵を怯ませる。",
        "1秒毎に敵の最大HPに応じた割合ダメージを与える。敵の強さに応じて割合ダメージが変わる。",
        "体が凍てついて動けなくなる。",
        "防御力-10%、追加で2ダメージを受ける。",
        "効果中ダメージを受けなくなる。",
        "決められた秒数の間に、回避、スキル使用を行うとスキル使用がキャンセルされる。",
        "数値分だけスキルを溜め込む事ができる。",
        "行動不能、被ダメージ2倍"
    };

    void Awake()
    {
        if (explanation == null) explanation = GetComponent<TextMeshProUGUI>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        Hide();
        if (tooltipRoot != null)
        {
            var graphics = tooltipRoot.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
            foreach (var graphic in graphics) graphic.raycastTarget = false;
        }
    }

    public void SetDescription(string description)
    {
        if (explanation == null) return;
        explanation.text = Linkify(description ?? "");
        Hide();
    }

    static string Linkify(string input)
    {
        var sb = new StringBuilder();
        bool insideTag = false;
        for (int i = 0; i < input.Length;)
        {
            if (input[i] == '<') insideTag = true;
            if (!insideTag)
            {
                int found = -1;
                for (int t = 0; t < Terms.Length; t++)
                {
                    if (i + Terms[t].Length <= input.Length &&
                        string.CompareOrdinal(input, i, Terms[t], 0, Terms[t].Length) == 0 &&
                        (found < 0 || Terms[t].Length > Terms[found].Length))
                        found = t;
                }
                if (found >= 0)
                {
                    sb.Append("<link=\"term").Append(found).Append("\"><color=#55CCFF><u>")
                      .Append(Terms[found]).Append("</u></color></link>");
                    i += Terms[found].Length;
                    continue;
                }
            }
            sb.Append(input[i]);
            if (input[i] == '>') insideTag = false;
            i++;
        }
        return sb.ToString();
    }

    void Update()
    {
        // Prefab生成後、親Canvasを自動取得する
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        if (explanation == null ||
            tooltipRoot == null ||
            tooltipText == null ||
            canvas == null)
        {
            return;
        }

        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        int index = TMP_TextUtilities.FindIntersectingLink(
            explanation,
            Input.mousePosition,
            cam
        );


        if (index < 0) { Hide(); return; }
        string id = explanation.textInfo.linkInfo[index].GetLinkID();
        if (!id.StartsWith("term") || !int.TryParse(id.Substring(4), out int term) || term < 0 || term >= Details.Length)
        { Hide(); return; }

        tooltipText.text = "<b>" + Terms[term] + "</b>\n" + Details[term];
        tooltipRoot.gameObject.SetActive(true);

        // スクリーン座標をCanvasローカル座標に変換して画面内に収める。
        RectTransform canvasRect = canvas.transform as RectTransform;
        if (canvasRect == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, cam, out Vector2 pos);
        pos += cursorOffset;
        tooltipRoot.SetParent(canvasRect, false);
        tooltipRoot.pivot = new Vector2(0f, 1f);
        LayoutRebuilderForce();
        Vector2 size = tooltipRoot.rect.size;
        Rect bounds = canvasRect.rect;
        pos.x = Mathf.Clamp(pos.x, bounds.xMin, bounds.xMax - size.x);
        pos.y = Mathf.Clamp(pos.y, bounds.yMin + size.y, bounds.yMax);
        tooltipRoot.anchoredPosition = pos;
    }

    void LayoutRebuilderForce()
    {
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRoot);
    }

    void Hide()
    {
        if (tooltipRoot != null) tooltipRoot.gameObject.SetActive(false);
    }

    void OnDisable() { Hide(); }
}
