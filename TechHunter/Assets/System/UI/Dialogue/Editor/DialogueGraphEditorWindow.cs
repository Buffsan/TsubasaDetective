#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

// Animator-style visual editor. Links and positions are stored directly in
// DialogueData, so no extra asset or runtime graph package is required.
public sealed class DialogueGraphEditorWindow : EditorWindow
{
    [SerializeField] DialogueData conversation;
    DialogueGraphView graph;
    ObjectField assetPicker;
    bool refreshQueued;

    [MenuItem("Tools/TechHunter/Dialogue/Open Graph Editor")]
    public static void OpenWindow()
    {
        GetWindow<DialogueGraphEditorWindow>("Dialogue Graph");
    }

    public static void Open(DialogueData asset)
    {
        DialogueGraphEditorWindow window = GetWindow<DialogueGraphEditorWindow>("Dialogue Graph");
        window.SetConversation(asset);
        window.Focus();
    }

    [OnOpenAsset(1)]
    static bool OnOpenConversation(int instanceId, int line)
    {
        DialogueData data = EditorUtility.InstanceIDToObject(instanceId) as DialogueData;
        if (data == null) return false;
        Open(data);
        return true;
    }

    void OnEnable()
    {
        Undo.undoRedoPerformed += OnUndoRedo;
        BuildUI();
    }

