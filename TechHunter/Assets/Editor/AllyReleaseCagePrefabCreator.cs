#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

// Creates an editable placeholder prefab. Replace the sprite with final cage art later.
public static class AllyReleaseCagePrefabCreator
{
    const string AssetFolder = "Assets/Character/Enemy/AllyReleaseCage";
    const string PrefabPath = AssetFolder + "/AllyReleaseCage.prefab";
    const string SpritePath = AssetFolder + "/CagePlaceholder.png";

    [MenuItem("Tools/Stages/Create Ally Release Cage Prefab")]
    public static void CreateCagePrefab()
    {
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "Character/Enemy/AllyReleaseCage"));
        AssetDatabase.Refresh();

        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (existing != null)
        {
            EditorGUIUtility.PingObject(existing);
            Selection.activeObject = existing;
            Debug.Log("既存の檻Prefabを開きました（上書きしません）。");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath) == null)
            CreatePlaceholderSprite();

        GameObject root = new GameObject("AllyReleaseCage");
        try
        {
            root.tag = "EnemyArea";  // PlayerAttackがIDamageableを見つけるタグ
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            renderer.sortingOrder = 0;

            BoxCollider2D hitbox = root.AddComponent<BoxCollider2D>();
            hitbox.isTrigger = true;
            hitbox.size = new Vector2(0.85f, 0.95f);

            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeAll;

            root.AddComponent<AllyReleaseCage>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            Debug.Log("破壊可能な檻Prefabを作成しました: " + PrefabPath);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    static void CreatePlaceholderSprite()
    {
        const int size = 64;
        Texture2D image = new Texture2D(size, size, TextureFormat.RGBA32, false);
        image.filterMode = FilterMode.Point;

        Color empty = new Color(0f, 0f, 0f, 0f);
        Color dark = new Color(0.20f, 0.22f, 0.24f, 1f);
        Color metal = new Color(0.55f, 0.59f, 0.61f, 1f);
        Color highlight = new Color(0.75f, 0.78f, 0.80f, 1f);
        Color[] colors = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool topOrBottom = x >= 6 && x < 58 &&
                    ((y >= 5 && y <= 11) || (y >= 52 && y <= 58));
                bool vertical = y >= 8 && y < 57 &&
                    ((x >= 6 && x <= 11) || (x >= 22 && x <= 26) ||
                     (x >= 37 && x <= 41) || (x >= 52 && x <= 57));
                if (!topOrBottom && !vertical)
                {
                    colors[y * size + x] = empty;
                    continue;
                }
                bool edge = x == 6 || x == 22 || x == 37 || x == 52 ||
                            y == 5 || y == 52;
                colors[y * size + x] = edge ? dark : (x % 8 == 0 ? highlight : metal);
            }
        }

        image.SetPixels(colors);
        image.Apply();

        string absolutePath = Path.Combine(
            Application.dataPath, "Character/Enemy/AllyReleaseCage/CagePlaceholder.png");
        File.WriteAllBytes(absolutePath, image.EncodeToPNG());
        Object.DestroyImmediate(image);
        AssetDatabase.ImportAsset(SpritePath);

        TextureImporter importer = AssetImporter.GetAtPath(SpritePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 64;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }
    }
}
#endif
