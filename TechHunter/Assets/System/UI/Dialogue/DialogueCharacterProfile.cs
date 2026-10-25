using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "TechHunter/Dialogue/Character")]
public class DialogueCharacterProfile : ScriptableObject
{
    [Serializable] public class ExtraPortrait
    {
        public string key;
        public Sprite image;
    }

    public DialogueCharacterId id;
    public string displayName;
    public bool flipHorizontally;
    [Range(0.5f, 2f)] public float scale = 1f;
    [Header("Expressions")]
    public Sprite normal, happy, angry, crying, fear, confused, laughing;
    public Sprite surprised, sad, embarrassed, thinking;
    public List<ExtraPortrait> others = new List<ExtraPortrait>();

    public Sprite GetSprite(DialogueExpression expression, string key)
    {
        Sprite sprite = null;
        switch (expression)
        {
            case DialogueExpression.Happy: sprite = happy; break;
            case DialogueExpression.Angry: sprite = angry; break;
            case DialogueExpression.Crying: sprite = crying; break;
            case DialogueExpression.Fear: sprite = fear; break;
            case DialogueExpression.Confused: sprite = confused; break;
            case DialogueExpression.Laughing: sprite = laughing; break;
            case DialogueExpression.Surprised: sprite = surprised; break;
            case DialogueExpression.Sad: sprite = sad; break;
            case DialogueExpression.Embarrassed: sprite = embarrassed; break;
            case DialogueExpression.Thinking: sprite = thinking; break;
            case DialogueExpression.Other:
                foreach (ExtraPortrait p in others)
                    if (p != null && p.key == key) { sprite = p.image; break; }
                break;
        }
        return sprite != null ? sprite : normal;
    }
}
