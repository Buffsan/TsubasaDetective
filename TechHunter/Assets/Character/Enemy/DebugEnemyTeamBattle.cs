using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 専用マップの空GameObjectにアタッチして使う敵VS敵の模擬戦ツール。
public class DebugEnemyTeamBattle : MonoBehaviour
{
    [Header("チームA（敵Prefab）")]
    [SerializeField] List<GameObject> teamA = new List<GameObject>();
    [Header("チームB（敵Prefab）")]
    [SerializeField] List<GameObject> teamB = new List<GameObject>();

    [Header("チームA（シーン配置済みの敵）")]
    [SerializeField] List<EnemyContoroller> placedTeamA = new List<EnemyContoroller>();
    [Header("チームB（シーン配置済みの敵）")]
    [SerializeField] List<EnemyContoroller> placedTeamB = new List<EnemyContoroller>();

    [Header("出現位置（未指定ならこのオブジェクトから左右に配置）")]
    [SerializeField] Transform teamASpawnPoint;
    [SerializeField] Transform teamBSpawnPoint;
    [SerializeField, Min(0.1f)] float spacing = 2f;
    [SerializeField] float teamDistance = 10f;
    [SerializeField] bool spawnOnStart = true;

    readonly List<EnemyContoroller> spawned = new List<EnemyContoroller>();
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
        SpawnTeam(teamA, Faction.DebugA, teamASpawnPoint, -1f);
        SpawnTeam(teamB, Faction.DebugB, teamBSpawnPoint, 1f);
        StartCoroutine(InitializeTargets());
    }

    void RegisterPlacedTeam(List<EnemyContoroller> members, Faction faction)
    {
        foreach (EnemyContoroller controller in members)
        {
            if (controller == null) continue;
            if (spawned.Contains(controller))
            {
                Debug.LogWarning("同じ敵が複数チームに登録されています: " + controller.name, controller);
                continue;
            }
            controller.SetDebugFaction(faction);
            spawned.Add(controller);
        }
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
            EnemyContoroller controller = instance.GetComponent<EnemyContoroller>();
            if (controller == null)
            {
                Debug.LogError("EnemyContorollerがないため模擬戦に参加できません: " + prefab.name, instance);
                Destroy(instance);
                continue;
            }

            // 相手の派閥のみを対象にする。Prefabアセット自体は変更しない。
            controller.SetDebugFaction(faction);
            spawned.Add(controller);
        }
    }

    IEnumerator InitializeTargets()
    {
        // 全敵のStartとSystemManagerへの登録を待ってからターゲットを確定。
        yield return null;
        foreach (EnemyContoroller controller in spawned)
        {
            if (controller == null) continue;
            Faction opponent = controller.Factions == Faction.DebugA
                ? Faction.DebugB : Faction.DebugA;
            controller.SetDebugOpponentFaction(opponent);

            // 既に存在する攻撃判定にも設定を反映する。
            foreach (EnemyAttack attack in controller.GetComponentsInChildren<EnemyAttack>(true))
                controller.ConfigureAttackTarget(attack);
        }
    }
}
