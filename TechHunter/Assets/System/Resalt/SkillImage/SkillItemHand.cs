using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // ���������o�̂��߂ɕK�v

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
        Debug.Log("UI����w������܂����I");
        // ������UI����w�����ꂽ���̏������L�q
    }

   
    public void OnPointerDown(PointerEventData eventData)
    {
        if (skilPagel.ChoicePage == null)
        {
            Debug.Log("����");
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
