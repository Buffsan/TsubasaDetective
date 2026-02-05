using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MO_LastHoliday_AI : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;
    [SerializeField] GameObject Attack;
    [SerializeField] GameObject Denger_AttackArea;

    [SerializeField] List<AudioClip> audioClips = new List<AudioClip>();

    Vector2 targetPos = Vector2.zero;

    private IEnumerator myRunningCoroutine; // 実行中のコルーチンを保持するための変数
    public float duration = 1;
    public float arcHeight = 30;
    float AttackCount1 = 0;
    bool StandOk = false;
    public bool AutAttack = true;
    float AttackCountStay = 0;

    public void A_NextPhase()
    {
        A_Phase++;

        enemyBase.AttackCount = 0;
    }
    public void A_Finish()
    {
        A_Phase = 0;
        enemyBase.AttackCount = 0;
        enemyBase.moveType = EnemyBase.MoveType.Move;
        StandOk = false;
        AttackCountStay = 0;
    }
    public override void EnemyConfusionPlay()
    {
        StandOk = false;
        A_Phase = 0;
        enemyBase.AttackCount = 0;
    }
    public override void EnemyStanPlay()
    {
        StandOk = false;
        A_Phase = 0;
        enemyBase.AttackCount = 0;
    }
    public override void EnemyMovePlay()
    {
        enemyBase.isPlayerLookBase();
        enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * enemyBase.SPEED;
        AttackCountStay += Time.fixedDeltaTime;
        if (!AutAttack) return;
        if (AttackCountStay > 3) 
        {
            AttackCountStay = 0;
            enemyBase.moveType = EnemyBase.MoveType.Attack;
        }
    }
    public override void EnemyAttackPlay()
    {
        enemyBase.AttackCount += Time.deltaTime;
        if (A_Phase ==0)
        {
            enemyBase.audioManager.PlaySE(enemyBase.AttackStayAudio);
            enemyBase.rb.velocity = Vector3.zero;
            A_NextPhase();
            enemyBase.animator.Play("攻撃待機",0,0);
        }
        if (A_Phase == 1)
        {
            enemyBase.isPlayerLookBase();
            enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized*enemyBase.SPEED*6;
            if (enemyBase.AttackCount > 0.5)
            {
                enemyBase.rb.velocity = Vector2.zero;
                enemyBase.animator.Play("攻撃待機２", 0, 0);
                A_NextPhase();
                SpawnObjectNow(Denger_AttackArea,transform.position, 1);
            }
        }
        if (A_Phase == 2)
        {
            
            if (enemyBase.AttackCount > 1)
            {
                enemyBase.animator.Play("攻撃", 0, 0);
                SpawnObjectNow(Attack,transform.position, 0.1f);
                A_NextPhase();
                AUDIO_manager.PlaySE(audioClips[0]);
            }
        }
        if (A_Phase == 3)
        {
            if (enemyBase.AttackCount > 2)
            {
                enemyBase.audioManager.PlaySE(enemyBase.AttackStayAudio);
                enemyBase.animator.Play("攻撃待機", 0, 0);
                A_NextPhase();
            }
        }
        if (A_Phase == 4) 
        {
            if (enemyBase.AttackCount > 0.5)
            {
                enemyBase.animator.Play("攻撃待機２", 0, 0);
                A_NextPhase();
                
            }
        }
        if (A_Phase == 5) 
        {
            if (enemyBase.AttackCount > 0.5)
            {
               A_NextPhase();
                targetPos = enemyBase.Player.transform.position;
                StartCoroutine(MoveInArc());
                enemyBase.isPlayerLookBase();
                AUDIO_manager.PlaySE(audioClips[1]);
            }
        }
        if (A_Phase == 6)
        {
            if (StandOk)
            {
                SpawnObjectNow(Attack, transform.position, 0.1f);
                A_NextPhase();
                enemyBase.animator.Play("攻撃",0,0);
                AUDIO_manager.PlaySE(audioClips[0]);
            }
        }
        if (A_Phase == 7) 
        {
            if (enemyBase.AttackCount > 1) 
            {
                A_Finish();
                enemyBase.animator.Play("歩行", 0, 0);
            }
        }
        
    }

    private IEnumerator MoveInArc()
    {
        enemyBase.animator.Play("跳躍", 0, 0);
        myRunningCoroutine = MoveInArc();
        // 移動開始前のBodyTypeを保存しておく
        var originalBodyType = enemyBase.rb.bodyType;
        // 移動中はKinematicにして、物理演算の影響をなくす
        enemyBase.rb.bodyType = RigidbodyType2D.Kinematic;

        Vector2 startPos = enemyBase.transform.position;
        Vector2 endPos = targetPos;

        GameObject CL_AttackPPP = Instantiate(Denger_AttackArea, endPos, Quaternion.identity);

        Destroy(CL_AttackPPP, 1.5f);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration); // 0から1の範囲に収める

            // XとYの直線的な移動を計算
            Vector2 linearPos = Vector2.Lerp(startPos, endPos, progress);

            // 高さを計算 (Sinカーブ)
            float height = Mathf.Sin(progress * Mathf.PI) * arcHeight;

            // Rigidbody2Dを使って位置を更新する
            // 2Dなので、Y座標に高さを加える
            enemyBase.rb.MovePosition(new Vector2(linearPos.x, linearPos.y + height));

            //yield return new WaitForFixedUpdate(); // 物理演算の更新タイミングで待つ
            yield return new WaitForEndOfFrame();
        }

       
        // BodyTypeを元に戻す
        enemyBase.rb.bodyType = originalBodyType;


        StandOk = true;
        
        Debug.Log("目的地に到着！" + targetPos);
    }
}
