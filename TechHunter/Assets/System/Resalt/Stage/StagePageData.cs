using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "UIData/CreateStagePage")]

public class StagePageData : ScriptableObject
{
    public string StageName;
    public int StageDifficultyStar = 0;
    public int BossDifficultyStar = 0;
    public Sprite StageImage;
    public Sprite BossImage;
    public charadata charadata;
    public SystemManager.Stage stage;
    public int stageNumber = 0;
    [TextArea]
    public string explaination;
}
