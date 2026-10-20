using System.Collections.Generic;
using UnityEngine;

// ワトウナイ：本体は移動せず、硬質な頭部だけを画面端まで射出する。
// 攻撃中は首を弱点として露出させ、最後の収納中は一方的な反撃機会になる。
// 既存のcharaDamage.DamageType=1によるダメージ転送を使う。
[DefaultExecutionOrder(200)]
public class KINGYO_Watounai_AI : EnemyMoveBase
{
    [Header("首と頭部（本体の子オブジェクト）")]
    [SerializeField] Transform headTransform;
    [SerializeField] Animator headAnimator;
    [SerializeField] SpriteRenderer headSprite;
    [SerializeField] SpriteRenderer neckSprite;
    [SerializeField] BoxCollider2D neckDamageCollider;
    [SerializeField] CircleCollider2D headDamageCollider;
    [SerializeField] Collider2D headAttackCollider;
    [SerializeField] EnemyAttack headAttack;
    [SerializeField] charaDamage neckDamage;
    [SerializeField] charaDamage headDamage;
    [SerializeField] charaDamage weakDamageReceiver;
    [SerializeField] charaDamage bodyDamage;
    [SerializeField, Min(0f)] float weakNeckDefense = 0f;
    [SerializeField, Min(0.05f)] float neckColliderThickness = 0.45f;
    [SerializeField, Min(0.05f)] float neckVisualThicknessScale = 1f;
    [SerializeField] Vector2 neckBaseOffset = new Vector2(0f, 0.15f);
    [SerializeField] bool neckSpriteAlongX = true;
    [SerializeField] float headAngleOffset = 0f;

    [Header("攻撃距離・移動")]
    [SerializeField, Min(1f)] float minimumHeadReach = 20f;
    [SerializeField, Min(0f)] float attackRangeMargin = 0.5f;
    [SerializeField, Min(0f)] float shootingRestTime = 0.45f;
    [SerializeField] GameObject straightWarningPrefab;
    [SerializeField, Min(0.05f)] float windupTime = 0.6f;

    [Header("砲弾頭突き・加速噛みつき")]
    [SerializeField, Min(0.05f)] float launchDuration = 0.45f;
    [SerializeField, Min(0f)] float launchInitialSpeed = 30f;
    [SerializeField, Min(0f)] float launchAcceleration = 100f;
    [SerializeField, Min(0)] int bitesBeforeSweep = 2;
    [SerializeField, Min(0)] int bitesAfterSweep = 1;
    [SerializeField, Min(0.05f)] float biteDuration = 0.46f;
    [SerializeField, Min(0f)] float biteInitialSpeed = 15f;
    [SerializeField, Min(0f)] float biteAcceleration = 85f;
    [SerializeField, Min(0.05f)] float bitePause = 0.12f;
    [SerializeField, Min(0.1f)] float biteStopDistance = 0.65f;

    [Header("首を引いた後の薙ぎ払い")]
    [SerializeField, Min(0.05f)] float pullbackDuration = 0.25f;
    [SerializeField, Range(0.05f, 0.95f)] float pullbackFraction = 0.35f;
    [SerializeField, Min(0.05f)] float sweepDuration = 0.6f;
    [SerializeField, Range(30f, 270f)] float sweepAngle = 155f;
    [SerializeField, Min(0.5f)] float sweepMinimumRadius = 3.5f;

    [Header("収納：頭は攻撃せず、首は被弾可能")]
    [SerializeField, Min(0.1f)] float neckReturnSpeed = 10f;
    [SerializeField, Min(0.05f)] float afterReturnRecovery = 0.55f;
    [SerializeField, Min(0.1f)] float maxSequenceDuration = 8f;

    const int Windup = 0;
    const int Launch = 1;
    const int Bite = 2;
    const int BitePause = 3;
    const int Pullback = 4;
    const int Sweep = 5;
    const int Retract = 6;
    const int Recovery = 7;