    void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedo;
        refreshQueued = false;
    }

    void OnUndoRedo() { QueueReload(); }

    void BuildUI()
    {
        rootVisualElement.Clear();
        rootVisualElement.style.flexDirection = FlexDirection.Column;

        Toolbar toolbar = new Toolbar();
        assetPicker = new ObjectField("会話データ")
        {
            objectType = typeof(DialogueData),
            allowSceneObjects = false
        };
        assetPicker.style.minWidth = 290;
        assetPicker.style.flexGrow = 1;
        assetPicker.SetValueWithoutNotify(conversation);
        assetPicker.RegisterValueChangedCallback(e => SetConversation(e.newValue as DialogueData));
        toolbar.Add(assetPicker);
        toolbar.Add(new ToolbarButton(() => CreateConversation()) { text = "新規作成" });
        toolbar.Add(new ToolbarButton(() => RegisterConversation(conversation)) { text = "ID登録" });
        toolbar.Add(new ToolbarButton(() => Validate()) { text = "接続を検証" });
        toolbar.Add(new ToolbarButton(() => AssetDatabase.SaveAssets()) { text = "保存" });
        toolbar.Add(new ToolbarButton(() => { if (graph != null) graph.FrameAll(); }) { text = "全体表示" });
        rootVisualElement.Add(toolbar);

        graph = new DialogueGraphView(this);
        graph.style.flexGrow = 1;
        rootVisualElement.Add(graph);

        if (conversation != null)
        {
            UpgradeLegacyGraph();
            graph.Reload();
        }
    }

    void CreateConversation()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create Conversation", "Dialogue", "asset", "Choose a location for the DialogueData asset.");
        if (string.IsNullOrEmpty(path)) return;
        DialogueData asset = CreateInstance<DialogueData>();
        asset.graphVersion = 1;
        asset.conversationId = System.IO.Path.GetFileNameWithoutExtension(path);
        AssetDatabase.CreateAsset(asset, path);
        RegisterConversation(asset);
        AssetDatabase.SaveAssets();
        SetConversation(asset);
        Selection.activeObject = asset;
    }

    static void RegisterConversation(DialogueData asset)
    {
        if (asset == null) return;
        const string path = "Assets/System/UI/Dialogue/SharedConversations.asset";
        DialogueDatabase registry = AssetDatabase.LoadAssetAtPath<DialogueDatabase>(path);
        if (registry == null)
        {
            Debug.LogWarning("Dialogue graph: SharedConversations.asset not found. Register manually.", asset);
            return;
        }
        DialogueData sameId = registry.Find(asset.conversationId);
        if (sameId != null && sameId != asset)
        {
            Debug.LogError("Dialogue graph: conversation ID '" + asset.conversationId +
                "' is already registered to " + sameId.name + ".", asset);
            return;
        }
        if (registry.conversations.Contains(asset)) return;
        Undo.RecordObject(registry, "Register dialogue conversation");
        registry.conversations.Add(asset);
        EditorUtility.SetDirty(registry);
    }

    public void SetConversation(DialogueData data)
    {
        conversation = data;
        if (assetPicker != null) assetPicker.SetValueWithoutNotify(data);
        if (conversation != null) UpgradeLegacyGraph();
        if (graph != null) graph.Reload();
    }

    // Convert implicit array-next links to explicit edges once, without
    // destroying existing serialized IDs, choices, expressions or text.
    void UpgradeLegacyGraph()
    {
        if (conversation == null || conversation.graphVersion >= 1) return;
        Undo.RecordObject(conversation, "Upgrade dialogue to visual graph");
        if (conversation.nodes == null) conversation.nodes = new List<DialogueData.Node>();
        for (int i = 0; i < conversation.nodes.Count; i++)
        {
            DialogueData.Node node = conversation.nodes[i];
            if (node == null) continue;
            if (node.endDialogue && node.type == DialogueData.NodeType.Line)
                node.type = DialogueData.NodeType.End;
            if (node.type == DialogueData.NodeType.Line && node.nextNodeId < 0 &&
                i + 1 < conversation.nodes.Count)
                node.nextNodeId = conversation.nodes[i + 1].nodeId;
            if (node.graphPosition == Vector2.zero)
                node.graphPosition = new Vector2(130 + (i % 3) * 440, (i / 3) * 400);
        }
        if (conversation.entryNodeId < 0 && conversation.nodes.Count > 0)
            conversation.entryNodeId = conversation.nodes[0].nodeId;
        conversation.graphVersion = 1;
        EditorUtility.SetDirty(conversation);
    }

    public DialogueData Data { get { return conversation; } }

    public void AddNode(DialogueData.NodeType type, Vector2 position)
    {
        if (conversation == null) return;
        Undo.RecordObject(conversation, "Add dialogue graph node");
        int id = 0;
        foreach (DialogueData.Node node in conversation.nodes)
            if (node != null) id = Mathf.Max(id, node.nodeId + 1);
        DialogueData.Node created = new DialogueData.Node
        {
            nodeId = id,
            type = type,
            graphPosition = position,
            nextNodeId = -1,
            falseNodeId = -1
        };
        if (type == DialogueData.NodeType.Choice)
            created.choices.Add(new DialogueData.Choice
                { label = "選択肢 1", targetNodeId = -1, falseTargetNodeId = -1 });
        if (type == DialogueData.NodeType.End) created.endDialogue = true;
        conversation.nodes.Add(created);
        EditorUtility.SetDirty(conversation);
        QueueReload();
    }

    public void QueueReload()
    {
        if (refreshQueued) return;
        refreshQueued = true;
        EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            refreshQueued = false;
            if (graph != null) graph.Reload();
            Repaint();
        };
    }

    void Validate()
    {
        if (conversation == null)
        {
            EditorUtility.DisplayDialog("Dialogue Graph", "会話データを選択してください。", "OK");
            return;
        }
        List<string> issues = DialogueGraphValidator.Check(conversation);
        string report = issues.Count == 0
            ? "ノードの接続に問題は見つかりませんでした。"
            : string.Join("\n", issues.Take(22).ToArray());
        if (issues.Count > 22) report += "\n... ほか " + (issues.Count - 22) + " 件";
        EditorUtility.DisplayDialog("Dialogue Graph 検証", report, "OK");
    }
}

sealed class DialogueGraphView : GraphView
{
    sealed class LinkInfo
    {
        public int nodeId;
        public int option = -1;
        public bool isFalse;
        public bool isStart;
        public string Key { get { return isStart ? "Start" :
            nodeId + "/" + option + "/" + (isFalse ? "False" : "True"); } }
    }

    readonly DialogueGraphEditorWindow window;
    readonly Dictionary<int, Port> inputs = new Dictionary<int, Port>();
    readonly Dictionary<string, Port> outputs = new Dictionary<string, Port>();
    bool rebuilding;

    public DialogueGraphView(DialogueGraphEditorWindow window)
    {
        this.window = window;
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        Insert(0, new GridBackground());
        AddManipulator(new ContentDragger());
        AddManipulator(new SelectionDragger());
        AddManipulator(new RectangleSelector());
        graphViewChanged = OnGraphChanged;
        style.flexGrow = 1;
    }

