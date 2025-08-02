using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;


[CreateAssetMenu(menuName = "Data/EnemySpawnData")]
public class EnemySpawnData : ScriptableObject
{
   
        public EnemySpawnBase enemyspawn;
  
        public List<EnemySpawnGroup> enemyspawnGroup = new List<EnemySpawnGroup>();

    

        public int StageLevel = 0;
        // Start is called before the first frame update
      
     
}

[System.Serializable]
public class EnemySpawnGroup
{
    public enum EnemyType
    {

        CG_RightKnight,
        CG_Knight,
        CG_shieldKnight,
        CG_Spire,
        CG_Sniper,
        CG_Wizard,
        CG_StarWizard,
        CG_Heavy_Knight,
        CG_Heavy_Sniper,
        CG_Heavy_Wizard,
        CG_Hanter,
        CG_CosmicSurveyor,

        MONSTAR_ErathSite


    }

    [CustomPropertyDrawer(typeof(EnemyType))]
    public class EnemyTypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var enumValue = (EnemyType)property.enumValueIndex;
            var displayName = enumValue.ToString().Replace("_", "/"); // ŠK‘w‚Á‚Û‚­Œ©‚¹‚é

            property.enumValueIndex = EditorGUI.Popup(position, label.text, property.enumValueIndex, GetOptions());

            EditorGUI.EndProperty();
        }

        private string[] GetOptions()
        {
            return System.Enum.GetNames(typeof(EnemyType))
                .Select(name => name.Replace("_", "/"))
                .ToArray();
        }
    }

    public EnemyType enemyType;

    public int SpawnNumber;
}
