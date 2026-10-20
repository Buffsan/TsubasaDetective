using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 専用マップの空GameObjectにアタッチして使う敵VS敵の模擬戦ツール。
// EnemyContorollerとBoss_Controllerを同じDebugA/DebugBに編成できる。
public class DebugEnemyTeamBattle : MonoBehaviour
{
    [Header("チームA（敵Prefab・ボスPrefab）")]
    [SerializeField] List<GameObject> teamA = new List<GameObject>();
    [Header("チームB（敵Prefab・ボスPrefab）")]
    [SerializeField] List<GameObject> teamB = new List<GameObject>();

    [Header("チームA（シーン配置済みの敵）")]
    [SerializeField] List<EnemyContoroller> placedTeamA = new List<EnemyContoroller>();
    [Header("チームB（シーン配置済みの敵）")]
    [SerializeField] List<EnemyContoroller> placedTeamB = new List<EnemyContoroller>();

    [Header("チームA（シーン配置済みのボス）")]
    [SerializeField] List<Boss_Controller> placedBossTeamA = new List<Boss_Controller>();
    [Header("チームB（シーン配置済みのボス）")]
    [SerializeField] List<Boss_Controller> placedBossTeamB = new List<Boss_Controller>();

    [Header("チームA（子階層の敵・ボスを自動登録）")]
    [SerializeField] Transform teamAParent;
    [Header("チームB（子階層の敵・ボスを自動登録）")]
    [SerializeField] Transform teamBParent;

    [Header("出現位置（未指定ならこのオブジェクトから左右に配置）")]
    [SerializeField] Transform teamASpawnPoint;
    [SerializeField] Transform teamBSpawnPoint;
    [SerializeField, Min(0.1f)] float spacing = 2f;
    [SerializeField] float teamDistance = 10f;
    [SerializeField] bool spawnOnStart = true;

    [Header("ボスのデバッグ設定")]
    [SerializeField] bool skipBossIntro = true;

    [Header("デバッグ診断")]
    [SerializeField] bool logTeamRegistration = true;

    readonly List<EnemyBase> spawned = new List<EnemyBase>();
    bool started;

    void Start()
    {
        if (spawnOnStart) StartBattle();
    }