    bool attacking;
    int state;
    int bitesDone;
    int totalBites;
    float stateTime;
    float sequenceTime;
    float headSpeed;
    float delayUntilNextAttack;
    float pullbackInitialDistance;
    float sweepRadius;
    float sweepCenterAngle;
    bool sweepDone;
    Vector2 currentHeadPosition;
    Vector2 launchDirection = Vector2.right;
    Vector2 lastHeadDirection = Vector2.right;
    bool warningMissingLogged;

    Transform Target => CurrentTarget;
    Vector2 NeckBase => (Vector2)enemyBase.transform.position + neckBaseOffset;
    float HeadReach
    {
        get
        {
            // 固定画面の場合もカメラの表示幅以上の射程を確保する。
            Camera camera = Camera.main;
            if (camera != null && camera.orthographic)
                return Mathf.Max(minimumHeadReach,
                    2f * camera.orthographicSize * camera.aspect);
            return minimumHeadReach;
        }
    }

    void Awake()
    {
        if (enemyBase == null)
            enemyBase = GetComponentInParent<EnemyBase>();
    }

    void Start()
    {
        if (enemyBase == null) enemyBase = GetComponentInParent<EnemyBase>();
        if (enemyBase == null) { enabled = false; return; }

        if (bodyDamage == null)
            bodyDamage = enemyBase.transform.Find("DamageArea") != null
                ? enemyBase.transform.Find("DamageArea").GetComponent<charaDamage>()
                : enemyBase.CharaDamage;
        if (bodyDamage == null) bodyDamage = enemyBase.charaDamage;

        // 首の被弾 -> 低防御の受け口 -> 本体HP。
        // 頭部の被弾 -> 本体の通常判定（硬い頭部の防御力）。
        if (weakDamageReceiver != null)
        {
            weakDamageReceiver.DamageType = 0;
            weakDamageReceiver.DF = weakNeckDefense;
        }
        if (neckDamage != null && weakDamageReceiver != null)
        {
            neckDamage.DamageType = 1;
            neckDamage.passDamageAreas.Clear();
            neckDamage.passDamageAreas.Add(weakDamageReceiver);
        }
        if (headDamage != null && bodyDamage != null)
        {
            headDamage.DamageType = 1;
            headDamage.passDamageAreas.Clear();
            headDamage.passDamageAreas.Add(bodyDamage);
        }
        if (headAttack != null)
        {
            EnemyContoroller controller = enemyBase as EnemyContoroller;
            if (controller != null) controller.ConfigureAttackTarget(headAttack);
            else headAttack.ConfigureAttack(enemyBase, headAttack.AttackPowerPercent);
        }

        enemyBase.LookPlayerDistance = Mathf.Max(1f, HeadReach - attackRangeMargin);
        HideExtendedParts();
    }

    public override void EnemyMovePlay()
    {
        if (enemyBase == null) return;
        if (attacking) CancelAttack();

        Transform target = Target;
        if (target == null)
        {
            enemyBase.rb.velocity = Vector2.zero;
            enemyBase.animator.SetInteger("Anim", 0);
            return;
        }

        LookAtCurrentTarget();
        Vector2 direction = (Vector2)target.position - (Vector2)enemyBase.transform.position;
        float distance = direction.magnitude;

        // 画面端相当の射程より外の敵は、従来の移動速度で接近する。
        if (distance >= HeadReach - attackRangeMargin)
        {
            enemyBase.rb.velocity = direction.normalized * enemyBase.SPEED;
            enemyBase.animator.SetInteger("Anim", 1);
        }
        else
        {
            enemyBase.rb.velocity = Vector2.zero;
            enemyBase.animator.SetInteger("Anim", 0);
        }

        // Controllerが距離によってAttackへ移るため、独立した待機時間は短縮しない。
        if (delayUntilNextAttack > 0f)
            delayUntilNextAttack -= Time.fixedDeltaTime;
    }

