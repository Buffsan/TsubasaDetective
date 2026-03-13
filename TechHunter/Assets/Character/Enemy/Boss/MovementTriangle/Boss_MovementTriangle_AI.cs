using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Boss_MovementTriangle_AI : Boss_MoveBase
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
    [SerializeField] GameObject Conducter;
    [SerializeField] List<GameObject> Effects = new List<GameObject>();

    [SerializeField] int SpawnEnemyNumber = 10;

    Vector2 targetPos = Vector2.zero;
    Vector2 saveTargetDir = Vector2.zero;

    int AIPhase = 0;

        public Tilemap tilemap;

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

    [SerializeField]bool flag = false;

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
                if (AttackWaitTime > 1f)
                {

                    if (!flag) 
                    {
                        EnemySpawn(Conducter,new Vector2(0,20));
                    flag = true;
                    }

                    SpawnCount++;
                    int random = 0;

                    //random = Random.RandomRange(1, 4);

                    if (AIPhase >= 0 && AIPhase < 3) 
                    {
                        random = 2;
                    }
                    if (AIPhase == 3)
                    {
                        random = 1;
                    }
                    if (AIPhase == 4)
                    {
                        random = 3;
                        AIPhase = 0;
                    }
                    AIPhase++;
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
            float waitTime = 0;
            if (AttackPhase == 0)
            {
            waitTime = 1.5f;
            enemyBase.isPlayerLookBase();
                enemyBase.animator.Play("攻撃待機", 0, 0);
                Vector3Int tile = GetTileBehindPlayer(tilemap, transform, enemyBase.playerController.transform);
                targetPos = (Vector3)tile;
                saveTargetDir = (Vector2)targetPos - (Vector2)transform.position;
                GameObject denger = DengerAreaSpawn(Denger_4AttackArea, enemyBase.transform.position, waitTime);
                denger.transform.up = saveTargetDir.normalized;

                NextPhase();
            }
            if (AttackPhase == 1)
            {
                waitTime = 1.5f;

                if (attackcount() > waitTime)
                {
                    asaltAttackArea.SetActive(true);
                    enemyBase.animator.Play("歩行", 0, 0);
                    NextPhase();
                }
            }
            if (AttackPhase == 2)
            {
                enemyBase.rb.velocity = saveTargetDir.normalized * enemyBase.SPEED * 8;
            float Range = Vector2.Distance((Vector2)targetPos , (Vector2)transform.position);
                if (attackcount() > 2 || Range < 0.3f)
                {
                    enemyBase.animator.Play("攻撃待機", 0, 0);
                    asaltAttackArea.SetActive(false);
                enemyBase.rb.velocity = Vector2.zero;
                    NextPhase();
                }

            }
            if (AttackPhase == 3)
            {
            if (attackcount() > 1f)
            {
                Finish();
                enemyBase.animator.Play("待機");
            }

            }

        }

        void A2()
    {
        float waitTime = 0;
        if (AttackPhase == 0)
        {
            waitTime = 1;
            enemyBase.isPlayerLookBase();
            enemyBase.animator.Play("攻撃待機", 0, 0);
            Vector3Int tile = GetTileBehindPlayer(tilemap, transform, enemyBase.playerController.transform);
            targetPos = (Vector3)tile;
            saveTargetDir = (Vector2)targetPos - (Vector2)transform.position;
            GameObject denger = DengerAreaSpawn(Denger_HoukouAttackArea, enemyBase.transform.position, waitTime);
            denger.transform.up = saveTargetDir.normalized;

            NextPhase();
        }
        if (AttackPhase == 1)
        {
            waitTime = 1;

            if (attackcount() > waitTime)
            {
                asaltAttackArea.SetActive(true);
                enemyBase.animator.Play("歩行", 0, 0);
                NextPhase();
            }
        }
        if (AttackPhase == 2)
        {
            enemyBase.rb.velocity = saveTargetDir.normalized * enemyBase.SPEED * 5;
            float Range = Vector2.Distance((Vector2)targetPos, (Vector2)transform.position);
            if (attackcount() > 0.3f || Range < 0.3f)
            {
                enemyBase.animator.Play("攻撃待機", 0, 0);
                asaltAttackArea.SetActive(false);
                NextPhase();
            }

        }
        if (AttackPhase == 3)
        {

            Finish();
            enemyBase.animator.Play("待機");
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
            if (attackcount() > 0.5f)
            {
                enemyBase.animator.Play("攻撃");
                for (int i = 0; i < 60;i++) 
                {
                    StartCoroutine(ShotMoter((float)i/60));
                }
                
                NextPhase();
            }
        }
        if (AttackPhase == 2)
        {
            if (attackcount() > 5)
            {
                enemyBase.animator.Play("待機");
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
                StartCoroutine(MoveInArc(1.5f, arcHeight));
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
        //enemyBase.audioManager.PlaySE(audioClips[2]);
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
    public Vector3Int GetTileBehindPlayer(Tilemap tilemap, Transform enemy, Transform player)
    {
        Vector3Int enemyTile = tilemap.WorldToCell(enemy.position);
        Vector3Int playerTile = tilemap.WorldToCell(player.position);

        Vector3Int dir = playerTile - enemyTile;

        dir.x = Mathf.Clamp(dir.x, -1, 1);
        dir.y = Mathf.Clamp(dir.y, -1, 1);
        dir.z = 0;

        Vector3Int checkTile = playerTile;

        while (tilemap.HasTile(checkTile + dir))
        {
            checkTile += dir;
        }

        return checkTile;
    }
}
