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
    public override void EnemyConfusionPlay()
    {
        AttackFinish();
    }
    public override void EnemyStanPlay()
    {
        AttackFinish();
    }
    public override void EnemyMovePlay()
    {
        enemyBase.isPlayerLookBase();
        enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * enemyBase.SPEED;

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
            enemyBase.isPlayerLookBase();
            enemyBase.rb.velocity = Vector2.zero;
            GameObject CL_Effect = SpawnObjectNow_Input(AttackChargeEffect, transform.position, 4);
            GameObject CL_DengerArea = SpawnObjectNow_Input(Denger_area,transform.position,4);
            CL_DengerArea.transform.up = enemyBase.PlayerDirection;
            SaveComponent animator = CL_DengerArea.GetComponent<SaveComponent>();
            animator.animator.speed = 0.33f;
            enemyBase.animator.Play("UŒ‚‘Ò‹@", 0, 0);
            AUDIO_manager.isPlaySE(audioClips[0]);
            A_Phase = 1;
        }
        if (A_Phase == 1) 
        {
            if (enemyBase.AttackCount > 3) 
            {
                GameObject CL_Effect = SpawnObjectNow_Input(Effect, transform.position, 3);
                GameObject CL_Attack = SpawnObjectNow_Input(AttackArea, transform.position, 0.3f);
                CL_Effect.transform.up = enemyBase.PlayerDirection;
                CL_Attack.transform.up = enemyBase.PlayerDirection;
                enemyBase.AttackCount = 0;
                A_Phase =2;
                AUDIO_manager.isPlaySE(audioClips[1]);
                enemyBase.animator.Play("UŒ‚", 0, 0);
            }
        }
        if (A_Phase == 2) 
        {
            enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * -0.5f;
            if (enemyBase.AttackCount > 1) 
            {
                
                enemyBase.animator.Play("‘Ò‹@", 0, 0);
                AttackFinish();
            }
        }
    }
}
