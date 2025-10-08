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

        TERUSETO_TGST_Eaterofleftovers,
        TERUSETO_TGST_FoodServer,
        TERUSETO_TGST_ARAKAZEFoodServer,
        TERUSETO_FishHanter_Shoygun,

        MONSTAR_ErathSite,
        MONSTAR_LastHoliday


    }

    

    public EnemyType enemyType;

    public int SpawnNumber;
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(EnemySpawnGroup.EnemyType))]
public class EnemyTypeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var enumValue = (EnemySpawnGroup.EnemyType)property.enumValueIndex;
        var displayName = enumValue.ToString().Replace("_", "/");

        property.enumValueIndex = EditorGUI.Popup(position, label.text, property.enumValueIndex, GetOptions());

        EditorGUI.EndProperty();
    }

    private string[] GetOptions()
    {
        return System.Enum.GetNames(typeof(EnemySpawnGroup.EnemyType))
            .Select(name => name.Replace("_", "/"))
            .ToArray();
    }
}
#endif