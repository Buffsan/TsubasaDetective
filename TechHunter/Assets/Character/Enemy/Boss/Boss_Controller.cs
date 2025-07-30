using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Boss_Controller : EnemyBase
{
    [SerializeField] Boss_MoveBase moveBase;
    [SerializeField] GameObject EnemyMoveBaseObject;

    public TextMeshProUGUI HPtext;

    
    private void Start()
    {
        GameObject CL_MoveBase = Instantiate(EnemyMoveBaseObject);
        moveBase = CL_MoveBase.GetComponent<Boss_MoveBase>();

        moveBase.enemyBase = this;
        CL_MoveBase.transform.SetParent(gameObject.transform);


        

    }
    private void FixedUpdate()
    {
        switch (status)
        {
            case Status.Nomal:
                isNomalActive();
                break;
            case Status.Stan:
                isStanStan();
                break;
            case Status.ConfusionResistance:
                isConfusion();
                break;
        }
        HP_TextChange();
    }
    void HP_TextChange() 
    {

        HPtext.text = MAXHP + "/" + HP; 

    }
    // Update is called once per frame

    void isConfusion()
    {
        animator.SetBool("Damage", true);
        if (ConfusionTimer + 3 > StanCount)
        {
            StanCount += Time.deltaTime;
        }
        else
        {

            StanCount = 0;
            status = Status.Nomal;
            animator.SetBool("Damage", false);
            animator.SetInteger("Anim", 0);
        }
    }
    void isStanStan()
    {
        animator.SetBool("Damage", true);
        if (StanTime > StanCount)
        {
            StanCount += Time.deltaTime;
        }
        else
        {

            StanCount = 0;
            status = Status.Nomal;
            animator.SetBool("Damage", false);
            animator.SetInteger("Anim", 0);
        }
    }
    private void isNomalActive()
    {
        switch (moveType)
        {
            case MoveType.Move:
                isMove();
                break;
            case MoveType.Attack:
                isAttack();
                break;
                case MoveType.AI:
                //isAI();
                break;

        }

        
    }
    void isAI() 
    {
        moveBase.M_AI();
    }
    void isAttack()
    {//çUåÇÉÇÅ[ÉVÉáÉìÇï ìrì¸ÇÍÇÈ
        moveBase.M_Attack();
    }
    private void isMove()
    {
        //moveBase.M_BaseMove();
        isAI();
    }
    public void isPlayerLook()
    {
        PlayerDirection = Player.transform.position - transform.position;
        PlayerDistance = Vector2.Distance(Player.transform.position, transform.position);
        if (Player.transform.position.x < transform.position.x)
        {
            EnemyAnimBody.transform.localScale = new Vector2(-2.5f, 2.5f);
        }
        else
        {
            EnemyAnimBody.transform.localScale = new Vector2(2.5f, 2.5f);
        }
    }
}
