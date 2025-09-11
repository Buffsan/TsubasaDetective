using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TG_WhiskeylHigh_Bonus : MonoBehaviour
{
    // Start is called before the first frame update

    PlayerController playerController => PlayerController.Instance;
    Vector2 SavePos = Vector2.zero;
    Vector2 NowPos = Vector2.zero;
    float Distance = 0;
    public float DistanceTime = 0.1f;
    public float DistanceCount = 0;

    float DistanceAdd = 0;

    bool Set = false;
    void Start()
    {
        //SavePos = playerController.transform.position;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (!Set) 
        {
            Set = true;
            SavePos = playerController.transform.position;
        }
        if (DistanceCount >= DistanceTime)
        {
            DistanceCount = 0;

            NowPos = playerController.transform.position;
            Distance = Vector2.Distance(NowPos, SavePos);
            DistanceAdd += Distance;

            // 1ユニット以上移動していたら、その回数分だけ処理
            while (DistanceAdd >= 1f)
            {
                DistanceAdd -= 1f;

                foreach (var skill in playerController.skillINFO)
                {
                    if (skill.skillDATA != null &&
                        skill.SkillCardManager.mode == SkillCardManager.Mode.CoolTime)
                    {
                        skill.SkillCardManager.isCoolTime_add(0.7f);
                    }
                }
            }

            SavePos = NowPos;
        }
        else
        {
            DistanceCount += Time.fixedDeltaTime; // FixedUpdateではこちらを使う
        }
    }
}
