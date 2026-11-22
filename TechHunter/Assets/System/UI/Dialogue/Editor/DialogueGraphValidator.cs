#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Checks the exact edges that the dialogue runtime will follow.</summary>
public static class DialogueGraphValidator
{
    public static List<string> Check(DialogueData data)
    {
        List<string> issues = new List<string>();
        if (data == null) { issues.Add("会話データがありません。"); return issues; }
        if (data.nodes == null || data.nodes.Count == 0)
        {
            issues.Add("会話ノードがありません。");
            return issues;
        }

        Dictionary<int, DialogueData.Node> map = new Dictionary<int, DialogueData.Node>();
        foreach (DialogueData.Node node in data.nodes)
        {
            if (node == null) { issues.Add("null のノードがあります。"); continue; }
            if (map.ContainsKey(node.nodeId))
                issues.Add("重複ノードID: " + node.nodeId);
            else map.Add(node.nodeId, node);
        }

        if (!map.ContainsKey(data.entryNodeId))
            issues.Add("STARTから開始ノードへの接続がありません。");

        foreach (DialogueData.Node node in map.Values)
        {
            string prefix = "#" + node.nodeId + " ";
            if (node.type == DialogueData.NodeType.Line)
            {
                if (node.endDialogue) continue;
                CheckTarget(node.nextNodeId, map, issues, prefix + "次");
            }
            else if (node.type == DialogueData.NodeType.Choice)
            {
                if (node.choices == null || node.choices.Count == 0)
                {
                    issues.Add(prefix + "選択肢がありません。");
                    continue;
                }
                for (int i = 0; i < node.choices.Count; i++)
                {
                    DialogueData.Choice choice = node.choices[i];
                    if (choice == null) { issues.Add(prefix + "null の選択肢があります。"); continue; }
                    string p = prefix + "選択 " + (i + 1);
                    if (string.IsNullOrWhiteSpace(choice.label))
                        issues.Add(p + " の表示文が空です。");
                    CheckTarget(choice.targetNodeId, map, issues, p + " True");
                    if (!string.IsNullOrWhiteSpace(choice.conditionKey))
                        CheckTarget(choice.falseTargetNodeId, map, issues, p + " False");
                    else if (choice.falseTargetNodeId >= 0)
                        issues.Add(p + " に条件キーがないためFalse側は使われません。");
                }
            }
            else if (node.type == DialogueData.NodeType.Branch)
            {
                if (string.IsNullOrWhiteSpace(node.conditionKey))
                    issues.Add(prefix + "条件変数キーが未設定です。");
                CheckTarget(node.nextNodeId, map, issues, prefix + "True");
                CheckTarget(node.falseNodeId, map, issues, prefix + "False");
            }
            else if (node.type == DialogueData.NodeType.Purchase)
            {
                CheckTarget(node.nextNodeId, map, issues, prefix + "購入成功");
                CheckTarget(node.falseNodeId, map, issues, prefix + "購入失敗");
            }
            else if (node.type != DialogueData.NodeType.End)
                issues.Add(prefix + "不明なノード種類です。");
        }

        if (map.ContainsKey(data.entryNodeId))
        {
            HashSet<int> visited = new HashSet<int>();
            Stack<int> pending = new Stack<int>();
            pending.Push(data.entryNodeId);
            while (pending.Count > 0)
            {
                int id = pending.Pop();
                DialogueData.Node node;
                if (!visited.Add(id) || !map.TryGetValue(id, out node)) continue;
                foreach (int target in ReachableEdges(node))
                    if (target >= 0 && !visited.Contains(target)) pending.Push(target);
            }
            foreach (int id in map.Keys)
                if (!visited.Contains(id))
                    issues.Add("到達できないノード: #" + id);
            if (!visited.Any(id => map.ContainsKey(id) && (map[id].type == DialogueData.NodeType.End || map[id].endDialogue)))
                issues.Add("開始ノードから到達できる終了ノードがありません。");
        }
        return issues;
    }

    static void CheckTarget(int id, Dictionary<int, DialogueData.Node> map,
        List<string> issues, string label)
    {
        if (id < 0) issues.Add(label + " の接続先がありません。");
        else if (!map.ContainsKey(id)) issues.Add(label + " の接続先 #" + id + " は存在しません。");
    }

    static IEnumerable<int> ReachableEdges(DialogueData.Node node)
    {
        if (node.endDialogue || node.type == DialogueData.NodeType.End) yield break;
        if (node.type == DialogueData.NodeType.Choice)
        {
            if (node.choices == null) yield break;
            foreach (DialogueData.Choice choice in node.choices)
            {
                if (choice == null) continue;
                yield return choice.targetNodeId;
                if (!string.IsNullOrWhiteSpace(choice.conditionKey))
                    yield return choice.falseTargetNodeId;
            }
        }
        else
        {
            yield return node.nextNodeId;
            if (node.type == DialogueData.NodeType.Branch ||
                node.type == DialogueData.NodeType.Purchase)
                yield return node.falseNodeId;
        }
    }
}

[CustomEditor(typeof(DialogueData))]
public sealed class DialogueDataGraphInspector : Editor
{
    bool showRaw;

    public override void OnInspectorGUI()
    {
        DialogueData data = (DialogueData)target;
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("conversationId"),
            new GUIContent("会話ID"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("participants"),
            new GUIContent("表示キャラクター（左から順）"), true);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(10);
        if (GUILayout.Button("会話グラフを開く", GUILayout.Height(36)))
            DialogueGraphEditorWindow.Open(data);

        EditorGUILayout.LabelField("ノード数", data.nodes != null ? data.nodes.Count.ToString() : "0");
        List<string> issues = data.graphVersion >= 1 ? DialogueGraphValidator.Check(data) :
            new List<string> { "初回グラフ編集時に旧形式から自動変換されます。" };

        if (issues.Count == 0)
            EditorGUILayout.HelpBox("接続検証: OK", MessageType.Info);
        else
        {
            EditorGUILayout.HelpBox("検証: " + issues.Count + " 件の警告/エラー", MessageType.Warning);
            foreach (string issue in issues.Take(8))
                EditorGUILayout.LabelField("・" + issue, EditorStyles.wordWrappedLabel);
            if (issues.Count > 8) EditorGUILayout.LabelField("ほか " + (issues.Count - 8) + " 件");
        }

        EditorGUILayout.Space(8);
        showRaw = EditorGUILayout.Foldout(showRaw, "高度な設定（ノード生データ）");
        if (showRaw)
        {
            EditorGUILayout.HelpBox("直接Node IDを変更すると既存の接続が切れる可能性があります。通常はグラフで編集してください。", MessageType.Warning);
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("entryNodeId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nodes"), true);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
