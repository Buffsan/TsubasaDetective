using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Add this to an event object; call StartDialogue from a UnityEvent or script.
/// Optional trigger / play-on-start behaviour is provided for testing and stages.
/// </summary>
public class DialogueEventTrigger : MonoBehaviour
{
    [SerializeField] DialogueData dialogue;
    [SerializeField] bool playOnStart;
    [SerializeField] bool playWhenPlayerEntersTrigger;
    [SerializeField] bool onlyOnce = true;
    [SerializeField] UnityEvent onDialogueFinished;

    bool hasPlayed;

    void Start()
    {
        if (playOnStart)
            StartDialogue();
    }

    [ContextMenu("Start Dialogue")]
    public void StartDialogue()
    {
        if (onlyOnce && hasPlayed)
            return;

        DialogueUIController ui = DialogueUIController.Instance;
        if (ui == null)
        {
            Debug.LogWarning("DialogueEventTrigger: DialogueCanvas is missing from the scene.", this);
            return;
        }

        if (ui.Play(dialogue, () => onDialogueFinished?.Invoke()))
            hasPlayed = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (playWhenPlayerEntersTrigger && other.CompareTag("Player"))
            StartDialogue();
    }
}
