using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AllSkilPagel : MonoBehaviour
{
    
    public GameObject ChoicePage;
    public GameObject FrontSpace;
    
    public List<AllSkilPages> AllSkillPagesList = new List<AllSkilPages>();

    

    public enum Move
    {

        Move,
        Wait,
        Run,

    }
    public Move move = Move.Move;

    bool Totch = false;
    public bool Select = false;

    public SkillController SkillController;
    public float TargetRectPos = 0;


    Animator animator;
    RectTransform rect;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        
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
    void isWait()
    {
 
    }
    void isRun()
    {
        rect.transform.localPosition = new Vector2(rect.transform.localPosition.x - 20, 0);
      
    }
    void isMove()
    {
        if (rect.transform.localPosition.x < TargetRectPos)
        {

            rect.transform.localPosition = new Vector2(rect.transform.localPosition.x + 70, 0);

        }
        else
        {
            rect.transform.localPosition = new Vector2(TargetRectPos, 0);

        }
    }

   
}

[System.Serializable]
public class AllSkilPages
{
    public AllSkilPagel allSkilPagel;

    public GameObject SkillImageObject;
    public Image SkillImage;
    int SkillLevel = 0;

    public TextMeshProUGUI PageControllText;
    public TextMeshProUGUI LevelText;
    
    
    
}