    public override void EnemyAttackPlay()
    {
        if (enemyBase == null) return;
        enemyBase.rb.velocity = Vector2.zero;

        if (!attacking)
        {
            if (delayUntilNextAttack > 0f)
            {
                delayUntilNextAttack -= Time.fixedDeltaTime;
                return;
            }
            if (Target == null)
            {
                FinishAttack();
                return;
            }
            BeginAttack();
        }

        float dt = Time.fixedDeltaTime;
        stateTime += dt;
        sequenceTime += dt;
        if (sequenceTime >= maxSequenceDuration && state != Retract && state != Recovery)
            SwitchState(Retract);

        switch (state)
        {
            case Windup: StepWindup(); break;
            case Launch: StepLaunch(dt); break;
            case Bite: StepBite(dt); break;
            case BitePause: StepBitePause(); break;
            case Pullback: StepPullback(); break;
            case Sweep: StepSweep(); break;
            case Retract: StepRetract(dt); break;
            case Recovery:
                if (stateTime >= afterReturnRecovery) FinishAttack();
                break;
        }

        if (attacking)
            UpdateExtendedParts();
    }

    void BeginAttack()
    {
        attacking = true;
        state = Windup;
        stateTime = 0f;
        sequenceTime = 0f;
        bitesDone = 0;
        totalBites = Mathf.Max(0, bitesBeforeSweep) + Mathf.Max(0, bitesAfterSweep);
        sweepDone = false;
        currentHeadPosition = NeckBase;

        Vector2 direction = Target != null
            ? (Vector2)Target.position - NeckBase : lastHeadDirection;
        launchDirection = direction.sqrMagnitude > 0.001f
            ? direction.normalized : lastHeadDirection;

        LookAtCurrentTarget();
        enemyBase.animator.SetInteger("Anim", 2);
        enemyBase.mode = EnemyBase.ModeType.M1;
        enemyBase.audioManager.PlaySE(enemyBase.AttackStayAudio);

        if (straightWarningPrefab != null)
        {
            GameObject warning = SpawnAttackWarning(EnemyContoroller.WarningRectangle,
                straightWarningPrefab, NeckBase, Quaternion.identity,
                windupTime, null, 0f, false);
            if (warning != null) warning.transform.up = launchDirection;
        }
        else if (!warningMissingLogged)
        {
            Debug.LogWarning("KINGYO_Watounai: 攻撃警告Prefabは未設定です。", this);
            warningMissingLogged = true;
        }
    }

    void StepWindup()
    {
        if (stateTime < windupTime) return;

        enemyBase.animator.Play("首無し", 0, 0f);
        if (headAnimator != null) headAnimator.Play("頭部", 0, 0f);
        if (headSprite != null) headSprite.enabled = true;
        if (neckSprite != null) neckSprite.enabled = true;
        headSpeed = launchInitialSpeed;
        enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
        SwitchState(Launch);
    }

    void StepLaunch(float dt)
    {
        SetHeadAttackActive(true);
        headSpeed += launchAcceleration * dt;
        MoveHead(currentHeadPosition + launchDirection * headSpeed * dt);

        if (stateTime >= launchDuration ||
            Vector2.Distance(NeckBase, currentHeadPosition) >= HeadReach - 0.05f)
        {
            if (totalBites > 0) BeginBite();
            else SwitchState(Pullback);
        }
    }

    void BeginBite()
    {
        headSpeed = biteInitialSpeed;
        SwitchState(Bite);
        SetHeadAttackActive(true);
    }

    void StepBite(float dt)
    {
        // 頭部射出後も現在の標的の位置を追尾してさらに加速する。
        Vector2 targetPosition = Target != null
            ? (Vector2)Target.position : currentHeadPosition + lastHeadDirection;
        Vector2 toTarget = targetPosition - currentHeadPosition;
        Vector2 direction = toTarget.sqrMagnitude > 0.001f
            ? toTarget.normalized : lastHeadDirection;

        headSpeed += biteAcceleration * dt;
        MoveHead(currentHeadPosition + direction * headSpeed * dt);
        SetHeadAttackActive(true);

        if (toTarget.magnitude <= biteStopDistance || stateTime >= biteDuration)
        {
            bitesDone++;
            SetHeadAttackActive(false);
            SwitchState(BitePause);
        }
    }

