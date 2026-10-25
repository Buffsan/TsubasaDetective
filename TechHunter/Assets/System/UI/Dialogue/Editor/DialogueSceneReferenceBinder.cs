#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Ensures the common GameSystem prefab has one permanent dialogue bootstrap,
/// and binds scene-local ALL_SystemManager references to that prefab instance.
/// </summary>
[InitializeOnLoad]
public static class DialogueSceneReferenceBinder
{
    const string GameSystemPrefabPath = "Assets/System/-----GameSysem-----.prefab";
    const string SharedCharactersPath = "Assets/System/UI/Dialogue/SharedCharacters.asset";
    const string JapaneseFontPath = "Assets/Font/Honoka-Shin-Antique-Maru_R SDF.asset";
    const string SampleScenePath = "Assets/Scenes/SampleScene.unity";

    static DialogueSceneReferenceBinder()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
        QueueBind();
    }

    static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        QueueBind();
    }

    static void QueueBind()
    {
        EditorApplication.delayCall -= BindOpenScenes;
        EditorApplication.delayCall += BindOpenScenes;
    }

    [MenuItem("Tools/TechHunter/Dialogue/Bind GameSystem References")]
    static void BindOpenScenes()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        // Repair an incomplete prefab asset first; existing DialogueSystem
        // children are reused so the prefab never gains duplicate bootstraps.
        if (EnsurePermanentDialogueObject())
        {
            QueueBind(); // Revisit the scene after Unity refreshes its prefab instances.
            return;
        }

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded || string.IsNullOrEmpty(scene.path) ||
                !scene.path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                continue;

            bool wasDirty = scene.isDirty;
            bool changed = false;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (ALL_SystemManager manager in
                         root.GetComponentsInChildren<ALL_SystemManager>(true))
                {
                    // The manager is a scene-only child of the common GameSystem.
                    // Look only inside its own parent to avoid cross-scene links.
                    Transform parent = manager.transform.parent;
                    if (parent == null) continue;

                    DialogueRuntimeBootstrap bootstrap =
                        parent.GetComponentInChildren<DialogueRuntimeBootstrap>(true);
                    if (bootstrap == null) continue;

                    DialogueCharacterDatabase database = bootstrap.CharacterDatabase;
                    if (manager.dialogueRuntimeBootstrap == bootstrap &&
                        (database == null || manager.dialogueCharacterDatabase == database))
                        continue;

                    Undo.RecordObject(manager, "Bind DialogueSystem to ALL_SystemManager");
                    manager.dialogueRuntimeBootstrap = bootstrap;
                    if (database != null) manager.dialogueCharacterDatabase = database;
                    EditorUtility.SetDirty(manager);
                    changed = true;
                }
            }

            if (!changed) continue;
            EditorSceneManager.MarkSceneDirty(scene);

            // SampleScene is the canonical shared system scene. Persist the
            // reference immediately if it had no unrelated unsaved edits.
            // Never auto-save a user's dirty scene or an arbitrary other scene.
            if (!wasDirty && scene.path == SampleScenePath &&
                !EditorSceneManager.SaveScene(scene))
            {
                Debug.LogWarning("Dialogue references were bound but SampleScene could not be saved. Save it manually.");
            }
        }
    }

    static bool EnsurePermanentDialogueObject()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GameSystemPrefabPath);
        if (prefab == null) return false;

        Transform existing = prefab.transform.Find("DialogueSystem");
        if (existing != null && existing.GetComponent<DialogueRuntimeBootstrap>() != null)
            return false;

        GameObject root = PrefabUtility.LoadPrefabContents(GameSystemPrefabPath);
        if (root == null) return false;

        try
        {
            Transform child = root.transform.Find("DialogueSystem");
            if (child == null)
            {
                GameObject dialogue = new GameObject("DialogueSystem");
                dialogue.transform.SetParent(root.transform, false);
                child = dialogue.transform;
            }

            DialogueRuntimeBootstrap bootstrap =
                child.GetComponent<DialogueRuntimeBootstrap>();
            if (bootstrap == null)
                bootstrap = child.gameObject.AddComponent<DialogueRuntimeBootstrap>();

            SerializedObject serialized = new SerializedObject(bootstrap);
            serialized.FindProperty("characterDatabase").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<DialogueCharacterDatabase>(SharedCharactersPath);
            serialized.FindProperty("japaneseFont").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(JapaneseFontPath);
            serialized.FindProperty("createCanvasAutomatically").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, GameSystemPrefabPath);
            return true;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
#endif
