using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyContoroller : EnemyBase
{
    public EnemyMoveBase moveBase;
    [SerializeField] GameObject EnemyMoveBaseObject;
    public enum MoveToBase
    {
        Out,
        Special
    }
    public MoveToBase Choice_MoveToBase = MoveToBase.Out;

    // 旧Prefabの互換性のため、初期値はPlayerOnly。
    public enum TargetType { PlayerOnly, EnemyOnly, PlayerAndEnemy }
    public enum TargetPriority { Nearest, PlayerFirst, EnemyFirst }

    [Header("共通ターゲット管理")]
    [SerializeField] TargetType targetType = TargetType.PlayerOnly;
    [SerializeField] TargetPriority targetPriority = TargetPriority.Nearest;
    [SerializeField] bool targetAllFactions = true;
    [SerializeField] Faction targetFactions = Faction.None;
    [SerializeField] Faction excludedTargetFactions = Faction.None;
    [SerializeField, Min(0f)] float enemyFirstPlayerDistance = 0f;
    [SerializeField, Min(0.05f)] float targetSearchInterval = 0.25f;

    public TargetType TargetMode => targetType;
    public Transform CurrentTarget { get; private set; }
    public Vector2 TargetDirection { get; private set; }
    public float TargetDistance { get; private set; } = float.PositiveInfinity;

    // EnemyOnly専用。攻撃可能な敵がいない間だけtrue。
    public bool IsHostilityReleased { get; private set; }

    float targetSearchTimer;
    Transform forcedTarget;

    // PlayerOnlyでは従来の PlayerDistance / PlayerDirection / PlayerLook を一切変更しない。
    void RefreshTarget()
    {
        if (targetType == TargetType.PlayerOnly)
        {
            CurrentTarget = Player != null ? Player.transform : null;
            return;
        }

        if (moveType != MoveType.Attack)
        {
            targetSearchTimer += Time.fixedDeltaTime;
            // ターゲット不在時は指定間隔で探索し、毎FixedUpdateの全敵走査を防ぐ。
            if (targetSearchTimer >= Mathf.Max(0.05f, targetSearchInterval) ||
                (CurrentTarget != null && !IsValidTarget(CurrentTarget)))
            {
                ChooseTargetNow();
                targetSearchTimer = 0f;
            }
        }

        UpdateTargetDistance();
    }

    void UpdateTargetDistance()
    {
        if (CurrentTarget == null)
        {
            TargetDirection = Vector2.zero;
            TargetDistance = float.PositiveInfinity;
            return;
        }

        TargetDirection = (Vector2)CurrentTarget.position - (Vector2)transform.position;
        TargetDistance = TargetDirection.magnitude;
    }

    public void ForceTarget(Transform target)
    {
        forcedTarget = target;
        CurrentTarget = target;
        UpdateTargetDistance();
    }

    public void ClearForcedTarget()
    {
        forcedTarget = null;
        targetSearchTimer = targetSearchInterval;
    }

    // 強制攻撃の際には「プレイヤーより近い敵がいれば優先」を選択可能。
    public void ChooseTargetNow(bool closestEnemyIfCloserThanPlayer = false)
    {
        Transform player = Player != null ? Player.transform : null;
        if (targetType == TargetType.PlayerOnly)
        {
            CurrentTarget = player;
            UpdateTargetDistance();
            return;
        }

        if (forcedTarget != null && IsValidTarget(forcedTarget))
        {
            CurrentTarget = forcedTarget;
            UpdateTargetDistance();
            return;
        }
        forcedTarget = null;

        Transform enemy = FindNearestValidEnemyTarget();
        if (targetType == TargetType.EnemyOnly)
        {
            CurrentTarget = enemy; // 敵がいなければ攻撃せず待機
        }
        else if (player == null)
        {
            CurrentTarget = enemy;
        }
        else if (enemy == null)
        {
            CurrentTarget = player;
        }
        else
        {
            float playerSqr = ((Vector2)(player.position - transform.position)).sqrMagnitude;
            float enemySqr = ((Vector2)(enemy.position - transform.position)).sqrMagnitude;

            if (closestEnemyIfCloserThanPlayer)
                CurrentTarget = enemySqr < playerSqr ? enemy : player;
            else
            {
                switch (targetPriority)
                {
                    case TargetPriority.PlayerFirst:
                        CurrentTarget = player;
                        break;
                    case TargetPriority.EnemyFirst:
                        CurrentTarget = playerSqr >= enemyFirstPlayerDistance * enemyFirstPlayerDistance
                            ? enemy : player;
                        break;
                    default:
                        CurrentTarget = enemySqr < playerSqr ? enemy : player;
                        break;
                }
            }
        }
        UpdateTargetDistance();
    }

    public Transform FindNearestValidEnemyTarget()
    {
        SystemManager manager = SystemManager.Instance;
        if (manager == null || manager.AllEnemy == null) return null;

        Transform nearest = null;
        float minDistanceSqr = float.PositiveInfinity;
        foreach (GameObject candidate in manager.AllEnemy)
        {
            if (candidate == null || candidate == gameObject) continue;
            EnemyBase other = candidate.GetComponent<EnemyBase>();
            if (other == null || other == this ||
                other.status == Status.Die || other.HP <= 0f) continue;

            Faction faction = other.Factions;
            if ((faction & excludedTargetFactions) != Faction.None) continue;
            if (!targetAllFactions && (faction & targetFactions) == Faction.None) continue;

            float distanceSqr = ((Vector2)(other.transform.position - transform.position)).sqrMagnitude;
            if (distanceSqr < minDistanceSqr)
            {
                minDistanceSqr = distanceSqr;
                nearest = other.transform;
            }
        }
        return nearest;
    }

    bool IsValidTarget(Transform candidate)
    {
        if (candidate == null) return false;
        if (Player != null && candidate == Player.transform)
            return targetType != TargetType.EnemyOnly;

        EnemyBase other = candidate.GetComponent<EnemyBase>();
        if (other == null || other == this || other.status == Status.Die || other.HP <= 0f)
            return false;

        Faction faction = other.Factions;
        if ((faction & excludedTargetFactions) != Faction.None) return false;
        return targetAllFactions || (faction & targetFactions) != Faction.None;
    }

    // 攻撃判定がPrefab本体に付いている場合・子階層に付いている場合の両方に対応。
    // 全子階層（非アクティブ含む）のEnemyAttackを設定する。
    public int ConfigureAttackObject(GameObject attackObject, float attackPowerPercent,
        Faction attackableFactions, bool hitPlayer)
    {
        if (attackObject == null) return 0;
        EnemyAttack[] attacks = attackObject.GetComponentsInChildren<EnemyAttack>(true);
        foreach (EnemyAttack attack in attacks)
            attack.ConfigureAttack(this, attackPowerPercent, attackableFactions, hitPlayer);
        return attacks.Length;
    }

    // 派閥・プレイヤー攻撃設定をPrefabのままにしたい場合。
    public int ConfigureAttackObject(GameObject attackObject, float attackPowerPercent)
    {
        if (attackObject == null) return 0;
        EnemyAttack[] attacks = attackObject.GetComponentsInChildren<EnemyAttack>(true);
        foreach (EnemyAttack attack in attacks)
            attack.ConfigureAttack(this, attackPowerPercent);
        return attacks.Length;
    }

    public GameObject SpawnAttack(GameObject prefab, Vector3 position, Quaternion rotation,
        float lifeTime, float powerPercent, Faction attackableFactions,
        bool hitPlayer, Transform parent = null)
    {
        if (prefab == null) return null;
        GameObject instance = Instantiate(prefab, position, rotation, parent);
        if (ConfigureAttackObject(instance, powerPercent, attackableFactions, hitPlayer) == 0)
            Debug.LogWarning("生成したPrefabにEnemyAttackがありません: " + prefab.name, instance);
        if (lifeTime > 0f) Destroy(instance, lifeTime);
        return instance;
    }

    // 攻撃倍率だけ指定し、攻撃対象はPrefab側の設定を使う。
    public GameObject SpawnAttack(GameObject prefab, Vector3 position, Quaternion rotation,
        float lifeTime, float powerPercent = 100f, Transform parent = null)
    {
        if (prefab == null) return null;
        GameObject instance = Instantiate(prefab, position, rotation, parent);
        if (ConfigureAttackObject(instance, powerPercent) == 0)
            Debug.LogWarning("生成したPrefabにEnemyAttackがありません: " + prefab.name, instance);
        if (lifeTime > 0f) Destroy(instance, lifeTime);
        return instance;
    }

    private void Start()
    {
        if (moveBase == null)
        {
            GameObject CL_MoveBase = Instantiate(EnemyMoveBaseObject, transform.position, Quaternion.identity);
            moveBase = CL_MoveBase.GetComponent<EnemyMoveBase>();
            CL_MoveBase.transform.SetParent(gameObject.transform);
        }
        moveBase.enemyBase = this;
        CurrentTarget = Player != null ? Player.transform : null;
        if (targetType != TargetType.PlayerOnly) ChooseTargetNow();
    }
    private void FixedUpdate()
    {
        //敵全体のAI管理
        switch (status)
        {
            case Status.Nomal://敵ごとの自由行動
                NomalActive();
                break;
            case Status.Stan://スタン（ダメージによる行動中断）
                StanStanActive();
                break;
            case Status.ConfusionResistance://混乱（ダメージによる行動不能状態）
                ConfusionActive();
                break;
        }
    }
    void ConfusionActive()
    {
        animator.SetBool("Damage", true);
        if (ConfusionknockBackTimer + 3 + playerController.ConfusionTime + ConfusionAddTime > StanCount)
        {
            moveBase.EnemyConfusionPlay();
            StanCount += Time.deltaTime;
        }
        else
        {
            
            StanCount = 0;
            status = Status.Nomal;
            animator.SetBool("Damage", false);
            animator.SetInteger("Anim", 0);
        }
    }
    void StanStanActive()
    {
        animator.SetBool("Damage", true);
        if (StanTime > StanCount)
        {
            moveBase.EnemyStanPlay();
            StanCount += Time.deltaTime;
        }
        else
        {   
            StanCount = 0;
            status = Status.Nomal;
            animator.SetBool("Damage", false);
            animator.SetInteger("Anim", 0);
        }
    }
    private void NomalActive()
    {
        RefreshTarget();

        // EnemyOnlyで敵がいなければ友好行動ループに移る。
        // 攻撃中は現在の攻撃を完了させ、終了後に移行する。
        if (targetType == TargetType.EnemyOnly && CurrentTarget == null &&
            moveType != MoveType.Attack)
        {
            FriendlyActive();
            return;
        }

        // 敵が再出現したときは通常の移動・攻撃ループへ戻す。
        if (IsHostilityReleased)
        {
            IsHostilityReleased = false;
            moveType = MoveType.Move;
            moveBase.EnemyFriendlyExit();
        }

        // 攻撃終了フレームでTargetDistanceにより即時再攻撃しないようにする。
        bool wasAttacking = moveType == MoveType.Attack;
        switch (moveType)
        {
            case MoveType.Move:
                MoveAction();
                break;
            case MoveType.Attack:
                AttackAction();
                break;
        }
        // PlayerOnlyは従来のPlayerDistance、それ以外は選択対象とのTargetDistanceで判定。
        if (targetType != TargetType.PlayerOnly && wasAttacking &&
            moveType != MoveType.Attack)
            return;

        if ((targetType == TargetType.PlayerOnly ? PlayerDistance : TargetDistance) < LookPlayerDistance)
            moveType = MoveType.Attack;
    }
    // EnemyOnlyで攻撃対象がいない間、FixedUpdateから継続的に呼ぶ。
    void FriendlyActive()
    {
        if (!IsHostilityReleased)
        {
            IsHostilityReleased = true;
            rb.velocity = Vector2.zero;
            animator.SetInteger("Anim", 0);
            moveBase.EnemyFriendlyEnter();
        }

        // 個別の友好NPCはこの処理をoverrideして追従・待機などを実装可能。
        moveBase.EnemyFriendlyPlay();
    }

    void AttackAction()
    {//攻撃モーションを別途入れる
        moveBase.EnemyAttackPlay();
    }
    private void MoveAction()
    {
        if (Choice_MoveToBase == MoveToBase.Out)
        {
            if (targetType == TargetType.PlayerOnly)
            {
                PlayerLook();
                rb.velocity = PlayerDirection.normalized * SPEED;
            }
            else if (CurrentTarget != null)
            {
                UpdateTargetDistance();
                isTargetLook(CurrentTarget.position);
                rb.velocity = TargetDirection.normalized * SPEED;
            }
            else
            {
                rb.velocity = Vector2.zero;
                animator.SetInteger("Anim", 0);
                return;
            }
            animator.SetInteger("Anim", 1);
        }
        else 
        { 
            moveBase.EnemyMovePlay();
        }
    }
    public void PlayerLook()
    {
        Vector2 targetPos = Player.transform.position;
        Vector2 myPos = transform.position;

        PlayerDirection = targetPos - myPos;
        PlayerDistance = PlayerDirection.magnitude;

        EnemyAnimBody.transform.localScale =
            new Vector2(
                targetPos.x < myPos.x ? -DefoScale : DefoScale,
                DefoScale
            );
    }
}
