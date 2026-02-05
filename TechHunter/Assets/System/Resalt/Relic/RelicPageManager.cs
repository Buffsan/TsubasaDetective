using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RelicPageManager : MonoBehaviour
{

    public TextMeshProUGUI MainName;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI MainExplanation;
    public TextMeshProUGUI Explanation;
    public Image MainImage;
    [Space]
    [SerializeField] float xAddpos = 90;
    public float TargetRectPos = 0;
    [SerializeField] AudioClip Clip1;
    [SerializeField] AudioClip Clip2;

    public GameObject TriggerCanvas;

    public bool DontTotch = false;
    AudioManager audio => AudioManager.instance;
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
        if (Input.GetMouseButtonDown(0) && Totch && !DontTotch)
        {
            audio.PlaySE(Clip2);
            Select = true;
            DontTotch = true;
            //Debug.Log("選択完了");
            SkillController.ChoiceRelicPage();
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
        animator.SetInteger("Anim", 3);
    }
    void isRun()
    {
        rect.transform.localPosition = new Vector2(rect.transform.localPosition.x + xAddpos, 380);
        animator.SetInteger("Anim", 0);
        DontTotch = true;
    }
    void isMove()
    {
        if (rect.transform.localPosition.x < TargetRectPos)
        {

            rect.transform.localPosition = new Vector2(rect.transform.localPosition.x + xAddpos, 380);

        }
        else
        {
            rect.transform.localPosition = new Vector2(TargetRectPos, 380);
            TriggerCanvas.SetActive(true);
            if (Totch)
            {
                animator.SetInteger("Anim", 2);
            }
            else
            {
                animator.SetInteger("Anim", 1);
            }
        }
    }
    public void OnPointerEnter(BaseEventData eventData)
    {
        //Debug.Log("マウスが画像に触れました！");
        Totch = true;
        audio.PlaySE(Clip1);
    }
    public void OnPointerExit(BaseEventData eventData)
    {
        //Debug.Log("マウスが画像に離れました！");
        Totch = false;
    }
}
