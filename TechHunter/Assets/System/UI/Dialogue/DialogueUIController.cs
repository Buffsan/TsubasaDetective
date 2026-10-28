using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueUIController : MonoBehaviour
{
    public static DialogueUIController Instance { get; private set; }
    public bool IsPlaying { get; private set; }
    public DialogueShopItemData SelectedShopItem { get; private set; }

    [Header("Shared registry")]
    [SerializeField] DialogueCharacterDatabase characterDatabase;
    [SerializeField] DialogueDatabase conversationDatabase;
    [Header("UI")]
    [SerializeField] CanvasGroup dialogueRoot;
    [SerializeField] RectTransform portraitContainer;
    [SerializeField] Image portraitTemplate;
    [SerializeField] GameObject namePanel;
    [SerializeField] TextMeshProUGUI nameText, messageText;
    [SerializeField] GameObject continueIndicator;
    [Header("Display")]
    [SerializeField] Color inactivePortraitTint = new Color(.35f, .35f, .35f, 1f);
    [SerializeField, Min(1)] float charactersPerSecond = 35;
    [SerializeField] bool pauseWorldWhileTalking;

    readonly List<Image> portraits = new List<Image>();
    readonly List<DialogueCharacterId> portraitIds = new List<DialogueCharacterId>();
    readonly Dictionary<int, int> nodeMap = new Dictionary<int, int>();
    readonly Dictionary<string, string> textVariables = new Dictionary<string, string>();
    readonly Dictionary<DialogueCharacterId, DialogueExpression> expressions =
        new Dictionary<DialogueCharacterId, DialogueExpression>();
    readonly Dictionary<DialogueCharacterId, string> extraKeys = new Dictionary<DialogueCharacterId, string>();
    readonly List<Button> buttons = new List<Button>();

    DialogueData data;
    PlayerController player;
    Action finished;
    Action<int,int> picked;
    RectTransform choicesPanel;
    int current, visible, total, focusedChoice, lastInputFrame = -1;
    float charactersAccumulator, savedTimeScale;
    bool pauseApplied, usingLegacy, inChoice;

    public void SetDatabase(DialogueCharacterDatabase db) { characterDatabase = db; }
    public void SetConversationDatabase(DialogueDatabase db) { conversationDatabase = db; }

    public bool PlayById(string conversationId, Action onFinished = null, Action<int,int> onPicked = null)
    {
        if (conversationDatabase == null)
        {
            Debug.LogError("Dialogue: conversation database is not assigned.", this);
            return false;
        }
        DialogueData conversation = conversationDatabase.Find(conversationId);
        if (conversation == null)
        {
            Debug.LogError("Dialogue: unknown conversation ID '" + conversationId + "'.", this);
            return false;
        }
        return Play(conversation, onFinished, onPicked);
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (dialogueRoot != null) dialogueRoot.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!IsPlaying) return;
        Keyboard keyboard = Keyboard.current;
        Gamepad gamepad = Gamepad.current;
        if (inChoice)
        {
            bool up = (keyboard != null && (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)) ||
                      (gamepad != null && gamepad.dpad.up.wasPressedThisFrame);
            bool down = (keyboard != null && (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)) ||
                        (gamepad != null && gamepad.dpad.down.wasPressedThisFrame);
            if (up) FocusChoice(focusedChoice - 1);
            if (down) FocusChoice(focusedChoice + 1);
        }
        if ((keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)) ||
            (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame))
            Advance();

        if (inChoice || visible == total) return;
        charactersAccumulator += Time.unscaledDeltaTime * charactersPerSecond;
        int n = Mathf.FloorToInt(charactersAccumulator);
        if (n < 1) return;
        charactersAccumulator -= n;
        visible = Mathf.Min(total, visible + n);
        messageText.maxVisibleCharacters = visible;
        if (continueIndicator != null) continueIndicator.SetActive(visible >= total);
    }

    public bool Play(DialogueData conversation, Action onFinished = null, Action<int,int> onPicked = null)
    {
        if (IsPlaying || conversation == null) return false;
        bool newFormat = conversation.nodes != null && conversation.nodes.Count != 0;
        if (!newFormat && (conversation.lines == null || conversation.lines.Count == 0)) return false;
        if (dialogueRoot == null || portraitContainer == null || portraitTemplate == null ||
            namePanel == null || nameText == null || messageText == null)
        {
            Debug.LogError("Dialogue: missing UI references.", this);
            return false;
        }
        nodeMap.Clear();
        if (newFormat)
        {
            for (int i = 0; i < conversation.nodes.Count; i++)
            {
                int id = conversation.nodes[i].nodeId;
                if (nodeMap.ContainsKey(id))
                {
                    Debug.LogError("Dialogue: duplicate node ID " + id, conversation);
                    return false;
                }
                nodeMap.Add(id, i);
            }
            if (conversation.entryNodeId >= 0 && !nodeMap.ContainsKey(conversation.entryNodeId))
            {
                Debug.LogError("Dialogue: unknown entry ID " + conversation.entryNodeId, conversation);
                return false;
            }
        }
        data = conversation;
        usingLegacy = !newFormat;
        current = newFormat && data.entryNodeId >= 0 ? nodeMap[data.entryNodeId] : 0;
        finished = onFinished;
        picked = onPicked;
        lastInputFrame = Time.frameCount;
        IsPlaying = true;
        player = PlayerController.Instance;
        if (player != null) player.SetDialogueInputBlocked(true);
        if (pauseWorldWhileTalking)
        {
            savedTimeScale = Time.timeScale;
            pauseApplied = true;
            Time.timeScale = 0f;
        }
        dialogueRoot.gameObject.SetActive(true);
        HideChoices();
        BuildPortraits();
        Show();
        return true;
    }

    public void Advance()
    {
        if (!IsPlaying || lastInputFrame == Time.frameCount) return;
        // The Player input action maps Decide to left mouse. Let Button.onClick
        // resolve the exact clicked option instead of selecting the focused one.
        if (inChoice && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return;
        if (inChoice) { Choose(focusedChoice); return; }
        lastInputFrame = Time.frameCount;
        if (visible < total)
        {
            visible = total;
            messageText.maxVisibleCharacters = total;
            if (continueIndicator != null) continueIndicator.SetActive(true);
            return;
        }
        if (usingLegacy) { current++; Show(); return; }
        DialogueData.Node node = data.nodes[current];
        if (node.endDialogue) { Finish(); return; }
        Jump(node.nextNodeId, current + 1);
    }

    void Jump(int nodeId, int nextIndex)
    {
        if (nodeId < 0) current = nextIndex;
        else if (!nodeMap.TryGetValue(nodeId, out current))
        {
            Debug.LogError("Dialogue: missing destination node " + nodeId, data);
            Finish();
            return;
        }
        Show();
    }

    void Show()
    {
        if (!IsPlaying) return;
        int count = usingLegacy ? data.lines.Count : data.nodes.Count;
        if (current >= count || current < 0) { Finish(); return; }
        inChoice = false;
        HideChoices();

        if (usingLegacy)
        {
            DialogueData.Line line = data.lines[current];
            bool named = line.speakerIndex >= 0 && line.speakerIndex < data.characters.Count;
            namePanel.SetActive(named && line.showSpeakerName);
            nameText.text = named ? data.characters[line.speakerIndex].displayName : "";
            for (int i = 0; i < portraits.Count; i++)
                if (portraits[i].gameObject.activeSelf)
                    portraits[i].color = i == line.speakerIndex ? Color.white : inactivePortraitTint;
            BeginText(line.text);
            return;
        }

        DialogueData.Node node = data.nodes[current];
        DialogueCharacterProfile speaker = characterDatabase != null ? characterDatabase.Find(node.speakerId) : null;
        namePanel.SetActive(node.showSpeakerName && speaker != null);
        nameText.text = speaker != null ? speaker.displayName : "";
        UpdatePortraits(node.speakerId, node.expression, node.otherExpressionKey);
        if (node.type == DialogueData.NodeType.Choice)
        {
            messageText.text = ReplaceTokens(node.text);
            messageText.maxVisibleCharacters = int.MaxValue;
            messageText.ForceMeshUpdate();
            total = visible = messageText.textInfo.characterCount;
            if (continueIndicator != null) continueIndicator.SetActive(false);
            ShowChoices(node);
        }
        else BeginText(node.text);
    }

    void BeginText(string raw)
    {
        messageText.text = ReplaceTokens(raw);
        messageText.ForceMeshUpdate();
        total = messageText.textInfo.characterCount;
        visible = 0;
        charactersAccumulator = 0;
        messageText.maxVisibleCharacters = 0;
        if (continueIndicator != null) continueIndicator.SetActive(total == 0);
    }

    public void SetSelectedShopItem(DialogueShopItemData item)
    {
        SelectedShopItem = item;
        if (!IsPlaying || data == null) return;
        string raw = usingLegacy ? data.lines[current].text : data.nodes[current].text;
        int previous = visible;
        bool complete = visible >= total;
        messageText.text = ReplaceTokens(raw);
        messageText.ForceMeshUpdate();
        total = messageText.textInfo.characterCount;
        visible = complete ? total : Mathf.Min(previous, total);
        messageText.maxVisibleCharacters = inChoice ? int.MaxValue : visible;
        if (inChoice) RefreshChoiceLabels();
        if (continueIndicator != null) continueIndicator.SetActive(!inChoice && visible >= total);
    }

    public void SetTextVariable(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        textVariables[key] = value ?? "";
        RefreshVisibleText();
    }

    public void ClearTextVariables()
    {
        textVariables.Clear();
    }

    void RefreshVisibleText()
    {
        if (!IsPlaying || data == null) return;
        SetSelectedShopItem(SelectedShopItem);
    }

    string ReplaceTokens(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        if (SelectedShopItem != null)
        {
            raw = raw.Replace("{itemName}", SelectedShopItem.itemName ?? "")
                .Replace("{itemPrice}", SelectedShopItem.price.ToString("N0"))
                .Replace("{itemDescription}", SelectedShopItem.description ?? "");
        }
        foreach (KeyValuePair<string, string> pair in textVariables)
            raw = raw.Replace("{" + pair.Key + "}", pair.Value);
        return raw;
    }

    void BuildPortraits()
    {
        portraitIds.Clear();
        expressions.Clear(); extraKeys.Clear();
        int count;
        if (usingLegacy) count = data.characters != null ? data.characters.Count : 0;
        else
        {
            if (data.participants != null)
                foreach (DialogueCharacterId id in data.participants)
                    if (id != DialogueCharacterId.None && !portraitIds.Contains(id)) portraitIds.Add(id);
            count = portraitIds.Count;
        }

        for (int i = 0; i < count; i++)
        {
            if (i == portraits.Count)
            {
                Image created = Instantiate(portraitTemplate, portraitContainer);
                created.name = "Portrait_" + i;
                portraits.Add(created);
            }
            Image image = portraits[i];
            DialogueData.Character old = usingLegacy ? data.characters[i] : null;
            DialogueCharacterProfile profile = !usingLegacy && characterDatabase != null ? characterDatabase.Find(portraitIds[i]) : null;
            image.sprite = usingLegacy ? old.portrait : (profile != null ? profile.normal : null);
            image.enabled = image.sprite != null;
            image.preserveAspect = true;
            image.raycastTarget = false;
            bool flip = usingLegacy ? old.flipHorizontally : profile != null && profile.flipHorizontally;
            float scale = usingLegacy ? old.scale : profile != null ? profile.scale : 1f;
            image.rectTransform.localScale = new Vector3(flip ? -scale : scale, scale, 1f);
            image.color = inactivePortraitTint;
            image.gameObject.SetActive(true);
        }
        for (int i = count; i < portraits.Count; i++) portraits[i].gameObject.SetActive(false);
    }

    void UpdatePortraits(DialogueCharacterId speaker, DialogueExpression expression, string key)
    {
        if (speaker != DialogueCharacterId.None)
        {
            expressions[speaker] = expression;
            extraKeys[speaker] = key;
        }
        for (int i = 0; i < portraitIds.Count; i++)
        {
            var id = portraitIds[i];
            var profile = characterDatabase != null ? characterDatabase.Find(id) : null;
            Image image = portraits[i];
            if (profile != null)
            {
                DialogueExpression mood = expressions.ContainsKey(id) ? expressions[id] : DialogueExpression.Normal;
                string otherKey = extraKeys.ContainsKey(id) ? extraKeys[id] : "";
                image.sprite = profile.GetSprite(mood, otherKey);
                image.enabled = image.sprite != null;
            }
            image.color = id == speaker ? Color.white : inactivePortraitTint;
        }
    }

    void EnsureChoicesPanel()
    {
        if (choicesPanel != null) return;
        GameObject panel = new GameObject("DialogueChoices", typeof(RectTransform),
            typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        choicesPanel = panel.GetComponent<RectTransform>();
        choicesPanel.SetParent(dialogueRoot.transform, false);
        choicesPanel.anchorMin = choicesPanel.anchorMax = new Vector2(.5f, 0);
        choicesPanel.pivot = new Vector2(.5f, 0);
        choicesPanel.anchoredPosition = new Vector2(0, 250);
        choicesPanel.sizeDelta = new Vector2(760, 0);
        Image background = panel.GetComponent<Image>();
        background.color = new Color(.08f, .08f, .09f, .85f);
        background.raycastTarget = false;
        VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 12, 12);
        layout.spacing = 7;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;
        panel.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        panel.SetActive(false);
    }

    Button NewButton(int index)
    {
        GameObject go = new GameObject("Option_" + index,
            typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(choicesPanel, false);
        go.GetComponent<LayoutElement>().preferredHeight = 54;
        Image background = go.GetComponent<Image>();
        Button button = go.GetComponent<Button>();
        RectTransform rt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<RectTransform>();
        rt.SetParent(go.transform, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(18, 0); rt.offsetMax = new Vector2(-18, 0);
        TextMeshProUGUI label = rt.GetComponent<TextMeshProUGUI>();
        label.font = messageText.font;
        label.fontSize = 28;
        label.color = Color.black;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.raycastTarget = false;
        int captured = index;
        button.onClick.AddListener(() => Choose(captured));
        return button;
    }

    void ShowChoices(DialogueData.Node node)
    {
        if (node.choices == null || node.choices.Count == 0)
        {
            Debug.LogError("Dialogue: a Choice node has no options.", data);
            Finish(); return;
        }
        EnsureChoicesPanel();
        for (int i = buttons.Count; i < node.choices.Count; i++) buttons.Add(NewButton(i));
        for (int i = 0; i < buttons.Count; i++) buttons[i].gameObject.SetActive(i < node.choices.Count);
        inChoice = true;
        focusedChoice = 0;
        choicesPanel.gameObject.SetActive(true);
        RefreshChoiceLabels();
    }

    void RefreshChoiceLabels()
    {
        if (!inChoice) return;
        var options = data.nodes[current].choices;
        for (int i = 0; i < options.Count; i++)
            buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = ReplaceTokens(options[i].label);
        FocusChoice(focusedChoice);
    }

    void FocusChoice(int index)
    {
        int count = data.nodes[current].choices.Count;
        if (count == 0) return;
        focusedChoice = (index % count + count) % count;
        for (int i = 0; i < count; i++)
            buttons[i].GetComponent<Image>().color = i == focusedChoice
                ? new Color(1, .8f, .48f) : Color.white;
    }

    public void Choose(int index)
    {
        if (!IsPlaying || !inChoice || lastInputFrame == Time.frameCount) return;
        DialogueData conversation = data;
        DialogueData.Node node = data.nodes[current];
        if (index < 0 || index >= node.choices.Count) return;
        DialogueData.Choice option = node.choices[index];
        if (!nodeMap.ContainsKey(option.targetNodeId))
        {
            Debug.LogError("Dialogue: unknown target node " + option.targetNodeId, data);
            return;
        }
        lastInputFrame = Time.frameCount;
        inChoice = false;
        HideChoices();
        if (option.selectedShopItem != null) SetSelectedShopItem(option.selectedShopItem);
        picked?.Invoke(node.nodeId, index);
        if (!IsPlaying || data != conversation) return;
        if (node.endDialogue) { Finish(); return; }
        Jump(option.targetNodeId, current + 1);
    }

    void HideChoices() { if (choicesPanel != null) choicesPanel.gameObject.SetActive(false); }

    public void Finish()
    {
        if (!IsPlaying) return;
        IsPlaying = false;
        HideChoices();
        if (dialogueRoot != null) dialogueRoot.gameObject.SetActive(false);
        if (player != null) player.SetDialogueInputBlocked(false);
        player = null;
        if (pauseApplied) { Time.timeScale = savedTimeScale; pauseApplied = false; }
        data = null;
        ClearTextVariables();
        Action callback = finished;
        finished = null; picked = null;
        callback?.Invoke();
    }
    void OnDisable() { Finish(); }
    void OnDestroy() { Finish(); if (Instance == this) Instance = null; }

    public void AssignUI(CanvasGroup root, RectTransform parent, Image template,
        GameObject nameBox, TextMeshProUGUI name, TextMeshProUGUI message, GameObject indicator)
    {
        dialogueRoot = root; portraitContainer = parent; portraitTemplate = template;
        namePanel = nameBox; nameText = name; messageText = message; continueIndicator = indicator;
    }
}