    [ContextMenu("Start Debug Battle")]
    public void StartBattle()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("模擬戦はPlayモードで実行してください。", this);
            return;
        }
        if (started)
        {
            Debug.LogWarning("模擬戦は既に開始しています。", this);
            return;
        }
        started = true;

        RegisterPlacedTeam(placedTeamA, Faction.DebugA);
        RegisterPlacedTeam(placedTeamB, Faction.DebugB);
        RegisterPlacedBossTeam(placedBossTeamA, Faction.DebugA);
        RegisterPlacedBossTeam(placedBossTeamB, Faction.DebugB);
        RegisterChildTeam(teamAParent, Faction.DebugA);
        RegisterChildTeam(teamBParent, Faction.DebugB);
        SpawnTeam(teamA, Faction.DebugA, teamASpawnPoint, -1f);
        SpawnTeam(teamB, Faction.DebugB, teamBSpawnPoint, 1f);

        // 全メンバー登録後にターゲット確定。翌フレームにもStart後の状態を確認する。
        SetOpponentTargets();
        StartCoroutine(InitializeTargets());
    }

    void RegisterPlacedTeam(List<EnemyContoroller> members, Faction faction)
    {
        foreach (EnemyContoroller member in members)
            RegisterCombatant(member, faction);
    }

    void RegisterPlacedBossTeam(List<Boss_Controller> members, Faction faction)
    {
        foreach (Boss_Controller member in members)
            RegisterCombatant(member, faction);
    }

    // 指定したEmptyObject以下に配置された敵・ボスを孫階層も含めて登録。
    void RegisterChildTeam(Transform parent, Faction faction)
    {
        if (parent == null) return;

        EnemyBase[] members = parent.GetComponentsInChildren<EnemyBase>(true);
        int count = 0;
        foreach (EnemyBase member in members)
        {
            if (member == null || !IsSupportedCombatant(member)) continue;
            if (RegisterCombatant(member, faction)) count++;
        }
        if (logTeamRegistration)
            Debug.Log($"[DebugBattle] {parent.name} の子階層から{count}体を登録（{faction}）", parent);
    }

    void SpawnTeam(List<GameObject> prefabs, Faction faction, Transform point, float side)
    {
        Vector3 center = point != null
            ? point.position
            : transform.position + Vector3.right * side * teamDistance * 0.5f;

        for (int i = 0; i < prefabs.Count; i++)
        {
            GameObject prefab = prefabs[i];
            if (prefab == null) continue;

            Vector3 position = center + Vector3.up * ((i - (prefabs.Count - 1) * 0.5f) * spacing);
            GameObject instance = Instantiate(prefab, position, Quaternion.identity);
            EnemyBase combatant = instance.GetComponent<EnemyBase>();
            if (!IsSupportedCombatant(combatant))
            {
                Debug.LogError("模擬戦対応のEnemyContoroller/Boss_Controller等がありません: " + prefab.name, instance);
                Destroy(instance);
                continue;
            }

            RegisterCombatant(combatant, faction);
        }
    }

    bool IsSupportedCombatant(EnemyBase member)
    {
        return member is EnemyContoroller || member is Boss_Controller || member is CG_Knght;
    }

    bool RegisterCombatant(EnemyBase member, Faction faction)
    {
        if (member == null || !IsSupportedCombatant(member)) return false;
        if (spawned.Contains(member))
        {
            Debug.LogWarning("同じ敵・ボスが複数チームに登録されています: " + member.name, member);
            return false;
        }

        member.SetDebugFaction(faction);
        Boss_Controller boss = member as Boss_Controller;
        if (boss != null && skipBossIntro && boss.status == EnemyBase.Status.MovieStay)
        {
            // 通常のボス演出を経ず、専用デバッグマップで戦闘を開始する。
            boss.status = EnemyBase.Status.Nomal;
        }
        spawned.Add(member);
        RegisterInAllEnemy(member);
        if (logTeamRegistration)
            Debug.Log($"[DebugBattle] 登録: {member.name} / {faction} / {member.GetType().Name}", member);
        return true;
    }

    void RegisterInAllEnemy(EnemyBase member)
    {
        SystemManager manager = SystemManager.Instance;
        if (manager == null || manager.AllEnemy == null)
        {
            Debug.LogWarning("SystemManager.AllEnemyが見つかりません。", member);
            return;
        }
        if (!manager.AllEnemy.Contains(member.gameObject))
            manager.AllEnemy.Add(member.gameObject);
    }

    void SetOpponentTargets()
    {
        foreach (EnemyBase member in spawned)
        {
            if (member == null) continue;
            RegisterInAllEnemy(member);
            Faction opponent = member.Factions == Faction.DebugA ? Faction.DebugB : Faction.DebugA;

            EnemyContoroller controller = member as EnemyContoroller;
            Boss_Controller boss = member as Boss_Controller;
            CG_Knght knight = member as CG_Knght;
            Transform target = null;

            if (controller != null)
            {
                controller.SetDebugOpponentFaction(opponent);
                target = controller.CurrentTarget;
            }
            else if (boss != null)
            {
                boss.SetDebugOpponentFaction(opponent);
                target = boss.CurrentTarget;
            }
            else if (knight != null)
            {
                knight.CopyTargetSettings(EnemyContoroller.TargetType.EnemyOnly,
                    EnemyContoroller.TargetPriority.Nearest, false, opponent, Faction.None,
                    0f, 0.25f);
                knight.RefreshSummonedTarget();
                target = knight.CurrentTarget;
            }

            if (logTeamRegistration)
                Debug.Log($"[DebugBattle] {member.name}: 所属={member.Factions}, 相手派閥={opponent}, 標的={(target != null ? target.name : "なし")}", member);
        }
    }

    IEnumerator InitializeTargets()
    {
        yield return null;
        SetOpponentTargets();

        foreach (EnemyBase member in spawned)
        {
            if (member == null) continue;
            RegisterInAllEnemy(member);

            EnemyContoroller controller = member as EnemyContoroller;
            Boss_Controller boss = member as Boss_Controller;
            foreach (EnemyAttack attack in member.GetComponentsInChildren<EnemyAttack>(true))
            {
                if (controller != null) controller.ConfigureAttackTarget(attack);
                else if (boss != null) boss.ConfigureAttackTarget(attack);
            }
        }
    }
}
