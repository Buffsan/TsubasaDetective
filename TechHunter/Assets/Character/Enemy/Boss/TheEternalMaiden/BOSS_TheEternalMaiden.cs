using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BOSS_TheEternalMaiden : Boss_MoveBase
{
    [SerializeField] List<GameObject> ShotPoint;
    [SerializeField] List<GameObject> HoudanShotPoint;
    [SerializeField] GameObject Arrow;
    [SerializeField] float ArroeSpeed;
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

    [SerializeField] float RotateRadius = 8f;
    [SerializeField] float RotateSpeed = 90f;
    [SerializeField] float RadiusFixSpeed = 5f;
    [SerializeField] float RotateTime = 3f;

    [SerializeField] float BackStepDistance = 3f;
    [SerializeField] float BackStepTime = 0.3f;
    [SerializeField] float DashSpeed = 20f;

    [SerializeField] ParticleSystem AttackparticleSystem;
    [SerializeField] int DefaltEmmitionRate = 10;

    Vector2 DashDirection;
    Vector2 DashStartPos;

    float currentAngle;

    [SerializeField] int SpawnEnemyNumber = 10;
   
    Vector2 targetPos = Vector2.zero;

    public float duration = 1;
    [SerializeField] float SeriasDuration = 14;
    public float arcHeight = 30;

    bool jampAsalt = false;

    float AttackCount1 = 0;
    float saveAngle = 0;
    bool StandOk = false;
    [SerializeField] bool isSerious = false;

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
        AttackPhase = 0;
    }
    public override void M_Stan()
    {
        AllAttackNumber = 0;
        AttackWaitTime = 99;
        AttackNumber = 0;
        jampAsalt = false;
        AttackPhase = 0;
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

                    random = Random.RandomRange(1, 4);
                    random = 4;
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


                if (AttackWaitTime > 2.5f)
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
        AsaltAttackBody.SetActive(false);
        AttackparticleSystem.emissionRate = 0;
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
            enemyBase.animator.Play("移動", 0, 0);

            Vector2 offset = (Vector2)transform.position;

            currentAngle =
                Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;

            NextPhase();
        }

        if (AttackPhase == 1)
        {
            RotateAroundCenter(1);

            if (attackcount() > RotateTime)
            {
                enemyBase.animator.Play("攻撃", 0, 0);

                StartCoroutine(ShotArrow_A1(0, (enemyBase.Player.transform.position - transform.position).normalized) );
                StartCoroutine(ShotArrow_A1(0.25f, (enemyBase.Player.transform.position - transform.position).normalized));
                StartCoroutine(ShotArrow_A1(0.5f, (enemyBase.Player.transform.position - transform.position).normalized));

                NextPhase();
            }
        }

        if (AttackPhase == 2)
        {
            RotateAroundCenter(0.2f);
            if (attackcount() > 1f)
            {
                
                Finish();
            }
        }
    }

    void A2()
    {
        if (AttackPhase == 0)
        {
            enemyBase.animator.Play("攻撃待機", 0, 0);

            enemyBase.isPlayerLookBase();

            DashDirection = enemyBase.PlayerDirection.normalized;

            DashStartPos =
                (Vector2)transform.position
                - DashDirection * BackStepDistance;

            NextPhase();
        }

        if (AttackPhase == 1)
        {

            float t = attackcount() / BackStepTime;
            AttackparticleSystem.emissionRate = DefaltEmmitionRate;
            enemyBase.rb.MovePosition(
                Vector2.Lerp(
                    transform.position,
                    DashStartPos,
                    t
                )
            );

            if (attackcount() > BackStepTime)
            {
                enemyBase.animator.Play("歩行", 0, 0);

                

                NextPhase();
            }
        }

        if (AttackPhase == 2)
        {
            enemyBase.rb.velocity =
                DashDirection * DashSpeed;
            AsaltAttackBody.SetActive(true);
            if (attackcount() > 1.5f)
            {
                AsaltAttackBody.SetActive(false);
                enemyBase.rb.velocity = Vector2.zero;

                enemyBase.animator.Play("待機", 0, 0);
                AttackparticleSystem.emissionRate = 0;
                NextPhase();
            }
        }

        if (AttackPhase == 3)
        {
            if (attackcount() > 0.3f)
            {
                Finish();
            }
        }

    }
    void A3()
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
                        Random.Range(-0.2f, 0.2f) + enemyBase.Player.transform.position.x,
                        Random.Range(-0.2f, 0.2f) + enemyBase.Player.transform.position.y
                    );

                    FindPlayerPos = IsTagAtPosition(TargetFindPlayerPos, "Stage_Floor");

                    if (FindPlayerPos)
                    {
                        targetPos = TargetFindPlayerPos;
                        targetDirection = targetPos - (Vector2)transform.position;
                        enemyBase.isTargetLook(targetPos);
                    }
                }


               
                StartCoroutine(MoveInArc(1.2f, arcHeight));
                NextPhase();
            }
        }
        if (AttackPhase == 1)
        {
            if (attackcount() > 1.2f)
            {
                isStarShot(7,1);
                all_System.camera_Controller.Shake(0.1f, 0.1f);
                
                enemyBase.animator.Play("着地");
                GameObject leftAttack = Instantiate(AttackObjects[0], transform.position, Quaternion.identity);
                Destroy(leftAttack, 0.1f);
                Finish();
            }
        }
    }
    void A4()
    {
        if (AttackPhase == 0)
        {
            enemyBase.animator.Play("移動", 0, 0);

            Vector2 offset = (Vector2)transform.position;

            currentAngle =
                Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;

            NextPhase();
        }

        if (AttackPhase == 1)
        {
            RotateAroundCenter(-1);

            if (attackcount() > RotateTime)
            {
                enemyBase.animator.Play("攻撃", 0, 0);

                GameObject CL_Laser = Instantiate(AttackObjects[1], transform.position, Quaternion.identity);
                Destroy(CL_Laser, 5);

                CL_Laser.transform.Rotate(0, 0, Random.Range(0,360));

                NextPhase();
            }
        }

        if (AttackPhase == 2)
        {
            RotateAroundCenter(-0.2f);
            if (attackcount() > 1f)
            {

                Finish();
            }
        }
    }
    void A5()
    {

    }

    void A10()
    {
       
            
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
    IEnumerator LateKingyosAttaker(float delayTime, GameObject Kingyo)
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

    private IEnumerator MoveInArc(float jampTime, float jampHeigh)
    {


        enemyBase.animator.Play("移動", 0, 0);
        // 移動開始前のBodyTypeを保存しておく
        var originalBodyType = enemyBase.rb.bodyType;
        // 移動中はKinematicにして、物理演算の影響をなくす
        enemyBase.rb.bodyType = RigidbodyType2D.Kinematic;

        Vector2 startPos = enemyBase.transform.position;
        Vector2 endPos = targetPos;

        DengerAreaSpawn(DengerObjects[0], endPos, jampTime);

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

    void RotateAroundCenter(float speedstrengs)
    {
        currentAngle += RotateSpeed * speedstrengs * Time.deltaTime;

        Vector2 currentPos = transform.position;

        float currentRadius = currentPos.magnitude;


        float targetRadius =
            Mathf.Lerp(
                currentRadius,
                RotateRadius,
                RadiusFixSpeed  * Time.deltaTime
            );

        float rad = currentAngle * Mathf.Deg2Rad;

        Vector2 nextPos = new Vector2(
            Mathf.Cos(rad),
            Mathf.Sin(rad)
        ) * targetRadius;

        enemyBase.rb.MovePosition(nextPos);

        Vector2 moveDir =
            (nextPos - currentPos).normalized;

        if (moveDir != Vector2.zero)
        {
            enemyBase.isTargetLook(
                (Vector2)transform.position + moveDir
            );
        }
    }

    IEnumerator ShotArrow_A1(float waitTime ,Vector2 direction)
    {
        yield return new WaitForSeconds(waitTime);

        enemyBase.audioManager.PlaySE(audioClips[0]);

        Vector2 shotPos = transform.position;

        GameObject arrow = Instantiate(
            Arrow,
            shotPos,
            Quaternion.identity
        );

        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();

       
            

        arrow.transform.up = direction;

        rb.AddForce(direction * ArroeSpeed, ForceMode2D.Impulse);

        Destroy(arrow, ArrowDestroy);
    }
    void isStarShot(int value, int Addvalue)
    {
        for (int i = 0; i < value; i++)
        {
            GameObject CL_Star = Instantiate(Arrow, enemyBase.gameObject.transform.position, Quaternion.identity);
            Rigidbody2D CL_rb = CL_Star.GetComponent<Rigidbody2D>();
            EnemyAttack CL_EnemyAttack = CL_Star.GetComponent<EnemyAttack>();

            CL_EnemyAttack.enemyBase = enemyBase;

            float T_Angle = (((360 / value) * i) + Addvalue) * Mathf.Deg2Rad;

            Vector3 T_velocity = new Vector3(Mathf.Cos(T_Angle), Mathf.Sin(T_Angle), 0) * 2.5f;
            CL_rb.velocity = T_velocity;

            Destroy(CL_Star, 10);
        }
    }
}
