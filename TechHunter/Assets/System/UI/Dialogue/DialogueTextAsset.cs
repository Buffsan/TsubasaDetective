using UnityEngine;

[CreateAssetMenu(fileName = "DialogueText", menuName = "TechHunter/Dialogue/Text")]
public class DialogueTextAsset : ScriptableObject
{
    [TextArea(2, 12)] public string text;
}