    void StepBitePause()
    {
        if (stateTime < bitePause) return;

        if (!sweepDone && bitesDone >= bitesBeforeSweep)
        {
            pullbackInitialDistance = Vector2.Distance(NeckBase, currentHeadPosition);
            SwitchState(Pullback);
        }
        else if (bitesDone < totalBites)
            BeginBite();
        else
            SwitchState(Retract);
    }

    void StepPullback()
    {
        // 完全収納ではない。首をいったん引き下げて薙ぎ払いへつなぐ。
        SetHeadAttackActive(false);
        Vector2 dir = currentHeadPosition - NeckBase;
        if (dir.sqrMagnitude < 0.001f) dir = lastHeadDirection;
        float ratio = Mathf.Clamp01(stateTime / pullbackDuration);
        float from = Mathf.Max(0.2f, pullbackInitialDistance);
        float to = Mathf.Max(0.5f, from * pullbackFraction);
        MoveHead(NeckBase + dir.normalized * Mathf.Lerp(from, to, ratio));

        if (ratio >= 1f)
        {
            Vector2 facing = Target != null
                ? (Vector2)Target.position - NeckBase : lastHeadDirection;
            if (facing.sqrMagnitude < 0.001f) facing = lastHeadDirection;
            sweepCenterAngle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
            sweepRadius = Mathf.Clamp(
                Target != null ? Vector2.Distance(Target.position, NeckBase) : from,
                sweepMinimumRadius, HeadReach);
            SwitchState(Sweep);
        }
    }

