using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // 長押し検出のために必要
using UnityEngine.UI;

public class SkillItemHand : MonoBehaviour, IPointerDownHandler,IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] AllSkilPagel skilPagel;
    [SerializeField] Image My;
    public GameObject SkillImageObject;

    public bool PushButton = false;

    public int _ID_Number = 100;

    public float WaitTime = 0.1f;
     float WaitCount = 0;
    public bool PutImage = false;

    int Phase = 0;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0)) 
        {
            if (skilPagel.ChoicePage != null)
            {
                //My.raycastTarget = true;
                PutImage = true;
                
                PushButton = false;
                skilPagel.ChoicePage = null;
            }
        }

        if (PutImage) 
        {
            if (Phase == 0)
            {
                Phase++;             
                skilPagel.ChangeSkill();
                WaitCount = 0;
    
            }
            if (WaitTime < WaitCount && Phase == 1)
            {
                Phase=0;
                PutImage = false;
                        
                WaitCount = 0;
                skilPagel._ID_ChoiceNumber = -1;
            }

            WaitCount += Time.deltaTime;
            
        
        }
    }


   

    public void OnPointerUp(PointerEventData eventData)
    {
        /*
        if (skilPagel.ChoicePage != null)
        {

            skilPagel._ID_ChoiceNumber = -1;
        PushButton = false;
        skilPagel.ChoicePage = null;
        }
        Debug.Log("UIから指が離れました！");*/
        // ここにUIから指が離れた時の処理を記述
    }

   
    public void OnPointerDown(PointerEventData eventData)
    {
        if (skilPagel.ChoicePage == null)
        {
            Debug.Log("押す");
            PushButton = true;
            skilPagel.ChoicePage = this.gameObject;
            skilPagel._ID_ChoiceNumber = _ID_Number;
           // My.raycastTarget = false;
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_ID_Number != skilPagel._ID_ChoiceNumber_NowPage)
        {
            skilPagel._ID_ChoiceNumber_NowPage = _ID_Number;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("マウスがUIから離れた！");
        if (_ID_Number == skilPagel._ID_ChoiceNumber_NowPage)
        {
            skilPagel._ID_ChoiceNumber_NowPage = -1;
        }
    }
    public void isUpButton() 
    {
        if (skilPagel.ChoicePage != null)
        {
            
        }
    }
}
