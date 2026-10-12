using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/CreateCharaData")]
public class charadata : ScriptableObject
{

    public string NAME;
    public float MAXHP;

    public enum EnemyStrength
    {
        None,       // 分類なし。練習場のダミー等
        Fodder,     // 一撃で倒せるような雑魚
        Normal,     // 標準
        Veteran,    // やや強い
        Elite,      // 強敵
        Boss        // ボス
    }
    public EnemyStrength enemyStrength = EnemyStrength.Normal;

    public float Regen;
    public float MAXMP;
    public float SPEED;
    public float ATK;
    public float ATKDF;
    public int GOLD;
    public int XP;
    public float StanHP;
    public float ConfusionResistance;
    public float Critical = 0;
    public float Range = 0;
    public float ConfusionResistanceRegen =15;

}
