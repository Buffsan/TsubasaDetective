using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueDatabase", menuName = "TechHunter/Dialogue/Conversation Database")]
public class DialogueDatabase : ScriptableObject
{
    [Tooltip("Only registered conversations can be started by ID.")]
    public List<DialogueData> conversations = new List<DialogueData>();

    public DialogueData Find(string conversationId)
    {
        if (string.IsNullOrWhiteSpace(conversationId)) return null;
        foreach (DialogueData conversation in conversations)
            if (conversation != null && conversation.conversationId == conversationId)
                return conversation;
        return null;
    }

    void OnValidate()
    {
        HashSet<string> ids = new HashSet<string>();
        foreach (DialogueData conversation in conversations)
        {
            if (conversation == null) continue;
            if (string.IsNullOrWhiteSpace(conversation.conversationId) || !ids.Add(conversation.conversationId))
                Debug.LogWarning("Dialogue registry: missing or duplicate conversation ID on " + conversation.name, this);
        }
    }
}
