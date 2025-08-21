using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AllSkilPagel : MonoBehaviour
{
    
    public GameObject ChoicePage;//ページの保存先
    public GameObject FrontSpace;
    public GameObject SaveFrame;
    public int _ID_ChoiceNumber_NowPage;
    
    public List<AllSkilPages> AllSkillPagesList = new List<AllSkilPages>();
    PlayerController playerController => PlayerController.Instance;
    public SkillController skillController;
    public int _ID_ChoiceNumber = -1;

    public GameObject HaveCarsor_PageImage;

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

        if (_ID_ChoiceNumber != -1 && HaveCarsor_PageImage ==null) 
        {

            int i = 0;
            foreach (var allPage in AllSkillPagesList) 
            {

                if (i == _ID_ChoiceNumber) 
                {
                    SaveFrame = allPage.Frame;
                    HaveCarsor_PageImage = allPage.SkillImageObject;
                    HaveCarsor_PageImage.transform.parent = FrontSpace.transform;
                }
                i++;
            }
        }
        if (HaveCarsor_PageImage != null) 
        {

            HaveCarsor_PageImage.transform.position = playerController.cursorController.MouseScreenPos;
            if (_ID_ChoiceNumber == -1) 
            {
                HaveCarsor_PageImage.transform.parent = SaveFrame.transform;

                HaveCarsor_PageImage.transform.localPosition =  Vector3.zero;
                HaveCarsor_PageImage = null;
            }

        }


    }
    public void ChangeSkill() 
    {

        //&& _ID_ChoiceNumber != -1 && _ID_ChoiceNumber_NowPage != -1
        if (_ID_ChoiceNumber != _ID_ChoiceNumber_NowPage && _ID_ChoiceNumber != -1 && _ID_ChoiceNumber_NowPage != -1) 
        {
            Debug.Log("ページ変更" + _ID_ChoiceNumber +"(初選択)　と　" +_ID_ChoiceNumber_NowPage +"　（最終選択）の取り換え");
            var temp = playerController.skillINFO[_ID_ChoiceNumber_NowPage].skillDATA;
            playerController.skillINFO[_ID_ChoiceNumber_NowPage].skillDATA = playerController.skillINFO[_ID_ChoiceNumber].skillDATA;
            playerController.skillINFO[_ID_ChoiceNumber].skillDATA = temp;

            skillController.Change_AllSkillPage();

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
    public GameObject Frame;
    
    public AllSkilPagel allSkilPagel;

    public GameObject SkillImageObject;
    public Image SkillImage;
    int SkillLevel = 0;

    public TextMeshProUGUI PageControllText;
    public TextMeshProUGUI LevelText;
    
    
    
}