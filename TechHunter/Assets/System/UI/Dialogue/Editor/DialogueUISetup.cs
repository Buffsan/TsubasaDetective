using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates a UI prefab using native Unity UI objects (no external plugin required).
/// Run Tools > TechHunter > Dialogue > Create and Place Dialogue UI.
/// </summary>
public static class DialogueUISetup
{
    const string PrefabPath = "Assets/System/UI/Dialogue/DialogueCanvas.prefab";
    const string JapaneseFontPath = "Assets/Font/Honoka-Shin-Antique-Maru_R SDF.asset";
    const string DatabasePath = "Assets/System/UI/Dialogue/DialogueCharacterDatabase.asset";

    [MenuItem("Tools/TechHunter/Dialogue/Create and Place Dialogue UI")]
    static void CreateAndPlace()
    {
        DialogueCharacterDatabase database = AssetDatabase.LoadAssetAtPath<DialogueCharacterDatabase>(DatabasePath);
        if (database == null)
        {
            database = ScriptableObject.CreateInstance<DialogueCharacterDatabase>();
            AssetDatabase.CreateAsset(database, DatabasePath);
            AssetDatabase.SaveAssets();
        }
        EnsureVastorProfile(database);

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            GameObject canvas = BuildCanvas();
            canvas.GetComponent<DialogueUIController>().SetDatabase(database);
            prefab = PrefabUtility.SaveAsPrefabAsset(canvas, PrefabPath);
            Object.DestroyImmediate(canvas);
        }
        else
        {
            GameObject editing = PrefabUtility.LoadPrefabContents(PrefabPath);
            DialogueUIController ui = editing.GetComponent<DialogueUIController>();
            if (ui != null) ui.SetDatabase(database);
            PrefabUtility.SaveAsPrefabAsset(editing, PrefabPath);
            PrefabUtility.UnloadPrefabContents(editing);
        }

        DialogueUIController existing = Object.FindObjectOfType<DialogueUIController>();
        if (existing == null)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instance, "Add DialogueCanvas");
            EditorSceneManager.MarkSceneDirty(instance.scene);
            Selection.activeGameObject = instance;
        }
        else
        {
            Undo.RecordObject(existing, "Assign Dialogue Character Database");
            existing.SetDatabase(database);
            EditorUtility.SetDirty(existing);
            EditorSceneManager.MarkSceneDirty(existing.gameObject.scene);
            Selection.activeGameObject = existing.gameObject;
        }

        Debug.Log("Dialogue UI is ready. Create a DialogueData asset and call DialogueEventTrigger.StartDialogue.");
    }

    static void EnsureVastorProfile(DialogueCharacterDatabase database)
    {
        const string profilePath = "Assets/System/UI/Dialogue/Vastor.asset";
        DialogueCharacterProfile profile = AssetDatabase.LoadAssetAtPath<DialogueCharacterProfile>(profilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<DialogueCharacterProfile>();
            profile.id = DialogueCharacterId.Vastor;
            profile.displayName = "Vastor";
            AssetDatabase.CreateAsset(profile, profilePath);
        }

        if (database.Find(DialogueCharacterId.Vastor) == null)
        {
            Undo.RecordObject(database, "Register Vastor dialogue profile");
            database.profiles.Add(profile);
            EditorUtility.SetDirty(database);
        }
        AssetDatabase.SaveAssets();
    }

    static GameObject BuildCanvas()
    {
        GameObject canvasObject = new GameObject("DialogueCanvas",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
            typeof(GraphicRaycaster), typeof(DialogueUIController));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform root = CreateRect("DialogueRoot", canvasObject.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        CanvasGroup group = root.gameObject.AddComponent<CanvasGroup>();

        Image blocker = root.gameObject.AddComponent<Image>();
        blocker.color = new Color(0f, 0f, 0f, 0f);
        blocker.raycastTarget = true;

        RectTransform portraits = CreateRect("Portraits", root,
            Vector2.zero, Vector2.one, new Vector2(40f, 245f), new Vector2(-40f, -25f));
        HorizontalLayoutGroup layout = portraits.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 0f;
        layout.childAlignment = TextAnchor.LowerCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        RectTransform templateRect = CreateRect("PortraitTemplate", portraits,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image portraitTemplate = templateRect.gameObject.AddComponent<Image>();
        portraitTemplate.preserveAspect = true;
        portraitTemplate.raycastTarget = false;
        templateRect.gameObject.SetActive(false);

        RectTransform border = CreatePanel("DialogueBorder", root,
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(18f, 18f), new Vector2(-18f, 238f),
            new Color(0.91f, 0.63f, 0.29f, 1f));
        RectTransform inner = CreatePanel("MessageBackground", border,
            Vector2.zero, Vector2.one,
            new Vector2(12f, 12f), new Vector2(-12f, -12f),
            new Color(0.98f, 0.98f, 0.98f, 0.98f));

        TextMeshProUGUI body = CreateText("MessageText", inner,
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(30f, 26f), new Vector2(-30f, -24f), 36f);
        body.alignment = TextAlignmentOptions.TopLeft;
        body.overflowMode = TextOverflowModes.Truncate;

        RectTransform speakerBorder = CreatePanel("SpeakerNamePanel", border,
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(25f, -4f), new Vector2(425f, 63f),
            new Color(0.91f, 0.63f, 0.29f, 1f));
        RectTransform speakerInner = CreatePanel("SpeakerNameBackground", speakerBorder,
            Vector2.zero, Vector2.one,
            new Vector2(9f, 8f), new Vector2(-9f, -8f), Color.white);
        TextMeshProUGUI nameText = CreateText("SpeakerName", speakerInner,
            Vector2.zero, Vector2.one,
            new Vector2(14f, 0f), new Vector2(-14f, 0f), 42f);
        nameText.fontStyle = FontStyles.Bold;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.overflowMode = TextOverflowModes.Ellipsis;

        TextMeshProUGUI nextText = CreateText("ContinueIndicator", inner,
            new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-68f, 12f), new Vector2(-12f, 62f), 32f);
        nextText.text = "▼";
        nextText.alignment = TextAlignmentOptions.Center;

        DialogueUIController controller = canvasObject.GetComponent<DialogueUIController>();
        controller.AssignUI(group, portraits, portraitTemplate, speakerBorder.gameObject,
            nameText, body, nextText.gameObject);
        return canvasObject;
    }

    static RectTransform CreateRect(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        return rect;
    }

    static RectTransform CreatePanel(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    static TextMeshProUGUI CreateText(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, float size)
    {
        RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        TMP_FontAsset japaneseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(JapaneseFontPath);
        if (japaneseFont != null)
            text.font = japaneseFont;
        text.fontSize = size;
        text.color = Color.black;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        return text;
    }
}
