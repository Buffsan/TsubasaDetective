using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpotBoard_Controller : MonoBehaviour
{

    GameSpot_SpotInfo nowSpot;
    [SerializeField] TextMeshProUGUI SpotNameText;
    [SerializeField] TextMeshProUGUI ExplainText;
    [SerializeField] Image SpotImage;
    [SerializeField] List<Image> Page;
    [SerializeField] List<Image> Denger;
    [SerializeField] RectTransform rectTransform;
    [Space]
    [SerializeField] float MoveSpeed = 20;
    [SerializeField] float RIGHT_GOAL_MOVE_POINT_X = 256;
    [SerializeField] float RIGHT_START_MOVE_X = 400;
    [SerializeField] bool MoveRight = true;

    public sytem_GameSpot gameSpot;
    public int Angle = 1;
    int Phase = 0;
    private void FixedUpdate()
    {
        if (Phase == 0) 
        {
            rectTransform.localPosition = new Vector3(RIGHT_START_MOVE_X * Angle, 0,0);
            Phase++;
        }
        if (Phase == 1) 
        {
            rectTransform.localPosition = new Vector3(rectTransform.localPosition.x - MoveSpeed * Angle, 0, 0);
            if (rectTransform.localPosition.x > -RIGHT_GOAL_MOVE_POINT_X && rectTransform.localPosition.x < RIGHT_GOAL_MOVE_POINT_X) 
            {
                Phase++;
            }
        }
        if (Phase == 2) 
        {
            if (!gameSpot.SecondTotch) 
            {
                Phase++;
            }
        }
        if (Phase == 3) 
        {
            rectTransform.localPosition = new Vector3(rectTransform.localPosition.x + MoveSpeed * Angle, 0, 0);
            if (rectTransform.localPosition.x < -RIGHT_START_MOVE_X && rectTransform.localPosition.x > RIGHT_START_MOVE_X)
            {
                Destroy(gameObject);
            }
        }
    }

    public void ChangeUI(GameSpot_SpotInfo inputSpot,int angle) 
    {
        nowSpot = inputSpot;
        Angle = angle;
        SpotImage.sprite = inputSpot.SpotImage;
        SpotNameText.text = inputSpot.Name;
        ExplainText.text = inputSpot.Explain;

        int i = 0;
        foreach (var img in Page)
        {
            if (inputSpot.PageRarity > i)
            {
                img.color = Color.white;
            }
            else 
            {
                img.color = Color.black;
            }
            i++;
        }
        int j = 0;
        foreach (var img in Denger)
        {
            if (inputSpot.Denger > j)
            {
                img.color = Color.white;
            }
            else
            {
                img.color = Color.black;
            }
            j++;
        }
    }
}
