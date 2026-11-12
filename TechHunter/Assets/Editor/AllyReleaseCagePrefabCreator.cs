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
    const string CageDataPath = AssetFolder + "/CageCharaData.asset";

    [MenuItem("Tools/Stages/Create Ally Release Cage Prefab")]
    public static void CreateCagePrefab()
    {
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "Character/Enemy/AllyReleaseCage"));
        AssetDatabase.Refresh();

        // 檻の能力値も通常敵と同じcharadataのアセットとして保存。
        // 既存のSOがある場合は編集・上書きしない。
        charadata data = GetOrCreateCageData();
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (existing != null)
        {
            // 旧版で生成したPrefabにはデータ参照だけ補完する。
            // 既に別のcharadataをセットしている場合は変更しない。
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                AllyReleaseCage cage = prefabRoot.GetComponent<AllyReleaseCage>();
                if (cage == null)
                {
                    Debug.LogWarning("既存PrefabにAllyReleaseCageがありません。手動確認してください。", existing);
                }
                else
                {
                    SerializedObject serialized = new SerializedObject(cage);
                    SerializedProperty enemyData = serialized.FindProperty("enemyData");
                    if (enemyData != null && enemyData.objectReferenceValue == null)
                    {
                        enemyData.objectReferenceValue = data;
                        serialized.ApplyModifiedPropertiesWithoutUndo();
                        PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
                    }
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            EditorGUIUtility.PingObject(existing);
            Selection.activeObject = existing;
            AssetDatabase.SaveAssets();
            Debug.Log("既存の檻Prefabを確認しました（SO未設定なら補完、既存設定は維持）。");
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

            AllyReleaseCage cageComponent = root.AddComponent<AllyReleaseCage>();
            SerializedObject serializedCage = new SerializedObject(cageComponent);
            serializedCage.FindProperty("enemyData").objectReferenceValue = data;
            serializedCage.ApplyModifiedPropertiesWithoutUndo();

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

    static charadata GetOrCreateCageData()
    {
        charadata data = AssetDatabase.LoadAssetAtPath<charadata>(CageDataPath);
        if (data != null) return data;

        data = ScriptableObject.CreateInstance<charadata>();
        data.NAME = "檻";
        data.MAXHP = 30f;
        data.ATKDF = 0f;
        data.GOLD = 0;
        data.XP = 0;
        data.enemyStrength = charadata.EnemyStrength.None;

        AssetDatabase.CreateAsset(data, CageDataPath);
        AssetDatabase.SaveAssets();
        return data;
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
