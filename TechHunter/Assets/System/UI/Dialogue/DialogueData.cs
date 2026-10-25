using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "TechHunter/Dialogue/Conversation")]
public class DialogueData : ScriptableObject
{
    [Serializable]
    public class Character
    {
        public string displayName;
        public Sprite portrait;
        public bool flipHorizontally;
        [Range(0.5f, 2f)] public float scale = 1f;
    }

    [Serializable]
    public class Line
    {
        [Tooltip("Index of characters above. -1 means narration (no portrait highlighted).")]
        public int speakerIndex = 0;
        public bool showSpeakerName = true;
        [TextArea(2, 7)] public string text;
    }

    [Tooltip("Portraits appear from left to right in this order.")]
    public List<Character> characters = new List<Character>();
    public List<Line> lines = new List<Line>();
}
