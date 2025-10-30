using TMPro;
using UnityEngine;


public class Boss_Controller : EnemyBase
{
    [SerializeField] Boss_MoveBase moveBase;
    [SerializeField] GameObject EnemyMoveBaseObject;

    public TextMeshProUGUI HPtext;

    
    private void Start()
    {
        if (moveBase == null)
        {
            GameObject CL_MoveBase = Instantiate(EnemyMoveBaseObject, transform.position, Quaternion.identity);
            moveBase = CL_MoveBase.GetComponent<Boss_MoveBase>();
            CL_MoveBase.transform.SetParent(gameObject.transform);
        }
        


        moveBase.enemyBase = this;
        


        ALL_System.systemManager.AllEnemy.Add(this.gameObject);

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
            case Status.Die:
                foreach (var enemys in moveBase.enemyList) 
                {
                    Destroy(enemys.gameObject);
                }
                moveBase.enemyList.Clear();
                return; // éÄñSíÜÇÕàÍêÿìÆÇ©Ç≥Ç»Ç¢
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
            moveBase.M_Con();
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
            moveBase.M_Stan();
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
    public void isTargetLook(Vector2 target) 
    {
        if (target.x < transform.position.x)
        {
            EnemyAnimBody.transform.localScale = new Vector2(-2.5f, 2.5f);
        }
        else
        {
            EnemyAnimBody.transform.localScale = new Vector2(2.5f, 2.5f);
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
