using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Uses the registered names for Speaker ID and each Participants entry.
[CustomPropertyDrawer(typeof(DialogueCharacterIdAttribute))]
public sealed class DialogueCharacterIdDrawer : PropertyDrawer
{
    const string RegistryPath = "Assets/System/UI/Dialogue/SharedCharacters.asset";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Integer)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        DialogueCharacterDatabase registry = AssetDatabase.LoadAssetAtPath<DialogueCharacterDatabase>(RegistryPath);
        if (registry == null)
        {
            EditorGUI.LabelField(position, label, new GUIContent("SharedCharacters.asset not found"));
            return;
        }

        List<int> ids = new List<int> { 0 };
        List<string> labels = new List<string> { "None (0)" };
        HashSet<int> seen = new HashSet<int> { 0 };

        foreach (DialogueCharacterProfile character in registry.profiles)
        {
            if (character == null || character.id <= 0 || !seen.Add(character.id)) continue;
            string display = string.IsNullOrWhiteSpace(character.displayName)
                ? character.name : character.displayName;
            ids.Add(character.id);
            labels.Add(display + " (" + character.id + ")");
        }

        int current = ids.IndexOf(property.intValue);
        if (current < 0)
        {
            current = ids.Count;
            ids.Add(property.intValue);
            labels.Add("Unregistered ID (" + property.intValue + ")");
        }

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
        EditorGUI.BeginChangeCheck();
        int selected = EditorGUI.Popup(position, label.text, current, labels.ToArray());
        if (EditorGUI.EndChangeCheck() && selected >= 0 && selected < ids.Count)
            property.intValue = ids[selected];
        EditorGUI.showMixedValue = false;
        EditorGUI.EndProperty();
    }
}

[CustomEditor(typeof(DialogueCharacterDatabase))]
public sealed class DialogueCharacterDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SerializedProperty profiles = serializedObject.FindProperty("profiles");

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(profiles, true);
        bool changed = EditorGUI.EndChangeCheck();
        serializedObject.ApplyModifiedProperties();

        DialogueCharacterDatabase registry = (DialogueCharacterDatabase)target;
        if (changed)
            AssignMissingOrDuplicateIds(registry);

        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox("IDs are permanent. Add characters here, then select their names in DialogueData.", MessageType.Info);

        if (GUILayout.Button("Register New Character"))
            CreateAndRegister(registry);

        if (GUILayout.Button("Repair Missing / Duplicate IDs"))
            AssignMissingOrDuplicateIds(registry);
    }

    static void CreateAndRegister(DialogueCharacterDatabase registry)
    {
        string registryPath = AssetDatabase.GetAssetPath(registry);
        string directory = Path.GetDirectoryName(registryPath).Replace('\\', '/');
        string profilePath = AssetDatabase.GenerateUniqueAssetPath(directory + "/Character.asset");

        Undo.RecordObject(registry, "Register dialogue character");
        DialogueCharacterProfile profile = ScriptableObject.CreateInstance<DialogueCharacterProfile>();
        profile.id = registry.AllocateId();
        profile.displayName = Path.GetFileNameWithoutExtension(profilePath);
        AssetDatabase.CreateAsset(profile, profilePath);
        Undo.RegisterCreatedObjectUndo(profile, "Create dialogue character");
        registry.profiles.Add(profile);
        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
        Selection.activeObject = profile;
    }

    static void AssignMissingOrDuplicateIds(DialogueCharacterDatabase registry)
    {
        HashSet<int> seenIds = new HashSet<int>();
        HashSet<DialogueCharacterProfile> seenProfiles = new HashSet<DialogueCharacterProfile>();
        bool changed = false;

        foreach (DialogueCharacterProfile profile in registry.profiles)
        {
            if (profile == null || !seenProfiles.Add(profile)) continue;
            if (profile.id > 0 && seenIds.Add(profile.id)) continue;

            if (!changed) Undo.RecordObject(registry, "Assign dialogue character IDs");
            Undo.RecordObject(profile, "Assign dialogue character ID");
            profile.id = registry.AllocateId();
            seenIds.Add(profile.id);
            EditorUtility.SetDirty(profile);
            changed = true;
        }

        if (changed)
        {
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
        }
    }
}

[CustomEditor(typeof(DialogueCharacterProfile))]
public sealed class DialogueCharacterProfileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.IntField("Character ID", ((DialogueCharacterProfile)target).id);

        DrawPropertiesExcluding(serializedObject, "m_Script", "id");
        serializedObject.ApplyModifiedProperties();
    }
}
