using UnityEditor;
using UnityEngine;

/// <summary>Validates or repairs the prototype with the Unity prefab API.</summary>
public static class StageCannonPrefabTools
{
    const string Folder = "Assets/Map/prefab/StageCannon/";
    const string PrefabPath = Folder + "StageCannon.prefab";

    [MenuItem("Tools/Stage Cannon/Validate Prefab")]
    public static void ValidatePrefab()
    {
        GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Folder + "CannonPrototypeSquare.png");
        if (root == null || sprite == null)
        {
            Debug.LogError("StageCannon: prefab or prototype sprite could not be imported.");
            return;
        }
        StageCannonController controller = root.GetComponent<StageCannonController>();
        if (controller == null)
        {
            Debug.LogError("StageCannon: controller is missing. Check Console compile errors first.");
            return;
        }
        int missing = 0;
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            missing += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject);
        SerializedObject so = new SerializedObject(controller);
        string[] required = { "cannonBase", "turret", "firePoint", "mountPoint", "gaugeAnchor", "prototypeSprite" };
        foreach (string propertyName in required)
        {
            SerializedProperty property = so.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == null)
            {
                Debug.LogError("StageCannon: missing reference " + propertyName, root);
                return;
            }
        }
        BoxCollider2D solid = root.GetComponentInChildren<BoxCollider2D>(true);
        if (missing != 0 || solid == null || solid.isTrigger ||
            root.GetComponentsInChildren<SpriteRenderer>(true).Length < 7)
        {
            Debug.LogError("StageCannon: missing script, solid collider, or prototype renderer.", root);
            return;
        }
        Debug.Log("StageCannon prefab references, sprite, collider and renderer validation passed. Play Mode is still required.", root);
        Selection.activeObject = root;
    }

    [MenuItem("Tools/Stage Cannon/Repair Prototype Prefab")]
    public static void RepairPrefab()
    {
        if (!EditorUtility.DisplayDialog("Stage Cannon", "Repair the prototype prefab with Unity's Prefab API? Existing named objects and controller settings are kept when the prefab can be loaded.", "Repair", "Cancel")) return;
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Folder + "CannonPrototypeSquare.png");
        if (sprite == null)
        {
            Debug.LogError("StageCannon: prototype sprite not imported. Check the PNG and its TextureImporter.");
            return;
        }
        Material material = AssetDatabase.LoadAssetAtPath<Material>(
            AssetDatabase.GUIDToAssetPath("a97c105638bdf8b4a8650670310a4cd3"));
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        bool loaded = existing != null;
        GameObject root = loaded ? PrefabUtility.LoadPrefabContents(PrefabPath) : new GameObject("StageCannon");
        try
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
            StageCannonController controller = root.GetComponent<StageCannonController>();
            if (controller == null) controller = root.AddComponent<StageCannonController>();
            Transform body = Child(root.transform, "Base", Vector3.zero);
            Transform turret = Child(root.transform, "Turret", Vector3.zero);
            Transform fire = Child(turret, "FirePoint", new Vector3(2.1f,0,0));
            Transform mount = Child(root.transform, "MountPoint", new Vector3(0,-1.55f,0));
            Transform gauge = Child(root.transform, "GaugeAnchor", new Vector3(0,1.4f,0));
            int layer = LayerMask.NameToLayer("Stage");
            if (layer >= 0) body.gameObject.layer = layer;
            BoxCollider2D collider = body.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = body.gameObject.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(2.3f,1.8f);
            }
            collider.isTrigger = false;
            Visual(body,"BaseHull",Vector3.zero,new Vector3(2.5f,2f,1),new Color(.16f,.19f,.22f),100,sprite,material);
            Visual(body,"BasePlate",Vector3.zero,new Vector3(2.22f,1.72f,1),new Color(.30f,.34f,.38f),101,sprite,material);
            Visual(turret,"TurretHousing",Vector3.zero,new Vector3(1.3f,1.25f,1),new Color(.4f,.44f,.48f),102,sprite,material);
            Visual(turret,"Barrel",new Vector3(1.13f,0,0),new Vector3(1.8f,.42f,1),new Color(.25f,.28f,.32f),103,sprite,material);
            Visual(turret,"Muzzle",new Vector3(1.95f,0,0),new Vector3(.32f,.62f,1),new Color(.46f,.49f,.52f),104,sprite,material);
            Visual(gauge,"ReloadBackground",Vector3.zero,new Vector3(2.7f,.26f,1),new Color(.06f,.07f,.08f,.9f),1000,sprite,material);
            Visual(gauge,"ReloadFill",Vector3.zero,new Vector3(2.6f,.16f,1),new Color(.34f,.92f,.48f),1001,sprite,material);
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("cannonBase").objectReferenceValue = body;
            so.FindProperty("turret").objectReferenceValue = turret;
            so.FindProperty("firePoint").objectReferenceValue = fire;
            so.FindProperty("mountPoint").objectReferenceValue = mount;
            so.FindProperty("gaugeAnchor").objectReferenceValue = gauge;
            so.FindProperty("prototypeSprite").objectReferenceValue = sprite;
            if (material != null) so.FindProperty("prototypeMaterial").objectReferenceValue = material;
            so.FindProperty("generateFallbackCollider").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out bool success);
            if (!success) { Debug.LogError("StageCannon: prefab repair failed."); return; }
        }
        finally
        {
            if (loaded) PrefabUtility.UnloadPrefabContents(root);
            else Object.DestroyImmediate(root);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(PrefabPath, ImportAssetOptions.ForceSynchronousImport);
        ValidatePrefab();
    }

    static Transform Child(Transform parent, string name, Vector3 position)
    {
        Transform found = parent.Find(name);
        if (found != null) return found;
        Transform t = new GameObject(name).transform;
        t.SetParent(parent,false);
        t.localPosition = position;
        return t;
    }

    static void Visual(Transform parent, string name, Vector3 pos, Vector3 scale,
        Color color, int order, Sprite sprite, Material material)
    {
        Transform t = Child(parent,name,pos);
        SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null) return;
        if (sr == null) sr = t.gameObject.AddComponent<SpriteRenderer>();
        t.localScale = scale;
        sr.sprite = sprite; sr.color = color; sr.sortingOrder = order;
        if (material != null) sr.sharedMaterial = material;
    }
}
