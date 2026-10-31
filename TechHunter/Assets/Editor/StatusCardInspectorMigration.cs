#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// One-time serialization into the EXISTING SkillController.StatusUPs list.
// No cards or TextMeshPro objects are created in Play Mode.
public static class StatusCardInspectorMigration
{
    const string MenuPath = "Tools/TechHunter/4種類の強化カードを既存StatusUPsに登録";

    [MenuItem(MenuPath)]
    static void Migrate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("強化カード登録", "Play Modeを終了してから実行してください。", "OK");
            return;
        }

        SkillController[] controllers = Object.FindObjectsOfType<SkillController>(true);
        if (controllers.Length == 0)
        {
            EditorUtility.DisplayDialog("強化カード登録",
                "SkillControllerのあるシーンを開いて実行してください。", "OK");
            return;
        }

        int cardsAdded = 0;
        int menusUpdated = 0;
        var modifiedMenuPrefabs = new HashSet<string>();

        foreach (SkillController controller in controllers)
        {
            if (!controller.gameObject.scene.IsValid()) continue;

            Undo.RecordObject(controller, "Register four status upgrade cards");
            int newCards = EnsureCards(controller);
            cardsAdded += newCards;
            if (newCards > 0)
            {
                EditorUtility.SetDirty(controller);
                PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
                EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            }

            // Follow the EXISTING StatusMenuObject prefab reference.
            SerializedObject controllerProps = new SerializedObject(controller);
            SerializedProperty menuProp = controllerProps.FindProperty("StatusMenuObject");
            GameObject menuSource = menuProp != null
                ? menuProp.objectReferenceValue as GameObject : null;
            if (menuSource == null) continue;

            string path = AssetDatabase.GetAssetPath(menuSource);
            if (!string.IsNullOrEmpty(path) && path.EndsWith(".prefab") &&
                modifiedMenuPrefabs.Add(path))
            {
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    StatusMenu menu = prefabRoot.GetComponentInChildren<StatusMenu>(true);
                    if (menu != null && EnsureUiRows(menu))
                    {
                        PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
                        menusUpdated++;
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }
            else if (string.IsNullOrEmpty(path))
            {
                StatusMenu menu = menuSource.GetComponentInChildren<StatusMenu>(true);
                if (menu != null && EnsureUiRows(menu))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(menu);
                    EditorSceneManager.MarkSceneDirty(menu.gameObject.scene);
                    menusUpdated++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("強化カード登録",
            "StatusUPsに追加したカード: " + cardsAdded +
            "件 / 更新したステータスUI: " + menusUpdated +
            "件\nシーンを保存し、カード画像とUI表示位置を確認してください。", "OK");
    }

    static int EnsureCards(SkillController controller)
    {
        if (controller.StatusUPs == null) controller.StatusUPs = new List<StatusUP>();

        int count = 0;
        count += AddIfMissing(controller.StatusUPs, "獲得資金増加", 1,
            "獲得する資金が増加する。", 20f, 0);
        count += AddIfMissing(controller.StatusUPs, "混乱時ダメージ", 1,
            "混乱中の敵に与えるダメージが増加する。", 20f, 1);
        count += AddIfMissing(controller.StatusUPs, "回復量増加", 1,
            "受けるHP回復量が増加する。", 20f, 2);
        count += AddIfMissing(controller.StatusUPs, "幸運ダメージ", 2,
            "クリティカルダメージの倍率部分と固定加算部分を共に強化する。", 20f, 3);
        return count;
    }

    static int AddIfMissing(List<StatusUP> cards, string name, int rarity,
        string explanation, float percent, int type)
    {
        foreach (StatusUP existing in cards)
            if (existing != null && existing.PageName == name) return 0;

        // Temporary card art: reuse an existing art reference. Replace in Inspector.
        Sprite image = null;
        foreach (StatusUP card in cards)
        {
            if (card == null || card.MainImage == null) continue;
            if (image == null) image = card.MainImage;
            if (card.Rarity == rarity)
            {
                image = card.MainImage;
                break;
            }
        }

        StatusUP added = new StatusUP
        {
            PageName = name,
            Rarity = rarity,
            Explanation = explanation,
            UpExplanation = "+" + percent.ToString("0.#") + "%",
            MainImage = image
        };
        switch (type)
        {
            case 0: added.CoinGainPercent = percent; break;
            case 1: added.ConfusionDamagePercent = percent; break;
            case 2: added.HealingPercent = percent; break;
            case 3: added.LuckyDamagePercent = percent; break;
        }
        cards.Add(added);
        return 1;
    }

    static bool EnsureUiRows(StatusMenu menu)
    {
        SerializedObject props = new SerializedObject(menu);
        SerializedProperty values = props.FindProperty("StatusValues");
        SerializedProperty deltas = props.FindProperty("StatusDeltas");
        if (values == null || deltas == null) return false;
        if (values.arraySize >= 16 && deltas.arraySize >= 16) return false;
        if (values.arraySize == 0 && deltas.arraySize == 0) return false;
        if (values.arraySize != 12 || deltas.arraySize != 12)
        {
            Debug.LogWarning("StatusMenuの行数が12ではありません。16行への拡張を手動で確認してください。", menu);
            return false;
        }

        TextMeshProUGUI referenceValue =
            values.GetArrayElementAtIndex(11).objectReferenceValue as TextMeshProUGUI;
        TextMeshProUGUI referenceDelta =
            deltas.GetArrayElementAtIndex(11).objectReferenceValue as TextMeshProUGUI;
        if (referenceValue == null || referenceDelta == null) return false;

        float step = Mathf.Max(24f, referenceValue.rectTransform.rect.height + 4f);
        for (int i = 0; i < 4; i++)
        {
            TextMeshProUGUI newValue =
                Object.Instantiate(referenceValue, referenceValue.transform.parent);
            newValue.name = "AdditionalStatusValue_" + i;
            newValue.raycastTarget = false;
            newValue.enableWordWrapping = false;
            newValue.overflowMode = TextOverflowModes.Overflow;
            newValue.rectTransform.anchoredPosition = referenceValue.rectTransform.anchoredPosition
                + new Vector2(0f, -step * (i + 1));
            newValue.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                Mathf.Max(referenceValue.rectTransform.rect.width, 260f));

            TextMeshProUGUI newDelta =
                Object.Instantiate(referenceDelta, referenceDelta.transform.parent);
            newDelta.name = "AdditionalStatusDelta_" + i;
            newDelta.raycastTarget = false;
            newDelta.rectTransform.anchoredPosition = referenceDelta.rectTransform.anchoredPosition
                + new Vector2(0f, -step * (i + 1));

            values.arraySize++;
            values.GetArrayElementAtIndex(12 + i).objectReferenceValue = newValue;
            deltas.arraySize++;
            deltas.GetArrayElementAtIndex(12 + i).objectReferenceValue = newDelta;
        }

        props.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(menu);
        return true;
    }
}
#endif
