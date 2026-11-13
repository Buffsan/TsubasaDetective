using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Attach this component to an NPC to show a world-space speech prompt when
/// the player is close and start a DialogueData conversation on Decide.
/// </summary>
[DisallowMultipleComponent]
public sealed class DialogueNPCInteractable : MonoBehaviour
{
    static readonly List<DialogueNPCInteractable> ActiveNPCs = new List<DialogueNPCInteractable>();

    [Header("Conversation")]
    [SerializeField] DialogueData conversation;
    [Tooltip("Optional registered conversation ID. Takes priority over the direct Conversation asset.")]
    [SerializeField] string conversationId;

    [Serializable]
    public sealed class RandomConversation
    {
        [Tooltip("Absolute chance in percent, rolled when starting a conversation.")]
        [Range(0f, 100f)] public float probabilityPercent = 10f;
        [Tooltip("Alternative DialogueData asset. Ignored when Conversation Id is set.")]
        public DialogueData conversation;
        [Tooltip("Optional registered conversation ID (takes precedence over the asset).")]
        public string conversationId;

        public bool IsAssigned =>
            conversation != null || !string.IsNullOrWhiteSpace(conversationId);
    }

    [Header("Random Conversations (optional)")]
    [Tooltip("Each entry has an absolute percentage. Their total must not exceed 100%; the remaining chance plays the default Conversation above.")]
    [SerializeField] List<RandomConversation> randomConversations = new List<RandomConversation>();

    [SerializeField] bool onlyOnce;
    [SerializeField] UnityEvent onDialogueFinished;
    [SerializeField] UnityEvent<int> onChoiceSelected;

    [Header("Interaction")]
    [SerializeField, Min(0.1f)] float interactionDistance = 2.5f;
    [Tooltip("Optional position used as the center of the interaction range. Defaults to this NPC.")]
    [SerializeField] Transform interactionPoint;

