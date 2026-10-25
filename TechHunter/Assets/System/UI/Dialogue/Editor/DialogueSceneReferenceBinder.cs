#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Keeps scene-local ALL_SystemManager references in sync with the permanent
/// DialogueSystem child supplied by the shared GameSystem prefab.
/// Runs on scene open/script reload so the links are visible before Play Mode.
/// </summary>
[InitializeOnLoad]
public static class DialogueSceneReferenceBinder
{
    static DialogueSceneReferenceBinder()
    {
        EditorApplication.delayCall += BindOpenScenes;
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        EditorApplication.delayCall -= BindOpenScenes;
        EditorApplication.delayCall += BindOpenScenes;
    }

    [MenuItem("Tools/TechHunter/Dialogue/Bind GameSystem References")]
    static void BindOpenScenes()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded || string.IsNullOrEmpty(scene.path) ||
                !scene.path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                continue;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (ALL_SystemManager manager in
                         root.GetComponentsInChildren<ALL_SystemManager>(true))
                {
                    // This is the *same* GameSystem tree, not a global search
                    // that could accidentally bind a different scene's system.
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
                    EditorSceneManager.MarkSceneDirty(scene);
                }
            }
        }
    }
}
#endif
