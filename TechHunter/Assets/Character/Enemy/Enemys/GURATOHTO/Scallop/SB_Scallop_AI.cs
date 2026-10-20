using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SB_Scallop_AI : EnemyMoveBase
{
    [SerializeField] GameObject Koaka;
    [SerializeField] GameObject RockNeedle;
    [SerializeField] GameObject DengerLenght;
    [SerializeField] GameObject AttackLenght;
    [SerializeField] GameObject AssultAttack;

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
    [SerializeField] GameObject LandingAttackPrefab;
    Vector2 lockedChargeDirection = Vector2.right;

    [Header("第二形態：HPと必殺技")]
    [SerializeField, Range(0.01f, 1f)] float secondPhaseHpRatio = 0.5f;
    [SerializeField, Min(1)] int attacksPerRoar = 6;
    [SerializeField, Min(0.1f)] float roarShellDuration = 1f;
    [SerializeField, Min(0.1f)] float roarAscentDuration = 0.28f;
    [SerializeField, Min(0.1f)] float roarDescentDuration = 0.3f;
    [SerializeField, Min(0f)] float roarJumpHeight = 3f;
    [SerializeField, Min(0f)] float roarRecoveryDuration = 0.5f;
    [SerializeField] GameObject roarAttackPrefab;
    [SerializeField] GameObject roarWarningPrefab;
    [SerializeField, Min(0.1f)] float roarWarningScale = 1.5f;
    [SerializeField] GameObject roarSludgeParticlePrefab;
    [SerializeField, Min(0.1f)] float roarParticleLifetime = 3f;
    [SerializeField, Min(1f)] float roarParticleEmissionMultiplier = 5f;

    [Header("第二形態：黒いヘドロ")]
    [SerializeField] GameObject sludgePrefab;
    [SerializeField, Min(1)] int roarSludgeCount = 30;
    [SerializeField, Min(1)] int normalLandingSludgeCount = 12;
    [SerializeField, Min(1)] int shortLandingSludgeCount = 18;
    [SerializeField, Min(0f)] float sludgeSpeed = 7f;
    [SerializeField, Min(0.1f)] float sludgeLifetime = 6f;
    [SerializeField, Range(0f, 360f)] float shortLandingSludgeArc = 115f;

    [Header("第二形態：低空飛びかかり")]
    [SerializeField, Min(0.1f)] float shortLeapTargetDistance = 1f;
    [SerializeField, Min(0.1f)] float shortLeapTriggerDistance = 3f;
    [SerializeField, Range(0f, 1f)] float shortLeapSelectionChance = 0.65f;
    [SerializeField, Min(0.05f)] float shortLeapWindup = 0.25f;
    [SerializeField, Min(0.1f)] float shortLeapDuration = 0.35f;
    [SerializeField, Min(0f)] float shortLeapHeight = 1.1f;
    [SerializeField, Min(0.1f)] float shortLeapRecovery = 0.4f;
    [SerializeField, Min(0.1f)] float landingTileTolerance = 0.75f;

    bool secondPhase;
    bool roarPending;
    int attacksSinceRoar;
    bool roarMissingLogged;
    bool sludgeMissingLogged;

    Vector2 shortLeapStart;
    Vector2 shortLeapDestination;
    Vector2 shortLeapFacing = Vector2.right;
    Vector2 roarOrigin;
    Vector2 lockedRoarDirection = Vector2.right;
    RigidbodyType2D savedSpecialBodyType;
    bool specialPhysicsChanged;
    GameObject activeRoarWarning;

    public override void EnemyConfusionPlay()
    {
        CancelSpecialAction();
        StandOk = false;
        A_Phase = 0;
        enemyBase.AttackCount = 0;
        AttackArea.SetActive(false);
    }
    public override void EnemyStanPlay()
    {
        CancelSpecialAction();
        StandOk = false;
        A_Phase = 0;
        enemyBase.AttackCount = 0;
        AttackArea.SetActive(false);
    }
    public override void EnemyMovePlay()
    {
        // 通常攻撃の途中で割り込まず、終了後の移動フェーズで第二形態へ。
        if (!secondPhase && enemyBase.HP <= enemyBase.MAXHP * secondPhaseHpRatio)
        {
            secondPhase = true;
            roarPending = true;
        }
        if (roarPending)
        {
            BeginRoar();
            return;
        }

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
            // 通常跳躍の代わりに、至近距離では約1mの低空飛びかかりを選ぶ。
            A_Phase = secondPhase && TargetDistance <= shortLeapTriggerDistance &&
                Random.value < shortLeapSelectionChance ? 2 : 1;
            WaitCunt = 0;
            enemyBase.moveType = EnemyBase.MoveType.Attack;
            RandomTime = Random.Range(2f, 5f);
        }

        RefreshTargetInfo();
        enemyBase.animator.Play("歩行");
        RefreshTargetInfo();
        float angle = Mathf.Atan2(TargetDirection.y, TargetDirection.x) * Mathf.Rad2Deg;
        float offset = 100f / (TargetDistance + 1f) * 3f; // 距離が遠いほど小さく

        if (trune)
        {
            angle += offset;
        }
        else
        {
            angle -= offset;
        }

        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

        LookAtCurrentTarget();

        enemyBase.rb.velocity = direction * enemyBase.SPEED;
    }
    public override void EnemyAttackPlay()
    {
        if (A_Phase == 0) A1();
        else if (A_Phase == 1) A2();
        else if (A_Phase == 2) ShortLeapAttack();
        else if (A_Phase == 3) RoarAttack();
    }
    void A2()
    {
        enemyBase.AttackCount += Time.deltaTime;
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            enemyBase.audioManager.PlaySE(enemyBase.AttackStayAudio);
            enemyBase.rb.velocity = Vector3.zero;
            enemyBase.mode = EnemyBase.ModeType.M2;
            enemyBase.animator.Play("攻撃待機");
            LookAtCurrentTarget();
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
            targetPos = TargetPosition;
            StartCoroutine(MoveInArc());
            LookAtCurrentTarget();
        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {
            if (StandOk)
            {
                enemyBase.mode = EnemyBase.ModeType.M5;
                enemyBase.AttackCount = 0;

                EnemyContoroller controller = enemyBase as EnemyContoroller;
                if (controller != null)
                    controller.SpawnAttack(LandingAttackPrefab != null ? LandingAttackPrefab : enemyBase.AttackArea,
                        enemyBase.transform.position, Quaternion.identity, 0.1f);
                enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
                StandOk = false;
                if (secondPhase)
                    ScatterSludge(normalLandingSludgeCount, enemyBase.transform.position,
                        Vector2.right, 360f);

                for (int j = 0; j < 12; j++)
                {
                    StartCoroutine(NeedleSpawnLing(0.05f * j, j * 0.5f, 0));
                }
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M5)
        {


           enemyBase.animator.Play("攻撃");
            enemyBase.mode = EnemyBase.ModeType.M6; enemyBase.AttackCount = 0;

        }
        if (enemyBase.mode == EnemyBase.ModeType.M6)
        {
            if (enemyBase.AttackCount > 1)
            {
                enemyBase.animator.Play("待機");
                LookAtCurrentTarget();
                GameObject CL_Denger = SpawnAttackWarning(EnemyContoroller.WarningRectangle, DengerLenght, transform.position, Quaternion.identity, 2f, null, 0f, false);
                lockedChargeDirection = TargetDirection.normalized;
                if (lockedChargeDirection == Vector2.zero) lockedChargeDirection = Vector2.right;
                CL_Denger.transform.up = lockedChargeDirection;
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M7;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M7) 
        {
            if (enemyBase.AttackCount > 1) 
            {
                enemyBase.animator.Play("歩行");

                EnemyContoroller controller = enemyBase as EnemyContoroller;
                if (controller != null)
                    controller.ConfigureAttackObject(AssultAttack, 100f);
                ConfigureSummonedAttack(AssultAttack);
        AssultAttack.SetActive(true);
                enemyBase.mode = EnemyBase.ModeType.M8; enemyBase.AttackCount = 0;
            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M8)
        {
            enemyBase.rb.velocity = lockedChargeDirection * enemyBase.SPEED * 6;
            if (enemyBase.AttackCount > 1)
            {
                AssultAttack.SetActive(false);
                enemyBase.rb.velocity = Vector2.zero;
                enemyBase.animator.Play("待機");
                enemyBase.mode = EnemyBase.ModeType.M1;
                enemyBase.AttackCount = 0;
                enemyBase.moveType = EnemyBase.MoveType.Move;
                enemyBase.PlayerDistance = 1000;
                RegisterCompletedAttack();
            }
        }
    }

    void A1()
    {
        enemyBase.AttackCount += Time.deltaTime;
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            enemyBase.audioManager.PlaySE(enemyBase.AttackStayAudio);
            enemyBase.rb.velocity = Vector3.zero;
            enemyBase.mode = EnemyBase.ModeType.M2;
            enemyBase.animator.SetInteger("Anim", 2);
            LookAtCurrentTarget();

            GameObject CL_Denger = SpawnAttackWarning(EnemyContoroller.WarningRectangle, Denger_Attackarea1, transform.position, Quaternion.identity, 1f, null, 0f, false);
            lockedChargeDirection = TargetDirection.normalized;
            if (lockedChargeDirection == Vector2.zero) lockedChargeDirection = Vector2.right;
            CL_Denger.transform.up = lockedChargeDirection;
            Destroy(CL_Denger, 1);
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {

            if (enemyBase.AttackCount > 0.4)
            {
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0;
                EnemyContoroller controller = enemyBase as EnemyContoroller;
                if (controller != null)
                    controller.ConfigureAttackObject(AttackArea, 100f);
                ConfigureSummonedAttack(AttackArea);
        AttackArea.SetActive(true);

            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            //LookAtCurrentTarget();
            AttackCount1 += Time.fixedDeltaTime;
            if (AttackCount1 > 0.05)
            {
                AttackCount1 = 0;
                SpawnRockNeedle(transform.position);
            }
            enemyBase.rb.velocity = lockedChargeDirection * enemyBase.SPEED * 9;
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


            enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
            enemyBase.animator.SetInteger("Anim", 3);
            enemyBase.mode = EnemyBase.ModeType.M6;

        }
        if (enemyBase.mode == EnemyBase.ModeType.M6)
        {

            if (enemyBase.AttackCount > 0.1)
            {
                RegisterCompletedAttack();
                int i = Random.Range(0, 2);
                if (i == 0)
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

                // 6回目が突進だった場合、追加の突進より咆哮を優先する。
                if (roarPending) enemyBase.moveType = EnemyBase.MoveType.Move;
            }
        }


    }
    void RegisterCompletedAttack()
    {
        if (!secondPhase) return;
        attacksSinceRoar++;
        if (attacksSinceRoar >= Mathf.Max(1, attacksPerRoar))
            roarPending = true;
    }

    void BeginRoar()
    {
        roarPending = false;
        attacksSinceRoar = 0;
        A_Phase = 3;
        enemyBase.mode = EnemyBase.ModeType.M1;
        enemyBase.AttackCount = 0f;
        enemyBase.moveType = EnemyBase.MoveType.Attack;
        enemyBase.rb.velocity = Vector2.zero;
    }

    void RoarAttack()
    {
        enemyBase.AttackCount += Time.fixedDeltaTime;

        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            enemyBase.rb.velocity = Vector2.zero;
            roarOrigin = enemyBase.transform.position;
            LookAtCurrentTarget();
            lockedRoarDirection = TargetDirection.normalized;
            if (lockedRoarDirection.sqrMagnitude < 0.0001f)
                lockedRoarDirection = Vector2.right;
            enemyBase.animator.Play("格納", 0, 0f);
            activeRoarWarning = SpawnAttackWarning(EnemyContoroller.WarningCircle,
                roarWarningPrefab != null ? roarWarningPrefab : Denger_AttackArea,
                roarOrigin, Quaternion.identity, roarShellDuration, null, 0f, false);
            if (activeRoarWarning != null)
            {
                activeRoarWarning.transform.up = lockedRoarDirection;
                activeRoarWarning.transform.localScale *= roarWarningScale;
            }
            enemyBase.AttackCount = 0f;
            enemyBase.mode = EnemyBase.ModeType.M2;
        }
        else if (enemyBase.mode == EnemyBase.ModeType.M2)
        {
            if (enemyBase.AttackCount >= roarShellDuration)
            {
                BeginSpecialLeap();
                enemyBase.animator.Play("攻撃", 0, 0f);
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0f;
            }
        }
        else if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            float t = Mathf.Clamp01(enemyBase.AttackCount / Mathf.Max(0.1f, roarAscentDuration));
            enemyBase.rb.MovePosition(roarOrigin + Vector2.up * (Mathf.Sin(t * Mathf.PI * 0.5f) * roarJumpHeight));
            if (t >= 1f)
            {
                FireRoarAndSludge();
                enemyBase.mode = EnemyBase.ModeType.M4;
                enemyBase.AttackCount = 0f;
            }
        }
        else if (enemyBase.mode == EnemyBase.ModeType.M4)
        {
            float t = Mathf.Clamp01(enemyBase.AttackCount / Mathf.Max(0.1f, roarDescentDuration));
            enemyBase.rb.MovePosition(roarOrigin + Vector2.up * ((1f - t) * roarJumpHeight));
            if (t >= 1f)
            {
                enemyBase.rb.MovePosition(roarOrigin);
                RestoreSpecialPhysics();
                enemyBase.animator.Play("待機", 0, 0f);
                enemyBase.mode = EnemyBase.ModeType.M5;
                enemyBase.AttackCount = 0f;
            }
        }
        else if (enemyBase.mode == EnemyBase.ModeType.M5 &&
                 enemyBase.AttackCount >= roarRecoveryDuration)
        {
            CompleteAttack();
        }
    }

    void FireRoarAndSludge()
    {
        // 攻撃警告を出した時点の方向で固定し、発動直前の再追尾をしない。
        Vector2 facing = lockedRoarDirection;

        if (roarAttackPrefab != null)
        {
            GameObject hitbox = Instantiate(roarAttackPrefab, roarOrigin, Quaternion.identity);
            ConfigureSummonedAttack(hitbox);
            hitbox.transform.up = facing;
            // ギンリンの咆哮攻撃と同じく、ごく短い時間だけ判定を出す。
            Destroy(hitbox, 0.1f);
        }
        else if (!roarMissingLogged)
        {
            Debug.LogWarning("SB_Scallop: Roar Attack Prefab が未設定です。", this);
            roarMissingLogged = true;
        }

        if (roarSludgeParticlePrefab != null)
        {
            GameObject particles = Instantiate(roarSludgeParticlePrefab, enemyBase.transform.position,
                Quaternion.identity);
            // 元Prefabは変更せず、咆哮時のインスタンスだけ大量放出させる。
            foreach (ParticleSystem particle in particles.GetComponentsInChildren<ParticleSystem>(true))
            {
                ParticleSystem.EmissionModule emission = particle.emission;
                emission.rateOverTimeMultiplier *= roarParticleEmissionMultiplier;
                particle.Play(true);
            }
            Destroy(particles, roarParticleLifetime);
        }

        ScatterSludge(roarSludgeCount, roarOrigin, facing, 360f);
        enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
    }

    void ShortLeapAttack()
    {
        enemyBase.AttackCount += Time.fixedDeltaTime;
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {
            LookAtCurrentTarget();
            shortLeapStart = enemyBase.transform.position;
            shortLeapFacing = TargetDirection.normalized;
            if (shortLeapFacing.sqrMagnitude < 0.0001f)
                shortLeapFacing = Vector2.right;
            shortLeapDestination = FindShortLeapDestination(shortLeapStart, shortLeapFacing);

            enemyBase.rb.velocity = Vector2.zero;
            enemyBase.animator.Play("攻撃待機", 0, 0f);
            // 有効な着地タイルがない場合でも、その場への着地を予告する。
            SpawnAttackWarning(EnemyContoroller.WarningCircle, Denger_AttackArea,
                shortLeapDestination, Quaternion.identity,
                shortLeapWindup + shortLeapDuration, null, 0f, false);
            enemyBase.AttackCount = 0f;
            enemyBase.mode = EnemyBase.ModeType.M2;
        }
        else if (enemyBase.mode == EnemyBase.ModeType.M2)
        {
            if (enemyBase.AttackCount >= shortLeapWindup)
            {
                BeginSpecialLeap();
                enemyBase.animator.Play("攻撃", 0, 0f);
                enemyBase.mode = EnemyBase.ModeType.M3;
                enemyBase.AttackCount = 0f;
            }
        }
        else if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            float t = Mathf.Clamp01(enemyBase.AttackCount / Mathf.Max(0.1f, shortLeapDuration));
            Vector2 pos = Vector2.Lerp(shortLeapStart, shortLeapDestination, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * shortLeapHeight;
            enemyBase.rb.MovePosition(pos);
            if (t >= 1f)
            {
                enemyBase.rb.MovePosition(shortLeapDestination);
                RestoreSpecialPhysics();
                // 着地攻撃は既存のLandingAttackPrefabと同じメソッドを使用。
                EnemyContoroller controller = enemyBase as EnemyContoroller;
                if (controller != null)
                    controller.SpawnAttack(LandingAttackPrefab != null ? LandingAttackPrefab : enemyBase.AttackArea,
                        shortLeapDestination, Quaternion.identity, 0.1f);
                enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
                ScatterSludge(shortLandingSludgeCount, shortLeapDestination,
                    shortLeapFacing, shortLandingSludgeArc);
                enemyBase.mode = EnemyBase.ModeType.M4;
                enemyBase.AttackCount = 0f;
            }
        }
        else if (enemyBase.mode == EnemyBase.ModeType.M4 &&
                 enemyBase.AttackCount >= shortLeapRecovery)
        {
            RegisterCompletedAttack();
            CompleteAttack();
        }
    }

    // 月の巡礼者と同じStandFloorのHasTile/GetCellCenterWorldを使う。
    // ただし遠くのタイルへのフォールバックはせず、約1m前方に候補がなければ現在位置。
    Vector2 FindShortLeapDestination(Vector2 origin, Vector2 toward)
    {
        if (toward.sqrMagnitude < 0.0001f) return origin;
        if (ALL_SystemManager.Instance == null ||
            ALL_SystemManager.Instance.enemySpawnBASE == null ||
            ALL_SystemManager.Instance.enemySpawnBASE.Seve_TileMap_C == null)
            return origin;

        Tilemap floor = ALL_SystemManager.Instance.enemySpawnBASE.Seve_TileMap_C.StandFloor;
        if (floor == null) return origin;

        Vector2 direction = toward.normalized;
        Vector2 wanted = origin + direction * shortLeapTargetDistance;
        Vector3Int center = floor.WorldToCell(wanted);
        Vector2 best = origin;
        float bestScore = float.PositiveInfinity;
        // 隣接セルのみ確認する。範囲外のタイルは絶対に選ばない。
        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                Vector3Int cell = new Vector3Int(center.x + x, center.y + y, center.z);
                if (!floor.HasTile(cell)) continue;

                Vector2 candidate = floor.GetCellCenterWorld(cell);
                Vector2 displacement = candidate - origin;
                float length = displacement.magnitude;
                if (length < 0.2f ||
                    Mathf.Abs(length - shortLeapTargetDistance) > landingTileTolerance)
                    continue;
                // 横や後方への飛びかかりは禁止。標的方向に概ね向いたセルだけ。
                if (Vector2.Dot(displacement.normalized, direction) < 0.55f) continue;

                float score = Vector2.Distance(candidate, wanted) +
                    Mathf.Abs(length - shortLeapTargetDistance) * 0.15f;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }
        }
        return best;
    }

    void ScatterSludge(int count, Vector2 origin, Vector2 direction, float arcDegrees)
    {
        if (sludgePrefab == null)
        {
            if (!sludgeMissingLogged)
            {
                Debug.LogWarning("SB_Scallop: Sludge Prefab を設定するとヘドロ攻撃を使用できます。", this);
                sludgeMissingLogged = true;
            }
            return;
        }
        if (direction.sqrMagnitude < 0.0001f) direction = Vector2.right;
        direction.Normalize();

        for (int i = 0; i < count; i++)
        {
            float angle = arcDegrees >= 359.9f
                ? Random.Range(0f, 360f)
                : Random.Range(-arcDegrees * 0.5f, arcDegrees * 0.5f);
            Vector2 velocityDirection = (Quaternion.Euler(0f, 0f, angle) * direction).normalized;
            float distance = Random.Range(1.5f, 4f);
            GameObject sludge = Instantiate(sludgePrefab, origin, Quaternion.identity);
            ConfigureSummonedAttack(sludge);

            Rigidbody2D sludgeRb = sludge.GetComponent<Rigidbody2D>();
            if (sludgeRb != null)
                sludgeRb.velocity = velocityDirection * sludgeSpeed * Random.Range(0.65f, 1.35f);
            else
                sludge.transform.position = origin + velocityDirection * distance;

            Destroy(sludge, sludgeLifetime);
        }
    }

    void BeginSpecialLeap()
    {
        if (specialPhysicsChanged) return;
        savedSpecialBodyType = enemyBase.rb.bodyType;
        enemyBase.rb.bodyType = RigidbodyType2D.Kinematic;
        specialPhysicsChanged = true;
    }

    void RestoreSpecialPhysics()
    {
        if (!specialPhysicsChanged) return;
        enemyBase.rb.bodyType = savedSpecialBodyType;
        enemyBase.rb.velocity = Vector2.zero;
        specialPhysicsChanged = false;
    }

    void CompleteAttack()
    {
        enemyBase.mode = EnemyBase.ModeType.M1;
        enemyBase.AttackCount = 0f;
        enemyBase.moveType = EnemyBase.MoveType.Move;
        enemyBase.PlayerDistance = 1000f;
        enemyBase.animator.Play("待機", 0, 0f);
        A_Phase = 0;
    }

    void CancelSpecialAction()
    {
        // 既存のA1/A2を中断したときは、従来のフェーズ処理を変えない。
        if (A_Phase != 2 && A_Phase != 3) return;
        if (A_Phase == 3) roarPending = true; // 中断された咆哮は回復後に再実行。
        RestoreSpecialPhysics();
        if (activeRoarWarning != null) Destroy(activeRoarWarning);
        activeRoarWarning = null;
        enemyBase.rb.velocity = Vector2.zero;
        enemyBase.mode = EnemyBase.ModeType.M1;
        enemyBase.AttackCount = 0f;
        enemyBase.moveType = EnemyBase.MoveType.Move;
    }

    public void SpawnRockNeedle(Vector2 pos)
    {
        EnemyContoroller controller = enemyBase as EnemyContoroller;
        if (controller != null)
            controller.SpawnAttack(RockNeedle, pos, Quaternion.identity, 5f, RockNeedle.GetComponentInChildren<EnemyAttack>(true) != null ? RockNeedle.GetComponentInChildren<EnemyAttack>(true).AttackPowerPercent : 100f);
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

        GameObject CL_AttackPPP = SpawnAttackWarning(EnemyContoroller.WarningRectangle, Denger_AttackArea, endPos, Quaternion.identity, 1.5f, null, 0f, false);

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
