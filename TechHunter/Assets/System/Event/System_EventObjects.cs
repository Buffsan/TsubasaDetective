using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class System_EventObjects : MonoBehaviour
{
    //ƒLƒƒƒ“ƒoƒX
    public GameObject UI_HP_Slider;
    public GameObject UI_Conin;
    public GameObject UI_SkillUIs;

    public void UI_Active_FALSE() 
    {
        
        UI_HP_Slider.SetActive(false);
        UI_Conin.SetActive(false);
        UI_SkillUIs.SetActive(false);
    }
    public void UI_Active_TRUE()
    {
        UI_HP_Slider.SetActive(true);
        UI_Conin.SetActive(true);
        UI_SkillUIs.SetActive(true);
    }

}
