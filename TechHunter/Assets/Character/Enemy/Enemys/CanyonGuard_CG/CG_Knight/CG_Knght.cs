using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CG_Knght : EnemyBase
{
    
    // Start is called before the first frame update
    void Start()
    {
        
        
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
        if (ConfusionTimer+3 > StanCount)
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

        if (PlayerDistance < 2.4) 
        { 
        moveType = MoveType.Attack;
        }
    }
    void isAttack() 
    { 
    AttackCount += Time.deltaTime;
        if (mode == EnemyBase.ModeType.M1)
        {

            //Œø‰Ê‰¹
            audioManager.isPlaySE(AttackStayAudio);
            AttackCount = 0;
            mode = EnemyBase.ModeType.M2;
        }
        if (mode == EnemyBase.ModeType.M2) 
        {isPlayerLookBase();
            rb.velocity = new Vector2(rb.velocity.x*0.8f,rb.velocity.y*0.8f);
            animator.SetInteger("Anim", 2);
            if (AttackCount > 0.4) 
            {
                AttackCount = 0;
                mode = EnemyBase.ModeType.M3;
            }
        }if (mode == EnemyBase.ModeType.M3) 
        {

            //Œø‰Ê‰¹
            audioManager.isPlaySE(AttackStayAudio);
            AttackCount = 0;
            mode = EnemyBase.ModeType.M4;
        }
        if (mode == EnemyBase.ModeType.M4) 
        {
            
            rb.velocity = PlayerDirection.normalized * 4f;
            float Distance = Vector2.Distance(Player.transform.position, transform.position);
            if (Distance < 0.3) 
            {
                rb.velocity = rb.velocity * 0.1f;
                AttackCount = 0;
                mode = EnemyBase.ModeType.M5;
                audioManager.isPlaySE(AttackAudio);
            }
            if (AttackCount > 0.4f)
            {
                rb.velocity = rb.velocity * 0.1f;
                AttackCount = 0;
                mode = EnemyBase.ModeType.M5;
                audioManager.isPlaySE(AttackAudio);
            }
        }
        if (mode == EnemyBase.ModeType.M5)
        {
            if (AttackCount > 0.3) 
            { 
            mode = EnemyBase.ModeType.M6;
                AttackCount = 0;

                
            }
            
                

        }
        if (mode == EnemyBase.ModeType.M6)
        {
            AttackAreaSpawn();
                mode = EnemyBase.ModeType.M7;

        }
        if (mode == EnemyBase.ModeType.M7) 
        {
            animator.SetInteger("Anim", 3);
            if (AttackCount > 1)
            {
                animator.SetInteger("Anim", 1);
                AttackCount = 0;
                mode = EnemyBase.ModeType.M1;
                moveType = MoveType.Move;
                isPlayerLook();
            }
        }
    }
    private void isMove()
    {
        isPlayerLook();
        rb.velocity = PlayerDirection.normalized * SPEED;
        animator.SetInteger("Anim", 1);
    }
    void isPlayerLook() 
    {
        PlayerDirection = Player.transform.position - transform.position;
        PlayerDistance = Vector2.Distance(Player.transform.position, transform.position);
        if (Player.transform.position.x < transform.position.x)
        {
            EnemyAnimBody.transform.localScale = new Vector2(-2.5f, 2.5f);
        }else{
            EnemyAnimBody.transform.localScale = new Vector2(2.5f, 2.5f);
        }
    }
    
}