    public override List<Port> GetCompatiblePorts(Port start, NodeAdapter adapter)
    {
        return ports.ToList().Where(p => p != start && p.direction != start.direction &&
            p.node != start.node).ToList();
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        base.BuildContextualMenu(evt);
        if (window.Data == null) return;
        Vector2 local = contentViewContainer.WorldToLocal(evt.mousePosition);
        evt.menu.AppendAction("追加/セリフ", _ => window.AddNode(DialogueData.NodeType.Line, local));
        evt.menu.AppendAction("追加/選択肢", _ => window.AddNode(DialogueData.NodeType.Choice, local));
        evt.menu.AppendAction("追加/条件分岐", _ => window.AddNode(DialogueData.NodeType.Branch, local));
        evt.menu.AppendAction("追加/購入処理", _ => window.AddNode(DialogueData.NodeType.Purchase, local));
        evt.menu.AppendAction("追加/終了", _ => window.AddNode(DialogueData.NodeType.End, local));
    }

    public void Reload()
    {
        rebuilding = true;
        foreach (GraphElement element in graphElements.ToList())
            RemoveElement(element);
        inputs.Clear();
        outputs.Clear();

        DialogueData data = window.Data;
        if (data != null)
        {
            CreateStart(data);
            foreach (DialogueData.Node node in data.nodes)
                if (node != null) CreateNode(node);
            foreach (Port output in outputs.Values)
            {
                LinkInfo info = output.userData as LinkInfo;
                if (info == null) continue;
                int destination = GetDestination(data, info);
                Port input;
                if (!inputs.TryGetValue(destination, out input)) continue;
                Edge edge = output.ConnectTo(input);
                AddElement(edge);
            }
        }
        rebuilding = false;
    }

    void CreateStart(DialogueData data)
    {
        Node start = new Node { title = "START / 会話開始" };
        start.userData = -1;
        start.capabilities &= ~Capabilities.Deletable;
        start.style.width = 220;
        start.titleContainer.style.backgroundColor = new Color(0.15f, .43f, .29f);
        start.SetPosition(new Rect(data.graphStartPosition, new Vector2(220, 85)));
        AddOutput(start, "開始 →", new LinkInfo { isStart = true });
        start.RefreshPorts();
        AddElement(start);
    }

    void CreateNode(DialogueData.Node data)
    {
        Node view = new Node { title = MakeTitle(data) };
        view.userData = data.nodeId;
        view.style.width = 340;
        if (data.type == DialogueData.NodeType.Choice)
            view.titleContainer.style.backgroundColor = new Color(.22f, .31f, .52f);
        else if (data.type == DialogueData.NodeType.Branch)
            view.titleContainer.style.backgroundColor = new Color(.45f, .32f, .13f);
        else if (data.type == DialogueData.NodeType.Purchase)
            view.titleContainer.style.backgroundColor = new Color(.4f, .25f, .12f);
        else if (data.type == DialogueData.NodeType.End)
            view.titleContainer.style.backgroundColor = new Color(.43f, .18f, .24f);

        view.SetPosition(new Rect(data.graphPosition, new Vector2(340, 240)));
        Port input = view.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        input.portName = "入力";
        inputContainerAdd(view, input);
        inputs[data.nodeId] = input;

        if (data.type == DialogueData.NodeType.Line)
            AddOutput(view, "次へ →", new LinkInfo { nodeId = data.nodeId });
        else if (data.type == DialogueData.NodeType.Branch ||
                 data.type == DialogueData.NodeType.Purchase)
        {
            AddOutput(view, data.type == DialogueData.NodeType.Purchase ? "購入成功 →" : "True →",
                new LinkInfo { nodeId = data.nodeId });
            AddOutput(view, data.type == DialogueData.NodeType.Purchase ? "残高不足 →" : "False →",
                new LinkInfo { nodeId = data.nodeId, isFalse = true });
        }
        else if (data.type == DialogueData.NodeType.Choice && data.choices != null)
        {
            for (int i = 0; i < data.choices.Count; i++)
            {
                AddOutput(view, "選択 " + (i + 1) + " →",
                    new LinkInfo { nodeId = data.nodeId, option = i });
                AddOutput(view, "選択 " + (i + 1) + " 条件False →",
                    new LinkInfo { nodeId = data.nodeId, option = i, isFalse = true });
            }
        }

        int id = data.nodeId;
        IMGUIContainer inspector = new IMGUIContainer(() => DrawProperties(id, view));
        inspector.style.paddingLeft = 8;
        inspector.style.paddingRight = 8;
        inspector.style.paddingTop = 5;
        inspector.style.paddingBottom = 7;
        view.extensionContainer.Add(inspector);
        view.RefreshExpandedState();
        view.RefreshPorts();
        AddElement(view);
    }