    void StepSweep()
    {
        // 頭を大きく振り回す間、伸びた首全体が低防御の攻撃チャンス。
        float progress = Mathf.Clamp01(stateTime / sweepDuration);
        float angle = sweepCenterAngle + Mathf.Lerp(-sweepAngle * 0.5f,
            sweepAngle * 0.5f, progress);
        Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad));
        MoveHead(NeckBase + dir * sweepRadius);
        SetHeadAttackActive(true);

        if (progress >= 1f)
        {
            sweepDone = true;
            SetHeadAttackActive(false);
            if (bitesDone < totalBites) BeginBite();
            else SwitchState(Retract);
        }
    }

    void StepRetract(float dt)
    {
        // 収縮中は頭部攻撃を無効化。首の被弾判定は最後まで残る。
        SetHeadAttackActive(false);
        MoveHead(Vector2.MoveTowards(currentHeadPosition, NeckBase,
            neckReturnSpeed * dt));

        if (Vector2.Distance(currentHeadPosition, NeckBase) <= 0.06f)
        {
            HideExtendedParts();
            enemyBase.animator.Play("待機", 0, 0f);
            SwitchState(Recovery);
        }
    }

    void SwitchState(int newState)
    {
        state = newState;
        stateTime = 0f;
        enemyBase.mode = (EnemyBase.ModeType)Mathf.Clamp(newState, 0, 7);
    }

    void MoveHead(Vector2 wantedPosition)
    {
        Vector2 fromBase = wantedPosition - NeckBase;
        currentHeadPosition = fromBase.sqrMagnitude > HeadReach * HeadReach
            ? NeckBase + fromBase.normalized * HeadReach : wantedPosition;
        if (fromBase.sqrMagnitude > 0.001f)
            lastHeadDirection = fromBase.normalized;
    }

    void UpdateExtendedParts()
    {
        if (headTransform == null || neckSprite == null) return;
        Vector2 start = NeckBase;
        Vector2 delta = currentHeadPosition - start;
        float length = delta.magnitude;
        Vector2 direction = length > 0.02f ? delta / length : lastHeadDirection;

        headTransform.position = currentHeadPosition;
        headTransform.rotation = Quaternion.Euler(0f, 0f,
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + headAngleOffset);

        if (headSprite != null) headSprite.enabled = state != Windup && state != Recovery;
        if (neckSprite != null)
        {
            neckSprite.enabled = state != Windup && state != Recovery && length > 0.07f;
            neckSprite.transform.position = start + delta * 0.5f;
            neckSprite.transform.rotation = Quaternion.Euler(0f, 0f,
                Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg
                + (neckSpriteAlongX ? 0f : -90f));

            // Spriteの寸法を使い、根元から頭まで首を伸縮させる。
            float naturalLength = neckSprite.sprite != null
                ? (neckSpriteAlongX ? neckSprite.sprite.bounds.size.x
                    : neckSprite.sprite.bounds.size.y) : 1f;
            float stretch = length / Mathf.Max(0.01f, naturalLength);
            neckSprite.transform.localScale = neckSpriteAlongX
                ? new Vector3(stretch, neckVisualThicknessScale, 1f)
                : new Vector3(neckVisualThicknessScale, stretch, 1f);
        }

        if (neckDamageCollider != null)
        {
            neckDamageCollider.enabled = state != Windup && state != Recovery && length > 0.2f;
            float naturalLength = neckSprite.sprite != null
                ? (neckSpriteAlongX ? neckSprite.sprite.bounds.size.x
                    : neckSprite.sprite.bounds.size.y) : 1f;
            neckDamageCollider.size = neckSpriteAlongX
                ? new Vector2(naturalLength, neckColliderThickness /
                    Mathf.Max(0.05f, neckVisualThicknessScale))
                : new Vector2(neckColliderThickness /
                    Mathf.Max(0.05f, neckVisualThicknessScale), naturalLength);
        }

        if (headDamageCollider != null)
            headDamageCollider.enabled = state != Windup && state != Recovery;
    }

    void SetHeadAttackActive(bool active)
    {
        if (headAttackCollider != null)
            headAttackCollider.enabled = active;
    }

    void HideExtendedParts()
    {
        SetHeadAttackActive(false);
        if (neckDamageCollider != null) neckDamageCollider.enabled = false;
        if (headDamageCollider != null) headDamageCollider.enabled = false;
        if (neckSprite != null) neckSprite.enabled = false;
        if (headSprite != null) headSprite.enabled = false;
        if (enemyBase != null && headTransform != null)
            headTransform.position = NeckBase;
    }

    void FinishAttack()
    {
        attacking = false;
        HideExtendedParts();
        if (enemyBase == null) return;
        enemyBase.rb.velocity = Vector2.zero;
        enemyBase.animator.SetInteger("Anim", 0);
        enemyBase.animator.Play("待機", 0, 0f);
        enemyBase.mode = EnemyBase.ModeType.M1;
        enemyBase.AttackCount = 0f;
        delayUntilNextAttack = shootingRestTime;
        enemyBase.moveType = EnemyBase.MoveType.Move;
        enemyBase.PlayerDistance = 1000f;
    }

    void CancelAttack()
    {
        if (!attacking) return;
        attacking = false;
        HideExtendedParts();
        if (enemyBase != null)
        {
            enemyBase.rb.velocity = Vector2.zero;
            enemyBase.mode = EnemyBase.ModeType.M1;
            enemyBase.AttackCount = 0f;
            enemyBase.moveType = EnemyBase.MoveType.Move;
        }
    }

    public override void EnemyStanPlay() { CancelAttack(); }
    public override void EnemyConfusionPlay() { CancelAttack(); }

    void LateUpdate()
    {
        // 死亡処理ではEnemyStanPlayが呼ばれないので、伸びた頭部をここで消す。
        if (enemyBase != null && enemyBase.status == EnemyBase.Status.Die && attacking)
            CancelAttack();
    }

    void OnDisable()
    {
        if (attacking) CancelAttack();
    }
}
