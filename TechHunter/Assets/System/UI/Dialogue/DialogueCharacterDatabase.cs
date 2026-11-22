using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueCharacterDatabase", menuName = "TechHunter/Dialogue/Character Database")]
public class DialogueCharacterDatabase : ScriptableObject
{
    public List<DialogueCharacterProfile> profiles = new List<DialogueCharacterProfile>();

    // Increasing counter is independent of list order; existing IDs remain stable.
    [SerializeField, HideInInspector] int nextCharacterId = 1;

    public DialogueCharacterProfile Find(int id)
    {
        if (id <= 0) return null;
        foreach (DialogueCharacterProfile character in profiles)
            if (character != null && character.id == id) return character;
        return null;
    }

    // Only the editor registry allocates IDs. Zero means "None".
    public int AllocateId()
    {
        int candidate = Mathf.Max(1, nextCharacterId);
        foreach (DialogueCharacterProfile character in profiles)
            if (character != null && character.id >= candidate)
                candidate = character.id + 1;

        nextCharacterId = candidate + 1;
        return candidate;
    }

    void OnValidate()
    {
        HashSet<int> ids = new HashSet<int>();
        HashSet<DialogueCharacterProfile> registered = new HashSet<DialogueCharacterProfile>();
        foreach (DialogueCharacterProfile character in profiles)
        {
            if (character == null) continue;
            if (!registered.Add(character))
                Debug.LogWarning("Dialogue registry: profile registered more than once: " + character.name, this);
            else if (character.id <= 0 || !ids.Add(character.id))
                Debug.LogWarning("Dialogue registry: missing or duplicate ID for " + character.name +
                    ". Open SharedCharacters and use Repair IDs.", this);
        }
    }
}
