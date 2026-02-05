using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/CreateSkillCardData")]
public class SkillCardData : ScriptableObject
{

    public Sprite SkillImage;
    public string SkillName;
    [TextArea]
    public string SkillMessage;
    //public int SkillCost = 1;

    public float CoolTime=0;
    public GameObject SkillData;
    public List<GameObject> SkillDataList = new List<GameObject>();

    public int MaxStack = 0;

    [Space]
    public float TotalDamage =0;

}