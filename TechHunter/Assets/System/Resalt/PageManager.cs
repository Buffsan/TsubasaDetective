using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PageManager : MonoBehaviour
{

    public TextMeshProUGUI MainName;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI UP_Number;
    public TextMeshProUGUI MainExplanation;
    public TextMeshProUGUI Explanation;
    public TextMeshProUGUI NumberExplanation;
    public Image MainImage;
    [Space]
    public Image Page_rarity;
    public Sprite CommonPage;
    public Sprite UnCommonPage;
    public Sprite Rare;


    public float TargetRectPos=0;
    [SerializeField] AudioClip Clip1;
    [SerializeField] AudioClip Clip2;

    [SerializeField] GameObject TrigerObject;
    AudioManager audio => AudioManager.instance;
    public enum Move 
    { 
    
        Move,
        Wait,
        Run,
    
    }
    public Move move = Move.Move;

    bool Totch = false;
    bool SelectAfter = true;
    public bool Select = false;

    public SkillController SkillController;

    

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
        if (Input.GetMouseButtonDown(0)&& Totch && !Select) 
        {
            TrigerObject.SetActive(false);
            audio.isPlaySE(Clip2);
            Select = true;
            Debug.Log("選択完了");
            SkillController.ChoicePage();
        }
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
        TrigerObject.SetActive(true);
        animator.SetInteger("Anim", 3);
    }
    void isRun() 
    {
        TrigerObject.SetActive(false);
        rect.transform.localPosition = new Vector2(rect.transform.localPosition.x + 60, 320);
        animator.SetInteger("Anim", 0);
    }
    void isMove() 
    { 
    if (rect.transform.localPosition.x < TargetRectPos)
            {

                rect.transform.localPosition = new Vector2(rect.transform.localPosition.x+ 70, 320);

            }
            else 
            {
            rect.transform.localPosition = new Vector2( TargetRectPos,320);

            SelectAfter = false;
                if (Totch)
                {
                    animator.SetInteger("Anim",2);
                }
                else
                {
                    animator.SetInteger("Anim", 1);
                }
            }
    }
    public void OnPointerEnter(BaseEventData eventData)
    {
        
        Debug.Log("マウスが画像に触れました！");
        Totch = true;
        audio.isPlaySE(Clip1);
        
    }
    public void OnPointerExit(BaseEventData eventData)
    {
        Debug.Log("マウスが画像に離れました！");
        Totch = false;
    }
}
