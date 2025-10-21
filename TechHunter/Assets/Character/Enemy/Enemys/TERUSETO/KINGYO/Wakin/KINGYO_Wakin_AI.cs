using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KINGYO_Wakin_AI : EnemyMoveBase
{
    [SerializeField] GameObject Koaka;
    [SerializeField] GameObject RockNeedle;

    int Phase = 0;

    float PhaseCount = 0;
    float PhaseCount1 = 0;
    Vector2 SaveAngle = Vector2.zero;
    //Vector2 SavePos = Vector2.zero;

    bool one = false;
    Vector2 Direction;
    bool trune = false;
    public float RandomTime;
    float AttackCount1 = 0;

    public float WaitCunt = 0;


    private IEnumerator myRunningCoroutine; // 実行中のコルーチンを保持するための変数
    public float duration = 1;
    public float arcHeight = 30;
    bool StandOk = false;
    Vector2 targetPos = Vector2.zero;
    [SerializeField] GameObject Denger_AttackArea;
    [SerializeField] GameObject Denger_Attackarea1;
    [SerializeField] GameObject AttackArea;

    public override void EnemyConfusionPlay()
    {
        StandOk = false;
        A_Phase = 0;
        enemyBase.AttackCount = 0;
        AttackArea.SetActive(false);
    }
    public override void EnemyStanPlay()
    {
        StandOk = false;
        A_Phase = 0;
        enemyBase.AttackCount = 0;
        AttackArea.SetActive(false);
    }
    public override void EnemyMovePlay()
    {
        if (!one)
        {
            one = true;
            RandomTime = Random.Range(1f, 3f);
            int boolrandom = Random.Range(0, 2);
            if (boolrandom == 0)
            {
                trune = true;
            }
            else
            {
                trune = false;
            }
        }
        WaitCunt += Time.deltaTime;
        if (WaitCunt > RandomTime)
        {
            A_Phase = Random.Range(0, 2);
            WaitCunt = 0;
            enemyBase.moveType = EnemyBase.MoveType.Attack;
            RandomTime = Random.Range(2f, 5f);
        }

        enemyBase.PlayerDistance = Vector2.Distance(enemyBase.Player.transform.position, enemyBase.gameObject.transform.position);
        enemyBase.animator.SetInteger("Anim", 1);
        enemyBase.PlayerDirection = enemyBase.Player.transform.position - enemyBase.transform.position;
        float angle = Mathf.Atan2(enemyBase.PlayerDirection.y, enemyBase.PlayerDirection.x) * Mathf.Rad2Deg;
        float offset = 100f / (enemyBase.PlayerDistance + 1f) * 3f; // 距離が遠いほど小さく

        if (trune)
        {
            angle += offset;
        }
        else
        {
            angle -= offset;
        }

        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

        enemyBase.isPlayerLookBase();

        enemyBase.rb.velocity = direction * enemyBase.SPEED;
    }
    public override void EnemyAttackPlay()
    {
        if (A_Phase == 0) A1();
        if (A_Phase == 1) A2();
    }
    void A2() 
    {
        enemyBase.AttackCount += Time.deltaTime;
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            enemyBase.audioManager.isPlaySE(enemyBase.AttackStayAudio);
            enemyBase.rb.velocity = Vector3.zero;
            enemyBase.mode = EnemyBase.ModeType.M2;
            enemyBase.animator.SetInteger("Anim", 2);
            enemyBase.isPlayerLookBase();
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {
            if (enemyBase.AttackCount > 0.4)
            {
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
              enemyBase.mode = EnemyBase.ModeType.M4;
                enemyBase.AttackCount = 0;
                targetPos = enemyBase.Player.transform.position;
                StartCoroutine(MoveInArc());
                enemyBase.isPlayerLookBase();          
        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {
            if (StandOk)
            {
                enemyBase.mode = EnemyBase.ModeType.M5;
                enemyBase.AttackCount = 0;

                GameObject CL_Attack = Instantiate(enemyBase.AttackArea, enemyBase.transform.position, Quaternion.identity);
                EnemyAttack enemyAttack = CL_Attack.GetComponent<EnemyAttack>();
                enemyAttack.enemyBase = enemyBase;
                Destroy(CL_Attack, 0.1f);
                enemyBase.audioManager.isPlaySE(enemyBase.AttackAudio);
                StandOk = false;

                for (int j = 0; j < 10; j++)
                {
                    StartCoroutine(NeedleSpawnLing(0.05f * j, j * 0.5f, 0));
                }
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M5)
        {
            
            
            enemyBase.animator.SetInteger("Anim", 3);
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

    void A1() 
    {
        enemyBase.AttackCount += Time.deltaTime;
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            enemyBase.audioManager.isPlaySE(enemyBase.AttackStayAudio);
            enemyBase.rb.velocity = Vector3.zero;
            enemyBase.mode = EnemyBase.ModeType.M2;
            enemyBase.animator.SetInteger("Anim", 2);
            enemyBase.isPlayerLookBase();

            GameObject CL_Denger = Instantiate(Denger_Attackarea1, transform.position, Quaternion.identity);
            CL_Denger.transform.up = enemyBase.PlayerDirection.normalized;
            Destroy(CL_Denger ,1);
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {

            if (enemyBase.AttackCount > 0.4)
            {
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;
                AttackArea.SetActive(true);

            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            //enemyBase.isPlayerLookBase();
            AttackCount1 += Time.fixedDeltaTime;
            if (AttackCount1 > 0.05)
            {
                AttackCount1 = 0;
                SpawnRockNeedle(transform.position);
            }
            enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * enemyBase.SPEED * 9;
            if (enemyBase.AttackCount > 0.3f)
            {
                enemyBase.mode = EnemyBase.ModeType.M4;
                enemyBase.AttackCount = 0;
                AttackArea.SetActive(false);
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {
            enemyBase.rb.velocity = new Vector2(enemyBase.rb.velocity.x * 0.5f, enemyBase.rb.velocity.y * 0.5f);
            if (enemyBase.AttackCount > 0.3)
            {
                enemyBase.mode = EnemyBase.ModeType.M5;
                enemyBase.AttackCount = 0;


            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M5)
        {
            

            enemyBase.audioManager.isPlaySE(enemyBase.AttackAudio);
            enemyBase.animator.SetInteger("Anim", 3);
            enemyBase.mode = EnemyBase.ModeType.M6;

        }
        if (enemyBase.mode == EnemyBase.ModeType.M6)
        {

            if (enemyBase.AttackCount > 0.1)
            {
                int i = Random.Range(0, 2);
                if (i == 0) { 
                enemyBase.mode = EnemyBase.ModeType.M1;
                enemyBase.AttackCount = 0;
                enemyBase.moveType = EnemyBase.MoveType.Move;
                enemyBase.PlayerDistance = 1000;
                enemyBase.animator.SetInteger("Anim", 0);
                } else {
                    enemyBase.mode = EnemyBase.ModeType.M1;
                    enemyBase.AttackCount = 0;
                    enemyBase.PlayerDistance = 1000;
                    enemyBase.animator.SetInteger("Anim", 0);
                }
                
            }
        }

        
    }
        public void SpawnRockNeedle(Vector2 pos)
        {
            GameObject CL_Rock = Instantiate(RockNeedle, pos, Quaternion.identity);
            Destroy(CL_Rock, 5);
        }
    private IEnumerator MoveInArc()
    {
        enemyBase.animator.Play("攻撃", 0, 0);
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

        // 念のため、最終的にきっちり目標地点に合わせる
        transform.position = endPos;
        // BodyTypeを元に戻す
        enemyBase.rb.bodyType = originalBodyType;


        StandOk = true;
        // 念のため、最終的にきっちり目標地点に合わせる
        transform.position = targetPos;
        Debug.Log("目的地に到着！" + targetPos);
    }
    private IEnumerator NeedleSpawnLing(float delayTime, float Lange, int Plass)
    {
        //Debug.Log("コルーチンが開始されました。今から " + delayTime + " 秒待ちます...");

        // ここで指定した秒数だけ待機する
        yield return new WaitForSeconds(delayTime);
        for (int i = 0; i < 10 + Plass; i++)
        {

            float angle = (360 / 10 + Plass) * i;
            float angleRad = angle * Mathf.Deg2Rad;

            float newX = enemyBase.transform.position.x + 2 * Lange * Mathf.Cos(angleRad);
            float newY = enemyBase.transform.position.y + 2 * Lange * Mathf.Sin(angleRad);

            // 新しい位置にクローンを作成
            Vector2 newPosition = new Vector2(newX, newY);

            SpawnRockNeedle(newPosition);

        }
    }
}