using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class sytem_GameSpot : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] GameObject UpLine;
    [SerializeField] GameObject DownLine;
    [SerializeField] GameObject MastLine;
    [SerializeField] GameObject EX_RightLine;
    [SerializeField] GameObject EX_LeftLine;
    [SerializeField] GameObject BackImage;
    [SerializeField] GameObject HoImage;

    public sytem_GameSpotsController controller;

    public bool Up = false;
    public bool Down = false;
    public bool Mast = true;
    public bool EX_Left = false;
    public bool EX_Right = false;

    public bool Totch = false;
    public bool SecondTotch = false;

    public Vector2 SpotSystemPos;

    public GameSpot_SpotInfo SpotInfo;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer.sprite = SpotInfo.SpotImage;
        if (Up)
        {
            UpLine.SetActive(true);
        }
        else 
        {
            UpLine.SetActive(false);
        }
        if (Down)
        {
            DownLine.SetActive(true);
        }
        else 
        {
            DownLine.SetActive(false);
        }
        if (Mast)
        {
            MastLine.SetActive(true);
        }
        else
        {
            MastLine.SetActive(false);
        }
        if (EX_Left) 
        { 
        EX_LeftLine.SetActive(true);
        }
        if (EX_Right) 
        { 
        EX_RightLine.SetActive(true);
        }
    }
    /*
    public void OnPointerEnter(BaseEventData eventData)
    {
        Debug.Log("マウスが画像に触れました！");
        Totch = true;
    }
    public void OnPointerExit(BaseEventData eventData)
    {
        Debug.Log("マウスが画像に離れました！");
        Totch = false;
    }*/

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)&& Totch) 
        {
            if (SecondTotch)
            {
                string Mode = SpotInfo.gameMode.ToString();
                Debug.Log(Mode);
                controller.SelextSpot(SpotSystemPos,Mode);
            }
            else 
            {
                SecondTotch = true;
            }
        }
        HoImage.SetActive(SecondTotch);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Cursor")) 
        {
            

            foreach (GameSpot_SpotInfo next in controller.NextInputSpot) 
            {
                if (next.SpotPos == SpotSystemPos || next.SpotPos == new Vector2(99,99)) 
                {
                    
                Totch = true;//Debug.Log("P");
                BackImage.SetActive(true);
                }
            }

            
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Cursor")) 
        {
            //Debug.Log("Pp");
            Totch = false;
            BackImage.SetActive(false);
            SecondTotch = false;
        }
    }
}

