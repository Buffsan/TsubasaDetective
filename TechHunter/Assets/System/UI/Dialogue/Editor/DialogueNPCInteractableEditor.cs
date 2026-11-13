#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Keeps the existing NPC Inspector intact and adds an explicit probability
// summary, so designers don't have to calculate the default chance by hand.
[CustomEditor(typeof(DialogueNPCInteractable))]
public sealed class DialogueNPCInteractableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        serializedObject.Update();
        SerializedProperty alternatives = serializedObject.FindProperty("randomConversations");
        SerializedProperty defaultConversation = serializedObject.FindProperty("conversation");
        SerializedProperty defaultId = serializedObject.FindProperty("conversationId");
        if (alternatives == null || defaultConversation == null || defaultId == null)
            return;

        float total = 0f;
        int incompleteCount = 0;
        for (int i = 0; i < alternatives.arraySize; i++)
        {
            SerializedProperty entry = alternatives.GetArrayElementAtIndex(i);
            float probability = entry.FindPropertyRelative("probabilityPercent").floatValue;
            bool hasAsset = entry.FindPropertyRelative("conversation").objectReferenceValue != null;
            string id = entry.FindPropertyRelative("conversationId").stringValue;
            bool hasConversation = hasAsset || !string.IsNullOrWhiteSpace(id);

            if (!hasConversation && probability > 0f)
                incompleteCount++;
            if (hasConversation)
                total += Mathf.Clamp(probability, 0f, 100f);
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Random Conversation Chances", EditorStyles.boldLabel);

        bool hasDefault = defaultConversation.objectReferenceValue != null ||
            !string.IsNullOrWhiteSpace(defaultId.stringValue);
        if (!hasDefault)
            EditorGUILayout.HelpBox("通常会話が未設定です。Conversation または Conversation Id を設定してください。", MessageType.Error);

        if (incompleteCount > 0)
            EditorGUILayout.HelpBox("確率が設定されているのに会話SO/IDが未指定の候補が " +
                incompleteCount + " 件あります。未設定候補は抽選から除外されます。", MessageType.Warning);

        if (total > 100.0001f)
        {
            EditorGUILayout.HelpBox("ランダム会話の確率合計が " + total.ToString("0.##") +
                "% で100%を超えています。超過中はランダム抽選を行わず通常会話を使用します。",
                MessageType.Error);
        }
        else
        {
            EditorGUILayout.HelpBox("別会話の合計: " + total.ToString("0.##") +
                "% ／ 通常会話: " + Mathf.Max(0f, 100f - total).ToString("0.##") +
                "%。会話を開始するたびに1回抽選します。", MessageType.Info);
        }
    }
}
#endif
