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

    List<Vector2> PosHistory = new List<Vector2>();

    [SerializeField]
    float SaveInterval = 0.1f;

    float saveTimer = 0;

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
    [SerializeField] AudioClip BossBGM;

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

    [Header("形態移行：HP割合")]
    [Range(0.01f, 1f)][SerializeField] float secondPhaseHP = 0.93f;
    [Range(0.01f, 1f)][SerializeField] float thirdPhaseHP = 0.50f;

    [Header("通常攻撃間隔（攻撃終了後～次の選択）")]
    [SerializeField] float phase1AttackInterval = 3.5f;
    [SerializeField] float phase2AttackInterval = 2.5f;
    [SerializeField] float phase3AttackIntervalMultiplier = 0.5f;

    [Header("第二形態：爆発・のけぞり回転演出")]
    [SerializeField] ParticleSystem phase2ExplosionPrefab;
    [Min(0f)][SerializeField] float phase2ExplosionLifetime = 5f;
    [Min(0.05f)][SerializeField] float recoilSeconds = 2f;
    [Min(0f)][SerializeField] float recoilDistance = 1.5f;
    [Min(0f)][SerializeField] float recoilStartSpinSpeed = 1440f;
    [Min(0f)][SerializeField] float recoilEndSpinSpeed = 40f;
    [SerializeField] bool recoilClockwise = true;

    [Header("第二形態：痙攣演出")]
    [Min(0f)][SerializeField] float twitchSeconds = 0.5f;
    [Min(0f)][SerializeField] float twitchPositionJitter = 0.12f;
    [Min(0f)][SerializeField] float twitchRotationJitter = 12f;
    [Min(0.02f)][SerializeField] float twitchStepSeconds = 0.06f;

    [Header("第二形態：場外への吹き飛び")]
    [SerializeField] Vector2 arenaCenter = Vector2.zero;
    [SerializeField] float outsideDistance = 26f;
    [SerializeField] float flyOutSeconds = 0.65f;
    [Range(0.05f, 2f)][SerializeField] float flyOutSpeedMultiplier = 0.5f;
    [SerializeField] float flyOutSpinDegrees = 1080f;
    [SerializeField] float outsideWaitSeconds = 2f;
    [SerializeField] float flyBackSeconds = 0.8f;

    [Header("第二形態：帰還後の旋回～投擲（合計約6秒）")]
    [Min(0.1f)][SerializeField] float returnToThrowSeconds = 6f;
    [Min(0.1f)][SerializeField] float playerOrbitRadius = 5f;
    [Min(0f)][SerializeField] float orbitEntrySeconds = 0.6f;
    [Min(0f)][SerializeField] float orbitStartDegreesPerSecond = 540f;
    [Min(0f)][SerializeField] float orbitEndDegreesPerSecond = 75f;
    [SerializeField] bool orbitClockwise = true;

    [Header("第二形態：血濡れた複製機")]
    [SerializeField] GameObject bloodyReplicatorPrefab;
    [SerializeField] Transform carryAnchor;
    [SerializeField] Transform landingPointOverride;
    [SerializeField] Vector2 landingOffset = Vector2.zero;
    [SerializeField] float throwSeconds = 0.55f;
    [SerializeField] float throwArcHeight = 3f;
    [SerializeField] float embeddedAngle = -22f;
    [SerializeField] GameObject machineImpactEffect;
    [SerializeField] AudioClip bloodyReplicatorSpawnAudioClip;
    [SerializeField] float bloodyReplicatorRemoveEffect = 5;

    [Header("第三形態：中央への跳躍・円形隊列")]
    [Min(0.1f)][SerializeField] float phase3CenterJumpSeconds = 1.2f;
    [Min(0.1f)][SerializeField] float phase3FormationRadius = 2f;
    [Min(0f)][SerializeField] float phase3OrbitDegreesPerSecond = 15f;
    [Min(0.1f)][SerializeField] float phase3FormationJoinSpeed = 12f;
    [Min(0.01f)][SerializeField] float phase3FormationTolerance = 0.18f;
    [Min(1f)][SerializeField] float phase3GatherTimeout = 12f;
    [SerializeField] float phase3FormationStartAngle = 90f;

    [Header("第三形態：レーザー連射")]
    [Min(1)][SerializeField] int phase3LaserRounds = 3;
    [Min(0f)][SerializeField] float phase3LaserOrderInterval = 0.1f;
    [Min(0f)][SerializeField] float phase3LaserRoundInterval = 1f;
    [Range(0f, 180f)][SerializeField] float phase3AimSpread = 80f;
    [Min(0.1f)][SerializeField] float phase3LaserLifetime = 5f;

    [Header("第三形態：フィニッシュ・停止")]
    [Min(1)][SerializeField] int phase3FinishShotsPerEcho = 5;
    [Min(0f)][SerializeField] float phase3FinishShotInterval = 0.05f;
    [Range(0f, 180f)][SerializeField] float phase3FinishSpread = 15f;
    [Min(0f)][SerializeField] float phase3EchoStunSeconds = 3f;

    [Header("第三形態：複製機混乱中の本体リアクション")]
    [Min(0.05f)][SerializeField] float phase3ConfusionLookInterval = 0.3f;
    [Min(0f)][SerializeField] float phase3ConfusionTwitchDegrees = 12f;
    [Min(0f)][SerializeField] float phase3ConfusionTwitchOffset = 0.08f;
    [Min(0.1f)][SerializeField] float phase3ConfusionTwitchFrequency = 12f;

    // 1=P1→P2演出、2=P2→P3演出、0=通常。
    int transition = 0;
    bool transitionStarted;
    bool savedDamageBodyActive;
    bool arcInProgress;
    RigidbodyType2D arcPreviousBodyType;
    GameObject placedReplicator;
    BOSS_EternalMaidenReplay echoReplay;
    readonly List<BOSS_EternalMaidenEcho> phase3Formation = new List<BOSS_EternalMaidenEcho>();
    bool phase3OrbitActive;
    bool phase3FormationReady;
    float phase3OrbitAngle;
    bool phase3ConfusionReacting;
    bool phase3NeedsReformation;
    float phase3ConfusionStartedAt;
    float savedPhase3AnimatorSpeed;
    Transform phase3ReactionBody;
    Vector3 savedPhase3VisualPosition;
    Quaternion savedPhase3VisualRotation;
    Vector3 savedPhase3VisualScale;

    bool Phase3MachineConfused
    {
        get { return echoReplay != null && echoReplay.IsMachineConfused; }
    }

    private void Awake()
    {
        echoReplay = GetComponent<BOSS_EternalMaidenReplay>();
    }


    private void FixedUpdate()
    {
        saveTimer += Time.deltaTime;

        if (saveTimer >= SaveInterval)
        {
            saveTimer = 0;

            PosHistory.Add(transform.position);

            if (PosHistory.Count > 100)
            {
                PosHistory.RemoveAt(0);
            }
        }

        // 第三形態の特殊行動中、複製機の混乱に合わせて本体も狼狽する。
        UpdatePhase3ConfusionReaction();

        // 複製体は複製機の混乱中、位置と隊列角度を完全に固定する。
        if (phase3OrbitActive) UpdatePhase3Formation();
    }
    public override void M_Con()
    {
        CancelInterruptedAttackEffects();
        AllAttackNumber = 0;
        AttackWaitTime = 99;
        AttackNumber = 0;
        jampAsalt = false;
        AttackPhase = 0;
    }
    public override void M_Stan()
    {
        CancelInterruptedAttackEffects();
        AllAttackNumber = 0;
        AttackWaitTime = 99;
        AttackNumber = 0;
        jampAsalt = false;
        AttackPhase = 0;
    }

    public override void M_Die()
    {
        CancelInterruptedAttackEffects();
    }

    // 突進(A2)をスタン/混乱/死亡で中断したとき、Finish()を通れず
    // 突進判定とエフェクトが残り続ける問題を防ぐ。
    void CancelInterruptedAttackEffects()
    {
        if (AsaltAttackBody != null && AsaltAttackBody.activeSelf)
            AsaltAttackBody.SetActive(false);
        SetAttackEmission(0f);
        if (AttackparticleSystem != null && AttackparticleSystem.isPlaying)
            AttackparticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        // Rigidbodyの速度はスタン/混乱側のノックバック制御を優先して上書きしない。
    }

    public override void M_AI()
    {
        if (transition != 0) return;
        if (CheckPhaseTransition()) return;
        if (enemyBase.moveType == EnemyBase.MoveType.Attack) return;

        // 元コードは方向ベクトルを指定した直後に速度0へ戻していたため、
        // 移動仕様は変えず、攻撃待ち時間の進行だけ維持する。
        enemyBase.rb.velocity = Vector2.zero;
        enemyBase.animator.SetInteger("Anim", 0);
        AttackWaitTime += Time.deltaTime;

        float interval = phase1AttackInterval;
        if (phase == Phase.P2) interval = phase2AttackInterval;
        if (phase == Phase.P3) interval = phase2AttackInterval * phase3AttackIntervalMultiplier;
        if (AttackWaitTime < Mathf.Max(0.01f, interval)) return;

        SpawnCount++;
        AttackWaitTime = 0f;
        enemyBase.moveType = EnemyBase.MoveType.Attack;
        int random = Random.Range(1, 5); // A1～A4
        enemyBase.attackType = (EnemyBase.AttackType)(random - 1);
    }

    // 形態移行は攻撃途中でも発生させる。
    bool CheckPhaseTransition()
    {
        if (transition != 0 || enemyBase.MAXHP <= 0f ||
            enemyBase.status == EnemyBase.Status.Die || enemyBase.HP <= 0f)
            return false;

        float hpRate = enemyBase.HP / enemyBase.MAXHP;
        if (phase == Phase.P1 && hpRate < secondPhaseHP)
        {
            BeginTransition(1);
            return true;
        }
        if (phase == Phase.P2 && hpRate < thirdPhaseHP)
        {
            BeginTransition(2);
            return true;
        }
        return false;
    }

    void BeginTransition(int next)
    {
        transition = next;
        transitionStarted = false;
        if (next == 1)
        {
            // 93%を下回った瞬間から、投擲終了まで被弾判定を無効化する。
            savedDamageBodyActive = enemyBase.DamageBody != null && enemyBase.DamageBody.activeSelf;
            if (enemyBase.DamageBody != null) enemyBase.DamageBody.SetActive(false);

            // HP閾値を下回った瞬間に爆発エフェクトを一度だけ生成する。
            if (phase2ExplosionPrefab != null)
            {
                ParticleSystem explosion = Instantiate(phase2ExplosionPrefab, transform.position, Quaternion.identity);
                explosion.Play();
                if (phase2ExplosionLifetime > 0f)
                    Destroy(explosion.gameObject, phase2ExplosionLifetime);
            }
        }
        // 前の攻撃（A1連射やA3跳躍など）を停止する。
        StopAllCoroutines();
        if (arcInProgress)
        {
            enemyBase.rb.bodyType = arcPreviousBodyType;
            arcInProgress = false;
        }
        enemyBase.rb.velocity = Vector2.zero;
        if (AsaltAttackBody != null) AsaltAttackBody.SetActive(false);
        SetAttackEmission(0);
        AttackPhase = 0;
        AttackNumber = 0;
        enemyBase.AttackCount = 0f;
        enemyBase.attackType = EnemyBase.AttackType.A10;
        enemyBase.moveType = EnemyBase.MoveType.Attack;
        AttackWaitTime = 0f;
    }


    public override void M_Attack()
    {
        if (transition == 0) CheckPhaseTransition();
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
        SetAttackEmission(0);
    }
    void SetAttackEmission(float rate)
    {
        if (AttackparticleSystem == null) return;
        var emission = AttackparticleSystem.emission;
        emission.rateOverTime = rate;

        // 怯み・混乱でStopされた場合、次の突進で再生を再開する。
        if (rate > 0f && !AttackparticleSystem.isPlaying)
            AttackparticleSystem.Play();
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

                StartCoroutine(ShotArrow_A1(0f));
                StartCoroutine(ShotArrow_A1(0.25f));
                StartCoroutine(ShotArrow_A1(0.5f));

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

            LookAtCurrentTarget();

            DashDirection = TargetDirection.normalized;

            DashStartPos =
                (Vector2)transform.position
                - DashDirection * BackStepDistance;

            NextPhase();
        }

        if (AttackPhase == 1)
        {

            float t = attackcount() / BackStepTime;
            SetAttackEmission(DefaltEmmitionRate);
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
                SetAttackEmission(0);
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
                targetPos = TargetPosition;
                while (!FindPlayerPos && safeCount < 100)
                {
                    safeCount++;

                    Vector2 TargetFindPlayerPos = new Vector2(
                        Random.Range(-0.2f, 0.2f) + TargetPosition.x,
                        Random.Range(-0.2f, 0.2f) + TargetPosition.y
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
                isStarShot(7, 1);
                all_System.camera_Controller.Shake(0.1f, 0.1f);

                enemyBase.animator.Play("着地");
                GameObject leftAttack = Instantiate(AttackObjects[0], transform.position, Quaternion.identity);
                if (echoReplay != null) echoReplay.RecordSpawn(AttackObjects[0], leftAttack, 0.1f);
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

                CL_Laser.transform.up = (TargetPosition - transform.position).normalized;
                CL_Laser.transform.Rotate(0, 0, 180);
                CL_Laser.transform.Rotate(0, 0, Random.Range(-80, 80));
                if (echoReplay != null) echoReplay.RecordSpawn(AttackObjects[1], CL_Laser, 5f, aimMode: 2, aimSpread: 80f);

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
        if (transitionStarted || transition == 0) return;
        transitionStarted = true;
        if (transition == 1) StartCoroutine(TransitionToPhase2());
        else if (transition == 2) StartCoroutine(TransitionToPhase3());
    }

    IEnumerator TransitionToPhase2()
    {
        if (echoReplay != null) echoReplay.BeginSpecialAction();
        if (audioClips.Count > 1 && audioClips[1] != null)
            enemyBase.audioManager.PlaySE(audioClips[1]);
        // 被弾判定は BeginTransition(1) で既に無効化済み。
        enemyBase.rb.velocity = Vector2.zero;

        RigidbodyType2D previousBodyType = enemyBase.rb.bodyType;
        enemyBase.rb.bodyType = RigidbodyType2D.Kinematic;

        Vector2 start = transform.position;
        float initialRotation = transform.eulerAngles.z;

        // 1. 爆発後、低速でプレイヤーからのけぞりながら高速回転 → 徐々に減速。
        // 回転の累積角度を保持するので、痙攣へ移る直前に角度が飛ばない。
        Vector2 recoilDirection = CurrentTarget != null
            ? (start - (Vector2)TargetPosition)
            : (start - arenaCenter);
        if (recoilDirection.sqrMagnitude < 0.0001f) recoilDirection = Vector2.right;
        recoilDirection.Normalize();
        Vector2 recoilEndPosition = start + recoilDirection * recoilDistance;
        float recoilElapsed = 0f;
        float recoilRotation = initialRotation;
        float spinSign = recoilClockwise ? -1f : 1f;
        while (recoilElapsed < recoilSeconds)
        {
            float dt = Mathf.Min(Time.fixedDeltaTime, recoilSeconds - recoilElapsed);
            recoilElapsed += dt;
            float p = Mathf.Clamp01(recoilElapsed / Mathf.Max(0.01f, recoilSeconds));
            float slowdown = Mathf.SmoothStep(0f, 1f, p);
            float spinSpeed = Mathf.Lerp(recoilStartSpinSpeed, recoilEndSpinSpeed, slowdown);
            recoilRotation += spinSpeed * spinSign * dt;
            enemyBase.rb.MovePosition(Vector2.Lerp(start, recoilEndPosition, slowdown));
            enemyBase.rb.MoveRotation(recoilRotation);
            yield return new WaitForFixedUpdate();
        }

        start = recoilEndPosition;
        initialRotation = recoilRotation;
        enemyBase.rb.MovePosition(start);
        enemyBase.rb.MoveRotation(initialRotation);
        yield return new WaitForFixedUpdate();

        // 2. 0.5秒ほど、のけぞり終えた位置で不規則に痙攣する。
        float twitchElapsed = 0f;
        float nextTwitch = 0f;
        enemyBase.audioManager.BgmSource.Stop();
        while (twitchElapsed < twitchSeconds)
        {
            if (twitchElapsed >= nextTwitch)
            {
                Vector2 jitter = Random.insideUnitCircle * twitchPositionJitter;
                float angleJitter = Random.Range(-twitchRotationJitter, twitchRotationJitter);
                enemyBase.rb.MovePosition(start + jitter);
                enemyBase.rb.MoveRotation(initialRotation + angleJitter);
                nextTwitch += Mathf.Max(0.02f, twitchStepSeconds);
            }
            twitchElapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        enemyBase.rb.MovePosition(start);
        enemyBase.rb.MoveRotation(initialRotation);
        yield return new WaitForFixedUpdate();

        Vector2 outward = start - arenaCenter;
        if (outward.sqrMagnitude < 0.01f) outward = Vector2.right;
        Vector2 outside = arenaCenter + outward.normalized * outsideDistance;

        // 3. 従来の半分の速度で回転しながら場外へ吹っ飛ぶ。
        // flyOutSecondsは旧Prefabの設定値を維持し、倍率から実際の移動時間を計算する。
        float actualFlyOutSeconds = flyOutSeconds / Mathf.Max(0.05f, flyOutSpeedMultiplier);
        float t = 0f;
        while (t < actualFlyOutSeconds)
        {
            t += Time.fixedDeltaTime;
            float p = Mathf.Clamp01(t / Mathf.Max(0.01f, actualFlyOutSeconds));
            enemyBase.rb.MovePosition(Vector2.Lerp(start, outside, p));
            enemyBase.rb.MoveRotation(initialRotation + flyOutSpinDegrees * p);
            yield return new WaitForFixedUpdate();
        }
        enemyBase.rb.MovePosition(outside);
        enemyBase.rb.MoveRotation(0f);
        yield return new WaitForFixedUpdate();

        // 3. 場外に2秒滞在し、複製機を担いで戻ってくる。
        yield return new WaitForSeconds(Mathf.Max(0f, outsideWaitSeconds));
        GameObject carriedMachine = null;
        if (bloodyReplicatorPrefab != null)
        {
            Transform anchor = carryAnchor != null ? carryAnchor : transform;
            carriedMachine = Instantiate(bloodyReplicatorPrefab, anchor.position, anchor.rotation, anchor);
            carriedMachine.transform.localPosition = Vector3.zero;
            carriedMachine.transform.localRotation = Quaternion.identity;
            // 投擲時まで自身の物理挙動を止める。Prefabは表示用を推奨。
            Rigidbody2D machineRb = carriedMachine.GetComponent<Rigidbody2D>();
            if (machineRb != null) { machineRb.velocity = Vector2.zero; machineRb.simulated = false; }
        }
        else
        {
            Debug.LogWarning("永遠の少女: Bloody Replicator Prefab が未設定です。分身は生成されません。", this);
        }

        enemyBase.animator.Play("移動", 0, 0);
        t = 0f;
        while (t < flyBackSeconds)
        {
            t += Time.fixedDeltaTime;
            float p = Mathf.Clamp01(t / Mathf.Max(0.01f, flyBackSeconds));
            enemyBase.rb.MovePosition(Vector2.Lerp(outside, start, p));
            yield return new WaitForFixedUpdate();
        }
        enemyBase.rb.MovePosition(start);
        yield return new WaitForFixedUpdate();

        // 4. 複製機を抱えたままプレイヤーの周囲を高速で回り、徐々に減速する。
        //    帰還完了から投擲終了までの総時間を returnToThrowSeconds に揃える。
        //    複製機は carryAnchor の子なので、本体の周回に追従する。
        float orbitSeconds = Mathf.Max(0f, returnToThrowSeconds - Mathf.Max(0f, throwSeconds));
        if (orbitSeconds > 0f && CurrentTarget != null)
        {
            Vector2 orbitCenter = TargetPosition;
            Vector2 initialOffset = (Vector2)transform.position - orbitCenter;
            float orbitAngle = initialOffset.sqrMagnitude > 0.0001f
                ? Mathf.Atan2(initialOffset.y, initialOffset.x) * Mathf.Rad2Deg
                : 0f;
            float initialRadius = initialOffset.magnitude;
            float orbitElapsed = 0f;
            float directionSign = orbitClockwise ? -1f : 1f;

            while (orbitElapsed < orbitSeconds)
            {
                // プレイヤーが移動した場合も、その時点の位置を周回中心にする。
                orbitCenter = TargetPosition;
                float dt = Mathf.Min(Time.fixedDeltaTime, orbitSeconds - orbitElapsed);
                orbitElapsed += dt;
                float progress = Mathf.Clamp01(orbitElapsed / orbitSeconds);
                // 周回速度そのものを高速から低速へ滑らかに減らす。
                float slowdown = Mathf.SmoothStep(0f, 1f, progress);
                float angularSpeed = Mathf.Lerp(orbitStartDegreesPerSecond,
                                                orbitEndDegreesPerSecond, slowdown);
                orbitAngle += angularSpeed * directionSign * dt;

                // 帰還直後の位置から指定半径へ移す（半径差による瞬間移動を避ける）。
                float entryProgress = orbitEntrySeconds <= 0f ? 1f :
                    Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(orbitElapsed / orbitEntrySeconds));
                float radius = Mathf.Lerp(initialRadius, playerOrbitRadius, entryProgress);
                float radians = orbitAngle * Mathf.Deg2Rad;
                Vector2 orbitPosition = orbitCenter + new Vector2(Mathf.Cos(radians),
                                                                   Mathf.Sin(radians)) * radius;
                enemyBase.rb.MovePosition(orbitPosition);
                enemyBase.isTargetLook(orbitCenter);
                yield return new WaitForFixedUpdate();
            }
        }
        else if (orbitSeconds > 0f)
        {
            // プレイヤーが取得できない場合は場で待機し、投擲までの尺は維持する。
            yield return new WaitForSeconds(orbitSeconds);
        }

        // 5. プレイヤーへ投げ、地面に突き刺す。投擲が終わるまでは無敵のまま。
        if (carriedMachine != null)
        {
            carriedMachine.transform.SetParent(null, true);
            Vector3 throwStart = carriedMachine.transform.position;
            Vector3 target = landingPointOverride != null ? landingPointOverride.position :
                (CurrentTarget != null ? TargetPosition : transform.position);
            target += (Vector3)landingOffset;
            target.z = throwStart.z;
            t = 0f;
            while (t < throwSeconds)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / Mathf.Max(0.01f, throwSeconds));
                Vector3 pos = Vector3.Lerp(throwStart, target, p);
                pos.y += Mathf.Sin(p * Mathf.PI) * throwArcHeight;
                carriedMachine.transform.position = pos;
                carriedMachine.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 360f + embeddedAngle, p));
                yield return null;
            }
            carriedMachine.transform.position = target;
            carriedMachine.transform.rotation = Quaternion.Euler(0f, 0f, embeddedAngle);
            if (machineImpactEffect != null)
            {
                GameObject impact = Instantiate(machineImpactEffect, target, Quaternion.identity);
                Destroy(impact, 3f);
            }
            placedReplicator = carriedMachine;
            // 担いでいる間は simulated=false にしているため、着地後は物理判定を再開する。
            // 複製機を攻撃・混乱させられるよう、Rigidbody2DとCollider2Dを有効にする。
            Rigidbody2D landedMachineRb = placedReplicator.GetComponent<Rigidbody2D>();
            if (landedMachineRb != null)
            {
                landedMachineRb.bodyType = RigidbodyType2D.Kinematic;
                landedMachineRb.gravityScale = 0f;
                landedMachineRb.velocity = Vector2.zero;
                landedMachineRb.angularVelocity = 0f;
                landedMachineRb.constraints = RigidbodyConstraints2D.FreezeAll;
                landedMachineRb.simulated = true;
            }

            // 複製機Prefab内に「CloneExit」子があれば、そこから分身が出現する。

            Transform exit = placedReplicator.transform.Find("CloneExit");

            enemyBase.ALL_System.camera_Controller.Shake(0.5f,0.2f);

            EnemyBase machineEnemy = placedReplicator.GetComponentInChildren<EnemyBase>(true);
            if (echoReplay != null)
                echoReplay.ActivateMachine(exit != null ? exit : placedReplicator.transform,
                                           secondPhaseHP, machineEnemy);
        }
        enemyBase.audioManager.BgmSource.clip = BossBGM;

        enemyBase.audioManager.BgmSource.Play();
        enemyBase.rb.bodyType = previousBodyType;
        enemyBase.rb.velocity = Vector2.zero;
        // 投擲（複製機の設置）が完了するまで無敵を維持し、ここで元の被弾判定へ戻す。
        if (enemyBase.DamageBody != null) enemyBase.DamageBody.SetActive(savedDamageBodyActive);
        phase = Phase.P2;
        transition = 0;
        transitionStarted = false;
        if (echoReplay != null) echoReplay.EndSpecialAction();
        Finish();
    }

    // 混乱で複製体が停止している間、本体はその位置で左右を見渡し、小さく痙攣する。
    // Animatorに新しい状態は要求しないので、既存のアニメーション設定は変更不要。
    void UpdatePhase3ConfusionReaction()
    {
        bool shouldReact = transition == 2 && transitionStarted && Phase3MachineConfused;
        if (!shouldReact)
        {
            if (phase3ConfusionReacting) EndPhase3ConfusionReaction();
            return;
        }

        if (!phase3ConfusionReacting)
        {
            phase3ConfusionReacting = true;
            phase3ConfusionStartedAt = Time.time;
            phase3ReactionBody = enemyBase.EnemyAnimBody != null
                ? enemyBase.EnemyAnimBody.transform : null;
            if (phase3ReactionBody != null)
            {
                savedPhase3VisualPosition = phase3ReactionBody.localPosition;
                savedPhase3VisualRotation = phase3ReactionBody.localRotation;
                savedPhase3VisualScale = phase3ReactionBody.localScale;
            }
            if (enemyBase.animator != null)
            {
                savedPhase3AnimatorSpeed = enemyBase.animator.speed;
                enemyBase.animator.speed = 0f;
            }
            // 解除後は、停止していた円形隊列をもう一度整えてから射撃する。
            if (phase3OrbitActive)
            {
                phase3FormationReady = false;
                phase3NeedsReformation = true;
            }
        }

        if (enemyBase.rb != null)
        {
            enemyBase.rb.velocity = Vector2.zero;
            enemyBase.rb.angularVelocity = 0f;
        }

        float elapsed = Time.time - phase3ConfusionStartedAt;
        int direction = Mathf.FloorToInt(elapsed / Mathf.Max(0.05f, phase3ConfusionLookInterval)) % 2;
        enemyBase.isTargetLook((Vector2)transform.position +
                               (direction == 0 ? Vector2.left : Vector2.right));
        if (phase3ReactionBody != null)
        {
            float wave = elapsed * phase3ConfusionTwitchFrequency * Mathf.PI * 2f;
            phase3ReactionBody.localPosition = savedPhase3VisualPosition +
                new Vector3(Mathf.Sin(wave) * phase3ConfusionTwitchOffset,
                            Mathf.Cos(wave * 1.23f) * phase3ConfusionTwitchOffset * 0.5f, 0f);
            phase3ReactionBody.localRotation = savedPhase3VisualRotation *
                Quaternion.Euler(0f, 0f, Mathf.Sin(wave) * phase3ConfusionTwitchDegrees);
        }
    }

    void EndPhase3ConfusionReaction()
    {
        phase3ConfusionReacting = false;
        if (enemyBase.animator != null)
            enemyBase.animator.speed = savedPhase3AnimatorSpeed;
        if (phase3ReactionBody != null)
        {
            phase3ReactionBody.localPosition = savedPhase3VisualPosition;
            phase3ReactionBody.localRotation = savedPhase3VisualRotation;
            phase3ReactionBody.localScale = savedPhase3VisualScale;
            phase3ReactionBody = null;
        }
    }

    // 固定半径で生成順に並べ、全員が到着してからレーザー攻撃を始める。
    // 角度を全員で共有するので、巡回中も複製体の並び順は崩れない。
    void UpdatePhase3Formation()
    {
        if (echoReplay != null && echoReplay.IsMachineConfused) return;

        int count = phase3Formation.Count;
        if (count == 0)
        {
            phase3FormationReady = true;
            return;
        }

        phase3OrbitAngle += phase3OrbitDegreesPerSecond * Time.fixedDeltaTime;
        bool allReady = true;
        for (int i = 0; i < count; i++)
        {
            BOSS_EternalMaidenEcho echo = phase3Formation[i];
            if (echo == null) continue;
            float angle = (phase3FormationStartAngle + phase3OrbitAngle + 360f * i / count)
                          * Mathf.Deg2Rad;
            Vector2 destination = (Vector2)transform.position +
                new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * phase3FormationRadius;
            if (!echo.SpecialMoveTowards(destination, phase3FormationJoinSpeed,
                                         phase3FormationTolerance))
                allReady = false;
        }
        if (allReady) phase3FormationReady = true;
    }

    // 通常A4と同じレーザーPrefabを使用。記録には加えず、特殊行動中の複製体だけが撃つ。
    void FirePhase3Laser(BOSS_EternalMaidenEcho echo, bool reverseDirection)
    {
        if (echo == null || AttackObjects == null || AttackObjects[1] == null) return;

        Vector2 origin = echo.transform.position;
        Vector2 direction;
        if (reverseDirection)
        {
            // 本体→複製体のベクトルを延長した方向（プレイヤーは狙わない）。
            direction = origin - (Vector2)transform.position;
        }
        else
        {
            // 最初の3セットは従来のA4と同様、発射時点のプレイヤーを狙う。
            direction = CurrentTarget != null
                ? (Vector2)TargetPosition - origin
                : origin - (Vector2)transform.position;
        }
        if (direction.sqrMagnitude < 0.0001f) direction = Vector2.up;

        GameObject laser = Instantiate(AttackObjects[1], origin, Quaternion.identity);
        laser.transform.up = direction.normalized;
        laser.transform.Rotate(0f, 0f, 180f);
        float spread = reverseDirection ? phase3FinishSpread : phase3AimSpread;
        laser.transform.Rotate(0f, 0f, Random.Range(-spread, spread));

        // 分身の攻撃は分身自身のEnemyBaseを参照させる。
        EnemyAttack[] hits = laser.GetComponentsInChildren<EnemyAttack>(true);
        for (int i = 0; i < hits.Length; i++) hits[i].enemyBase = echo.LocalEnemy;
        Destroy(laser, phase3LaserLifetime);
    }

    // 混乱が解除されたら、停止した円形隊列が整ってから攻撃を再開する。
    // 既に発射したレーザーには触れず、未発射の攻撃だけ保留する。
    IEnumerator WaitUntilMachineAvailable()
    {
        float reformElapsed = 0f;
        while (true)
        {
            // 同じ特殊行動中に何度混乱しても、再帰を増やさず対応する。
            if (Phase3MachineConfused)
            {
                reformElapsed = 0f;
                yield return null;
                continue;
            }
            if (!phase3OrbitActive || !phase3NeedsReformation)
                yield break;
            if (phase3FormationReady)
            {
                phase3NeedsReformation = false;
                yield break;
            }
            reformElapsed += Time.deltaTime;
            if (reformElapsed >= phase3GatherTimeout)
            {
                Debug.LogWarning("永遠の少女: 混乱解除後の円形隊列再集合がタイムアウトしました。", this);
                phase3NeedsReformation = false;
                yield break;
            }
            yield return null;
        }
    }

    // 混乱・再集合している間は0.1秒・1秒・0.05秒の待機時間を消費しない。
    IEnumerator WaitPhase3ActiveSeconds(float seconds)
    {
        float remaining = Mathf.Max(0f, seconds);
        while (remaining > 0f)
        {
            if (Phase3MachineConfused || phase3NeedsReformation)
            {
                yield return StartCoroutine(WaitUntilMachineAvailable());
                continue;
            }
            remaining -= Time.deltaTime;
            yield return null;
        }
        yield return StartCoroutine(WaitUntilMachineAvailable());
    }

    IEnumerator TransitionToPhase3()
    {
        if (echoReplay != null) echoReplay.BeginSpecialAction();
        enemyBase.rb.velocity = Vector2.zero;

        // A3のジャンプ軌道と着地攻撃を流用。着地点のみ(0,0)に変更する。
        targetPos = Vector2.zero;
        yield return StartCoroutine(MoveInArc(Mathf.Max(0.1f, phase3CenterJumpSeconds), arcHeight, true));
        yield return StartCoroutine(WaitUntilMachineAvailable());
        enemyBase.rb.MovePosition(Vector2.zero);
        yield return new WaitForFixedUpdate();
        enemyBase.animator.Play("着地", 0, 0);
        isStarShot(7, 1);
        if (AttackObjects != null && AttackObjects[0] != null)
        {
            GameObject landingBall = Instantiate(AttackObjects[0], transform.position, Quaternion.identity);
            Destroy(landingBall, 0.1f);
        }

        // 現在存在する複製体を生成順に取得。特殊行動中は新しい分身生成を保留する。
        phase3Formation.Clear();
        if (echoReplay != null)
        {
            phase3Formation.AddRange(echoReplay.GetEchoes());
            phase3Formation.Sort((a, b) =>
                a == null ? (b == null ? 0 : 1) :
                (b == null ? -1 : a.Index.CompareTo(b.Index)));
        }
        phase3OrbitAngle = 0f;
        phase3FormationReady = false;
        phase3OrbitActive = true;

        // 全員が円形の隊列に入り、巡回を開始するまで待つ。
        float gatheringTime = 0f;
        while (!phase3FormationReady)
        {
            if (echoReplay == null || !echoReplay.IsMachineConfused)
                gatheringTime += Time.fixedDeltaTime;
            if (gatheringTime >= phase3GatherTimeout)
            {
                Debug.LogWarning("永遠の少女: 第三形態の隊列集合がタイムアウトしました。現在位置から射撃を開始します。", this);
                break;
            }
            yield return new WaitForFixedUpdate();
        }

        // 集合の最終フレームに混乱した場合も、攻撃は解除後まで行わない。
        yield return StartCoroutine(WaitUntilMachineAvailable());

        // 各複製体が0.1秒差で1本ずつ照射。全員終了後1秒待ち、計3セット。
        for (int round = 0; round < phase3LaserRounds; round++)
        {
            for (int i = 0; i < phase3Formation.Count; i++)
            {
                yield return StartCoroutine(WaitUntilMachineAvailable());
                FirePhase3Laser(phase3Formation[i], false);
                if (i < phase3Formation.Count - 1 && phase3LaserOrderInterval > 0f)
                    yield return StartCoroutine(WaitPhase3ActiveSeconds(phase3LaserOrderInterval));
            }
            // 最終セット後もフィニッシュ前に同じ1秒の溜めを置く。
            if (phase3LaserRoundInterval > 0f)
                yield return StartCoroutine(WaitPhase3ActiveSeconds(phase3LaserRoundInterval));
        }

        // フィニッシュ：複製体が生成順に1発ずつ撃ち、全員で1周とする。
        // phase3FinishShotsPerEcho 回だけ周回する（各複製体の合計発射数も同じ）。
        for (int shot = 0; shot < phase3FinishShotsPerEcho; shot++)
        {
            for (int i = 0; i < phase3Formation.Count; i++)
            {
                yield return StartCoroutine(WaitUntilMachineAvailable());
                FirePhase3Laser(phase3Formation[i], true);
                bool hasNext = i < phase3Formation.Count - 1 ||
                               shot < phase3FinishShotsPerEcho - 1;
                if (hasNext && phase3FinishShotInterval > 0f)
                    yield return StartCoroutine(WaitPhase3ActiveSeconds(phase3FinishShotInterval));
            }
        }

        // フィニッシュの最後の発射直後に混乱した場合も、解除を待ってから終了する。
        yield return StartCoroutine(WaitUntilMachineAvailable());

        // 本体はすぐP3通常行動へ。複製体だけ3秒停止し、隊列へ戻って再生を再開。
        phase3OrbitActive = false;
        phase3NeedsReformation = false;
        if (phase3ConfusionReacting) EndPhase3ConfusionReaction();
        if (echoReplay != null) echoReplay.EndSpecialAction();
        for (int i = 0; i < phase3Formation.Count; i++)
            if (phase3Formation[i] != null)
                phase3Formation[i].BeginPhase3Recovery(phase3EchoStunSeconds);
        phase3Formation.Clear();

        phase = Phase.P3;
        transition = 0;
        transitionStarted = false;
        Finish();
    }

    void OnDestroy()
    {
        if (phase3ConfusionReacting) EndPhase3ConfusionReaction();
        phase3OrbitActive = false;
        if (placedReplicator != null) Destroy(placedReplicator);
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
        Vector2 direction = (Vector2)TargetPosition - shotPos;
        CL_Arrow.transform.up = direction;
        CL_Arrow.transform.Rotate(0, 0, 90);
        rb.velocity = direction.normalized * ArrowSpeed;

        if (echoReplay != null) echoReplay.RecordSpawn(Arrow, CL_Arrow, ArrowDestroy, aimMode: 1, aimAngleOffset: 90f);
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
        mortar.ChangePos(TargetPosition + new Vector3(Random.Range(-MoterRandomRange, MoterRandomRange), Random.Range(-MoterRandomRange, MoterRandomRange), 0));
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
        Vector2 direction = (transform.position - TargetPosition).normalized;
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
        if (echoReplay != null) echoReplay.RecordSpawn(RockNeedle, CL_Needle, 5f);

        Destroy(CL_Needle, 5);
    }

    private IEnumerator MoveInArc(float jampTime, float jampHeigh, bool pauseForPhase3 = false)
    {


        enemyBase.animator.Play("移動", 0, 0);
        // 移動開始前のBodyTypeを保存しておく
        var originalBodyType = enemyBase.rb.bodyType;
        arcPreviousBodyType = originalBodyType;
        arcInProgress = true;
        // 移動中はKinematicにして、物理演算の影響をなくす
        enemyBase.rb.bodyType = RigidbodyType2D.Kinematic;

        Vector2 startPos = enemyBase.transform.position;
        Vector2 endPos = targetPos;

        GameObject marker = DengerAreaSpawn(DengerObjects[0], endPos, jampTime);
        if (echoReplay != null) echoReplay.RecordSpawn(DengerObjects[0], marker, jampTime + 0.2f);

        float elapsedTime = 0f;

        while (elapsedTime < jampTime)
        {
            // P3移行のジャンプ中も、複製機の混乱では時間と位置を停止させる。
            // 通常A3のジャンプには影響しない。
            if (pauseForPhase3 && Phase3MachineConfused)
            {
                yield return null;
                continue;
            }
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
        arcInProgress = false;

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
                RadiusFixSpeed * Time.deltaTime
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

    IEnumerator ShotArrow_A1(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        enemyBase.audioManager.PlaySE(audioClips[0]);

        Vector2 shotPos = transform.position;
        // 発射予約時ではなく、この1発が出る瞬間にプレイヤーを狙う。
        Vector2 direction = ((Vector2)TargetPosition - shotPos).normalized;

        GameObject arrow = Instantiate(
            Arrow,
            shotPos,
            Quaternion.identity
        );

        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();




        arrow.transform.up = direction;

        Vector2 initialVelocity = rb.velocity;
        rb.AddForce(direction * ArroeSpeed, ForceMode2D.Impulse);
        EnemyAttack enemyAttack = arrow.GetComponent<EnemyAttack>();
        if (enemyAttack != null) enemyAttack.enemyBase = enemyBase;

        // AddForce(Impulse)は直後のvelocityに反映されない場合があるので、力も記録。
        if (echoReplay != null)
            echoReplay.RecordSpawn(Arrow, arrow, ArrowDestroy,
                                   direction * ArroeSpeed, initialVelocity, aimMode: 1);
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
            if (echoReplay != null) echoReplay.RecordSpawn(Arrow, CL_Star, 10f);

            Destroy(CL_Star, 10);
        }
    }
}
