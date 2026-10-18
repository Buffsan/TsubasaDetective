using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CG_Knght : EnemyBase
{
    public EnemyMoveBase moveBase;

    // この旧式騎士はEnemyContorollerを継承していないため独自に互換対応。
    [SerializeField] EnemyContoroller.TargetType targetType = EnemyContoroller.TargetType.PlayerOnly;
    [SerializeField] bool targetAllFactions = true;
    [SerializeField] Faction targetFactions = Faction.None;
    [SerializeField] Faction excludedTargetFactions = Faction.None;
    [SerializeField, Min(0.05f)] float targetSearchInterval = 0.25f;
    public Transform CurrentTarget { get; private set; }
    float targetSearchTimer;

    float TargetDistance => CurrentTarget != null
        ? Vector2.Distance(CurrentTarget.position, transform.position)
        : float.PositiveInfinity;

    void RefreshTarget()
    {
        if (targetType == EnemyContoroller.TargetType.PlayerOnly)
        {
            CurrentTarget = Player != null ? Player.transform : null;
            return;
        }
        if (moveType == MoveType.Attack) return;
        targetSearchTimer += Time.fixedDeltaTime;
        if (targetSearchTimer < Mathf.Max(0.05f, targetSearchInterval) &&
            CurrentTarget != null) return;
        if (targetSearchTimer < Mathf.Max(0.05f, targetSearchInterval) &&
            CurrentTarget == null && targetSearchTimer > Time.fixedDeltaTime) return;
        targetSearchTimer = 0f;

        Transform enemy = null;
        float distanceSqr = float.PositiveInfinity;
        SystemManager manager = SystemManager.Instance;
        if (manager != null && manager.AllEnemy != null)
        {
            foreach (GameObject obj in manager.AllEnemy)
            {
                if (obj == null || obj == gameObject) continue;
                EnemyBase other = obj.GetComponent<EnemyBase>();
                if (other == null || other == this || other.HP <= 0f ||
                    other.status == Status.Die) continue;
                if ((other.Factions & excludedTargetFactions) != Faction.None) continue;
                if (!targetAllFactions && (other.Factions & targetFactions) == Faction.None)
                    continue;
                float sqr = ((Vector2)(other.transform.position - transform.position)).sqrMagnitude;
                if (sqr < distanceSqr)
                {
                    distanceSqr = sqr;
                    enemy = other.transform;
                }
            }
        }

        if (targetType == EnemyContoroller.TargetType.EnemyOnly)
            CurrentTarget = enemy;
        else if (Player == null)
            CurrentTarget = enemy;
        else if (enemy == null)
            CurrentTarget = Player.transform;
        else
        {
            float playerSqr = ((Vector2)(Player.transform.position - transform.position)).sqrMagnitude;
            CurrentTarget = distanceSqr < playerSqr ? enemy : Player.transform;
        }
    }

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
        if (ConfusionknockBackTimer+3 > StanCount)
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
        RefreshTarget();
        if (targetType == EnemyContoroller.TargetType.EnemyOnly &&
            CurrentTarget == null && moveType != MoveType.Attack)
        {
            rb.velocity = Vector2.zero;
            animator.SetInteger("Anim", 0);
            return;
        }
        switch (moveType) 
        {
            case MoveType.Move:
                isMove();
                break;
            case MoveType.Attack:
                isAttack();
                break;
        
        }

        if (TargetDistance < 2.4f) 
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
            audioManager.PlaySE(AttackStayAudio);
            AttackCount = 0;
            mode = EnemyBase.ModeType.M2;
        }
        if (mode == EnemyBase.ModeType.M2) 
        {isPlayerLook();
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
            audioManager.PlaySE(AttackStayAudio);
            AttackCount = 0;
            mode = EnemyBase.ModeType.M4;
        }
        if (mode == EnemyBase.ModeType.M4) 
        {
            
            rb.velocity = PlayerDirection.normalized * 4f;
            float Distance = TargetDistance;
            if (Distance < 0.3) 
            {
                rb.velocity = rb.velocity * 0.1f;
                AttackCount = 0;
                mode = EnemyBase.ModeType.M5;
                audioManager.PlaySE(AttackAudio);
            }
            if (AttackCount > 0.4f)
            {
                rb.velocity = rb.velocity * 0.1f;
                AttackCount = 0;
                mode = EnemyBase.ModeType.M5;
                audioManager.PlaySE(AttackAudio);
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
        if (CurrentTarget == null)
        {
            PlayerDirection = Vector2.zero;
            PlayerDistance = float.PositiveInfinity;
            return;
        }
        PlayerDirection = CurrentTarget.position - transform.position;
        PlayerDistance = TargetDistance;
        if (CurrentTarget.position.x < transform.position.x)
        {
            EnemyAnimBody.transform.localScale = new Vector2(-2.5f, 2.5f);
        }else{
            EnemyAnimBody.transform.localScale = new Vector2(2.5f, 2.5f);
        }
    }
    
}
