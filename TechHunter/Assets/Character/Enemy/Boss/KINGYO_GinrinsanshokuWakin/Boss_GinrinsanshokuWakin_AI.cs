using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_GinrinsanshokuWakin_AI : Boss_MoveBase
{
    [SerializeField] List<GameObject> ShotPoint;
    [SerializeField] List<GameObject> HoudanShotPoint;
    [SerializeField] GameObject Arrow;
    [SerializeField] GameObject AsaltAttackBody;
    [SerializeField] float ArrowDestroy = 20;
    [SerializeField] float AsaltSpeed = 10;
    [SerializeField] GameObject Moter;
    Vector2 SaveDirection = Vector2.zero;
    [SerializeField] float ArrowSpeed = 2;
    public GameObject S_AttackArea;

    [SerializeField] List<GameObject> EnemySpawnPoint = new List<GameObject>();
    [SerializeField] GameObject StarongKnight;
    [SerializeField] GameObject StorongSpier;
    [SerializeField] GameObject Tank;
    [SerializeField] int SpawnCount = 0;
    Vector2 targetDirection = Vector2.zero;
    [SerializeField] GameObject RockNeedle;

    [SerializeField] GameObject MikataShineShineAttack;
    [SerializeField] GameObject asaltAttackArea;
    [SerializeField] GameObject HoukouAttack;
    [SerializeField] GameObject Denger_HoukouAttackArea;
    [SerializeField] GameObject Denger_4AttackArea;
    [SerializeField] List<GameObject> Effects = new List<GameObject>();

    [SerializeField] int SpawnEnemyNumber = 10;
    [SerializeField] GameObject Wakin;
    [SerializeField] GameObject Koaka;
    [SerializeField] GameObject OORanchu;
    [SerializeField] GameObject Ranchu;
    [SerializeField] GameObject NomalWakin;
    [SerializeField] GameObject Zikin;
    [SerializeField] GameObject LederKoaka;
    [SerializeField] GameObject Tanchou;
    [SerializeField] GameObject LederWakin;
    Vector2 targetPos = Vector2.zero;

    public float duration = 1;
    [SerializeField] float SeriasDuration = 14;
    public float arcHeight = 30;

    bool jampAsalt = false;

    float AttackCount1 = 0;
    float saveAngle = 0;
    bool StandOk = false;
    [SerializeField]bool isSerious = false;

    int MaxSpawnEnemyCount = 20;
    int SpawnEnemyCount = 0;

    bool flag = false;

    [SerializeField] List<AudioClip> audioClips = new List<AudioClip>();

    [SerializeField] float MoterRandomRange = 4;

    public override void M_Con()
    {
        AllAttackNumber = 0;
        AttackWaitTime = 99;
        AttackNumber = 0;
        jampAsalt = false;
    }
    public override void M_Stan()
    {
        AllAttackNumber = 0;
        AttackWaitTime = 99;
        AttackNumber = 0;
        jampAsalt = false;
    }

    public override void M_Die()
    {

    }

    public override void M_AI()
    {

        
        if (enemyBase.moveType != EnemyBase.MoveType.Attack)
        {
            enemyBase.rb.velocity = (enemyBase.playerController.transform.position - transform.position).normalized * enemyBase.SPEED;
            enemyBase.animator.SetInteger("Anim", 0);
            AttackWaitTime += Time.deltaTime;

            enemyBase.rb.velocity = Vector3.zero;

            if (phase == Phase.P1)
            {


                if (AttackWaitTime > 3.5f)
                {
                    SpawnCount++;
                    int random = 0;
                    
                    random = Random.RandomRange(1, 5);
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;

                    if (enemyBase.HP < enemyBase.MAXHP * 0.5f)
                    {

                        enemyBase.audioManager.PlaySE(audioClips[1]);
                        phase = Phase.P2;
                        AttackWaitTime = 0;
                        enemyBase.DamageBody.SetActive(false);
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        random = 10;
                        enemyBase.attackType = EnemyBase.AttackType.A10;
                        isSerious = true;
                        
                    }

                    if (random == 1)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A1;
                    }
                    if (random == 2)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A2;
                    }
                    if (random == 3)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A3;
                    }
                    if (random == 4)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A4;
                    }
                    if (random == 5)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A3;
                    }
                    if (random == 6)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A2;
                    }
                    if (random == 10)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A10;
                    }

                }

            }
            if (phase == Phase.P2)
            {


                if (AttackWaitTime > 2)
                {
                    SpawnCount++;
                    int random = 0;

                    random = Random.RandomRange(1, 5);
                   
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;

                    
                    if (random == 1)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A1;
                    }
                    if (random == 2)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A2;
                    }
                    if (random == 3)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A3;
                    }
                    if (random == 4)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A4;
                    }
                    if (random == 5)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A3;
                    }
                    if (random == 6)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A2;
                    }
                    if (random == 10)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A10;
                    }

                }

            }

        }


    }

    public override void M_Attack()
    {

        enemyBase.AttackCount += Time.deltaTime;
        switch (enemyBase.attackType)
        {

            case EnemyBase.AttackType.A1:
                A1();
                break;
            case EnemyBase.AttackType.A2:
                A2();
                break;
            case EnemyBase.AttackType.A3:
                A3();
                break;
            case EnemyBase.AttackType.A4:
                A4();
                break;
            case EnemyBase.AttackType.A5:
                A5();
                break;
            case EnemyBase.AttackType.A6:
                //A6();
                break;
            case EnemyBase.AttackType.A7:
                //A7();
                break;
            case EnemyBase.AttackType.A8:
                //A8();
                break;
            case EnemyBase.AttackType.A9:
                //A9();
                break;
            case EnemyBase.AttackType.A10:
                A10();
                break;

        }

    }
    public override void M_BaseMove()
    {

    }
    void NextPhase()
    {
        AttackPhase++;
        enemyBase.AttackCount = 0;
        
    }
    void ChoiseNextPhase(int Number)
    {
        AttackPhase = Number;
        enemyBase.AttackCount = 0;
    }
    void Finish()
    {
        AttackPhase = 0;
        AttackNumber = 0;
        enemyBase.AttackCount = 0;
        enemyBase.moveType = EnemyBase.MoveType.Move;
    }
    float attackcount()
    {
        float count = enemyBase.AttackCount;
        return count;

    }
    void A1()
    {
        if (AttackPhase == 0)
        {
            enemyBase.animator.Play("咆哮待機", 0, 0);
            DengerAreaSpawn(Denger_HoukouAttackArea, transform.position, 2);

            NextPhase();
        }
        if (AttackPhase == 1)
        {
            if (attackcount() > 2)
            {
                enemyBase.animator.Play("攻撃", 0, 0);
                SpawnObjectNow(HoukouAttack, transform.position, 0.1f);
                SpawnObjectNow(Effects[0], transform.position, 3);
                all_System.camera_Controller.Shake(0.2f, 0.3f);
                enemyBase.audioManager.PlaySE(audioClips[0]);
                NextPhase();
                DengerAreaSpawn(Denger_HoukouAttackArea, transform.position, 0.5f);
            }
        }
        if (AttackPhase == 2)
        {
            if (attackcount() > 0.5)
            {
                DengerAreaSpawn(Denger_HoukouAttackArea, transform.position, 0.5f);
                SpawnObjectNow(HoukouAttack, transform.position, 0.1f);
                all_System.camera_Controller.Shake(0.2f, 0.3f);
                SpawnObjectNow(Effects[0], transform.position, 3);
                AttackNumber++;
                if (AttackNumber < 3)
                {
                    enemyBase.AttackCount = 0;
                }
                else
                {

                    NextPhase();
                    enemyBase.animator.Play("待機", 0, 0);
                }
            }
        }
        if (AttackPhase == 3)
        {
            if (attackcount() > 0.5)
            {

                ChoiseNextPhase(0);
                enemyBase.moveType = EnemyBase.MoveType.Attack;
                enemyBase.attackType = EnemyBase.AttackType.A2;
                //AttackFinish();
            }
        }
    }

    void A2()
    {
        float waitTime = 0;
        if (AttackPhase == 0)
        {
            
            if (AttackNumber == 0)
            {
                waitTime = 1.5f;
            }
            else
            {
                if (jampAsalt)
                {
                    waitTime = 1;
                }
                else
                {
                    waitTime = 0.5f;
                }
            }
            enemyBase.isPlayerLookBase();
            enemyBase.animator.Play("攻撃待機", 0, 0);
            GameObject denger = DengerAreaSpawn(Denger_4AttackArea, enemyBase.transform.position, waitTime);
            denger.transform.up =enemyBase.PlayerDirection.normalized;

            NextPhase();
        }
        if (AttackPhase == 1)
        {

            if (jampAsalt)
            {
                waitTime = 1;
            }
            else
            {
                waitTime = 0.5f;
            }
            if (enemyBase.AttackCount > waitTime)
            {
                asaltAttackArea.SetActive(true);
                enemyBase.animator.Play("歩行", 0, 0);
                NextPhase();
            }
        }
        if (AttackPhase == 2)
        {
            enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * enemyBase.SPEED * 8;
            if (enemyBase.AttackCount > 1)
            {
                enemyBase.animator.Play("攻撃待機", 0, 0);
                asaltAttackArea.SetActive(false);
                NextPhase();
            }
            AttackCount1 += Time.fixedDeltaTime;
            if (AttackCount1 > 0.2)
            {
                AttackCount1 = 0;
                if (isSerious)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        // 垂直方向（左右）ベクトル
                        Vector2 perp = new Vector2(-enemyBase.PlayerDirection.normalized.y, enemyBase.PlayerDirection.normalized.x);

                        // 攻撃判定を左右に生成する位置
                        Vector2 leftPos = (Vector2)transform.position + perp * 0.5f * (i + 3);
                        Vector2 rightPos = (Vector2)transform.position - perp * 0.5f * (i + 3);

                        // 攻撃判定オブジェクトを左右に生成
                        GameObject leftAttack = Instantiate(RockNeedle, leftPos, Quaternion.identity);
                        GameObject rightAttack = Instantiate(RockNeedle, rightPos, Quaternion.identity);
                        Destroy(leftAttack, 5);
                        Destroy(rightAttack, 5);
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        // 垂直方向（左右）ベクトル
                        Vector2 perp = new Vector2(-enemyBase.PlayerDirection.normalized.y, enemyBase.PlayerDirection.normalized.x);

                        // 攻撃判定を左右に生成する位置
                        Vector2 leftPos = (Vector2)transform.position + perp * 0.5f * (i + 3);
                        Vector2 rightPos = (Vector2)transform.position - perp * 0.5f * (i + 3);

                        // 攻撃判定オブジェクトを左右に生成
                        GameObject leftAttack = Instantiate(RockNeedle, leftPos, Quaternion.identity);
                        GameObject rightAttack = Instantiate(RockNeedle, rightPos, Quaternion.identity);
                        Destroy(leftAttack, 5);
                        Destroy(rightAttack, 5);
                    }
                }
            }


        }
        if (AttackPhase == 3)
        {
            enemyBase.rb.velocity = Vector2.zero;
            int randomValue = Random.Range(0, 2);
            all_System.camera_Controller.Shake(0.2f, 0.3f);
            if (isSerious) 
            {
                for (int i = 0; i < 5; i++)
                {
                    StartCoroutine(ShotMoter((float)i / 1.2f));
                }
            }
            if (AttackNumber < 3)
            {
                AttackNumber++;
                
                    if (jampAsalt)
                    {
                        ChoiseNextPhase(0);
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A4;
                    }
                    else
                    {
                        int randomnumberAttack = Random.Range(0, 2);
                        if (randomnumberAttack == 0)
                        {
                            ChoiseNextPhase(0);
                        }
                        else
                        {
                            Finish();
                        }
                    }
                    
                
            }
            else
            {
                Finish();
            }


        }
        
    }
    void A3()
    {
        if (AttackPhase == 0)
        {


            enemyBase.animator.Play("攻撃待機");
            NextPhase();
        }
        if (AttackPhase == 1)
        {
            if (attackcount() > 1)
            {

                for (int i = 0; i < 15; i++) 
                {

                    StartCoroutine(LateKingyosAttaker((float)i / 2.5f, Koaka));
                
                }
                for (int i = 0; i < 2; i++)
                {

                    StartCoroutine(LateKingyosAttaker((float)i , Wakin));

                }

                targetPos = enemyBase.Player.transform.position;
                enemyBase.DamageBody.SetActive(false);
                StartCoroutine(MoveInArc(duration, arcHeight));
                NextPhase();
            }
        }
        if (AttackPhase == 2)
        {
            if (attackcount() > duration)
            {

                SpawnObjectNow(Effects[0], transform.position, 3);
                all_System.camera_Controller.Shake(0.2f, 0.3f);
                enemyBase.DamageBody.SetActive(true);
                NextPhase();
                enemyBase.animator.Play("着地");
            }
        }
        if (AttackPhase == 3)
        {
            if (attackcount() > 2)
            {

                Finish();
            }
        }
    }
    void A4()
    {
        if (AttackPhase == 0)
        {
            if (attackcount() > 1)
            {
                bool FindPlayerPos = false;
                int safeCount = 0; // 安全カウンタ
                jampAsalt = true;
                targetPos = enemyBase.Player.transform.position;
                while (!FindPlayerPos && safeCount < 100)
                {
                    safeCount++;

                    Vector2 TargetFindPlayerPos = new Vector2(
                        Random.Range(-7f, 7f) + enemyBase.Player.transform.position.x,
                        Random.Range(-7f, 7f) + enemyBase.Player.transform.position.y
                    );

                    FindPlayerPos = IsTagAtPosition(TargetFindPlayerPos, "Stage_Floor");

                    if (FindPlayerPos)
                    {
                        targetPos = TargetFindPlayerPos;
                        targetDirection = targetPos - (Vector2)transform.position;
                        enemyBase.isTargetLook(targetPos);
                    }
                }

                
                enemyBase.DamageBody.SetActive(false);
                StartCoroutine(MoveInArc(1.5f,arcHeight));
                NextPhase();
            }
        }
        if (AttackPhase == 1)
        {
            if (attackcount() > 1.5f)
            {

                SpawnObjectNow(Effects[0], transform.position, 3);
                all_System.camera_Controller.Shake(0.2f, 0.3f);
                enemyBase.DamageBody.SetActive(true);
                enemyBase.animator.Play("着地");
                ChoiseNextPhase(0);
                enemyBase.moveType = EnemyBase.MoveType.Attack;
                enemyBase.attackType = EnemyBase.AttackType.A2;
            }
        }
    }
    void A5()
    {

    }

    void A10() 
    {
        if (AttackPhase == 0)
        {


            enemyBase.animator.Play("攻撃待機");
            NextPhase();
        }
        if (AttackPhase == 1)
        {
            if (attackcount() > 1)
            {
                for(int i =0; i < 12;i++)
                    EnemySpawn(OORanchu, EnemySpawnPoint[Random.Range(0,EnemySpawnPoint.Count)].transform.position);
                for (int i = 0; i < 4; i++)
                    EnemySpawn(Ranchu, EnemySpawnPoint[Random.Range(0, EnemySpawnPoint.Count)].transform.position);
                for (int i = 0; i < 8; i++)
                    EnemySpawn(NomalWakin, EnemySpawnPoint[Random.Range(0, EnemySpawnPoint.Count)].transform.position);
                for (int i = 0; i < 1; i++)
                    EnemySpawn(LederWakin, EnemySpawnPoint[Random.Range(0, EnemySpawnPoint.Count)].transform.position);
                for (int i = 0; i < 2; i++)
                    EnemySpawn(LederKoaka, EnemySpawnPoint[Random.Range(0, EnemySpawnPoint.Count)].transform.position);
                for (int i = 0; i < 20; i++)
                    EnemySpawn(Tanchou, EnemySpawnPoint[Random.Range(0, EnemySpawnPoint.Count)].transform.position);

                targetPos = enemyBase.Player.transform.position;
                enemyBase.DamageBody.SetActive(false);
                all_System.camera_Controller.Shake(0.1f, 0.3f);
                StartCoroutine(MoveInArc(SeriasDuration, arcHeight*20));
                NextPhase();
            }
        }
        if (AttackPhase == 2)
        {
            if (attackcount() > SeriasDuration-0.3f)
            {

                SpawnObjectNow(MikataShineShineAttack, transform.position, 0.1f);
                NextPhase();
               
            }
        }
        if (AttackPhase == 3)
        {
            if (attackcount() > 0.3f)
            {
                GameObject CL_effect = SpawnObjectNow_Input(Effects[0], transform.position, 3);
                CL_effect.transform.localScale = new Vector2(3,3);
                all_System.camera_Controller.Shake(0.2f, 0.3f);
                enemyBase.DamageBody.SetActive(true);
                NextPhase();
                enemyBase.animator.Play("着地"); 
                Finish();
            }
        }
        if (AttackPhase == 4)
        {
            if (attackcount() > 2)
            {

                Finish();
            }
        }
    }
    IEnumerator ShotArrow(float WaitTime)
    {
        yield return new WaitForSeconds(WaitTime);

        enemyBase.audioManager.PlaySE(audioClips[0]);

        Vector2 shotPos = ShotPoint[Random.Range(0, ShotPoint.Count)].transform.position;
        GameObject CL_Arrow = Instantiate(
            Arrow
            , shotPos
            , Quaternion.identity
            );

        Rigidbody2D rb = CL_Arrow.GetComponent<Rigidbody2D>();
        Vector2 direction = (Vector2)enemyBase.playerController.transform.position - shotPos;
        CL_Arrow.transform.up = direction;
        CL_Arrow.transform.Rotate(0, 0, 90);
        rb.velocity = direction.normalized * ArrowSpeed;

        Destroy(CL_Arrow, ArrowDestroy);
    }
    IEnumerator ShotMoter(float WaitTime)
    {
        yield return new WaitForSeconds(WaitTime);
        enemyBase.audioManager.PlaySE(audioClips[2]);
        Vector2 shotPos = transform.position;
        GameObject CL_Arrow = Instantiate(
            Moter
            , shotPos
            , Quaternion.identity
            );

        Mortar_Controller mortar = CL_Arrow.GetComponent<Mortar_Controller>();
        mortar.ChangePos(enemyBase.Player.transform.position + new Vector3(Random.Range(-MoterRandomRange, MoterRandomRange), Random.Range(-MoterRandomRange, MoterRandomRange), 0));
        mortar.enemyBase = enemyBase;
        mortar.AttackArea = S_AttackArea;

    }
    IEnumerator LateSpawnEnemy(float WaitTime, GameObject Enemy, Vector2 pos)
    {
        yield return new WaitForSeconds(WaitTime);
        EnemySpawn(Enemy, pos);
    }
    IEnumerator LateKingyosAttaker(float delayTime,GameObject Kingyo) 
    {
        yield return new WaitForSeconds(delayTime);

        // プレイヤー → 自分 の方向ベクトル
        Vector2 direction = (transform.position - enemyBase.Player.transform.position).normalized;
        // 反対方向に20m（単位）離れた座標を計算
        Vector2 targetPos = (Vector2)transform.position + direction * 10f;
        for (int i = 0; i < SpawnEnemyNumber; i++)
        {
            Vector2 randomPos = new Vector2(Random.Range(-3f, 3), Random.Range(-2f, 2));
            GameObject CL_koaka = EnemySpawn(Kingyo, targetPos + randomPos);
            Destroy(CL_koaka, 30);
        }
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

            NeedleSpawn(newPosition);

        }
    }
    void NeedleSpawn(Vector2 Ptransform)
    {
        GameObject CL_Needle = Instantiate(RockNeedle, Ptransform, Quaternion.identity);

        Destroy(CL_Needle, 5);
    }

    private IEnumerator MoveInArc(float jampTime,float jampHeigh)
    {


        enemyBase.animator.Play("移動", 0, 0);
        // 移動開始前のBodyTypeを保存しておく
        var originalBodyType = enemyBase.rb.bodyType;
        // 移動中はKinematicにして、物理演算の影響をなくす
        enemyBase.rb.bodyType = RigidbodyType2D.Kinematic;

        Vector2 startPos = enemyBase.transform.position;
        Vector2 endPos = targetPos;

        DengerAreaSpawn(Denger_HoukouAttackArea, endPos, jampTime);
        
        float elapsedTime = 0f;

        while (elapsedTime < jampTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / jampTime); // 0から1の範囲に収める

            // XとYの直線的な移動を計算
            Vector2 linearPos = Vector2.Lerp(startPos, endPos, progress);

            // 高さを計算 (Sinカーブ)
            float height = Mathf.Sin(progress * Mathf.PI) * jampHeigh;

            // Rigidbody2Dを使って位置を更新する
            // 2Dなので、Y座標に高さを加える
            enemyBase.rb.MovePosition(new Vector2(linearPos.x, linearPos.y + height));

            //yield return new WaitForFixedUpdate(); // 物理演算の更新タイミングで待つ
            yield return new WaitForEndOfFrame();
        }


        // BodyTypeを元に戻す
        enemyBase.rb.bodyType = originalBodyType;


        StandOk = true;

    }
    bool IsTagAtPosition(Vector2 position, string tag)
    {
        Collider2D hit = Physics2D.OverlapPoint(position);

        if (hit != null && hit.CompareTag(tag))
        {
            return true;
        }

        return false;
    }
}