    [Header("Prompt above NPC")]
    [Tooltip("Optional head location. Otherwise the NPC transform plus Offset is used.")]
    [SerializeField] Transform promptAnchor;
    [SerializeField] Vector3 promptOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] Sprite promptSprite;
    [SerializeField, Min(0.01f)] float promptWorldHeight = 0.5f;
    [SerializeField] int sortingOrderOffset = 100;

    GameObject promptObject;
    SpriteRenderer promptRenderer;
    bool hasPlayed;

    void Reset()
    {
#if UNITY_EDITOR
        // This is an ordinary asset under "resource", NOT a Resources.Load path.
        // Saving a direct Sprite reference makes it work in player builds, too.
        promptSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/System/UI/Dialogue/resource/会話待機用コメント.PNG");
#endif
    }

    void OnValidate()
    {
        interactionDistance = Mathf.Max(0.1f, interactionDistance);
        promptWorldHeight = Mathf.Max(0.01f, promptWorldHeight);
        if (randomConversations != null)
            foreach (RandomConversation candidate in randomConversations)
                if (candidate != null)
                    candidate.probabilityPercent = Mathf.Clamp(candidate.probabilityPercent, 0f, 100f);
    }

    void Awake()
    {
        EnsurePrompt();
    }

    void OnEnable()
    {
        if (!ActiveNPCs.Contains(this)) ActiveNPCs.Add(this);
    }

    void OnDisable()
    {
        ActiveNPCs.Remove(this);
        SetPromptVisible(false);
    }

    void OnDestroy()
    {
        if (promptObject != null) Destroy(promptObject);
    }

    void LateUpdate()
    {
        bool available = IsAvailableFor(PlayerController.Instance);
        if (available) EnsurePrompt();
        SetPromptVisible(available);

        if (!available || promptObject == null || promptSprite == null) return;

        promptObject.transform.position =
            (promptAnchor != null ? promptAnchor.position : transform.position) + promptOffset;
        promptRenderer.sprite = promptSprite;
        float spriteHeight = Mathf.Max(0.001f, promptSprite.bounds.size.y);
        promptObject.transform.localScale = Vector3.one * (promptWorldHeight / spriteHeight);

        // Match the NPC's sorting layer, but keep the prompt in front.
        SpriteRenderer baseSprite = GetComponentInChildren<SpriteRenderer>();
        if (baseSprite != null)
        {
            promptRenderer.sortingLayerID = baseSprite.sortingLayerID;
            promptRenderer.sortingOrder = baseSprite.sortingOrder + sortingOrderOffset;
        }
    }

    void EnsurePrompt()
    {
        if (promptObject != null || promptSprite == null) return;
        promptObject = new GameObject(name + "_DialoguePrompt");
        promptRenderer = promptObject.AddComponent<SpriteRenderer>();
        promptRenderer.sprite = promptSprite;
        promptRenderer.enabled = true;
        promptObject.SetActive(false);
    }

    void SetPromptVisible(bool show)
    {
        if (promptObject != null && promptObject.activeSelf != show)
            promptObject.SetActive(show);
    }

    bool IsAvailableFor(PlayerController player)
    {
        if (player == null || !isActiveAndEnabled || !gameObject.activeInHierarchy) return false;
        if (onlyOnce && hasPlayed) return false;
        if (conversation == null && string.IsNullOrWhiteSpace(conversationId)) return false;

        DialogueUIController ui = DialogueUIController.Instance;
        if (ui == null || ui.IsPlaying || player.IsDialogueInputBlocked) return false;

        if (player.movetype == PlayerController.MoveType.Wait) return false;

        Vector2 playerPosition = player.transform.position;
        Vector2 npcPosition = interactionPoint != null
            ? (Vector2)interactionPoint.position : (Vector2)transform.position;
        return (playerPosition - npcPosition).sqrMagnitude <= interactionDistance * interactionDistance;
    }

    static DialogueNPCInteractable FindClosest(PlayerController player)
    {
        if (player == null) return null;
        DialogueNPCInteractable closest = null;
        float bestDistance = float.PositiveInfinity;
        Vector2 playerPosition = player.transform.position;
        foreach (DialogueNPCInteractable npc in ActiveNPCs)
        {
            if (npc == null || !npc.IsAvailableFor(player)) continue;
            Vector2 npcPosition = npc.interactionPoint != null
                ? (Vector2)npc.interactionPoint.position : (Vector2)npc.transform.position;
            float distance = (playerPosition - npcPosition).sqrMagnitude;
            if (distance >= bestDistance) continue;
            bestDistance = distance;
            closest = npc;
        }
        return closest;
    }

    public static bool HasAvailableNPC(PlayerController player)
    {
        return FindClosest(player) != null;
    }

    /// <returns>True only when a conversation was actually started.</returns>
    public static bool TryInteractClosest(PlayerController player)
    {
        if (player == null) return false;

        // Interaction is proximity-based. The mouse cursor does not need to
        // point at the NPC or any particular position on the screen.
        DialogueNPCInteractable closest = FindClosest(player);
        return closest != null && closest.TryStartDialogue(player);
    }

    public bool TryStartDialogue(PlayerController player)
    {
        if (!IsAvailableFor(player)) return false;

        DialogueUIController ui = DialogueUIController.Instance;
        Action finished = () => onDialogueFinished?.Invoke();
        Action<int, int> picked = (nodeId, optionIndex) => onChoiceSelected?.Invoke(optionIndex);

        // Roll only here, on actual interaction. Entering/leaving the area
        // and drawing the overhead prompt never consume a random roll.
        RandomConversation selected = ChooseRandomConversation();
        bool started = selected != null && PlayConversation(
            ui, selected.conversation, selected.conversationId, finished, picked);

        if (selected != null && !started)
            Debug.LogWarning("NPC dialogue: random conversation could not start. Trying the default conversation.", this);

        // If no alternative was selected (or it was invalid), keep the old
        // default conversation behavior unchanged.
        if (!started)
            started = PlayConversation(ui, conversation, conversationId, finished, picked);

        if (started)
        {
            hasPlayed = true;
            SetPromptVisible(false);
        }
        return started;
    }

    static bool PlayConversation(DialogueUIController ui, DialogueData asset,
        string id, Action finished, Action<int, int> picked)
    {
        return !string.IsNullOrWhiteSpace(id)
            ? ui.PlayById(id, finished, picked)
            : ui.Play(asset, finished, picked);
    }

    RandomConversation ChooseRandomConversation()
    {
        if (randomConversations == null || randomConversations.Count == 0)
            return null;

        float total = 0f;
        foreach (RandomConversation candidate in randomConversations)
        {
            if (candidate == null || !candidate.IsAssigned) continue;
            total += Mathf.Clamp(candidate.probabilityPercent, 0f, 100f);
        }

        // Never silently normalize percentages: the Inspector displays the
        // exact odds and requires the total to be at most 100%.
        if (total > 100.0001f)
        {
            Debug.LogWarning("NPC dialogue: random conversation probabilities exceed 100%. " +
                "Using the default conversation until they are corrected.", this);
            return null;
        }

        // Integer Range excludes the upper bound, so a configured 100% chance
        // is guaranteed rather than rarely falling through on Random.value == 1.
        float roll = UnityEngine.Random.Range(0, 1000000) / 10000f;
        float accumulated = 0f;
        foreach (RandomConversation candidate in randomConversations)
        {
            if (candidate == null || !candidate.IsAssigned) continue;
            float chance = Mathf.Clamp(candidate.probabilityPercent, 0f, 100f);
            if (chance <= 0f) continue;
            accumulated += chance;
            if (roll < accumulated)
                return candidate;
        }
        return null; // The remaining percentage uses the default conversation.
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.95f, 0.65f, 0.9f);
        Vector3 center = interactionPoint != null ? interactionPoint.position : transform.position;
        Gizmos.DrawWireSphere(center, interactionDistance);
    }
}
