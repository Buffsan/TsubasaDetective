using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyContoroller : EnemyBase
{
    [SerializeField] EnemyMoveBase moveBase;
    [SerializeField] GameObject EnemyMoveBaseObject;

    public enum MoveToBase
    {

        Out,
        Special

    }
    public MoveToBase Choice_MoveToBase = MoveToBase.Out;

    private void Start()
    {
        GameObject CL_MoveBase = Instantiate(EnemyMoveBaseObject);
        moveBase = CL_MoveBase.GetComponent<EnemyMoveBase>();
   
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

        }

        if (PlayerDistance < LookPlayerDistance)
        {
            moveType = MoveType.Attack;
        }
    }
    void isAttack()
    {//UŒ‚ƒ‚[ƒVƒ‡ƒ“‚ð•Ê“r“ü‚ê‚é
        moveBase.EnemyAttackPlay();
    }
    private void isMove()
    {
        if (Choice_MoveToBase == MoveToBase.Out)
        {
            isPlayerLook();
            rb.velocity = PlayerDirection.normalized * SPEED;
            animator.SetInteger("Anim", 1);
        }
        else 
        { 
        moveBase.EnemyMovePlay();
        }
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
