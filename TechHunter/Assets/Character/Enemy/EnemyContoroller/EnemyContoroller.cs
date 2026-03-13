using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyContoroller : EnemyBase
{
    public EnemyMoveBase moveBase;
    [SerializeField] GameObject EnemyMoveBaseObject;
    public enum MoveToBase
    {
        Out,
        Special
    }
    public MoveToBase Choice_MoveToBase = MoveToBase.Out;
    private void Start()
    {
        if (moveBase == null)
        {
            GameObject CL_MoveBase = Instantiate(EnemyMoveBaseObject, transform.position, Quaternion.identity);
            moveBase = CL_MoveBase.GetComponent<EnemyMoveBase>();
            CL_MoveBase.transform.SetParent(gameObject.transform);
        }
        moveBase.enemyBase = this;
    }
    private void FixedUpdate()
    {
        //敵全体のAI管理
        switch (status)
        {
            case Status.Nomal://敵ごとの自由行動
                NomalActive();
                break;
            case Status.Stan://スタン（ダメージによる行動中断）
                StanStanActive();
                break;
            case Status.ConfusionResistance://混乱（ダメージによる行動不能状態）
                ConfusionActive();
                break;
        }
    }
    void ConfusionActive()
    {
        animator.SetBool("Damage", true);
        if (ConfusionknockBackTimer + 3 + playerController.ConfusionTime + ConfusionAddTime > StanCount)
        {
            moveBase.EnemyConfusionPlay();
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
    void StanStanActive()
    {
        animator.SetBool("Damage", true);
        if (StanTime > StanCount)
        {
            moveBase.EnemyStanPlay();
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
    private void NomalActive()
    {
        switch (moveType)
        {
            case MoveType.Move:
                MoveAction();
                break;
            case MoveType.Attack:
                AttackAction();
                break;
        }
        if (PlayerDistance < LookPlayerDistance)
        {
            moveType = MoveType.Attack;
        }
    }
    void AttackAction()
    {//攻撃モーションを別途入れる
        moveBase.EnemyAttackPlay();
    }
    private void MoveAction()
    {
        if (Choice_MoveToBase == MoveToBase.Out)
        {
            PlayerLook();
            rb.velocity = PlayerDirection.normalized * SPEED;
            animator.SetInteger("Anim", 1);
        }
        else 
        { 
            moveBase.EnemyMovePlay();
        }
    }
    public void PlayerLook()
    {
        PlayerDirection = Player.transform.position - transform.position;
        PlayerDistance = Vector2.Distance(Player.transform.position, transform.position);
        if (Player.transform.position.x < transform.position.x)
        {
            EnemyAnimBody.transform.localScale = new Vector2(-DefoScale, DefoScale);
        }
        else
        {
            EnemyAnimBody.transform.localScale = new Vector2(DefoScale, DefoScale);
        }
    }
}
