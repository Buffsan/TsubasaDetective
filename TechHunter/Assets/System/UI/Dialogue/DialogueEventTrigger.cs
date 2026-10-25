using UnityEngine;
using UnityEngine.Events;

public class DialogueEventTrigger : MonoBehaviour
{
    [SerializeField] DialogueData dialogue;
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
        if (controller.Play(dialogue, () => onDialogueFinished?.Invoke(),
            (nodeId, optionIndex) => onChoiceSelected?.Invoke(optionIndex)))
            hasPlayed = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (playWhenPlayerEntersTrigger && other.CompareTag("Player")) StartDialogue();
    }
}
