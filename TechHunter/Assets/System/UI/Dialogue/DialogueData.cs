using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "TechHunter/Dialogue/Conversation")]
public class DialogueData : ScriptableObject
{
    [Tooltip("Stable unique ID used to start this conversation from game events.")]
    public string conversationId;

    public enum NodeType { Line, Choice }

    [Serializable] public class Choice
    {
        public string label;
        [Tooltip("Destination node ID. Can target the same final node from different branches.")]
        public int targetNodeId;
        [Tooltip("Optional bool variable. If false, use falseTargetNodeId instead of targetNodeId.")]
        public string conditionKey;
        [Tooltip("Destination node ID when conditionKey is false.")]
        public int falseTargetNodeId;
        public DialogueShopItemData selectedShopItem;
    }

    [Serializable] public class Node
    {
        public int nodeId;
        public NodeType type;
        public DialogueCharacterId speakerId = DialogueCharacterId.None;
        public DialogueExpression expression;
        public string otherExpressionKey;
        public bool showSpeakerName = true;
        [TextArea(2, 8)] public string text;
        [Tooltip("End this conversation when the player advances after reading this node.")]
        public bool endDialogue;
        [Tooltip("-1 = next entry in Nodes array, otherwise jump to this node ID.")]
        public int nextNodeId = -1;
        public List<Choice> choices = new List<Choice>();
    }

    [Header("Character IDs, in their left-to-right display order")]
    public List<DialogueCharacterId> participants = new List<DialogueCharacterId>();
    [Tooltip("-1 = start at the first node.")]
    public int entryNodeId = -1;
    public List<Node> nodes = new List<Node>();

    // Old assets from the first version of this feature remain usable.
    [Serializable] public class Character
    {
        public string displayName;
        public Sprite portrait;
        public bool flipHorizontally;
        [Range(0.5f, 2f)] public float scale = 1f;
    }
    [Serializable] public class Line
    {
        public int speakerIndex;
        public bool showSpeakerName = true;
        [TextArea(2, 7)] public string text;
    }
    [HideInInspector] public List<Character> characters = new List<Character>();
    [HideInInspector] public List<Line> lines = new List<Line>();
}
