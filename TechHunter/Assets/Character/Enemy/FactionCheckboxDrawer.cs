#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

// Faction型のInspector表示を、閉じないチェックボックス一覧に変更します。
// EnemyBase.Factions / EnemyAttack.attackableFactions の双方に適用されます。
[CustomPropertyDrawer(typeof(Faction))]
public class FactionCheckboxDrawer : PropertyDrawer
{
    private const int Columns = 2;
    private const float ColumnSpacing = 8f;
    private const float RowSpacing = 2f;

    private static readonly Faction[] Choices = GetChoices();
    private static readonly GUIContent[] Labels = GetLabels();

    private static Faction[] GetChoices()
    {
        Array values = Enum.GetValues(typeof(Faction));
        var result = new System.Collections.Generic.List<Faction>();
        foreach (Faction value in values)
        {
            int bits = (int)value;
            // Noneや複合フラグは個別のチェック項目から除外します。
            if (bits > 0 && (bits & (bits - 1)) == 0)
                result.Add(value);
        }
        return result.ToArray();
    }

    private static GUIContent[] GetLabels()
    {
        var labels = new GUIContent[Choices.Length];
        for (int i = 0; i < Choices.Length; i++)
            labels[i] = new GUIContent(Choices[i].ToString());
        return labels;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int rows = (Choices.Length + Columns - 1) / Columns;
        return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing
               + rows * (EditorGUIUtility.singleLineHeight + RowSpacing);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect header = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(header, label);

        Rect body = EditorGUI.IndentedRect(position);
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float columnWidth = (body.width - ColumnSpacing) / Columns;
        float top = position.y + lineHeight + EditorGUIUtility.standardVerticalSpacing;
        int mask = property.intValue;
        int newMask = mask;

        // Prefab・複数オブジェクト編集時もUndoとOverrideをUnity標準に任せます。
        EditorGUI.BeginChangeCheck();
        for (int i = 0; i < Choices.Length; i++)
        {
            int row = i / Columns;
            int col = i % Columns;
            var rect = new Rect(body.x + col * (columnWidth + ColumnSpacing),
                                top + row * (lineHeight + RowSpacing), columnWidth, lineHeight);
            int bit = (int)Choices[i];
            bool checkedNow = (mask & bit) != 0;
            bool checkedNew = EditorGUI.ToggleLeft(rect, Labels[i], checkedNow);
            if (checkedNew) newMask |= bit;
            else newMask &= ~bit;
        }
        if (EditorGUI.EndChangeCheck())
            property.intValue = newMask;

        EditorGUI.EndProperty();
    }
}
#endif
