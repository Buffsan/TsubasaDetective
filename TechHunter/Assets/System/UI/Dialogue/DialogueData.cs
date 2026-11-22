using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "TechHunter/Dialogue/Conversation")]
public class DialogueData : ScriptableObject
{
    [Tooltip("Stable unique ID used to start this conversation from game events.")]
    public string conversationId;

    // Keep Line=0 and Choice=1 for serialized conversations already in the project.
    public enum NodeType { Line = 0, Choice = 1, Branch = 2, Purchase = 3, End = 4 }

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
        [DialogueCharacterId] public int speakerId;
        public DialogueExpression expression;
        public string otherExpressionKey;
        public bool showSpeakerName = true;
        [TextArea(2, 8)] public string text;
        [Tooltip("End this conversation when the player advances after reading this node.")]
        public bool endDialogue;
        [Tooltip("-1 = next node in the array (NOT conversation end). To finish here, check End Dialogue; Next Node ID is ignored when End Dialogue is checked.")]
        public int nextNodeId = -1;
        [Tooltip("Condition key for a Branch node; true follows Next Node, false follows False Node.")]
        public string conditionKey;
        [Tooltip("False branch destination, also used for insufficient funds on Purchase nodes.")]
        public int falseNodeId = -1;
        [Tooltip("Position in the node-based dialogue graph.")]
        public Vector2 graphPosition;
        public List<Choice> choices = new List<Choice>();
    }

    [Header("Character IDs, in their left-to-right display order")]
    [DialogueCharacterId] public List<int> participants = new List<int>();
    [Tooltip("-1 = start at the first node.")]
    public int entryNodeId = -1;
    public List<Node> nodes = new List<Node>();

    // Version 0 conversations used -1 as an implicit array-next transition.
    // Version 1 uses only explicit graph links. The graph editor migrates version 0.
    [HideInInspector] public int graphVersion;
    [HideInInspector] public Vector2 graphStartPosition = new Vector2(-420, 0);

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
