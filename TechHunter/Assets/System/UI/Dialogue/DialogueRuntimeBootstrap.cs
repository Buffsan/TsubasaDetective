using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Shared game-system bootstrap. Creates one dialogue UI for NPCs, events and shops.
/// The source of the conversation does not own its rendering or character registry.
/// </summary>
public class DialogueRuntimeBootstrap : MonoBehaviour
{
    [SerializeField] DialogueCharacterDatabase characterDatabase;
    [SerializeField] DialogueDatabase conversationDatabase;
    [SerializeField] TMP_FontAsset japaneseFont;
    [SerializeField] bool createCanvasAutomatically = true;

    GameObject createdCanvas;
    GameObject createdEventSystem;

    public DialogueUIController DialogueController { get; private set; }
    public DialogueCharacterDatabase CharacterDatabase { get { return characterDatabase; } }
    public DialogueDatabase ConversationDatabase { get { return conversationDatabase; } }

    void Awake()
    {
        if (!createCanvasAutomatically) return;
        DialogueUIController existing = DialogueUIController.Instance;
        if (existing == null) existing = FindObjectOfType<DialogueUIController>();
        if (existing != null)
        {
            if (characterDatabase != null) existing.SetDatabase(characterDatabase);
            if (conversationDatabase != null) existing.SetConversationDatabase(conversationDatabase);
            DialogueController = existing;
            return;
        }

        createdCanvas = CreateDialogueCanvas();
        if (createdCanvas != null)
            DialogueController = createdCanvas.GetComponent<DialogueUIController>();
    }

    void Start()
    {
        // ALL_SystemManager is initialized by now, regardless of Awake order.
        if (ALL_SystemManager.Instance != null)
            ALL_SystemManager.Instance.RegisterDialogueSystem(
                this, DialogueController, characterDatabase);
    }

    void OnDestroy()
    {
        if (ALL_SystemManager.Instance != null)
            ALL_SystemManager.Instance.UnregisterDialogueSystem(this);
        if (createdCanvas != null) Destroy(createdCanvas);
        if (createdEventSystem != null) Destroy(createdEventSystem);
    }

    GameObject CreateDialogueCanvas()
    {
        GameObject canvasObject = new GameObject("DialogueCanvas (runtime)",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = .5f;

        RectTransform root = Rect("DialogueRoot", canvasObject.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        CanvasGroup group = root.gameObject.AddComponent<CanvasGroup>();
        Image inputBlocker = root.gameObject.AddComponent<Image>();
        inputBlocker.color = Color.clear;
        inputBlocker.raycastTarget = true;

        RectTransform portraits = Rect("Portraits", root,
            Vector2.zero, Vector2.one, new Vector2(32, 245), new Vector2(-32, -20));
        HorizontalLayoutGroup row = portraits.gameObject.AddComponent<HorizontalLayoutGroup>();
        row.childAlignment = TextAnchor.LowerCenter;
        row.childControlHeight = true;
        row.childControlWidth = true;
        row.childForceExpandHeight = true;
        row.childForceExpandWidth = true;
        RectTransform template = Rect("PortraitTemplate", portraits,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image portraitImage = template.gameObject.AddComponent<Image>();
        portraitImage.preserveAspect = true;
        portraitImage.raycastTarget = false;
        template.gameObject.SetActive(false);

        RectTransform border = Panel("DialogueBorder", root,
            new Vector2(0, 0), new Vector2(1, 0),
            new Vector2(18, 18), new Vector2(-18, 238),
            new Color(.91f, .63f, .29f, 1));
        RectTransform inner = Panel("MessageBackground", border,
            Vector2.zero, Vector2.one, new Vector2(12, 12), new Vector2(-12, -12),
            new Color(.98f, .98f, .98f, .98f));
        TextMeshProUGUI body = Text("MessageText", inner,
            Vector2.zero, Vector2.one, new Vector2(30, 24), new Vector2(-30, -24), 36);
        body.alignment = TextAlignmentOptions.TopLeft;
        body.overflowMode = TextOverflowModes.Truncate;

        RectTransform nameBorder = Panel("SpeakerNamePanel", border,
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(25, -4), new Vector2(425, 63),
            new Color(.91f, .63f, .29f, 1));
        RectTransform nameInner = Panel("SpeakerNameBackground", nameBorder,
            Vector2.zero, Vector2.one, new Vector2(9, 8), new Vector2(-9, -8), Color.white);
        TextMeshProUGUI name = Text("SpeakerName", nameInner,
            Vector2.zero, Vector2.one, new Vector2(14, 0), new Vector2(-14, 0), 42);
        name.fontStyle = FontStyles.Bold;
        name.alignment = TextAlignmentOptions.Center;
        name.overflowMode = TextOverflowModes.Ellipsis;

        TextMeshProUGUI next = Text("ContinueIndicator", inner,
            new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(-68, 12), new Vector2(-12, 62), 32);
        next.text = "▼";
        next.alignment = TextAlignmentOptions.Center;

        // Add the component after the UI is built. Awake then runs with no
        // assigned root; AssignUI and SetActive(false) complete initialization.
        DialogueUIController ui = canvasObject.AddComponent<DialogueUIController>();
        ui.AssignUI(group, portraits, portraitImage, nameBorder.gameObject, name, body, next.gameObject);
        ui.SetDatabase(characterDatabase);
        ui.SetConversationDatabase(conversationDatabase);
        root.gameObject.SetActive(false);

        // The existing EventSystem is reused; only create one when missing.
        if (FindObjectOfType<EventSystem>() == null)
        {
            createdEventSystem = new GameObject("Shop DialogueEventSystem",
                typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        return canvasObject;
    }

    RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max,
        Vector2 offsetsMin, Vector2 offsetsMax)
    {
        RectTransform rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = offsetsMin;
        rt.offsetMax = offsetsMax;
        return rt;
    }

    RectTransform Panel(string name, Transform parent, Vector2 min, Vector2 max,
        Vector2 offsetsMin, Vector2 offsetsMax, Color color)
    {
        RectTransform rt = Rect(name, parent, min, max, offsetsMin, offsetsMax);
        Image image = rt.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return rt;
    }

    TextMeshProUGUI Text(string name, Transform parent, Vector2 min, Vector2 max,
        Vector2 offsetsMin, Vector2 offsetsMax, float size)
    {
        RectTransform rt = Rect(name, parent, min, max, offsetsMin, offsetsMax);
        TextMeshProUGUI label = rt.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = japaneseFont != null ? japaneseFont : TMP_Settings.defaultFontAsset;
        label.fontSize = size;
        label.color = Color.black;
        label.enableWordWrapping = true;
        label.raycastTarget = false;
        return label;
    }
}
