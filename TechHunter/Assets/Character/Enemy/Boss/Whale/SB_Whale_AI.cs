using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SB_Whale_AI : Boss_MoveBase
{

    private IEnumerator myRunningCoroutine; // 実行中のコルーチンを保持するための変数
    public float duration = 1;
    public float arcHeight = 30;
    float AttackCount1 = 0;
    float saveAngle = 0;
    bool StandOk = false;
    bool isSerious = false;
    [Header("第三形態")]
    [SerializeField, Range(0.01f, 0.99f)] float thirdHpThreshold = 0.25f;
    [SerializeField, Min(1f)] float thirdBodyMultiplier = 1.5f;
    [SerializeField, Min(1f)] float thirdAttackMultiplier = 1.5f;
    [SerializeField, Min(1f)] float thirdRoarGrowth = 1.4f;
    [SerializeField, Min(0f)] float thirdRecoilDuration = 1f;
    [SerializeField, Min(0.01f)] float thirdFourwayAttackDelay = 0.9f;
    [SerializeField, Min(0f)] float thirdWarpRadius = 3f;
    [SerializeField, Range(1, 50)] int thirdShockwaveEveryNthRift = 3;
    [SerializeField, Min(0.01f)] float thirdShockwaveAreaScale = 0.35f;
    [SerializeField, Min(0.01f)] float thirdShockwaveHitDuration = 0.8f;
    [SerializeField, Min(0f)] float thirdShockwaveWarningDuration = 0.5f;
    [SerializeField] Vector2 thirdShockwaveDelay = new Vector2(3f, 4f);
    bool thirdRecoilActive;
    bool thirdIntroRoar;
    int thirdRoarPulse;
    Vector3 initialBodyScale = Vector3.zero;

    [SerializeField] GameObject ARM;
    [SerializeField] GameObject ArmAttackPoint;
    [SerializeField] Animator ARManim;
    [SerializeField] List<GameObject> GunLaser = new List<GameObject>();
    [SerializeField] AudioClip BossBGM;

    [SerializeField] List<AudioClip> audioclips = new List<AudioClip>();

    Vector2 targetPos = Vector2.zero;
    Vector2 targetDirection = Vector2.zero;

    [SerializeField] GameObject slashAttack;
    [SerializeField] GameObject asaltAttackArea;
    [SerializeField] GameObject Bullet;
    [SerializeField] GameObject RockNeedle;
    [SerializeField] GameObject SlashLit;
    [SerializeField] GameObject HalfBullet;
    [SerializeField] GameObject DamageArea;
    [SerializeField] GameObject HoukouAttack;
    [SerializeField] GameObject AttackArea4;

    [SerializeField] List<GameObject> AttackAreas = new List<GameObject>();
    [SerializeField] GameObject Denger_AttackArea_Slash;
    [SerializeField] GameObject Denger_AttackArea_Stand;
    [SerializeField] GameObject Denger_HoukouAttackArea;
    [SerializeField] GameObject Denger_4AttackArea;
    [SerializeField] ParticleSystem thirdParticleSystem;

    [SerializeField] List<GameObject> Effects = new List<GameObject>();

    AudioManager audioManager => AudioManager.instance;

    void Start()
    {
        if (enemyBase != null) initialBodyScale = enemyBase.transform.localScale;
    }

    void Update()
    {
        // Watch HP even while the boss is attacking or stunned.
        if (enemyBase == null || phase == Phase.P3 || enemyBase.IsCharmed ||
            enemyBase.status == EnemyBase.Status.Die || enemyBase.MAXHP <= 0f) return;
        if (enemyBase.HP < enemyBase.MAXHP * thirdHpThreshold) BeginThirdPhase();
    }

    void OnDisable()
    {
        if (enemyBase != null) enemyBase.IsInvulnerable = false;
    }

    void BeginThirdPhase()
    {
        phase = Phase.P3;
        thirdRecoilActive = true;
        thirdIntroRoar = true;
        isSerious = true; // Retain the second-phase dimensional-rift follow-ups.
        if (initialBodyScale == Vector3.zero) initialBodyScale = enemyBase.transform.localScale;
        enemyBase.IsInvulnerable = true;
        enemyBase.status = EnemyBase.Status.Nomal;
        enemyBase.StanCount = 0f;
        enemyBase.rb.velocity = Vector2.zero;
        enemyBase.animator.SetBool("Damage", false);
        enemyBase.moveType = EnemyBase.MoveType.Attack;
        enemyBase.attackType = EnemyBase.AttackType.A1;
        enemyBase.AttackCount = 0f;
        AttackNumber = 0;
        AttackPhase = 0;
        AttackWaitTime = 0f;
        AttackCount1 = 0f;
        if (DamageArea != null) DamageArea.SetActive(false);
        if (asaltAttackArea != null) asaltAttackArea.SetActive(false);
        AllDestroyDengerArea();
        StartCoroutine(ThirdRecoil());
    }

    IEnumerator ThirdRecoil()
    {
        enemyBase.animator.Play("被弾", 0, 0);
        yield return new WaitForSeconds(thirdRecoilDuration);
        if (enemyBase == null || enemyBase.status == EnemyBase.Status.Die) yield break;
        thirdRecoilActive = false;
        enemyBase.rb.velocity = Vector2.zero;
        enemyBase.AttackCount = 0f;
        AttackNumber = 0;
        AttackPhase = 0;
    }

    void FinishThirdIntro()
    {
        if (!thirdIntroRoar) return;
        thirdIntroRoar = false;
        SetThirdBodyScale(thirdBodyMultiplier);
        if (DamageArea != null) DamageArea.SetActive(true);
        enemyBase.IsInvulnerable = false;
        ApplyThirdParticles();
    }

    void SetThirdBodyScale(float amount)
    {
        if (enemyBase == null) return;
        if (initialBodyScale == Vector3.zero) initialBodyScale = enemyBase.transform.localScale;
        enemyBase.transform.localScale = initialBodyScale * amount;
    }

    void ApplyThirdParticles()
    {
        if (thirdParticleSystem == null && enemyBase != null)
        {
            foreach (ParticleSystem particle in enemyBase.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (particle != null &&
                    string.Equals(particle.gameObject.name, "third", System.StringComparison.OrdinalIgnoreCase))
                {
                    thirdParticleSystem = particle;
                    break;
                }
            }
        }
        if (thirdParticleSystem == null)
        {
            Debug.LogWarning("SB_Whale_AI: thirdParticleSystem is not assigned.", this);
            return;
        }
        ParticleSystem.EmissionModule emission = thirdParticleSystem.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(3f);
    }

    float ThirdRangeScale => phase == Phase.P3 ? thirdAttackMultiplier : 1f;

    GameObject ScaleSpawn(GameObject instance, float amount)
    {
        if (instance != null && !Mathf.Approximately(amount, 1f))
            instance.transform.localScale *= amount;
        return instance;
    }

    GameObject SpawnThirdScaledObject(GameObject prefab, Vector2 position, float lifetime)
    {
        return ScaleSpawn(SpawnObjectNow_Input(prefab, position, lifetime), ThirdRangeScale);
    }

    void SpawnRoarWarning(float seconds)
    {
        float amount = phase == Phase.P3 ? Mathf.Pow(thirdRoarGrowth, thirdRoarPulse) : 1f;
        ScaleSpawn(DengerAreaSpawn(Denger_HoukouAttackArea, transform.position, seconds), amount);
    }

    void EmitRoarPulse()
    {
        float amount = phase == Phase.P3 ? Mathf.Pow(thirdRoarGrowth, thirdRoarPulse) : 1f;
        ScaleSpawn(SpawnObjectNow_Input(HoukouAttack, transform.position, 0.1f), amount);
        ScaleSpawn(SpawnObjectNow_Input(Effects[0], transform.position, 3f), amount);
        if (thirdIntroRoar)
            SetThirdBodyScale(Mathf.Lerp(1f, thirdBodyMultiplier,
                Mathf.Clamp01((thirdRoarPulse + 1f) / 4f)));
        thirdRoarPulse++;
    }

    void WarpThirdFourway()
    {
        if (enemyBase == null || thirdWarpRadius <= 0f) return;
        Vector2 origin = enemyBase.transform.position;
        Collider2D body = DamageArea != null ? DamageArea.GetComponent<Collider2D>() : null;
        Vector2 bodySize = body != null ? (Vector2)body.bounds.size * 0.9f :
            new Vector2(2f, 2f) * thirdBodyMultiplier;

        for (int attempt = 0; attempt < 36; attempt++)
        {
            Vector2 offset = Random.insideUnitCircle * thirdWarpRadius;
            if (offset.sqrMagnitude < 0.16f) continue;
            Vector2 next = origin + offset;
            bool hasFloor = false;
            foreach (Collider2D hit in Physics2D.OverlapPointAll(next))
            {
                if (hit != null && hit.CompareTag("Stage_Floor"))
                {
                    hasFloor = true;
                    break;
                }
            }
            if (!hasFloor) continue;
            bool blocked = false;
            foreach (Collider2D hit in Physics2D.OverlapBoxAll(next, bodySize, 0f))
            {
                if (hit != null && hit.CompareTag("Stage_Wall"))
                {
                    blocked = true;
                    break;
                }
            }
            if (blocked) continue;
            enemyBase.rb.velocity = Vector2.zero;
            enemyBase.transform.position = next;
            LookAtCurrentTarget();
            if (audioclips.Count > 2 && audioclips[2] != null) audioManager.PlaySE(audioclips[2]);
            return;
        }
        // Do not teleport into walls if no suitable floor exists within three tiles.
    }

    IEnumerator ThirdRiftShockwave(Vector2 position, float delay)
    {
        // Show the danger zone throughout the staggered 3-4 second fuse.
        ScaleSpawn(DengerAreaSpawn(Denger_HoukouAttackArea, position,
            Mathf.Max(0.1f, delay)), thirdShockwaveAreaScale);
        yield return new WaitForSeconds(delay);
        if (enemyBase == null || enemyBase.status == EnemyBase.Status.Die) yield break;
        ScaleSpawn(SpawnObjectNow_Input(HoukouAttack, position, thirdShockwaveHitDuration),
            thirdShockwaveAreaScale);
        ScaleSpawn(SpawnObjectNow_Input(Effects[0], position, 2f), thirdShockwaveAreaScale);
        all_System.camera_Controller.Shake(0.2f, 0.3f);
    }

    public override void M_Stan()
    {
        Finish(); AttackWaitTime = 0; StandOk = false; asaltAttackArea.SetActive(false);
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
        Finish(); AttackWaitTime = 0; StandOk = false; asaltAttackArea.SetActive(false);
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
        if (thirdRecoilActive) return;
        
        if (enemyBase.moveType != EnemyBase.MoveType.Attack)
        {
            enemyBase.animator.Play("移動");
            AttackWaitTime += Time.deltaTime;

            LookAtCurrentTarget();
            enemyBase.rb.velocity = TargetDirection.normalized * enemyBase.SPEED;

            if (phase == Phase.P1)
            {
                
                if (enemyBase.HP < enemyBase.MAXHP * 0.65f)
                {
                    audioManager.BgmSource.clip = BossBGM;
                    
                    phase = Phase.P2;
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.attackType = EnemyBase.AttackType.A6;
                    DamageArea.SetActive(false);
                    isSerious = true;
                }
                
                if (AttackWaitTime > 3)
                {
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.rb.velocity = Vector2.zero;
                    float random = Random.RandomRange(1, 5);
                    //float random = 5;
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


                if (AttackWaitTime > 1.5)
                {
                    AttackWaitTime = 0;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.rb.velocity = Vector2.zero;
                    float random = Random.RandomRange(1, 6);
                    //float random = 5;
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

            if (phase == Phase.P3 && AttackWaitTime > 1.5f)
            {
                AttackWaitTime = 0f;
                enemyBase.moveType = EnemyBase.MoveType.Attack;
                enemyBase.rb.velocity = Vector2.zero;
                switch (Random.Range(1, 6))
                {
                    case 1: enemyBase.attackType = EnemyBase.AttackType.A1; break;
                    case 2: enemyBase.attackType = EnemyBase.AttackType.A2; break;
                    case 3: enemyBase.attackType = EnemyBase.AttackType.A3; break;
                    case 4: enemyBase.attackType = EnemyBase.AttackType.A4; break;
                    case 5: enemyBase.attackType = EnemyBase.AttackType.A5; break;
                }
            }
        }

    }
    public override void M_Attack()
    {
        if (thirdRecoilActive) return;

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
                A6();
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
        AttackPhase = Number;
        enemyBase.AttackCount = 0;
    }
    void Finish()
    {
        AttackPhase = 0;
        AttackNumber = 0;
        AttackCount1 = 0;
        enemyBase.AttackCount = 0;
        enemyBase.moveType = EnemyBase.MoveType.Move;
    }
    
    void A1()
    {
        if (AttackPhase == 0)
        {
            enemyBase.animator.Play("攻撃待機", 0, 0);
            thirdRoarPulse = 0;
            SpawnRoarWarning(2f);
            
            NextPhase();
        }
        if(AttackPhase == 1) 
        {
            if (attackcount() > 2) 
            {
                enemyBase.animator.Play("攻撃", 0, 0);
                EmitRoarPulse();
                all_System.camera_Controller.Shake(0.2f, 0.3f);
                audioManager.PlaySE(audioclips[0]);
                NextPhase();
                SpawnRoarWarning(0.5f);
            }
        }
        if (AttackPhase == 2) 
        {
            if (attackcount() > 0.5) 
            {
                // Unlike the legacy warning timing, P3 warns before each growing pulse.
                if (phase != Phase.P3)
                    DengerAreaSpawn(Denger_HoukouAttackArea, transform.position, 0.5f);
                EmitRoarPulse();
                all_System.camera_Controller.Shake(0.2f, 0.3f);
                AttackNumber++;
                if (phase == Phase.P3 && AttackNumber < 3)
                    SpawnRoarWarning(0.5f);
                if (AttackNumber < 3)
                {
                    enemyBase.AttackCount = 0;
                }
                else 
                {
                    if (isSerious)
                    {
                        for (int i = 0; i < 50; i++)
                        {
                            for (int j = 0; j < 6; j++)
                            {
                                Vector2 randomPos = Vector2.zero;
                                bool ok = false;
                                float range = 12f;
                                float minrange = 5.5f;
                                while (!ok)
                                {
                                    randomPos = new Vector2(Random.Range(-range, range), Random.Range(-range, range));
                                    if (Mathf.Abs(randomPos.x) > minrange || Mathf.Abs(randomPos.y) > minrange) ok = true;
                                }
                                StartCoroutine(LiftSpawnLing((float)i / 30, (Vector2)transform.position + randomPos * ThirdRangeScale));
                            }
                        }
                        for (int i = 0; i < 50; i++)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                Vector2 randomPos = Vector2.zero;
                                bool ok = false;
                                float range = 20;
                                float minrange = 12f;
                                while (!ok)
                                {
                                    randomPos = new Vector2(Random.Range(-range, range), Random.Range(-range, range));
                                    if (Mathf.Abs(randomPos.x) > minrange || Mathf.Abs(randomPos.y) > minrange) ok = true;
                                }
                                StartCoroutine(LiftSpawnLing((float)i / 30 + 2, (Vector2)transform.position + randomPos * ThirdRangeScale));
                            }
                        }
                    }
                    NextPhase();
                }
            }
        }
        if (AttackPhase == 3) 
        {
            if (attackcount() > 0.5) 
            {
                FinishThirdIntro();
                AttackFinish();
            }
        }
    }
   
    void A2()
    {
        if (AttackPhase == 0)
        {
            enemyBase.animator.Play("攻撃");
            saveAngle = 0;
            if (attackcount() > 1) 
            {
                NextPhase();
            }
        }
        if (AttackPhase == 1) 
        {
            if (attackcount() > 0.1)
            {
                if (phase == Phase.P3) WarpThirdFourway();
                for (int i = 0; i < 4; i++) 
                {
                    GameObject CL_Effect = ScaleSpawn(
                        DengerAreaSpawn(Denger_4AttackArea, transform.position,
                            phase == Phase.P3 ? thirdFourwayAttackDelay : 1f),
                        ThirdRangeScale);

                    float dirAngle = saveAngle + 90 * i;
                    CL_Effect.transform.Rotate(0, 0, dirAngle);
                    
                    
                }
                NextPhase();
            }
        }
        if (AttackPhase == 2) 
        {
            if (attackcount() > (phase == Phase.P3 ? thirdFourwayAttackDelay : 1.1f))
            {
                for (int i = 0; i < 4; i++)
                {
                    GameObject CL_Effect = SpawnThirdScaledObject(AttackArea4, transform.position, 0.1f);
                    CL_Effect.transform.Rotate(0, 0, saveAngle + 90 * i);
                }
                saveAngle += 45;
                enemyBase.animator.Play("攻撃待機");

                audioManager.PlaySE(audioclips[1]);
                all_System.camera_Controller.Shake(0.3f, 0.2f);
                SpawnThirdScaledObject(Effects[0], transform.position, 3f);

                NextPhase();

                if (isSerious)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            Vector2 randomPos = Vector2.zero;
                            float range = 12f;
                            randomPos = new Vector2(Random.Range(-range, range), Random.Range(-range, range));

                            StartCoroutine(LiftSpawnLing((float)i / 30, (Vector2)transform.position + randomPos * ThirdRangeScale));
                        }
                    }
                }
            }
            
        }
        if (AttackPhase == 3) 
        {
            if (attackcount() > 0.1f) 
            {
                AttackNumber++;
                if (AttackNumber > 4)
                {
                    NextPhase();
                }
                else
                {
                    ChoiseNextPhase(1);
                    enemyBase.animator.Play("攻撃");
                }
            }
        }
        if (AttackPhase == 4) 
        {
            AttackFinish();
        }
    }
    void A3()
    {
        if (AttackPhase == 0)
        {
            LookAtCurrentTarget();
            enemyBase.animator.Play("攻撃待機", 0, 0);

            GameObject CL_AttackPPP = SpawnAttackWarning(EnemyContoroller.WarningRectangle, Denger_4AttackArea, enemyBase.transform.position, Quaternion.identity, 1f, null, 0f, false);
            CL_AttackPPP.transform.up = TargetDirection.normalized;
            

            NextPhase();
        }
        if (AttackPhase == 1)
        {
            if (enemyBase.AttackCount > 0.8)
            {
                asaltAttackArea.SetActive(true);
                enemyBase.animator.Play("歩行", 0, 0);
                NextPhase();
            }
        }
        if (AttackPhase == 2)
        {
            enemyBase.rb.velocity = TargetDirection.normalized * enemyBase.SPEED * 8;
            if (enemyBase.AttackCount > 2 * (1 + AttackNumber * 0.5))
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
                        Vector2 perp = new Vector2(-TargetDirection.normalized.y, TargetDirection.normalized.x);

                        // 攻撃判定を左右に生成する位置
                        Vector2 leftPos = (Vector2)transform.position + perp * 0.5f * (i + 3);
                        Vector2 rightPos = (Vector2)transform.position - perp * 0.5f * (i + 3);

                        // 攻撃判定オブジェクトを左右に生成
                        GameObject leftAttack = Instantiate(RockNeedle, leftPos, Quaternion.identity);
                        ConfigureSpawnedAttack(leftAttack);
                        GameObject rightAttack = Instantiate(RockNeedle, rightPos, Quaternion.identity);
                        ConfigureSpawnedAttack(rightAttack);
                        Destroy(leftAttack, 5);
                        Destroy(rightAttack, 5);
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        // 垂直方向（左右）ベクトル
                        Vector2 perp = new Vector2(-TargetDirection.normalized.y, TargetDirection.normalized.x);

                        // 攻撃判定を左右に生成する位置
                        Vector2 leftPos = (Vector2)transform.position + perp * 0.5f * (i + 3);
                        Vector2 rightPos = (Vector2)transform.position - perp * 0.5f * (i + 3);

                        // 攻撃判定オブジェクトを左右に生成
                        GameObject leftAttack = Instantiate(RockNeedle, leftPos, Quaternion.identity);
                        ConfigureSpawnedAttack(leftAttack);
                        GameObject rightAttack = Instantiate(RockNeedle, rightPos, Quaternion.identity);
                        ConfigureSpawnedAttack(rightAttack);
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
            if (AttackNumber == 0)
            {
                AttackNumber++;
                if (isSerious)
                {
                    ChoiseNextPhase(10);
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
                        ChoiseNextPhase(0);
                        enemyBase.moveType = EnemyBase.MoveType.Attack;
                        enemyBase.attackType = EnemyBase.AttackType.A1;
                    }
                }
            }
            else
            {
                Finish();
            }


        }
        if (AttackPhase == 10) 
        {
            if (attackcount() > 0.2) 
            {
                enemyBase.animator.Play("ワープ", 0, 0);
                DamageArea.SetActive(false);
                audioManager.PlaySE(audioclips[2]);
                NextPhase();
            }
        }
        if (AttackPhase == 11) 
        {
            if (attackcount() > 1) 
            {
                audioManager.PlaySE(audioclips[2]);

                bool FindPlayerPos = false;
                int safeCount = 0; // 安全カウンタ

                while (!FindPlayerPos && safeCount < 100)
                {
                    safeCount++;

                    Vector2 TargetFindPlayerPos = new Vector2(
                        Random.Range(-4f, 4f) + TargetPosition.x,
                        Random.Range(-2f, 2f) + TargetPosition.y
                    );

                    FindPlayerPos = IsTagAtPosition(TargetFindPlayerPos, "Stage_Floor");

                    if (FindPlayerPos)
                    {
                        targetPos = TargetFindPlayerPos;
                        targetDirection = targetPos - (Vector2)transform.position;
                        enemyBase.isTargetLook(targetPos);
                    }
                }

                // 見つからなかったときの代替処理
                if (!FindPlayerPos)
                {
                    // 代替位置を設定（例：現在位置のまま or デフォルト座標）
                    targetPos = new Vector2 (TargetPosition.x, TargetPosition.y+2);
                    
                    Debug.LogWarning("Floor not found. Using player position as fallback.");
                }


                enemyBase.transform.position = targetPos;
                enemyBase.animator.Play("出現", 0, 0);
                NextPhase();

                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        Vector2 randomPos = Vector2.zero;
                        float range = 6f;
                        randomPos = new Vector2(Random.Range(-range, range), Random.Range(-range, range));

                        StartCoroutine(LiftSpawnLing((float)i / 30, (Vector2)transform.position + randomPos, true));
                    }
                }
            }
        }
        if (AttackPhase == 12) 
        {
            if (attackcount() > 1) 
            {
                DamageArea.SetActive(true);
                NextPhase();
            }
        }
        if (AttackPhase == 13)
        {
            if (attackcount() > 0.5)
            {
                int randomnumberAttack = Random.Range(0, 2);
                if (randomnumberAttack == 0)
                {
                    ChoiseNextPhase(0);
                }
                else 
                {
                    ChoiseNextPhase(0);
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.attackType = EnemyBase.AttackType.A1;
                }
                
            }
        }
    }
    void A4()
    {
        if (AttackPhase == 0) 
        {
            LookAtCurrentTarget();
            enemyBase.animator.Play("攻撃待機");
            GameObject CL_denger = ScaleSpawn(
                DengerAreaSpawn(Denger_4AttackArea, transform.position, 1f), ThirdRangeScale);
            CL_denger.transform.up = TargetDirection.normalized;
            targetPos = TargetPosition;
            NextPhase();
        }
        if (AttackPhase == 1)
        {
            if (attackcount() > 1)
            {
                GameObject CL_Attack = SpawnThirdScaledObject(AttackArea4, transform.position, 0.1f);
                CL_Attack.transform.up = TargetDirection.normalized;
                NextPhase();
                audioManager.PlaySE(audioclips[1]);

                all_System.camera_Controller.Shake(0.3f, 0.2f);
                SpawnThirdScaledObject(Effects[0], transform.position, 3f);

                if (isSerious)
                {
                    for (int i = 0; i < 50; i++)
                    {
                        bool withShockwave = phase == Phase.P3 &&
                            i % Mathf.Max(1, thirdShockwaveEveryNthRift) == 0;
                        StartCoroutine(Ougi(i, targetPos, (float)i / 40, withShockwave));
                    }
                }
            }
        }
        if (AttackPhase == 2) 
        {
            if (attackcount() > 1) 
            {
                AttackNumber++;
                if (AttackNumber > 2)
                {
                    AttackFinish();
                }
                else 
                {
                    ChoiseNextPhase(0);
                }
            }
        }
    }
    void A5()
    {
        if (AttackPhase == 0)
        {
            if (attackcount() > 0.2)
            {
                enemyBase.animator.Play("ワープ", 0, 0);
                DamageArea.SetActive(false);
                audioManager.PlaySE(audioclips[2]);
                NextPhase();
            }
        }
        if (AttackPhase == 1)
        {
            if (attackcount() > 1)
            {
                audioManager.PlaySE(audioclips[2]);

                bool FindPlayerPos = false;
                int safeCount = 0; // 安全カウンタ

                while (!FindPlayerPos && safeCount < 100)
                {
                    safeCount++;

                    Vector2 TargetFindPlayerPos = new Vector2(
                        Random.Range(-4f, 4f) + TargetPosition.x,
                        Random.Range(-2f, 2f) + TargetPosition.y
                    );

                    FindPlayerPos = IsTagAtPosition(TargetFindPlayerPos, "Stage_Floor");

                    if (FindPlayerPos)
                    {
                        targetPos = TargetFindPlayerPos;
                        targetDirection = targetPos - (Vector2)transform.position;
                        enemyBase.isTargetLook(targetPos);
                    }
                }

                // 見つからなかったときの代替処理
                if (!FindPlayerPos)
                {
                    // 代替位置を設定（例：現在位置のまま or デフォルト座標）
                    targetPos = new Vector2(TargetPosition.x, TargetPosition.y + 2);
                    Debug.LogWarning("Floor not found. Using player position as fallback.");
                }


                enemyBase.transform.position = targetPos;
                enemyBase.animator.Play("出現", 0, 0);
                NextPhase();

                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        Vector2 randomPos = Vector2.zero;
                        float range = 6f;
                        randomPos = new Vector2(Random.Range(-range, range), Random.Range(-range, range));

                        StartCoroutine(LiftSpawnLing((float)i / 30, (Vector2)transform.position + randomPos * ThirdRangeScale));
                    }
                }
            }
        }
        if (AttackPhase == 2)
        {
            if (attackcount() > 1)
            {
                DamageArea.SetActive(true);
                NextPhase();
            }
        }
        if (AttackPhase == 3)
        {
            if (attackcount() > 1)
            {
                NextPhase();
            }
        }
        if (AttackPhase == 4)
        {
            LookAtCurrentTarget();
            enemyBase.animator.Play("攻撃");
            GameObject CL_denger = ScaleSpawn(
                DengerAreaSpawn(Denger_4AttackArea, transform.position, 1f), ThirdRangeScale);
            CL_denger.transform.up = TargetDirection.normalized;
            targetPos = TargetPosition;
            NextPhase();
        }
        if (AttackPhase == 5)
        {
            if (attackcount() > 1)
            {
                enemyBase.animator.Play("攻撃待機");
                GameObject CL_Attack = SpawnThirdScaledObject(AttackArea4, transform.position, 0.1f);
                CL_Attack.transform.up = TargetDirection.normalized;
                NextPhase();
                audioManager.PlaySE(audioclips[1]);

                all_System.camera_Controller.Shake(0.3f, 0.2f);
                SpawnThirdScaledObject(Effects[0], transform.position, 3f);

                for (int i = 0; i < 50; i++)
                {
                    StartCoroutine(Ougi(i, targetPos, (float)i / 40));
                }

                
            }
        }
        if(AttackPhase == 6) 
        {
            if (attackcount() > 1) 
            { 
            AttackNumber++;
                if (AttackNumber > 3)
                {
                    AttackFinish();
                }
                else 
                {
                    ChoiseNextPhase(0);
                }
            }
        }
    }

    void A6() 
    {
        if (AttackPhase == 0) 
        {
            NextPhase();

            all_System.camera_Controller.Shake(1, 0.3f);
            audioManager.PlaySE(audioclips[0]);
            enemyBase.animator.Play("被弾");
        }
        if (AttackPhase == 1) 
        {
            LookAtCurrentTarget();
            enemyBase.rb.velocity = TargetDirection.normalized * -enemyBase.SPEED;
            if (attackcount() > 1) 
            {
                NextPhase();
                enemyBase.animator.Play("攻撃");
            }
        }
        if (AttackPhase == 2) 
        {
            enemyBase.rb.velocity = TargetDirection.normalized * -enemyBase.SPEED *0.5f;
            if (attackcount() > 0.5)
            {
                enemyBase.rb.velocity = Vector2.zero;
                
                audioManager.PlaySE(audioclips[2]);
                enemyBase.animator.Play("消える");
                NextPhase();
            }
        }
        if (AttackPhase == 3)
        {
            if (attackcount() > 5)
            {
                targetPos = Vector2.zero;
                NextPhase();
            }
        }
        if (AttackPhase == 4)
        {
            if (attackcount() > 1)
            {
                audioManager.PlaySE(audioclips[2]);
                audioManager.BgmSource.Play();


                enemyBase.transform.position = targetPos;
                enemyBase.animator.Play("出現", 0, 0);
                DamageArea.SetActive(true);
                audioManager.BgmSource.Play();

                NextPhase();
                
                for (int i = 0; i < 30; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        Vector2 randomPos = Vector2.zero;
                        float range = 15f;
                        randomPos = new Vector2(Random.Range(-range, range), Random.Range(-range, range));

                        StartCoroutine(LiftSpawnLing((float)i / 7, (Vector2)transform.position + randomPos * ThirdRangeScale));
                    }
                }
            }
        }
        if (AttackPhase == 5)
        {
            if (attackcount() > 1)
            {
                ChoiseNextPhase(0);
                enemyBase.moveType = EnemyBase.MoveType.Attack;
                enemyBase.attackType = EnemyBase.AttackType.A1;

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

        GameObject CL_AttackPPP = SpawnAttackWarning(EnemyContoroller.WarningRectangle, Denger_AttackArea_Stand, endPos, Quaternion.identity, 1.5f, null, 0f, false);

        
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
    float attackcount() 
    {
        float count = enemyBase.AttackCount;
        return count;

    }

    void BulletShot()
    {
        audioManager.PlaySE(audioclips[0]);
        Vector2 ARMdirection = TargetPosition - ArmAttackPoint.transform.position;
        GameObject CL_Bullet = Instantiate(Bullet, ArmAttackPoint.transform.position, Quaternion.identity);
        ConfigureSpawnedAttack(CL_Bullet);
        CL_Bullet.transform.up = ARMdirection.normalized;

        Rigidbody2D CL_rb = CL_Bullet.GetComponent<Rigidbody2D>();
        CL_rb.velocity = ARMdirection.normalized * 25;
        Destroy(CL_Bullet, 1);

    }
    void DoubleBulletShot()
    {
        audioManager.PlaySE(audioclips[0]);
        Vector2 ARMdirection = TargetPosition - ArmAttackPoint.transform.position;
        for (int i = 0; i < 2; i++)
        {
            GameObject CL_Slash = Instantiate(HalfBullet, ArmAttackPoint.transform.position, Quaternion.identity);
            ConfigureSpawnedAttack(CL_Slash);
            Vector2 NewLookDirection = Quaternion.Euler(0, 0, -6 + 12 * i) * ARMdirection.normalized;
            CL_Slash.transform.up = NewLookDirection.normalized;

            if (TargetDirection.x > 0)
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
        ConfigureSpawnedAttack(CL_Needle);

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
    private IEnumerator LiftSpawnLing(float delayTime, Vector2 Pos, bool keepSecondPhaseSize = false)
    {
        yield return new WaitForSeconds(delayTime);
        if (keepSecondPhaseSize)
            SpawnObjectNow(SlashLit, Pos, 5f);
        else
            SpawnThirdScaledObject(SlashLit, Pos, 5f);
    }
    IEnumerator Ougi(int number, Vector2 pos, float delayTime, bool withShockwave = false)
    {
        yield return new WaitForSeconds(delayTime);
        //number++;

        float AttackDistance = number / 2.5f * ThirdRangeScale;

        // ランダムな角度を生成 (0 ~ 360度)
        float randomAngle = Random.Range(-80f, 80f);

        // ランダムな角度をラジアンに変換
        float radians = randomAngle * Mathf.Deg2Rad;

       
        Quaternion rotation = Quaternion.AngleAxis(randomAngle, Vector3.forward);
        Vector2 newDirection = (pos - (Vector2)transform.position).normalized;

       
        Vector2 rotatedDirection = rotation * newDirection; // SavePos方向を基準に角度を回転
        Vector2 newPosition = (Vector2)transform.position + rotatedDirection * AttackDistance; // 回転方向で新しい位置を計算

        SpawnThirdScaledObject(SlashLit, newPosition, 2f);
        if (withShockwave)
        {
            float minDelay = Mathf.Max(0f, Mathf.Min(thirdShockwaveDelay.x, thirdShockwaveDelay.y));
            float maxDelay = Mathf.Max(minDelay, Mathf.Max(thirdShockwaveDelay.x, thirdShockwaveDelay.y));
            StartCoroutine(ThirdRiftShockwave(newPosition, Random.Range(minDelay, maxDelay)));
        }
    }
}

