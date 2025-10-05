using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusMenu : MonoBehaviour
{
    [SerializeField] Vector2 SetPosition;
    [SerializeField] TextMeshProUGUI Statuss;
    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;
    HashSet<int> lineBreaks = new HashSet<int> { 2, 5, 6, 9 };

    public enum Move
    {

        Move,
        Wait,
        Run,

    }
    public Move move = Move.Move;
    RectTransform rect;
    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
    }
    private void FixedUpdate()
    {
        switch (move)
        {

            case Move.Move:
                isMove();
                break;
            case Move.Run:
                isRun();
                break;
            case Move.Wait:
                isWait();
                break;

        }


    }
    public void SetStatus() 
    {
        
        float ALL_HP = ALL_System.playerController.MaxHP;
        float ALL_Attack_multiplier = ALL_System.playerController.AddATK;
        float ALL_Attack = 0;

        float ALL_Recovery =  ALL_System.playerController.Regen;
        float ALL_Deffence_multiplier = 0;
        float ALL_Deffence =  ALL_System.playerController.ATKDF;

        float ALL_CoolTime =  ALL_System.playerController.Range;

        float ALL_Criticul = ALL_System.playerController.AddCritical + ALL_System.playerController.Critical;
        float ALL_CriticulAttack_multiplier = 0;
        float ALL_CriticulAttack = 0;

        float ALL_Speed_multiplier = ALL_System.playerController.AddSpeed;
        float ALL_Speed = ALL_System.playerController.SPEED;


        List<float> ALL_Status = new List<float>
        {
            ALL_HP,
            ALL_Attack_multiplier,
            ALL_Attack,
            ALL_Recovery,
            ALL_Deffence_multiplier,
            ALL_Deffence,
            ALL_CoolTime,
            ALL_Criticul,
            ALL_CriticulAttack_multiplier,
            ALL_CriticulAttack,
            ALL_Speed_multiplier,
            ALL_Speed
        };

        string AllStatus_String = "";
        for (int i = 0; i < ALL_Status.Count; i++) 
        {

            AllStatus_String += ALL_Status[i].ToString("F1") + "\n";
            if (lineBreaks.Contains(i))
            {
                AllStatus_String += "\n";
            }
        
        }
        Statuss.text = AllStatus_String;

    }
    void isWait()
    {

    }
    void isRun()
    {
        rect.transform.localPosition = new Vector2(rect.transform.localPosition.x - 100, 0);

    }
    void isMove()
    {
        if (rect.transform.localPosition.x < SetPosition.x)
        {

            rect.transform.localPosition = new Vector2(rect.transform.localPosition.x + 70, 0);

        }
        else
        {
            rect.transform.localPosition = new Vector2(SetPosition.x, 0);

        }
    }


}

