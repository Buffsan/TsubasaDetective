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
    // Include the fast vanish/appear animation while keeping shots slightly faster than P2.
    [SerializeField, Min(0.01f)] float thirdFourwayAttackDelay = 1.1f;
    [SerializeField, Min(0.01f)] float thirdFourwayWarningDuration = 1f;
    [SerializeField, Min(0.01f)] float thirdFourwayWarpAnimationSpeed = 6f;
    [SerializeField, Min(0.01f)] float thirdFourwayDisappearSeconds = 0.25f;
    [SerializeField, Min(0.01f)] float thirdFourwayAppearSeconds = 0.3f;
    [SerializeField, Min(0f)] float thirdWarpRadius = 3f;
    bool thirdFourwayWarpAnimating;
    float thirdFourwayOriginalAnimatorSpeed = 1f;
    // 50 existing rifts remain; only the additional shockwaves are thinned to ~half (17 -> 9).
    [SerializeField, Range(1, 50)] int thirdShockwaveEveryNthRift = 6;
    [SerializeField, Min(0.01f)] float thirdShockwaveAreaScale = 0.7f;
    [SerializeField, Min(0.01f)] float thirdShockwaveWarningDuration = 1.5f;
    [SerializeField, Min(0.01f)] float thirdShockwaveHitDuration = 0.8f;
    [SerializeField, Min(0f)] float thirdShockwaveShakeDuration = 0.2f;
    [SerializeField, Min(0f)] float thirdShockwaveShakeStrength = 0.15f;
    [SerializeField] Vector2 thirdShockwaveDelay = new Vector2(3f, 4f);
    [SerializeField, Min(0f)] float thirdShockwaveExplosionAdvance = 0.5f;
    // Phase 3 may start only after A6 has finished the full P2 transition,
    // including its BGM change. Interruption will replay A6 instead of skipping it.
    bool secondPhaseIntroCompleted;
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
        // Do not bypass P2 when a single powerful hit pushes P1 below 25%.
        // A6 owns the BGM switch and the mandatory phase-two cutscene.
        if (phase != Phase.P2 || !secondPhaseIntroCompleted) return;
        if (enemyBase.HP < enemyBase.MAXHP * thirdHpThreshold) BeginThirdPhase();
    }

    void OnDisable()
    {
        StopThirdFourwayWarpAnimation();
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

    bool IsThirdWarpSafe(Vector2 destination)
    {
        bool onFloor = false;
        foreach (Collider2D hit in Physics2D.OverlapPointAll(destination))
        {
            if (hit != null && hit.CompareTag("Stage_Floor"))
            {
                onFloor = true;
                break;
            }
        }
        if (!onFloor) return false;

        // Use collider's serialized dimensions: bounds is empty while DamageArea is disabled.
        Vector2 size = new Vector2(4f, 4.5f) * thirdBodyMultiplier;
        if (DamageArea != null)
        {
            CapsuleCollider2D capsule = DamageArea.GetComponent<CapsuleCollider2D>();
            if (capsule != null)
            {
                Vector3 scale = DamageArea.transform.lossyScale;
                size = Vector2.Scale(capsule.size,
                    new Vector2(Mathf.Abs(scale.x), Mathf.Abs(scale.y)));
            }
        }

        foreach (Collider2D hit in Physics2D.OverlapBoxAll(destination, size * 0.9f, 0f))
            if (hit != null && hit.CompareTag("Stage_Wall")) return false;
        return true;
    }

    bool warnedThirdWarpNoDestination;

    void WarpThirdFourway()
    {
        if (enemyBase == null || thirdWarpRadius <= 0f) return;
        Vector2 origin = enemyBase.transform.position;
        Vector2 destination = origin;
        bool found = false;

        // First attempt random positions, prioritizing a clearly visible teleport.
        for (int attempt = 0; attempt < 36; attempt++)
        {
            Vector2 offset = Random.insideUnitCircle * thirdWarpRadius;
            if (offset.sqrMagnitude < 1f) continue;
            Vector2 candidate = origin + offset;
            if (!IsThirdWarpSafe(candidate)) continue;
            destination = candidate;
            found = true;
            break;
        }

        // Deterministic fallback prevents random bad luck from cancelling every warp.
        // Grid offsets are world units; one unit corresponds to one tile on the stage grid.
        if (!found)
        {
            int radius = Mathf.CeilToInt(thirdWarpRadius);
            int validCount = 0;
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector2 offset = new Vector2(x, y);
                    if (offset.sqrMagnitude < 1f ||
                        offset.sqrMagnitude > thirdWarpRadius * thirdWarpRadius)
                        continue;

                    Vector2 candidate = origin + offset;
                    if (!IsThirdWarpSafe(candidate)) continue;

                    // Reservoir sampling: every valid grid point has the same probability.
                    validCount++;
                    if (Random.Range(0, validCount) == 0)
                    {
                        destination = candidate;
                        found = true;
                    }
                }
            }
        }

        if (!found)
        {
            if (!warnedThirdWarpNoDestination)
            {
                Debug.LogWarning(
                    "SB_Whale_AI: P3 four-direction warp has no safe Stage_Floor " +
                    "destination within the configured radius. " +
                    "Check floor/wall tags and the 1.5x boss footprint.", this);
                warnedThirdWarpNoDestination = true;
            }
            return;
        }

        warnedThirdWarpNoDestination = false;
        enemyBase.rb.velocity = Vector2.zero;
        enemyBase.transform.position = destination;
        LookAtCurrentTarget();
        if (audioclips.Count > 2 && audioclips[2] != null)
            audioManager.PlaySE(audioclips[2]);
    }

    IEnumerator ThirdRiftShockwave(Vector2 position, float delay)
    {
        // The warning is spawned together with the rift in Ougi().
        // Trigger the impact slightly earlier to compensate for the visible
        // gap after its warning animation. Both A4 and A5 use this routine.
        float adjustedDelay = Mathf.Max(0f, delay - thirdShockwaveExplosionAdvance);
        yield return new WaitForSeconds(adjustedDelay);
        if (enemyBase == null || enemyBase.status == EnemyBase.Status.Die) yield break;

        ScaleSpawn(SpawnObjectNow_Input(HoukouAttack, position, thirdShockwaveHitDuration),
            thirdShockwaveAreaScale);
        ScaleSpawn(SpawnObjectNow_Input(Effects[0], position, 2f),
            thirdShockwaveAreaScale);
        all_System.camera_Controller.Shake(thirdShockwaveShakeDuration,
            thirdShockwaveShakeStrength);
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
                    secondPhaseIntroCompleted = false;
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
                // If A6 was interrupted (e.g. by stun), replay it before
                // selecting any regular P2 attack or allowing phase 3.
                if (!secondPhaseIntroCompleted)
                {
                    if (audioManager != null && audioManager.BgmSource != null)
                        audioManager.BgmSource.clip = BossBGM;
                    AttackWaitTime = 0f;
                    enemyBase.rb.velocity = Vector2.zero;
                    enemyBase.moveType = EnemyBase.MoveType.Attack;
                    enemyBase.attackType = EnemyBase.AttackType.A6;
                    enemyBase.AttackCount = 0f;
                    AttackNumber = 0;
                    AttackPhase = 0;
                    if (DamageArea != null) DamageArea.SetActive(false);
                    return;
                }

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
        StopThirdFourwayWarpAnimation();
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
                        Vector2 origin = transform.position;
                        float scale = ThirdRangeScale;
                        // Originally 300 + 200 rifts in 500 separate coroutines.
                        // Preserve both waves and their 2-second offset.
                        StartCoroutine(SpawnRiftWaves(origin, 50, 6,
                            1f / 30f, 0f, 12f, 5.5f, scale));
                        StartCoroutine(SpawnRiftWaves(origin, 50, 4,
                            1f / 30f, 2f, 20f, 12f, scale));
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
   
    void StartThirdFourwayWarpAnimation()
    {
        if (enemyBase == null) return;
        enemyBase.rb.velocity = Vector2.zero;
        if (!thirdFourwayWarpAnimating)
            thirdFourwayOriginalAnimatorSpeed = enemyBase.animator.speed;
        thirdFourwayWarpAnimating = true;
        enemyBase.animator.speed = thirdFourwayOriginalAnimatorSpeed * thirdFourwayWarpAnimationSpeed;
        enemyBase.animator.Play("ワープ", 0, 0);
        if (DamageArea != null) DamageArea.SetActive(false);
        if (audioclips.Count > 2 && audioclips[2] != null)
            audioManager.PlaySE(audioclips[2]);
    }

    void StopThirdFourwayWarpAnimation()
    {
        if (!thirdFourwayWarpAnimating || enemyBase == null) return;
        thirdFourwayWarpAnimating = false;
        enemyBase.animator.speed = thirdFourwayOriginalAnimatorSpeed;
        if (DamageArea != null && enemyBase.status != EnemyBase.Status.Die)
            DamageArea.SetActive(true);
    }

    void A2Third()
    {
        // Unlike the previous instant position swap, replay the whale's actual
        // warp and appearance animations at an accelerated speed for every shot.
        if (AttackPhase == 0)
        {
            StartThirdFourwayWarpAnimation();
            NextPhase();
            return;
        }
        if (AttackPhase == 1)
        {
            if (attackcount() >= thirdFourwayDisappearSeconds)
            {
                WarpThirdFourway(); // Move only after the warp animation has hidden the whale.
                enemyBase.animator.Play("出現", 0, 0);
                NextPhase();
            }
            return;
        }
        if (AttackPhase == 2)
        {
            if (attackcount() >= thirdFourwayAppearSeconds)
            {
                StopThirdFourwayWarpAnimation();
                enemyBase.animator.Play("攻撃", 0, 0);
                NextPhase();
            }
            return;
        }
        if (AttackPhase == 3)
        {
            for (int i = 0; i < 4; i++)
            {
                GameObject warning = ScaleSpawn(
                    DengerAreaSpawn(Denger_4AttackArea, transform.position,
                        thirdFourwayWarningDuration), ThirdRangeScale);
                warning.transform.Rotate(0, 0, saveAngle + 90f * i);
            }
            NextPhase();
            return;
        }
        if (AttackPhase == 4)
        {
            if (attackcount() >= thirdFourwayAttackDelay)
            {
                for (int i = 0; i < 4; i++)
                {
                    GameObject hit = SpawnThirdScaledObject(AttackArea4, transform.position, 0.1f);
                    hit.transform.Rotate(0, 0, saveAngle + 90f * i);
                }
                saveAngle += 45f;
                enemyBase.animator.Play("攻撃待機", 0, 0);
                audioManager.PlaySE(audioclips[1]);
                all_System.camera_Controller.Shake(0.3f, 0.2f);
                SpawnThirdScaledObject(Effects[0], transform.position, 3f);

                // P2 dimensional-rift follow-ups are deliberately retained.
                if (isSerious)
                {
                    StartCoroutine(SpawnRiftWaves((Vector2)transform.position,
                        5, 10, 1f / 30f, 0f, 12f, 0f, ThirdRangeScale));
                }
                NextPhase();
            }
            return;
        }
        if (AttackPhase == 5 && attackcount() > 0.1f)
        {
            AttackNumber++;
            if (AttackNumber > 4)
                AttackFinish();
            else
                ChoiseNextPhase(0);
        }
    }

    void A2()
    {
        if (phase == Phase.P3)
        {
            A2Third();
            return;
        }
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
                    StartCoroutine(SpawnRiftWaves((Vector2)transform.position,
                        5, 10, 1f / 30f, 0f, 12f, 0f, ThirdRangeScale));
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

                    FindPlayerPos = phase == Phase.P3
                        ? IsThirdWarpSafe(TargetFindPlayerPos)
                        : IsTagAtPosition(TargetFindPlayerPos, "Stage_Floor");

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
                    targetPos = phase == Phase.P3
                        ? (Vector2)transform.position
                        : new Vector2(TargetPosition.x, TargetPosition.y + 2f);
                    
                    Debug.LogWarning("Floor not found. Using player position as fallback.");
                }


                enemyBase.transform.position = targetPos;
                enemyBase.animator.Play("出現", 0, 0);
                NextPhase();

                StartCoroutine(SpawnRiftWaves((Vector2)transform.position,
                    5, 5, 1f / 30f, 0f, 6f, 0f, 1f));
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
                    StartCoroutine(SpawnFanRifts((Vector2)transform.position,
                        targetPos, ThirdRangeScale, phase == Phase.P3));
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

                    FindPlayerPos = phase == Phase.P3
                        ? IsThirdWarpSafe(TargetFindPlayerPos)
                        : IsTagAtPosition(TargetFindPlayerPos, "Stage_Floor");

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
                    targetPos = phase == Phase.P3
                        ? (Vector2)transform.position
                        : new Vector2(TargetPosition.x, TargetPosition.y + 2f);
                    Debug.LogWarning("Floor not found. Using player position as fallback.");
                }


                enemyBase.transform.position = targetPos;
                enemyBase.animator.Play("出現", 0, 0);
                NextPhase();

                StartCoroutine(SpawnRiftWaves((Vector2)transform.position,
                    5, 5, 1f / 30f, 0f, 6f, 0f, ThirdRangeScale));
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

                StartCoroutine(SpawnFanRifts((Vector2)transform.position,
                    targetPos, ThirdRangeScale, phase == Phase.P3));

                
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
                
                StartCoroutine(SpawnRiftWaves((Vector2)transform.position,
                    30, 8, 1f / 7f, 0f, 15f, 0f, ThirdRangeScale));
            }
        }
        if (AttackPhase == 5)
        {
            if (attackcount() > 1)
            {
                // A6 has finished: its appearance and second-phase BGM have played.
                secondPhaseIntroCompleted = true;
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
    // A single coroutine schedules an entire wave of rifts, instead of one
    // coroutine/WaitForSeconds allocation per individual projectile.
    // Wave count, positions, total projectiles and timing match the former loops.
    private IEnumerator SpawnRiftWaves(Vector2 origin, int waves, int perWave,
        float interval, float initialDelay, float range, float exclusion, float scale)
    {
        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);
        else
            yield return null;

        float beginTime = Time.time;
        Phase activePhase = phase;
        for (int wave = 0; wave < waves; wave++)
        {
            if (wave > 0)
            {
                float dueTime = beginTime + wave * interval;
                while (Time.time < dueTime)
                    yield return null;
            }

            if (enemyBase == null || enemyBase.status == EnemyBase.Status.Die ||
                enemyBase.IsCharmed || phase != activePhase)
                yield break;

            for (int j = 0; j < perWave; j++)
            {
                Vector2 offset;
                do
                {
                    offset = new Vector2(Random.Range(-range, range),
                        Random.Range(-range, range));
                }
                while (Mathf.Abs(offset.x) <= exclusion &&
                    Mathf.Abs(offset.y) <= exclusion && range > exclusion);

                GameObject rift = SpawnObjectNow_Input(SlashLit,
                    origin + offset * scale, 5f);
                ScaleSpawn(rift, scale);
            }
        }
    }

    // One timer drives all fifty fan rifts. The cast origin/direction are
    // captured before the boss moves or warps, so delayed rifts don't drift.
    private IEnumerator SpawnFanRifts(Vector2 origin, Vector2 target,
        float scale, bool allowShockwaves)
    {
        yield return null;
        float beginTime = Time.time;
        Phase ownerPhase = phase;
        Vector2 direction = (target - origin).normalized;

        for (int i = 0; i < 50; i++)
        {
            if (i > 0)
            {
                float dueTime = beginTime + i / 40f;
                while (Time.time < dueTime)
                    yield return null;
            }
            if (enemyBase == null || enemyBase.status == EnemyBase.Status.Die ||
                enemyBase.IsCharmed || phase != ownerPhase)
                yield break;

            float angle = Random.Range(-80f, 80f);
            Vector2 directionRotated = Quaternion.AngleAxis(angle, Vector3.forward) * direction;
            Vector2 position = origin + directionRotated * (i / 2.5f * scale);
            ScaleSpawn(SpawnObjectNow_Input(SlashLit, position, 2f), scale);

            if (allowShockwaves && i % Mathf.Max(1, thirdShockwaveEveryNthRift) == 0)
            {
                // Spawn the 1.5s warning on the same frame as its rift.
                ScaleSpawn(DengerAreaSpawn(Denger_HoukouAttackArea, position,
                    thirdShockwaveWarningDuration), thirdShockwaveAreaScale);
                float minDelay = Mathf.Max(0f, Mathf.Min(thirdShockwaveDelay.x, thirdShockwaveDelay.y));
                float maxDelay = Mathf.Max(minDelay, Mathf.Max(thirdShockwaveDelay.x, thirdShockwaveDelay.y));
                StartCoroutine(ThirdRiftShockwave(position, Random.Range(minDelay, maxDelay)));
            }
        }
    }

}

