using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class SkillCardManager : MonoBehaviour
{
    // Start is called before the first frame update

  

    public SkillCardData cardData;

    public TextMeshProUGUI CoolCountText;
    public TextMeshProUGUI InputButtonText;
    public TextMeshProUGUI StackText;

    public GameObject StackTextObject;
    
    public Image SkillImage;
    public Sprite NoneSkillImage;
    public GameObject SkillData;
    public float CoolTime;

    public int MaxStack =0;
    public int CountStack =0;

    public int ID=0;

    PlayerController playerController => PlayerController.Instance;

    public Image cardImage;


    public Color defaltColor = Color.white;

    public Color BlackColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);



    float CoolCount=0;
    public enum Mode 
    { 
    
        None,

        CoolTime,

        Standby
    
    }
    public Mode mode = Mode.None;
    

    void Start()
    {
        BlackColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);
        /*
        if (playerController.skillINFO[ID].skillDATA != null)
        {
            Debug.Log("A");
            SetSkill();


        }
        else 
        {
            Debug.Log("スキルデータなし");
        }*/
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        // コントローラーが接続されているか確認
        var gamepads = Gamepad.all; // すべての接続されているGamepadのリスト
        if (gamepads.Count > 0)
        {
            //Debug.Log("コントローラーが接続されています");
            InputButtonText.text = playerController.skillINFO[ID].Xbox_ControllText;
        }
        else
        {
            //Debug.Log("コントローラーが接続されていません");
            InputButtonText.text = playerController.skillINFO[ID].PC_ControllText;
        }


        if (MaxStack == 0)
        {
            StackText.enabled = false;
            if (mode == Mode.CoolTime)
            {

                isCoolTime();
                cardImage.color = BlackColor;
                CoolCountText.enabled = true;



                if (CoolTime <= CoolCount)
                {
                    CoolCount = 0;
                    mode = Mode.Standby;
                }
                else
                {


                    float Count = CoolTime - CoolCount;
                    CoolCountText.text = Count.ToString("F1");
                }
            }
            else if (mode == Mode.Standby)
            {

                //Debug.Log("AAAA");
                CoolCountText.enabled = false;
                cardImage.color = defaltColor;
            }
        }
        else 
        {
            StackText.enabled = true;

            StackText.text = CountStack.ToString();
            if (mode == Mode.CoolTime)
            {

                if (CountStack == 0)
                {
                    isCoolTime();
                    cardImage.color = BlackColor;
                    CoolCountText.enabled = true;

                    if (CoolTime <= CoolCount)
                    {
                        CoolCount = 0;
                        mode = Mode.Standby;
                    }
                    else
                    {


                        float Count = CoolTime - CoolCount;
                        CoolCountText.text = Count.ToString("F1");
                    }
                }
                else if (CountStack != 0)
                {
                    CountStack--;
                    mode = Mode.Standby;
                }
            }
            else if (mode == Mode.Standby)
            {
                if (MaxStack > CountStack)
                {
                    CoolCount += Time.deltaTime;
                    if (CoolTime <= CoolCount) 
                    {
                        CoolCount = 0;
                        CountStack++;
                    }
                }

                //Debug.Log("AAAA");
                CoolCountText.enabled = false;
                cardImage.color = defaltColor;
            }
        }
       

        if (playerController.skillINFO[ID].skillDATA != cardData ) 
        {

            cardData = playerController.skillINFO[ID].skillDATA;
            SetSkill();

        }

    }
    public void isCoolTime() 
    {
        CoolCount += Time.deltaTime * (1 + playerController.playerBuff.Multiply_CoolTimeBuff +( (playerController.AddRange)/100));
    }
    public void isCoolTime_add(float value) 
    {
        if (mode == Mode.CoolTime) 
        {
            CoolCount +=value;
            if(CoolCount < 0)CoolCount = 0;
        }
    }
    public void isCoolTime_add_Over(float value) 
    {
        CoolCount += value;
    }
    public void SetSkill() 
    {


        MaxStack = cardData.MaxStack;

    SkillImage.sprite = cardData.SkillImage;
  
            SkillData = cardData.SkillData;

        CoolTime =  cardData.CoolTime;

        mode = Mode.Standby;
    }
}
