using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueCharacterDatabase", menuName = "TechHunter/Dialogue/Character Database")]
public class DialogueCharacterDatabase : ScriptableObject
{
    public List<DialogueCharacterProfile> profiles = new List<DialogueCharacterProfile>();
    public DialogueCharacterProfile Find(DialogueCharacterId id)
    {
        if (id == DialogueCharacterId.None) return null;
        foreach (DialogueCharacterProfile character in profiles)
            if (character != null && character.id == id) return character;
        return null;
    }
    void OnValidate()
    {
        var registered = new HashSet<DialogueCharacterId>();
        foreach (DialogueCharacterProfile c in profiles)
            if (c != null && (c.id == DialogueCharacterId.None || !registered.Add(c.id)))
                Debug.LogWarning("Dialogue registry: missing or duplicate character ID " + c.id, this);
    }
}
