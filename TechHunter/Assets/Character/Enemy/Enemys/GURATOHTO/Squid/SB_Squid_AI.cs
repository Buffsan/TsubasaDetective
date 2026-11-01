using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SB_Squid_AI : EnemyMoveBase
{
    [SerializeField] GameObject Denger_area;
    [SerializeField] GameObject Effect;
    [SerializeField] GameObject AttackChargeEffect;
    [SerializeField] GameObject AttackArea;
    [SerializeField] List<AudioClip> audioClips = new List<AudioClip>();
    float moveCount = 0;
    // プレイヤー戦と同じく、攻撃開始時点の照準を発射まで固定する。
    Vector2 lockedAttackDirection = Vector2.zero;
    public override void EnemyConfusionPlay()
    {
        lockedAttackDirection = Vector2.zero;
        AttackFinish();
    }
    public override void EnemyStanPlay()
    {
        lockedAttackDirection = Vector2.zero;
        AttackFinish();
    }
    public override void EnemyMovePlay()
    {
        if (CurrentTarget == null)
        {
            enemyBase.rb.velocity = Vector2.zero;
            moveCount = 0f;
            return;
        }
        LookAtCurrentTarget();
        enemyBase.rb.velocity = TargetDirection.normalized * enemyBase.SPEED;

        moveCount += Time.fixedDeltaTime;
        if (moveCount > 1) 
        { 
            moveCount = 0;
            enemyBase.moveType = EnemyBase.MoveType.Attack;
        }
    }
    public override void EnemyAttackPlay()
    {
        enemyBase.AttackCount += Time.fixedDeltaTime;
        if (A_Phase == 0) 
        {
            // 敵VS敵では構え開始前に標的が死亡することがある。
            // プレイヤー戦でも同じ判定を通し、標的不在なら誤射しない。
            if (CurrentTarget == null)
            {
                enemyBase.rb.velocity = Vector2.zero;
                lockedAttackDirection = Vector2.zero;
                AttackFinish();
                return;
            }

            LookAtCurrentTarget();
            lockedAttackDirection = TargetDirection;
            if (lockedAttackDirection.sqrMagnitude <= 0.000001f)
            {
                enemyBase.rb.velocity = Vector2.zero;
                lockedAttackDirection = Vector2.zero;
                AttackFinish();
                return;
            }

            enemyBase.rb.velocity = Vector2.zero;
            GameObject CL_Effect = SpawnObjectNow_Input(AttackChargeEffect, transform.position, 4);
            GameObject CL_DengerArea = SpawnAttackWarning(EnemyContoroller.WarningRectangle, Denger_area, transform.position, Quaternion.identity, 4f, null, 0f, false);
            if (CL_DengerArea != null) CL_DengerArea.transform.up = lockedAttackDirection;
            SaveComponent warningSave = CL_DengerArea.GetComponentInChildren<SaveComponent>(true);
            if (warningSave != null && warningSave.animator != null)
                warningSave.animator.speed = 0.33f;
            enemyBase.animator.Play("攻撃待機", 0, 0);
            AUDIO_manager.PlaySE(audioClips[0]);
            A_Phase = 1;
        }
        if (A_Phase == 1) 
        {
            if (enemyBase.AttackCount > 3) 
            {
                GameObject CL_Effect = SpawnObjectNow_Input(Effect, transform.position, 3);
                GameObject CL_Attack = SpawnObjectNow_Input(AttackArea, transform.position, 0.3f);
                CL_Effect.transform.up = lockedAttackDirection;
                CL_Attack.transform.up = lockedAttackDirection;
                enemyBase.AttackCount = 0;
                A_Phase =2;
                AUDIO_manager.PlaySE(audioClips[1]);
                enemyBase.animator.Play("攻撃", 0, 0);
            }
        }
        if (A_Phase == 2) 
        {
            enemyBase.rb.velocity = lockedAttackDirection.normalized * -0.5f;
            if (enemyBase.AttackCount > 1) 
            {
                
                enemyBase.animator.Play("待機", 0, 0);
                lockedAttackDirection = Vector2.zero;
                AttackFinish();
            }
        }
    }
}
