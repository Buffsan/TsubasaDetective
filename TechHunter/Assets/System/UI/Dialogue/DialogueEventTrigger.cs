using UnityEngine;
using UnityEngine.Events;

public class DialogueEventTrigger : MonoBehaviour
{
    [SerializeField] DialogueData dialogue;
    [Tooltip("When set, look up the conversation by ID; otherwise use the direct asset.")]
    [SerializeField] string dialogueId;
    [SerializeField] bool playOnStart;
    [SerializeField] bool playWhenPlayerEntersTrigger;
    [SerializeField] bool onlyOnce = true;
    [SerializeField] UnityEvent onDialogueFinished;
    [SerializeField] UnityEvent<int> onChoiceSelected;
    bool hasPlayed;

    void Start() { if (playOnStart) StartDialogue(); }

    [ContextMenu("Start Dialogue")]
    public void StartDialogue()
    {
        if (onlyOnce && hasPlayed) return;
        DialogueUIController controller = DialogueUIController.Instance;
        if (controller == null)
        {
            Debug.LogWarning("DialogueCanvas is missing from the scene.", this);
            return;
        }
        System.Action finished = () => onDialogueFinished?.Invoke();
        System.Action<int,int> picked = (nodeId, optionIndex) => onChoiceSelected?.Invoke(optionIndex);
        bool started = !string.IsNullOrWhiteSpace(dialogueId)
            ? controller.PlayById(dialogueId, finished, picked)
            : controller.Play(dialogue, finished, picked);
        if (started) hasPlayed = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (playWhenPlayerEntersTrigger && other.CompareTag("Player")) StartDialogue();
    }
}
