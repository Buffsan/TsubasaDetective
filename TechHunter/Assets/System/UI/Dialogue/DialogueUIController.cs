using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dialogue overlay. Does not change PlayerController.movetype or gameMode,
/// so scripted movement/scene events can continue during dialogue.
/// </summary>
public class DialogueUIController : MonoBehaviour
{
    public static DialogueUIController Instance { get; private set; }
    public bool IsPlaying { get; private set; }

    [Header("UI references")]
    [SerializeField] CanvasGroup dialogueRoot;
    [SerializeField] RectTransform portraitContainer;
    [SerializeField] Image portraitTemplate;
    [SerializeField] GameObject namePanel;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] GameObject continueIndicator;

    [Header("Appearance and progression")]
    [SerializeField] Color inactivePortraitTint = new Color(0.35f, 0.35f, 0.35f, 1f);
    [SerializeField, Min(1f)] float charactersPerSecond = 35f;
    [Tooltip("Normally OFF: keep world simulation active so scripted movement can run.")]
    [SerializeField] bool pauseWorldWhileTalking = false;

    readonly List<Image> portraitImages = new List<Image>();
    DialogueData currentDialogue;
    Action onFinished;
    PlayerController lockedPlayer;
    int currentLineIndex = -1;
    int visibleCharacters;
    int totalCharacters;
    int lastAdvanceFrame = -1;
    float characterAccumulator;
    float previousTimeScale = 1f;
    bool didPauseWorld;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (dialogueRoot != null)
            dialogueRoot.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!IsPlaying || visibleCharacters >= totalCharacters)
            return;

        characterAccumulator += Time.unscaledDeltaTime * charactersPerSecond;
        int revealCount = Mathf.FloorToInt(characterAccumulator);
        if (revealCount <= 0)
            return;

        characterAccumulator -= revealCount;
        visibleCharacters = Mathf.Min(totalCharacters, visibleCharacters + revealCount);
        messageText.maxVisibleCharacters = visibleCharacters;
        if (visibleCharacters >= totalCharacters && continueIndicator != null)
            continueIndicator.SetActive(true);
    }

    public bool Play(DialogueData dialogue, Action completed = null)
    {
        if (IsPlaying || dialogue == null || dialogue.lines == null || dialogue.lines.Count == 0)
            return false;

        if (dialogueRoot == null || portraitContainer == null || portraitTemplate == null ||
            messageText == null || namePanel == null || nameText == null)
        {
            Debug.LogError("DialogueUIController: UI references are missing.", this);
            return false;
        }

        currentDialogue = dialogue;
        onFinished = completed;
        currentLineIndex = -1;
        IsPlaying = true;
        lastAdvanceFrame = Time.frameCount;

        lockedPlayer = PlayerController.Instance;
        if (lockedPlayer != null)
            lockedPlayer.SetDialogueInputBlocked(true);

        if (pauseWorldWhileTalking)
        {
            previousTimeScale = Time.timeScale;
            didPauseWorld = true;
            Time.timeScale = 0f;
        }

        dialogueRoot.gameObject.SetActive(true);
        BuildPortraits();
        ShowNextLine();
        return true;
    }

    public void Advance()
    {
        if (!IsPlaying || lastAdvanceFrame == Time.frameCount)
            return;

        lastAdvanceFrame = Time.frameCount;
        if (visibleCharacters < totalCharacters)
        {
            visibleCharacters = totalCharacters;
            messageText.maxVisibleCharacters = totalCharacters;
            if (continueIndicator != null)
                continueIndicator.SetActive(true);
            return;
        }

        ShowNextLine();
    }

    void ShowNextLine()
    {
        currentLineIndex++;
        if (currentDialogue == null || currentLineIndex >= currentDialogue.lines.Count)
        {
            Finish();
            return;
        }

        DialogueData.Line line = currentDialogue.lines[currentLineIndex];
        int speaker = line.speakerIndex;
        bool validSpeaker = currentDialogue.characters != null &&
                            speaker >= 0 && speaker < currentDialogue.characters.Count;

        namePanel.SetActive(validSpeaker && line.showSpeakerName);
        nameText.text = validSpeaker ? (currentDialogue.characters[speaker].displayName ?? "") : "";

        for (int i = 0; i < portraitImages.Count; i++)
        {
            if (!portraitImages[i].gameObject.activeSelf)
                continue;
            portraitImages[i].color = i == speaker ? Color.white : inactivePortraitTint;
        }

        messageText.text = line.text ?? "";
        messageText.maxVisibleCharacters = 0;
        messageText.ForceMeshUpdate();
        totalCharacters = messageText.textInfo.characterCount;
        visibleCharacters = 0;
        characterAccumulator = 0f;
        if (continueIndicator != null)
            continueIndicator.SetActive(totalCharacters == 0);
    }

    void BuildPortraits()
    {
        int count = currentDialogue.characters == null ? 0 : currentDialogue.characters.Count;
        for (int i = 0; i < count; i++)
        {
            if (i >= portraitImages.Count)
            {
                Image portrait = Instantiate(portraitTemplate, portraitContainer);
                portrait.name = "Character_" + i;
                portraitImages.Add(portrait);
            }

            DialogueData.Character character = currentDialogue.characters[i];
            Image image = portraitImages[i];
            image.sprite = character.portrait;
            image.enabled = character.portrait != null;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.rectTransform.localScale = new Vector3(
                character.flipHorizontally ? -character.scale : character.scale,
                character.scale, 1f);
            image.color = inactivePortraitTint;
            image.gameObject.SetActive(true);
        }

        for (int i = count; i < portraitImages.Count; i++)
            portraitImages[i].gameObject.SetActive(false);
    }

    public void Finish()
    {
        if (!IsPlaying)
            return;

        IsPlaying = false;
        if (dialogueRoot != null)
            dialogueRoot.gameObject.SetActive(false);

        if (lockedPlayer != null)
            lockedPlayer.SetDialogueInputBlocked(false);
        lockedPlayer = null;

        if (didPauseWorld)
        {
            Time.timeScale = previousTimeScale;
            didPauseWorld = false;
        }

        currentDialogue = null;
        Action callback = onFinished;
        onFinished = null;
        callback?.Invoke();
    }

    void OnDisable()
    {
        Finish();
    }

    void OnDestroy()
    {
        Finish();
        if (Instance == this)
            Instance = null;
    }

    /// <summary>Used by the Editor prefab generator.</summary>
    public void AssignUI(CanvasGroup root, RectTransform portraits, Image template,
        GameObject speakerPanel, TextMeshProUGUI speakerName,
        TextMeshProUGUI body, GameObject next)
    {
        dialogueRoot = root;
        portraitContainer = portraits;
        portraitTemplate = template;
        namePanel = speakerPanel;
        nameText = speakerName;
        messageText = body;
        continueIndicator = next;
    }
}
