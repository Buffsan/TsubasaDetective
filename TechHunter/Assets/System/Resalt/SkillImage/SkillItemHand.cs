using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // 長押し検出のために必要

public class SkillItemHand : MonoBehaviour, IPointerDownHandler,IPointerUpHandler
{

    [SerializeField] AllSkilPagel skilPagel;
    public GameObject SkillImageObject;

    public bool PushButton = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


   

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("UIから指が離れました！");
        // ここにUIから指が離れた時の処理を記述
    }

   
    public void OnPointerDown(PointerEventData eventData)
    {
        if (skilPagel.ChoicePage == null)
        {
            Debug.Log("押す");
            PushButton = true;
            skilPagel.ChoicePage = this.gameObject;
        }
    }
    public void isUpButton() 
    {
        if (skilPagel.ChoicePage != null)
        {
            PushButton = false;
            skilPagel.ChoicePage = null;
        }
    }
}