    // Keep this separate from Node.data so ports and their connections remain stable.
    static void inputContainerAdd(Node node, Port port) { node.inputContainer.Add(port); }

    static string MakeTitle(DialogueData.Node n)
    {
        string kind = n.type == DialogueData.NodeType.Line ? "セリフ" :
            n.type == DialogueData.NodeType.Choice ? "選択肢" :
            n.type == DialogueData.NodeType.Branch ? "条件" :
            n.type == DialogueData.NodeType.Purchase ? "購入" : "終了";
        string preview = (n.text ?? "").Replace("\n", " ");
        if (preview.Length > 18) preview = preview.Substring(0, 18) + "...";
        return kind + " #" + n.nodeId + (preview.Length > 0 ? "  " + preview : "");
    }

    void AddOutput(Node node, string label, LinkInfo info)
    {
        Port port = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
        port.portName = label;
        port.userData = info;
        node.outputContainer.Add(port);
        outputs[info.Key] = port;
    }

    void DrawProperties(int nodeId, Node view)
    {
        DialogueData data = window.Data;
        if (data == null) return;
        int index = data.nodes.FindIndex(x => x != null && x.nodeId == nodeId);
        if (index < 0) return;

        SerializedObject serialized = new SerializedObject(data);
        serialized.Update();
        SerializedProperty element = serialized.FindProperty("nodes").GetArrayElementAtIndex(index);
        DialogueData.NodeType type = data.nodes[index].type;
        bool rebuildPorts = false;

        EditorGUI.BeginChangeCheck();
        if (type == DialogueData.NodeType.Line || type == DialogueData.NodeType.Choice ||
            type == DialogueData.NodeType.End)
        {
            EditorGUILayout.PropertyField(element.FindPropertyRelative("speakerId"), new GUIContent("話者"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("expression"), new GUIContent("表情"));
            if (data.nodes[index].expression == DialogueExpression.Other)
                EditorGUILayout.PropertyField(element.FindPropertyRelative("otherExpressionKey"), new GUIContent("表情キー"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("showSpeakerName"), new GUIContent("名前表示"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("text"), new GUIContent("会話テキスト"));
        }

        if (type == DialogueData.NodeType.Branch)
            EditorGUILayout.PropertyField(element.FindPropertyRelative("conditionKey"),
                new GUIContent("Bool変数キー"));
        else if (type == DialogueData.NodeType.Purchase)
            EditorGUILayout.HelpBox("現在の商品を購入。残高・購入可否はShopManagerが判定し、成功/失敗へ分岐します。", MessageType.Info);
        else if (type == DialogueData.NodeType.Choice)
        {
            SerializedProperty choices = element.FindPropertyRelative("choices");
            for (int i = 0; i < choices.arraySize; i++)
            {
                SerializedProperty choice = choices.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("選択肢 " + (i + 1), EditorStyles.boldLabel);
                if (GUILayout.Button("削除", GUILayout.Width(46)))
                {
                    choices.DeleteArrayElementAtIndex(i);
                    rebuildPorts = true;
                    EditorGUILayout.EndHorizontal();
                    break;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(choice.FindPropertyRelative("label"), new GUIContent("表示文"));
                EditorGUILayout.PropertyField(choice.FindPropertyRelative("conditionKey"), new GUIContent("条件キー（任意）"));
                EditorGUILayout.PropertyField(choice.FindPropertyRelative("selectedShopItem"), new GUIContent("商品SO（任意）"));
                EditorGUILayout.Space(5);
            }
            if (GUILayout.Button("+ 選択肢を追加"))
            {
                int added = choices.arraySize;
                choices.InsertArrayElementAtIndex(added);
                SerializedProperty choice = choices.GetArrayElementAtIndex(added);
                choice.FindPropertyRelative("label").stringValue = "選択肢 " + (added + 1);
                choice.FindPropertyRelative("conditionKey").stringValue = "";
                choice.FindPropertyRelative("targetNodeId").intValue = -1;
                choice.FindPropertyRelative("falseTargetNodeId").intValue = -1;
                choice.FindPropertyRelative("selectedShopItem").objectReferenceValue = null;
                rebuildPorts = true;
            }
        }

        bool changed = EditorGUI.EndChangeCheck() || rebuildPorts;
        if (changed)
        {
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(data);
            view.title = MakeTitle(data.nodes[index]);
            if (rebuildPorts) window.QueueReload();
        }
    }

    static int GetDestination(DialogueData data, LinkInfo info)
    {
        if (info.isStart) return data.entryNodeId;
        DialogueData.Node node = data.nodes.Find(x => x != null && x.nodeId == info.nodeId);
        if (node == null) return -1;
        if (info.option >= 0)
        {
            if (node.choices == null || info.option >= node.choices.Count) return -1;
            return info.isFalse ? node.choices[info.option].falseTargetNodeId :
                node.choices[info.option].targetNodeId;
        }
        return info.isFalse ? node.falseNodeId : node.nextNodeId;
    }

    static void SetDestination(DialogueData data, LinkInfo info, int destination)
    {
        if (info.isStart) { data.entryNodeId = destination; return; }
        DialogueData.Node node = data.nodes.Find(x => x != null && x.nodeId == info.nodeId);
        if (node == null) return;
        if (info.option >= 0)
        {
            if (node.choices == null || info.option >= node.choices.Count) return;
            if (info.isFalse) node.choices[info.option].falseTargetNodeId = destination;
            else node.choices[info.option].targetNodeId = destination;
            return;
        }
        if (info.isFalse) node.falseNodeId = destination;
        else node.nextNodeId = destination;
    }

    GraphViewChange OnGraphChanged(GraphViewChange change)
    {
        DialogueData data = window.Data;
        if (rebuilding || data == null) return change;
        bool dirty = false;

        if (change.elementsToRemove != null)
        {
            foreach (GraphElement element in change.elementsToRemove)
            {
                Edge edge = element as Edge;
                if (edge == null || edge.output == null || edge.input == null) continue;
                LinkInfo info = edge.output.userData as LinkInfo;
                if (info == null || !(edge.input.node.userData is int)) continue;
                int target = (int)edge.input.node.userData;
                if (GetDestination(data, info) != target) continue;
                if (!dirty) Undo.RecordObject(data, "Edit dialogue graph links");
                SetDestination(data, info, -1);
                dirty = true;
            }

            foreach (GraphElement element in change.elementsToRemove)
            {
                Node removed = element as Node;
                if (removed == null || !(removed.userData is int)) continue;
                int removedId = (int)removed.userData;
                if (removedId < 0) continue;
                if (!dirty) Undo.RecordObject(data, "Delete dialogue graph node");
                data.nodes.RemoveAll(n => n != null && n.nodeId == removedId);
                if (data.entryNodeId == removedId) data.entryNodeId = -1;
                foreach (DialogueData.Node node in data.nodes)
                {
                    if (node == null) continue;
                    if (node.nextNodeId == removedId) node.nextNodeId = -1;
                    if (node.falseNodeId == removedId) node.falseNodeId = -1;
                    if (node.choices == null) continue;
                    foreach (DialogueData.Choice option in node.choices)
                    {
                        if (option.targetNodeId == removedId) option.targetNodeId = -1;
                        if (option.falseTargetNodeId == removedId) option.falseTargetNodeId = -1;
                    }
                }
                dirty = true;
            }
        }

        if (change.edgesToCreate != null)
            foreach (Edge edge in change.edgesToCreate)
            {
                LinkInfo info = edge.output != null ? edge.output.userData as LinkInfo : null;
                if (info == null || edge.input == null || !(edge.input.node.userData is int)) continue;
                int destination = (int)edge.input.node.userData;
                if (GetDestination(data, info) == destination) continue;
                if (!dirty) Undo.RecordObject(data, "Connect dialogue graph nodes");
                SetDestination(data, info, destination);
                dirty = true;
            }

        if (change.movedElements != null)
            foreach (GraphElement element in change.movedElements)
            {
                Node moved = element as Node;
                if (moved == null || !(moved.userData is int)) continue;
                int id = (int)moved.userData;
                Vector2 position = moved.GetPosition().position;
                if (id < 0)
                {
                    if (data.graphStartPosition == position) continue;
                    if (!dirty) Undo.RecordObject(data, "Move dialogue graph node");
                    data.graphStartPosition = position;
                }
                else
                {
                    DialogueData.Node node = data.nodes.Find(n => n != null && n.nodeId == id);
                    if (node == null || node.graphPosition == position) continue;
                    if (!dirty) Undo.RecordObject(data, "Move dialogue graph node");
                    node.graphPosition = position;
                }
                dirty = true;
            }

        if (dirty) EditorUtility.SetDirty(data);
        return change;
    }
}
#endif
