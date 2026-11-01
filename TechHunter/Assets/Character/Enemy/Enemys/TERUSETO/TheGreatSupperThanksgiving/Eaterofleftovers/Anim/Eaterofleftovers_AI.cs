using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eaterofleftovers_AI : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;

    int Phase = 0;
    bool trune = false; bool Strate = false;
    float PhaseCount = 0;
    bool one = false;
    float PhaseCount1 = 0;
    Vector2 SaveAngle = Vector2.zero;
    Vector2 SavePos = Vector2.zero;
    public float RandomTime;

    [SerializeField] GameObject AttackBefore;
    [SerializeField] float MinDashCount = 0.1f;
    [SerializeField] float MaxDashCount = 0.4f;
    [SerializeField] int DashCountLimit = 5;
    [SerializeField] GameObject AttackPoint;
    [SerializeField] float DashSpeed = 5;
    float AttackMeeleLimit = 0;

    bool DashWait = false;

    public float WaitCunt = 0;
    public float LestTime = 0;
    float LestCount = 0;

    // TargetDistance は対象消失時に Infinity になることがある。
    // Infinity を角度へ渡すと Mathf.Sin/Cos が NaN を返すため、移動前に検査する。
    static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    static bool IsFinite(Vector2 value)
    {
        return IsFinite(value.x) && IsFinite(value.y);
    }

    void SetSafeVelocity(Vector2 velocity)
    {
        enemyBase.rb.velocity = IsFinite(velocity) ? velocity : Vector2.zero;
    }

    Vector2 SafeTargetDirection()
    {
        Vector2 direction = TargetDirection;
        if (!IsFinite(direction) || direction.sqrMagnitude < 0.000001f)
            return Vector2.zero;
        return direction.normalized;
    }

    public override void EnemyConfusionPlay()
    {
        enemyBase.DamageBody.SetActive(false);
    }
    public override void EnemyStanPlay()
    {
        enemyBase.DamageBody.SetActive(false); //Debug.Log("¬—");
    }
    public override void EnemyMovePlay()
    {
        enemyBase.DamageBody.SetActive(false);
        if (WaitCunt > RandomTime && !Strate || WaitCunt > RandomTime * 2 && Strate)
        {
            WaitCunt = 0;
            one = false;
            DashWait = true;
        }

        if (!DashWait)
        {
            if (!one)
            {
                one = true;
                RandomTime = Random.Range(MinDashCount, MaxDashCount);
                int boolrandom = Random.Range(0, 3);
                if (boolrandom == 0)
                {
                    trune = true;
                    Strate = false;
                }
                else if (boolrandom == 1)
                {
                    trune = false;
                    Strate = false;
                }
                else 
                {
                    Strate = true;
                }
            }
            WaitCunt += Time.deltaTime;

            RefreshTargetInfo();
            enemyBase.animator.SetInteger("Anim", 1);

            // 魅了・死亡・敵VS敵でターゲットが消えたフレームは安全に停止。
            // 有限でない距離を角度計算へ渡さない。
            Vector2 targetDirection = TargetDirection;
            float targetDistance = TargetDistance;
            float speed = enemyBase.SPEED;
            if (CurrentTarget == null || !IsFinite(targetDirection) ||
                !IsFinite(targetDistance) || !IsFinite(speed) ||
                targetDirection.sqrMagnitude < 0.000001f)
            {
                SetSafeVelocity(Vector2.zero);
                return;
            }

            if (Strate)
            {
                SetSafeVelocity(targetDirection.normalized * speed);
            }
            else
            {
                float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
                float turn = 150f - targetDistance * 12f;
                angle += trune ? turn : -turn;

                if (!IsFinite(angle))
                {
                    SetSafeVelocity(Vector2.zero);
                    return;
                }

                Vector2 direction = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

                if (direction.y > 0)
                {
                    enemyBase.EnemyAnimBody.transform.localScale = new Vector2(2.5f, 2.5f);
                }
                else
                {
                    enemyBase.EnemyAnimBody.transform.localScale = new Vector2(-2.5f, 2.5f);
                }

                SetSafeVelocity(direction * speed);
            }
        }
        else 
        {
            LestCount += Time.deltaTime;
            SetSafeVelocity(enemyBase.rb.velocity * 0.3f);
            if (LestCount > LestTime ) 
            {
                LestCount = 0;
                DashWait = false;
            }
        }
    }

    public override void EnemyAttackPlay()
    {
        enemyBase.AttackCount += Time.deltaTime;
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            enemyBase.audioManager.PlaySE(enemyBase.AttackStayAudio);
            enemyBase.rb.velocity = Vector3.zero;
            enemyBase.mode = EnemyBase.ModeType.M2;
            enemyBase.animator.SetInteger("Anim", 2); 
            
            LookAtCurrentTarget();
            GameObject CL_Before = Instantiate(AttackBefore, enemyBase.AttackSpawnPoint.transform.position,Quaternion.identity);
            Vector2 warningDirection = SafeTargetDirection();
            CL_Before.transform.up = warningDirection == Vector2.zero ? Vector2.up : warningDirection;
            Destroy(CL_Before, 1);

            
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {
            
            if (enemyBase.AttackCount > 1)
            {enemyBase.DamageBody.SetActive(true);
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;
                enemyBase.animator.SetInteger("Anim", 3);
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            
            float safeDashSpeed = IsFinite(DashSpeed) ? DashSpeed : 0f;
            SetSafeVelocity(SafeTargetDirection() * safeDashSpeed);
            if (enemyBase.AttackCount > 0.5f)
            {
                enemyBase.DamageBody.SetActive(false);
                enemyBase.mode = EnemyBase.ModeType.M4;
                enemyBase.AttackCount = 0;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {
            enemyBase.animator.SetInteger("Anim", 0);
            SetSafeVelocity(enemyBase.rb.velocity * 0.5f);
            if (enemyBase.AttackCount > 0.3)
            {
                enemyBase.mode = EnemyBase.ModeType.M5;
                enemyBase.AttackCount = 0;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M5)
        {
            /*
            GameObject CL_Attack = Instantiate(enemyBase.AttackArea, enemyBase.transform.position, Quaternion.identity);
        ConfigureSummonedAttack(CL_Attack);
            EnemyAttack enemyAttack = CL_Attack.GetComponent<EnemyAttack>();
            enemyAttack.enemyBase = enemyBase;
            Destroy(CL_Attack, 0.01f);*/

            enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
            
            enemyBase.mode = EnemyBase.ModeType.M6;

        }
        if (enemyBase.mode == EnemyBase.ModeType.M6)
        {

            if (enemyBase.AttackCount > 1)
            {
                enemyBase.mode = EnemyBase.ModeType.M1;
                enemyBase.AttackCount = 0;
                enemyBase.moveType = EnemyBase.MoveType.Move;
                enemyBase.PlayerDistance = 1000;
                enemyBase.animator.SetInteger("Anim", 0);
            }
        }
    }
}