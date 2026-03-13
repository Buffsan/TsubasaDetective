using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SE_Conductor_AI : EnemyMoveBase
{
    [SerializeField] GameObject RockNeedle;
    [SerializeField] Vector2 SearchPos = new Vector2 (10, 0);
    [SerializeField] GameObject RedScreen;
    [SerializeField] GameObject Text;
    [SerializeField] GameObject Hasira;

    SE_Conductor_Minion minion;
    [SerializeField] Vector2 minionPos = new Vector2 (0, 0);

    [SerializeField] float AttackLimitTime = 5;
    [SerializeField] int FindPlayerCount = 0;
    float AttacklimitCout = 0;

    [SerializeField] float SearchLimitTime = 8;
    [SerializeField] float SearchFindLimitTime = 3;
    [SerializeField] float SearchCount = 0;

    bool SEplayed = false;

    int Phase = 0;

    public enum EnemyPhase 
    { 
    
        Search,
        Attack,
        None,
    
    }
    [SerializeField] EnemyPhase enemyPhase = EnemyPhase.Search;

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
    [SerializeField] GameObject AllEnemyAttack;

    [SerializeField] AudioClip clip;
    [SerializeField] AudioClip clip1;
    [SerializeField] AudioClip clip2;
    private void Start()
    {
        enemyBase.EnemyAnimBody.SetActive(false);
        enemyBase.DamageBody.SetActive(false);
        enemyBase.Sliders.SetActive(false);
        RedScreen.SetActive(false);
        Text.SetActive(false);

        enemyBase.moveType = EnemyBase.MoveType.Move;

        GameObject CL_Minion = Instantiate(Hasira, minionPos, Quaternion.identity);
        minion = CL_Minion.GetComponent<SE_Conductor_Minion>();
        minion.MainObject = gameObject;

    }

    private void FixedUpdate()
    {
        switch (enemyPhase)
        {

            case EnemyPhase.Search:
                
                break;
            case EnemyPhase.Attack:
                if (AttackLimitTime * (1 + FindPlayerCount) < AttacklimitCout)
                {
                    enemyPhase = EnemyPhase.Search;
                    FindPlayerCount++;
                    AttacklimitCout = 0;
                    AttackCount = 0;
                    RedScreen.SetActive(false);
                }
                else 
                {
                    AttacklimitCout += Time.fixedDeltaTime;
                }
                break;
        }
    }

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

        switch (enemyPhase) 
        {

            case EnemyPhase.Search:
                SearchMode();
                break;
                case EnemyPhase.Attack:
                AttackMode();
                break;
        }
       
    }
    void SearchMode() 
    {
        enemyBase.EnemyAnimBody.SetActive(false);
        enemyBase.DamageBody.SetActive(false);
        enemyBase.Sliders.SetActive(false);
        
        
        transform.position = SearchPos;

        if (enemyBase.ALL_System.systemManager.AllEnemy.Count <= 1) 
        {

            Destroy(enemyBase.gameObject);
        
        }

        SearchCount += Time.fixedDeltaTime;
        if (SearchCount > SearchLimitTime) 
        { 
            Text.SetActive(true);
            RedScreen.SetActive(true);
            if (!SEplayed) 
            {
                enemyBase.audioManager.PlaySE(clip1);
                SEplayed = true;

                GameObject CL_Attack = Instantiate(AllEnemyAttack, transform.position, Quaternion.identity);
                Destroy(CL_Attack,0.1f);
            }
            if (SearchCount > SearchLimitTime + SearchFindLimitTime) 
            { 
                SearchCount = 0;
                Text.SetActive(false);
                SEplayed = false;
                GameObject CL_Attack = Instantiate(AllEnemyAttack, transform.position, Quaternion.identity);
                CL_Attack.transform.parent = transform;
                Destroy(CL_Attack, 0.1f);
                if (minion.PlayerHide)
                {
                    RedScreen.SetActive(false);
                    
                }
                else 
                {
                    enemyBase.audioManager.PlaySE(clip2);
                    enemyPhase = EnemyPhase.Attack;
                }
            }
        }
    }
    void AttackMode() 
    {

        enemyBase.EnemyAnimBody.SetActive(true);
        enemyBase.DamageBody.SetActive(true);
        enemyBase.Sliders.SetActive(true);
        RedScreen.SetActive(true);
        Text.SetActive(false);

        WaitCunt += Time.deltaTime;
        if (WaitCunt > RandomTime)
        {
            A_Phase = Random.Range(0, 2);
            WaitCunt = 0;
            enemyBase.moveType = EnemyBase.MoveType.Attack;
            RandomTime = Random.Range(2f, 5f);
        }
        enemyBase.isPlayerLookBase();
        enemyBase.rb.velocity = enemyBase.PlayerDirection * enemyBase.SPEED;
    }
    public override void EnemyAttackPlay()
    {
        A2();
    }
    void A2()
    {
        enemyBase.AttackCount += Time.deltaTime;
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            enemyBase.audioManager.PlaySE(enemyBase.AttackStayAudio);
            enemyBase.audioManager.PlaySE(clip);
            enemyBase.rb.velocity = Vector3.zero;
            enemyBase.mode = EnemyBase.ModeType.M2;
            enemyBase.animator.SetInteger("Anim", 2);
            enemyBase.isPlayerLookBase();
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {
            if (enemyBase.AttackCount > 0.2)
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
            if (duration < enemyBase.AttackCount)
            {
                enemyBase.mode = EnemyBase.ModeType.M5;
                enemyBase.AttackCount = 0;

                GameObject CL_Attack = Instantiate(enemyBase.AttackArea, enemyBase.transform.position, Quaternion.identity);
                EnemyAttack enemyAttack = CL_Attack.GetComponent<EnemyAttack>();
                enemyAttack.enemyBase = enemyBase;
                Destroy(CL_Attack, 0.1f);
                enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
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

            if (enemyBase.AttackCount > 0.1f)
            {
                int random = Random.Range(0, 2);
                if (random == 0)
                {
                    enemyBase.mode = EnemyBase.ModeType.M1;
                    enemyBase.AttackCount = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Move;
                    enemyBase.PlayerDistance = 1000;
                    enemyBase.animator.SetInteger("Anim", 0);
                }
                else 
                {
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
        SaveComponent saveComponent = CL_AttackPPP.GetComponent<SaveComponent>();
        saveComponent.animator.speed = 1 / duration;

        Destroy(CL_AttackPPP, duration * 1.5f);
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

