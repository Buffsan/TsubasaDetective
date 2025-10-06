using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SO_Eshema_AI : Boss_MoveBase
{

    private IEnumerator myRunningCoroutine; // 実行中のコルーチンを保持するための変数
    public float duration = 1;
    public float arcHeight = 30;
    float AttackCount1 =0;
    bool StandOk = false;

    [SerializeField] GameObject ARM;
    [SerializeField] GameObject ArmAttackPoint;
    [SerializeField] Animator ARManim;
    [SerializeField] List<GameObject> GunLaser = new List<GameObject>();

    [SerializeField] List<AudioClip> audioclips = new List<AudioClip>();

    Vector2 targetPos = Vector2.zero;
    Vector2 targetDirection = Vector2.zero;

    [SerializeField] GameObject slashAttack;
    [SerializeField] GameObject asaltAttackArea;
    [SerializeField] GameObject Bullet;
    [SerializeField] GameObject RockNeedle;
    [SerializeField] GameObject HalfBullet;

    [SerializeField] List<GameObject> AttackAreas = new List<GameObject>();
    [SerializeField] GameObject Denger_AttackArea_Slash;
    [SerializeField] GameObject Denger_AttackArea_Stand;

    AudioManager audioManager => AudioManager.instance;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    public override void M_Stan()
    {
        Finish(); AttackWaitTime = 0;StandOk = false;
        if (myRunningCoroutine != null)
        {
            AttackCount1 = 0;
            asaltAttackArea.SetActive(false);
            StopCoroutine(myRunningCoroutine); // 特定のコルーチンを停止
            Debug.Log("コルーチンを停止しました。");
            myRunningCoroutine = null; // 参照をクリア
        }
    }
    public override void M_Con()
    {
        Finish(); AttackWaitTime = 0; StandOk = false;
        if (myRunningCoroutine != null)
        {
            AttackCount1 = 0;
            asaltAttackArea.SetActive(false);
            StopCoroutine(myRunningCoroutine); // 特定のコルーチンを停止
            Debug.Log("コルーチンを停止しました。");
            myRunningCoroutine = null; // 参照をクリア
        }
    }
    public override void M_AI()
    {

        if (enemyBase.moveType != EnemyBase.MoveType.Attack)
        {

            AttackWaitTime += Time.deltaTime;

            enemyBase.rb.velocity = Vector3.zero;

            if (phase == Phase.P1)
            {
                /*
                if (enemyBase.HP < enemyBase.MAXHP / 2)
                {
                    phase = Phase.P2;
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.attackType = EnemyBase.AttackType.A7;
                }
                */
                if (AttackWaitTime > 1.2)
                {
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;

                    float random = Random.RandomRange(1, 6);
                    //random = 5;
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
                        enemyBase.attackType = EnemyBase.AttackType.A5;
                    }
                    if (random == 6)
                    {
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A2;
                    }

                }

            }
            if (phase == Phase.P2)
            {


                if (AttackWaitTime > 2)
                {
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;

                    if (AllAttackNumber > 3)
                    {
                        AllAttackNumber = 0;
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A1;
                    }
                    else
                    {
                        float random = Random.RandomRange(0, 3);
                        AllAttackNumber++;
                        if (random == 1)
                        {
                            enemyBase.moveType = EnemyBase.MoveType.Attack;
                            enemyBase.attackType = EnemyBase.AttackType.A8;
                        }
                        if (random == 2)
                        {
                            enemyBase.moveType = EnemyBase.MoveType.Attack;
                            enemyBase.attackType = EnemyBase.AttackType.A6;
                        }
                        if (random == 3)
                        {
                            enemyBase.moveType = EnemyBase.MoveType.Attack;
                            enemyBase.attackType = EnemyBase.AttackType.A9;
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
                //A10();
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
        AttackPhase=Number;
        enemyBase.AttackCount = 0;
    }
    void Finish() 
    {
        AttackPhase=0;
        AttackNumber = 0;
        AttackCount1 = 0;
        enemyBase.AttackCount = 0;
        enemyBase.moveType = EnemyBase.MoveType.Move;
        ARM.SetActive (false);
    }
    //追跡と斬撃 
    void A1() 
    {
        if (AttackPhase == 0) 
        {
            bool FindPlayerPos = false;
            while (!FindPlayerPos) 
            { 
                Vector2 TargetFindPlayerPos = new Vector2 (Random.Range(-2f,2f) + enemyBase.Player.transform.position.x, Random.Range(-2f, 2f) + enemyBase.Player.transform.position.y);
                FindPlayerPos = IsTagAtPosition(TargetFindPlayerPos,"Stage_NotFloor");
                    if (FindPlayerPos) 
                    { 
                        targetPos = TargetFindPlayerPos;
                    targetDirection = targetPos - (Vector2)transform.position;
                    enemyBase.isTargetLook(targetPos);
                }
            }
            enemyBase.animator.Play("歩行",0,0);
            
            NextPhase();
        }
        if (AttackPhase == 1) 
        {
            float TargetDistanse = Vector2.Distance(targetPos,transform.position);
            
            if (TargetDistanse > 1 && AttackCount1 < 2.5)
            {
                AttackCount1 += Time.fixedDeltaTime;
                enemyBase.rb.velocity = targetDirection.normalized * enemyBase.SPEED;
            }
            else 
            {
                AttackCount1 = 0;
                NextPhase();
                enemyBase.animator.Play("攻撃待機",0,0);
                audioManager.isPlaySE(audioclips[2]);
                enemyBase.isPlayerLookBase();
            }
        }
        if (AttackPhase == 2) 
        {
            enemyBase.rb.velocity *= 0.8f;
            if (enemyBase.AttackCount > 0.3) 
            {
                asaltAttackArea.SetActive(true);
                NextPhase();
                targetPos = enemyBase.Player.transform.position;
                targetDirection =  targetPos -(Vector2)transform.position;
                enemyBase.animator.Play("攻撃", 0, 0);
                audioManager.isPlaySE(audioclips[3]);
            }
        }
        if (AttackPhase == 3) 
        {
             

            if (enemyBase.AttackCount > 0.3) 
            {
                asaltAttackArea.SetActive(false);
                NextPhase();
            }
            enemyBase.rb.velocity = targetDirection.normalized * enemyBase.SPEED * 2;

        }
        if (AttackPhase == 4) 
        {
            if (enemyBase.AttackCount > 0.2)
            {
                int randomRe = Random.Range(0,4);
                if (randomRe != 1 && AttackNumber < 2)
                {
                    enemyBase.AttackCount = 0;
                    AttackPhase = 0;
                    AttackNumber ++;
                }
                else
                {
                    Finish();
                    enemyBase.animator.Play("待機", 0, 0);
                }
            }
            else 
            {
                enemyBase.rb.velocity *= 0.2f;
            }
        }
    }
    //斬撃飛ばし
    void A2() 
    {
        if (AttackPhase == 0)
        {
            
            NextPhase();
             enemyBase.isPlayerLookBase();
            if (AttackNumber == 0)
            {
                enemyBase.animator.Play("攻撃待機", 0, 0);
            }

            for (int i = 0; i < 5; i++)
            {
                GameObject CL_AttackPPP = Instantiate(Denger_AttackArea_Slash, enemyBase.transform.position, Quaternion.identity);
                CL_AttackPPP.transform.up = enemyBase.PlayerDirection.normalized;
                Vector2 NewLookDirection = Quaternion.Euler(0, 0, -80 + 40 * i) * enemyBase.PlayerDirection.normalized;
                CL_AttackPPP.transform.up = NewLookDirection.normalized;
                Destroy(CL_AttackPPP, 1);
            }
        }
        if (AttackPhase == 1) 
        {
            if (enemyBase.AttackCount > 0.2) 
            {
                NextPhase();
            }
          
        }
        if (AttackPhase == 2) 
        {
            if (enemyBase.AttackCount > 0.15)
            {
                enemyBase.animator.Play("攻撃", 0, 0);
                audioManager.isPlaySE(audioclips[2]);
                for (int i = 0; i < 5; i++)
                {
                    GameObject CL_Slash = Instantiate(slashAttack, enemyBase.transform.position, Quaternion.identity);
                    Vector2 NewLookDirection = Quaternion.Euler(0, 0, -80 + 40 * i) * enemyBase.PlayerDirection.normalized;
                    if (enemyBase.PlayerDirection.x > 0)
                    {
                        CL_Slash.transform.localScale = new Vector2(CL_Slash.transform.localScale.x, CL_Slash.transform.localScale.y);
                    }else{
                        CL_Slash.transform.localScale = new Vector2(-CL_Slash.transform.localScale.x, CL_Slash.transform.localScale.y);}
                    Rigidbody2D rb = CL_Slash.GetComponent<Rigidbody2D>();
                    rb.velocity = NewLookDirection.normalized * (8 - AttackNumber*1.2f);
                    Destroy(CL_Slash, 25);
                }
                if (AttackNumber >= 6)
                {
                    enemyBase.AttackCount = 0;
                    enemyBase.attackType = EnemyBase.AttackType.A1;
                    AttackPhase = 0;
                }
                else 
                { 
                    enemyBase.AttackCount = 0;
                    AttackPhase = 0;AttackNumber++;
                }
                
                
            }
        }
        if (AttackPhase == 2) 
        { 
        
        }
    }
    //射撃
    void A3() 
    {
        if (AttackPhase == 0) 
        {
            enemyBase.animator.Play("銃構え",0,0);
            NextPhase();
        }
        if (AttackPhase == 1) 
        {
            if (enemyBase.AttackCount > 0.2)
            {
                enemyBase.isPlayerLookBase();
                if (enemyBase.PlayerDistance > 2)
                {
                    NextPhase();
                }
                else
                {
                    ARM.SetActive(false);
                    GunLaser[0].SetActive(false);
                    GunLaser[1].SetActive(false);

                    AttackPhase = 0;
                    enemyBase.AttackCount = 0;
                    enemyBase.attackType = EnemyBase.AttackType.A1;
                }
            }
        }
        if (AttackPhase == 2) //singleショット
        {
            enemyBase.animator.Play("銃攻撃待機", 0, 0);
            ARManim.Play("腕待機", 0, 0);
            ARM.SetActive(true);
            GunLaser[0].SetActive(true);
            GunLaser[1].SetActive(false);
            NextPhase();
        }
        if (AttackPhase==3) 
        {
            enemyBase.isPlayerLookBase();
            Vector2 ARMdirection = enemyBase.Player.transform.position - ArmAttackPoint.transform.position;
            ARM.transform.up = ARMdirection;
            ARM.transform.Rotate(0,0,90);
            if (enemyBase.AttackCount > 0.4)
            {
                BulletShot();
                ChoiseNextPhase(6);
            }
        }
        if (AttackPhase == 4)//doubleショット
        {
            enemyBase.animator.Play("銃攻撃待機", 0, 0);
            ARManim.Play("腕リロード", 0,0);
            ARM.SetActive(true);
            GunLaser[0].SetActive(false);
            GunLaser[1].SetActive(true);
            audioManager.isPlaySE(audioclips[1]);
            NextPhase();
        }
        if (AttackPhase == 5)
        {
            enemyBase.isPlayerLookBase();
            Vector2 ARMdirection = enemyBase.Player.transform.position - ArmAttackPoint.transform.position;
            ARM.transform.up = ARMdirection;
            ARM.transform.Rotate(0, 0, 90);
            if (enemyBase.AttackCount > 0.5)
            {
                //ARManim.Play("腕待機", 0, 0);
                DoubleBulletShot();
                ChoiseNextPhase(6);
            }
        }
        if (AttackPhase == 6) 
        {
            if (enemyBase.AttackCount > 0.1) 
            {
                NextPhase();
            }
        }
        if (AttackPhase == 7) 
        { 
            
            if (AttackNumber > 5)
            {
                ARM.SetActive(false);
                GunLaser[0].SetActive(false);
                GunLaser[1].SetActive(false);
                Finish();
            }
            else 
            {
                enemyBase.isPlayerLookBase();
                if (enemyBase.PlayerDistance > 3)
                {
                    AttackNumber++;
                    int randomnumber = Random.Range(0,2);
                
                    if (randomnumber == 1)
                    {
                        ChoiseNextPhase(2);
                    }
                    else 
                    {
                        ChoiseNextPhase(4);
                    }
                }
                else
                {
                    ARM.SetActive(false);
                    GunLaser[0].SetActive(false);
                    GunLaser[1].SetActive(false);

                    AttackPhase = 0;
                    enemyBase.AttackCount = 0;
                    enemyBase.attackType = EnemyBase.AttackType.A1;
                }
                
            }
        }
    }
    void A4() 
    {//跳躍
        if (AttackPhase == 0)
        {
            enemyBase.animator.Play("ジャンプ待機", 0, 0);
            NextPhase();
        }
        if (AttackPhase == 1)
        {
            if (enemyBase.AttackCount > 0.3)
            {
                targetPos = (Vector2)enemyBase.Player.transform.position;
                StartCoroutine(MoveInArc());
                enemyBase.animator.Play("ジャンプ", 0, 0);
                
                NextPhase();
            }
        }
        if (AttackPhase == 2) 
        {
            if (StandOk) 
            {
                enemyBase.animator.Play("ジャンプ着地",0,0);
                StandOk = false;
                NextPhase();
            }
        }
        if(AttackPhase == 3) 
        {
            for (int j = 0; j < 15; j++)
            {
                StartCoroutine(NeedleSpawnLing(0.05f * j, j * 0.5f, 0));
            }

            GameObject CL_Attack = Instantiate(AttackAreas[0], enemyBase.transform.position, Quaternion.identity);
            Destroy(CL_Attack, 0.2f);
            NextPhase();
        }
        if (AttackPhase == 4) 
        {
            if (enemyBase.AttackCount > 0.4) 
            {
                AttackPhase = 0;
                enemyBase.AttackCount = 0;
                AttackNumber = 5;
                enemyBase.attackType = EnemyBase.AttackType.A3;
            }
        }
    }
    void A5() 
    {
        if (AttackPhase == 0) 
        {
            enemyBase.isPlayerLookBase();
            enemyBase.animator.Play("ジャンプ待機",0,0);

            GameObject CL_AttackPPP = Instantiate(Denger_AttackArea_Slash, enemyBase.transform.position, Quaternion.identity);
            CL_AttackPPP.transform.up = enemyBase.PlayerDirection.normalized;
            Destroy(CL_AttackPPP, 1);

            NextPhase() ;
        }
        if (AttackPhase == 1) 
        {
            if (enemyBase.AttackCount > 0.8) 
            {
                asaltAttackArea.SetActive(true);
                enemyBase.animator.Play("歩行",0,0);
                NextPhase();
            }
        }
        if (AttackPhase == 2) 
        {  
            enemyBase.rb.velocity =enemyBase.PlayerDirection.normalized * enemyBase.SPEED * 3;
            if (enemyBase.AttackCount > 0.7 * (1  + AttackNumber * 0.5)) 
            {
                enemyBase.animator.Play("ジャンプ待機", 0, 0);
                asaltAttackArea.SetActive(false);
                NextPhase();
            }
            AttackCount1 += Time.fixedDeltaTime;
            if (AttackCount1 > 0.04) 
            {
                AttackCount1 = 0;
            for (int i = 0; i < 9; i++) 
            {
                // 垂直方向（左右）ベクトル
                Vector2 perp = new Vector2(-enemyBase.PlayerDirection.normalized.y, enemyBase.PlayerDirection.normalized.x);

                // 攻撃判定を左右に生成する位置
                Vector2 leftPos = (Vector2)transform.position + perp * 0.5f*(i+3);
                Vector2 rightPos = (Vector2)transform.position - perp * 0.5f*(i+3);

                // 攻撃判定オブジェクトを左右に生成
                GameObject leftAttack = Instantiate(RockNeedle, leftPos, Quaternion.identity);
                GameObject rightAttack = Instantiate(RockNeedle, rightPos, Quaternion.identity);
                    Destroy(leftAttack,5);
                    Destroy(rightAttack, 5);
                }
            }
            

        }
        if (AttackPhase == 3) 
        {
            enemyBase.rb.velocity = enemyBase.rb.velocity * 0.6f;
            int randomValue = Random.Range(0, 2);
            if (AttackNumber == 0) 
            { 
            if (randomValue == 0)
            {
                AttackNumber++;
                ChoiseNextPhase(0);
            }
            else
            {
                if (enemyBase.AttackCount > 0.4)
                {
                    //Finish();
                    AttackNumber = 6;
                    ChoiseNextPhase(0);
                    enemyBase.attackType = EnemyBase.AttackType.A2;
                }
            }
            } 
            else 
            {
                Finish();
            }
            

        }
    }
    private IEnumerator MoveInArc()
    {
        myRunningCoroutine = MoveInArc();
        // 移動開始前のBodyTypeを保存しておく
        var originalBodyType = enemyBase.rb.bodyType;
        // 移動中はKinematicにして、物理演算の影響をなくす
        enemyBase.rb.bodyType = RigidbodyType2D.Kinematic;

        Vector2 startPos = enemyBase.transform.position;
        Vector2 endPos = targetPos;

        GameObject CL_AttackPPP = Instantiate(Denger_AttackArea_Stand, endPos, Quaternion.identity);

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

    void BulletShot() 
    {
        audioManager.isPlaySE(audioclips[0]);
        Vector2 ARMdirection = enemyBase.Player.transform.position - ArmAttackPoint.transform.position;
        GameObject CL_Bullet = Instantiate(Bullet, ArmAttackPoint.transform.position, Quaternion.identity);
        CL_Bullet.transform.up = ARMdirection.normalized;

        Rigidbody2D CL_rb = CL_Bullet.GetComponent<Rigidbody2D>();
        CL_rb.velocity = ARMdirection.normalized * 25;
        Destroy(CL_Bullet, 1);
    
    }
    void DoubleBulletShot()
    {
        audioManager.isPlaySE(audioclips[0]);
        Vector2 ARMdirection = enemyBase.Player.transform.position - ArmAttackPoint.transform.position;
        for (int i = 0; i < 2; i++)
        {
            GameObject CL_Slash = Instantiate(HalfBullet, ArmAttackPoint.transform.position, Quaternion.identity);
            Vector2 NewLookDirection = Quaternion.Euler(0, 0, -6 + 12 * i) * ARMdirection.normalized;
            CL_Slash.transform.up = NewLookDirection.normalized;
            
            if (enemyBase.PlayerDirection.x > 0)
            {
                CL_Slash.transform.localScale = new Vector2(CL_Slash.transform.localScale.x, CL_Slash.transform.localScale.y);
            }
            else
            {
                CL_Slash.transform.localScale = new Vector2(-CL_Slash.transform.localScale.x, CL_Slash.transform.localScale.y);
            }
            Rigidbody2D rb = CL_Slash.GetComponent<Rigidbody2D>();
            rb.velocity = NewLookDirection.normalized * 25;
            Destroy(CL_Slash, 1);
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

