using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class Boss_Controller : EnemyBase
{
    [SerializeField] Boss_MoveBase moveBase;
    [SerializeField] GameObject EnemyMoveBaseObject;

    // ボスはEnemyContorollerとは別系統のため、同じターゲット形式を独立して提供する。
    // 初期値PlayerOnlyで既存のボス戦・演出を維持。
    [Header("ボスターゲット設定")]
    [SerializeField] EnemyContoroller.TargetType targetType = EnemyContoroller.TargetType.PlayerOnly;
    [SerializeField] EnemyContoroller.TargetPriority targetPriority = EnemyContoroller.TargetPriority.Nearest;
    [SerializeField] bool targetAllFactions = true;
    [SerializeField] Faction targetFactions = Faction.None;
    [SerializeField] Faction excludedTargetFactions = Faction.None;
    [SerializeField, Min(0f)] float enemyFirstPlayerDistance;
    [SerializeField, Min(0.05f)] float targetSearchInterval = 0.25f;

    // 警告タイプはリストの要素番号。追加の形状はリスト末尾へ登録できる。
    public const int WarningRectangle = 0;
    public const int WarningCircle = 1;
    public const int WarningSemicircle = 2;

    [Header("攻撃警告Prefab（0=四角 / 1=円 / 2=半円、以降追加可）")]
    [SerializeField] List<GameObject> attackWarningPrefabs = new List<GameObject>();

    // このControllerを通じて生成した警告をまとめて管理する。
    readonly List<GameObject> activeAttackWarnings = new List<GameObject>();
    public IReadOnlyList<GameObject> ActiveAttackWarnings => activeAttackWarnings;

    public bool HasAttackWarningPrefab(int warningType)
    {
        return warningType >= 0 &&
            warningType < attackWarningPrefabs.Count &&
            attackWarningPrefabs[warningType] != null;
    }

    // 簡易版：形状、位置、表示秒数、角度、サイズを指定。
    public GameObject SpawnAttackWarning(int warningType, Vector3 position,
        float duration, float angleDegrees = 0f, Vector3? localScale = null)
    {
        return SpawnAttackWarning(warningType, position,
            Quaternion.Euler(0f, 0f, angleDegrees), duration,
            null, 0.2f, true, localScale);
    }

    // 登録したタイプで警告を生成する（角度や親子付けも指定可能）。
    public GameObject SpawnAttackWarning(int warningType, Vector3 position,
        Quaternion rotation, float duration, Transform parent = null,
        float extraLifetime = 0.2f, bool syncAnimation = true,
        Vector3? localScale = null)
    {
        if (!HasAttackWarningPrefab(warningType))
        {
            Debug.LogWarning("未登録の攻撃警告タイプ: " + warningType, this);
            return null;
        }

        return SpawnAttackWarningPrefab(attackWarningPrefabs[warningType],
            position, rotation, duration, parent, extraLifetime, syncAnimation, localScale);
    }

    // 既存の個別Prefabを使う場合も、生成・時間管理を共通化できる。
    public GameObject SpawnAttackWarningPrefab(GameObject prefab, Vector3 position,
        Quaternion rotation, float duration, Transform parent = null,
        float extraLifetime = 0.2f, bool syncAnimation = true,
        Vector3? localScale = null)
    {
        if (prefab == null) return null;

        GameObject instance = Instantiate(prefab, position, rotation, parent);
        if (localScale.HasValue) instance.transform.localScale = localScale.Value;
        if (syncAnimation && duration > 0f)
        {
            SaveComponent save = instance.GetComponentInChildren<SaveComponent>(true);
            if (save != null && save.animator != null)
                save.animator.speed = 1f / duration;
        }

        activeAttackWarnings.RemoveAll(warning => warning == null);
        activeAttackWarnings.Add(instance);
        Destroy(instance, Mathf.Max(0.01f, duration + extraLifetime));
        return instance;
    }

    // スタン等で警告を取り消したいAIから明示的に呼ぶ。
    public void ClearAttackWarnings()
    {
        foreach (GameObject warning in activeAttackWarnings)
            if (warning != null) Destroy(warning);
        activeAttackWarnings.Clear();
    }

    public void EnableCharmedAllyTargeting()
    {
        if (IsCharmed || targetType == EnemyContoroller.TargetType.EnemyOnly) return;
        if (targetType == EnemyContoroller.TargetType.PlayerOnly)
        {
            targetType = EnemyContoroller.TargetType.PlayerAndEnemy;
            targetPriority = EnemyContoroller.TargetPriority.Nearest;
            targetAllFactions = false;
            targetFactions = Faction.Charmed;
        }
        else if (!targetAllFactions)
            targetFactions |= Faction.Charmed;

        excludedTargetFactions &= ~Faction.Charmed;
        ChooseTargetNow();
    }

    public void BecomeCharmedAlly()
    {
        targetType = EnemyContoroller.TargetType.EnemyOnly;
        targetPriority = EnemyContoroller.TargetPriority.Nearest;
        targetAllFactions = true;
        targetFactions = Faction.None;
        excludedTargetFactions = Faction.Charmed;
        ClearForcedTarget();
        ChooseTargetNow();
    }

    public EnemyContoroller.TargetType TargetMode => targetType;

    // デバッグ専用。通常のボス戦のターゲット設定には影響しない。
    public void SetDebugOpponentFaction(Faction opponent)
    {
        targetType = EnemyContoroller.TargetType.EnemyOnly;
        targetPriority = EnemyContoroller.TargetPriority.Nearest;
        targetAllFactions = false;
        targetFactions = opponent;
        excludedTargetFactions = Faction.None;
        ClearForcedTarget();
        ChooseTargetNow();
    }

    // ボスの既存・新規攻撃判定にも同じ敵対派閥を設定する。
    public void ConfigureAttackTarget(EnemyAttack attack)
    {
        if (attack == null) return;
        Faction factions = Faction.None;
        if (targetType != EnemyContoroller.TargetType.PlayerOnly)
            factions = (targetAllFactions ? (Faction)(~0) : targetFactions)
                & ~excludedTargetFactions;
        bool hitPlayer = targetType != EnemyContoroller.TargetType.EnemyOnly;
        attack.ConfigureAttack(this, attack.AttackPowerPercent, factions, hitPlayer);
    }

    // ボスの敵対設定を召喚した雑魚敵へ渡す。PlayerOnlyの従来召喚は変更しない。
    public void CopyTargetSettingsTo(EnemyBase summoned)
    {
        if (summoned == null ||
            targetType == EnemyContoroller.TargetType.PlayerOnly) return;

        summoned.SetDebugFaction(Factions);
        EnemyContoroller child = summoned as EnemyContoroller;
        if (child != null)
        {
            child.ApplySummonerTargetSettings(targetType, targetPriority,
                targetAllFactions, targetFactions, excludedTargetFactions,
                enemyFirstPlayerDistance, targetSearchInterval);
        }
        else
        {
            CG_Knght knight = summoned as CG_Knght;
            if (knight != null)
                knight.CopyTargetSettings(targetType, targetPriority, targetAllFactions,
                    targetFactions, excludedTargetFactions, enemyFirstPlayerDistance,
                    targetSearchInterval);
        }
    }

    public Transform CurrentTarget { get; private set; }
    public Vector2 TargetDirection { get; private set; }
    public float TargetDistance { get; private set; } = float.PositiveInfinity;
    float targetSearchTimer;
    Transform forcedTarget;

    public void ForceTarget(Transform target)
    {
        forcedTarget = target;
        CurrentTarget = target;
        UpdateTargetDirection();
    }

    public void ClearForcedTarget()
    {
        forcedTarget = null;
        targetSearchTimer = targetSearchInterval;
    }

    void RefreshTarget()
    {
        if (targetType == EnemyContoroller.TargetType.PlayerOnly)
        {
            CurrentTarget = Player != null ? Player.transform : null;
            UpdateTargetDirection();
            return;
        }

        if (moveType != MoveType.Attack)
        {
            targetSearchTimer += Time.fixedDeltaTime;
            if (targetSearchTimer >= Mathf.Max(0.05f, targetSearchInterval) ||
                (CurrentTarget != null && !IsTargetValid(CurrentTarget)))
            {
                ChooseTargetNow();
                targetSearchTimer = 0f;
            }
        }
        UpdateTargetDirection();
    }

    void UpdateTargetDirection()
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

    bool IsTargetValid(Transform target)
    {
        if (target == null) return false;
        if (Player != null && target == Player.transform)
            return targetType != EnemyContoroller.TargetType.EnemyOnly;

        EnemyBase other = target.GetComponent<EnemyBase>();
        if (other == null || other == this ||
            other.status == Status.Die || other.HP <= 0f) return false;
        if ((other.Factions & excludedTargetFactions) != Faction.None) return false;
        return targetAllFactions || (other.Factions & targetFactions) != Faction.None;
    }

    public void ChooseTargetNow()
    {
        Transform player = Player != null ? Player.transform : null;
        if (targetType == EnemyContoroller.TargetType.PlayerOnly)
        {
            CurrentTarget = player;
            UpdateTargetDirection();
            return;
        }
        if (forcedTarget != null && IsTargetValid(forcedTarget))
        {
            CurrentTarget = forcedTarget;
            UpdateTargetDirection();
            return;
        }
        forcedTarget = null;

        Transform nearestEnemy = null;
        float nearestSqr = float.PositiveInfinity;
        SystemManager manager = SystemManager.Instance;
        if (manager != null && manager.AllEnemy != null)
        {
            foreach (GameObject obj in manager.AllEnemy)
            {
                if (obj == null || obj == gameObject) continue;
                EnemyBase other = obj.GetComponent<EnemyBase>();
                if (other == null || other == this ||
                    other.status == Status.Die || other.HP <= 0f) continue;
                if ((other.Factions & excludedTargetFactions) != Faction.None) continue;
                if (!targetAllFactions && (other.Factions & targetFactions) == Faction.None)
                    continue;

                float sqr = ((Vector2)(other.transform.position - transform.position)).sqrMagnitude;
                if (sqr < nearestSqr)
                {
                    nearestSqr = sqr;
                    nearestEnemy = other.transform;
                }
            }
        }

        // 味方化した敵はAllEnemyに含まれないため、敵側からは別リストも探索する。
        if (!IsCharmed && manager != null)
        {
            foreach (EnemyBase other in manager.CharmedAllies)
            {
                if (other == null || other.status == Status.Die || other.HP <= 0f) continue;
                if ((other.Factions & excludedTargetFactions) != Faction.None) continue;
                if (!targetAllFactions && (other.Factions & targetFactions) == Faction.None) continue;
                float sqr = ((Vector2)(other.transform.position - transform.position)).sqrMagnitude;
                if (sqr < nearestSqr)
                {
                    nearestSqr = sqr;
                    nearestEnemy = other.transform;
                }
            }
        }

        if (targetType == EnemyContoroller.TargetType.EnemyOnly)
            CurrentTarget = nearestEnemy;
        else if (nearestEnemy == null)
            CurrentTarget = player;
        else if (player == null)
            CurrentTarget = nearestEnemy;
        else
        {
            float playerSqr = ((Vector2)(player.position - transform.position)).sqrMagnitude;
            switch (targetPriority)
            {
                case EnemyContoroller.TargetPriority.PlayerFirst:
                    CurrentTarget = player;
                    break;
                case EnemyContoroller.TargetPriority.EnemyFirst:
                    CurrentTarget = playerSqr >= enemyFirstPlayerDistance * enemyFirstPlayerDistance
                        ? nearestEnemy : player;
                    break;
                default:
                    CurrentTarget = nearestSqr < playerSqr ? nearestEnemy : player;
                    break;
            }
        }
        UpdateTargetDirection();
    }


    [SerializeField] CutScene StartCutScene;
    
    [SerializeField] AudioClip StartAudio;
 
    public TextMeshProUGUI HPtext;
    float movieCount = 0;
    
    private void Start()
    {
        if (moveBase == null)
        {
            GameObject CL_MoveBase = Instantiate(EnemyMoveBaseObject, transform.position, Quaternion.identity);
            moveBase = CL_MoveBase.GetComponent<Boss_MoveBase>();
            CL_MoveBase.transform.SetParent(gameObject.transform);
        }
        


        moveBase.enemyBase = this;
        CurrentTarget = Player != null ? Player.transform : null;
        if (targetType != EnemyContoroller.TargetType.PlayerOnly) ChooseTargetNow();
        if (SystemManager.Instance != null && SystemManager.Instance.CharmedAllies.Count > 0)
            EnableCharmedAllyTargeting();

        if (status == Status.MovieStay) return;
        // デバッグ登録済みの場合でもAllEnemyに二重追加しない。
        if (!ALL_System.systemManager.AllEnemy.Contains(this.gameObject))
            ALL_System.systemManager.AllEnemy.Add(this.gameObject);

    }
    private void FixedUpdate()
    {
        switch (status)
        {
            case Status.Nomal:
                isNomalActive();
                break;
            case Status.Stan:
                isStanStan();
                break;
            case Status.ConfusionResistance:
                isConfusion();
                break;
            case Status.Die:
                foreach (var enemys in moveBase.AllEnemy) 
                {
                    if (enemys)Destroy(enemys.gameObject);
                }
                moveBase.enemyList.Clear();
                return; // 死亡中は一切動かさない
            case Status.MovieStay:
                isMovie();
                break;
        }
        HP_TextChange();
        
    }
    void HP_TextChange() 
    {

        HPtext.text = HP + "/" + MAXHP; 

    }
    // Update is called once per frame
    void isMovie() 
    {
       
        if (MoviePhase == 0) 
        {   
            charaDamage.gameObject.SetActive(false);
            audioManager.BgmSource.Stop();
            movieCount += Time.deltaTime;
            if (movieCount < 0.2) return;
            //Debug.Log(MoviePhase);
            if (ALL_System.systemManager.gameMode != SystemManager.GameMode.Nomal) return;
            ALL_System.systemManager.gameMode = SystemManager.GameMode.Movie;
            
            movieCount =0;
            MoviePhase ++;
        }
        if (MoviePhase == 1) 
        { 
            
            float PlayerDistanceNow = Vector2.Distance(playerController.transform.position, transform.position);
            if (PlayerDistanceNow < 3) 
            { 
                MoviePhase ++;
                playerController.movetype = PlayerController.MoveType.Wait;
                playerController.rb.velocity = Vector2.zero;
                playerController.MoveInput = Vector2.zero;

                audioManager.PlaySE(StartAudio);
            }
        }
        if (MoviePhase == 2) 
        {
            StartCutScene.animator.Play("出現");
            StartCutScene.ChangeImage(CutSceneData);
            MoviePhase++;
        }
        if (MoviePhase == 3) 
        {
            movieCount += Time.deltaTime;
            if (movieCount < 1) return;
            if (playerController.MoveInput != Vector2.zero) 
            {
                ALL_System.systemManager.gameMode = SystemManager.GameMode.Nomal;
                playerController.movetype = PlayerController.MoveType.Nomal;
                MoviePhase++;
                audioManager.BgmSource.Play();
                StartCutScene.animator.Play("消える");
                charaDamage.gameObject.SetActive(true);
                ALL_System.systemManager.AllEnemy.Add(this.gameObject);
                status = Status.Nomal;
            }
        }
        if (MoviePhase == 4) 
        { 
        
        }
    
    }

    void isConfusion()
    {
        animator.SetBool("Damage", true);
        if (ConfusionknockBackTimer + 3 + ConfusionAddTime + playerController.ConfusionTime> StanCount)
        {
            StanCount += Time.deltaTime;
        }
        else
        {
            moveBase.M_Con();
            StanCount = 0;
            status = Status.Nomal;
            animator.SetBool("Damage", false);
            animator.SetInteger("Anim", 0);
        }
    }
    void isStanStan()
    {
        animator.SetBool("Damage", true);
        if (StanTime > StanCount)
        {
            StanCount += Time.deltaTime;
        }
        else
        {
            moveBase.M_Stan();
            StanCount = 0;
            status = Status.Nomal;
            animator.SetBool("Damage", false);
            animator.SetInteger("Anim", 0);
        }
    }
    private void isNomalActive()
    {
        RefreshTarget();
        if (targetType == EnemyContoroller.TargetType.EnemyOnly &&
            CurrentTarget == null && moveType != MoveType.Attack)
        {
            rb.velocity = Vector2.zero;
            animator.SetInteger("Anim", 0);
            return;
        }

        switch (moveType)
        {
            case MoveType.Move:
                isMove();
                break;
            case MoveType.Attack:
                isAttack();
                break;
                case MoveType.AI:
                //isAI();
                break;

        }

        
    }
    void isAI() 
    {
        moveBase.M_AI();
    }
    void isAttack()
    {//攻撃モーションを別途入れる
        moveBase.M_Attack();
    }
    private void isMove()
    {
        //moveBase.M_BaseMove();
        isAI();
    }
    public void isTargetLook(Vector2 target) 
    {
        if (target.x < transform.position.x)
        {
            EnemyAnimBody.transform.localScale = new Vector2(-DefoScale, DefoScale);
        }
        else
        {
            EnemyAnimBody.transform.localScale = new Vector2(DefoScale, DefoScale);
        }
    }
    public void isPlayerLook()
    {
        // 旧メソッド名との互換性を残しつつ、攻撃対象の位置を参照する。
        Transform target = CurrentTarget;
        if (target == null && targetType == EnemyContoroller.TargetType.PlayerOnly &&
            Player != null)
            target = Player.transform;
        if (target == null)
        {
            PlayerDirection = Vector2.zero;
            PlayerDistance = float.PositiveInfinity;
            return;
        }

        PlayerDirection = (Vector2)target.position - (Vector2)transform.position;
        PlayerDistance = PlayerDirection.magnitude;
        isTargetLook(target.position);
    }
}
