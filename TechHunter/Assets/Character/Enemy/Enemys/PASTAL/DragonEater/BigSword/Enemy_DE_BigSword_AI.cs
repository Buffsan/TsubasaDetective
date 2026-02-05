using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_DE_BigSword_AI : EnemyMoveBase
{
    [SerializeField] GameObject DengerArea;
    [SerializeField] GameObject AttackArea;
    [SerializeField] GameObject Effect;
    [SerializeField] List<AudioClip> audioClips = new List<AudioClip>();
    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;
    public override void EnemyStanPlay()
    {
        A_Phase = 0;
        AttackCount = 0;
    }
    public override void EnemyConfusionPlay()
    {
        A_Phase = 0;
        AttackCount = 0;
    }
    public override void EnemyAttackPlay()
    {
        AttackCount += Time.fixedDeltaTime;
        if (A_Phase == 0) 
        {
            enemyBase.rb.velocity = Vector3.zero;
            enemyBase.animator.Play("UŒ‚‘Ò‹@");
            DengerAreaSpawn(DengerArea, transform.position, 3);
            AUDIO_manager.PlaySE(audioClips[0]);
            AttackNext();
        }
        if (A_Phase == 1) 
        {
            if (AttackCount > 3) 
            {
                enemyBase.animator.Play("UŒ‚");
                AUDIO_manager.PlaySE(audioClips[1]);
                ALL_System.camera_Controller.Shake(0.1f,0.2f);
                SpawnObjectNow(AttackArea,transform.position,0.2f);
                SpawnObjectNow(Effect, transform.position, 1);
                AttackNext();
            }
        }
        if (A_Phase == 2) 
        {
            if (AttackCount > 1) 
            {
                enemyBase.animator.Play("‘Ò‹@");
                enemyBase.animator.SetInteger("Anim", 0);
                AttackNext();
            }
        }
        if (A_Phase == 3)
        {
            if (AttackCount > 1)
            {
                enemyBase.isPlayerLookBase();
                AttackFinish();

            }
        }
    }
}
