using UnityEngine;

// Allows dialogue character IDs to be displayed as registered names in Inspector.
public sealed class DialogueCharacterIdAttribute : PropertyAttribute
{
}

public enum DialogueExpression
{
    Normal, Happy, Angry, Crying, Fear, Confused, Laughing, Other,
    Surprised, Sad, Embarrassed, Thinking
}
