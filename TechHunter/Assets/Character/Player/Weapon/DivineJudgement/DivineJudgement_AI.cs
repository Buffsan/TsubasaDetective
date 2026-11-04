using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 神の名の下に…「死ね！！」
/// 詠唱中にカーソルに最も近い敵へ最大20個のターゲットを割り振る。
/// 攻撃・演出は Inspector で設定するプレハブから生成する。
/// </summary>
public class DivineJudgement_AI : PlayerSkillBase
{
    [Header("詠唱")]
    [SerializeField, Min(0.1f)] float chantDuration = 4f;
    [SerializeField, Range(1, 100)] int maxMarks = 20;
    [Header("演出・攻撃")]
    [SerializeField] GameObject crossPrefab;
    [SerializeField] GameObject lightPillarAttackPrefab;
    [SerializeField, Min(0f)] float strikeInterval = 0.12f;
    [SerializeField, Min(0f)] float crownRadius = 0.65f;
    [SerializeField, Min(0f)] float crownHeight = 0.8f;

    readonly List<Mark> marks = new List<Mark>();
    float elapsed;
    int issued;
    bool fired;
    PlayerSkillBase ownSkill;

    class Mark
    {
        public EnemyBase target;
        public GameObject visual;
    }

    void Awake() => ownSkill = GetComponent<PlayerSkillBase>();

    void Update()
    {
        if (fired) return;
        if (playerController == null) return;
        // 別のスキルや回避により中断された場合も、それまでの指定を発射する。
        if (playerController.SaveSkillBase != ownSkill && issued > 0)
            Fire();
    }

    public override void SkillPlay()
    {
        if (fired) return;
        elapsed = Mathf.Min(chantDuration, elapsed + Time.deltaTime);
        float x = elapsed / chantDuration;
        int desired = Mathf.Min(maxMarks, Mathf.FloorToInt(maxMarks * 0.5f * (x * x + x * x * x) + 0.00001f));
        while (issued < desired)
        {
            issued++;
            EnemyBase target = FindNearestEnemy();
            if (target != null) AddMark(target);
        }
        if (elapsed >= chantDuration) Fire();
    }

    EnemyBase FindNearestEnemy()
    {
        if (playerController == null || playerController.cursorController == null) return null;
        Vector2 cursor = playerController.cursorController.MousePos;
        EnemyBase nearest = null;
        float distance = float.PositiveInfinity;
        // 詠唱は最大4秒。敵探索は必要な付与タイミングでのみ実行する。
        foreach (EnemyBase enemy in FindObjectsOfType<EnemyBase>())
        {
            if (enemy == null || !enemy.isActiveAndEnabled ||
                enemy.status == EnemyBase.Status.Die || enemy.HP <= 0 || enemy.IsCharmed)
                continue;
            float sqr = ((Vector2)enemy.transform.position - cursor).sqrMagnitude;
            if (sqr < distance) { nearest = enemy; distance = sqr; }
        }
        return nearest;
    }

    void AddMark(EnemyBase target)
    {
        Mark mark = new Mark { target = target };
        marks.Add(mark);
        if (crossPrefab == null) return;
        mark.visual = Instantiate(crossPrefab);
        UpdateCrown(target);
    }

    void UpdateCrown(EnemyBase target)
    {
        List<Mark> group = marks.FindAll(m => m.target == target && m.visual != null);
        for (int i = 0; i < group.Count; i++)
        {
            // 頭を中心とした針山状の放射配置。2D見下ろし向けに楕円へ投影する。
            float angle = (i + 0.5f) * Mathf.PI * 2f / Mathf.Max(1, group.Count);
            Vector3 offset = new Vector3(Mathf.Cos(angle) * crownRadius,
                crownHeight + Mathf.Sin(angle) * crownRadius * 0.4f, 0f);
            group[i].visual.transform.position = target.transform.position + offset;
            group[i].visual.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg - 90f);
        }
    }

    void LateUpdate()
    {
        if (fired) return;
        HashSet<EnemyBase> targets = new HashSet<EnemyBase>();
        foreach (Mark mark in marks)
            if (mark.target != null && mark.visual != null) targets.Add(mark.target);
        foreach (EnemyBase target in targets) UpdateCrown(target);
    }

    void Fire()
    {
        if (fired) return;
        fired = true;
        // スキルオブジェクトが破棄されても神罰が最後まで発生するように独立させる。
        GameObject runnerObject = new GameObject("DivineJudgement_StrikeRunner");
        DivineJudgementStrikeRunner runner = runnerObject.AddComponent<DivineJudgementStrikeRunner>();
        runner.Begin(marks.ConvertAll(m => m.target), marks.ConvertAll(m => m.visual),
            lightPillarAttackPrefab, strikeInterval);
        end();
    }

    void OnDestroy()
    {
        if (fired) return;
        foreach (Mark mark in marks)
            if (mark.visual != null) Destroy(mark.visual);
    }
}

/// <summary>神罰を詠唱オブジェクトの寿命から独立して連続生成する。</summary>
public class DivineJudgementStrikeRunner : MonoBehaviour
{
    readonly List<EnemyBase> targets = new List<EnemyBase>();
    readonly List<GameObject> crosses = new List<GameObject>();
    GameObject pillarPrefab;
    float interval;

    public void Begin(List<EnemyBase> assigned, List<GameObject> visuals, GameObject prefab, float delay)
    {
        targets.AddRange(assigned);
        crosses.AddRange(visuals);
        pillarPrefab = prefab;
        interval = delay;
        StartCoroutine(Strike());
    }

    System.Collections.IEnumerator Strike()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (crosses[i] != null) Destroy(crosses[i]);
            EnemyBase enemy = targets[i];
            if (enemy != null && enemy.HP > 0 && enemy.status != EnemyBase.Status.Die &&
                !enemy.IsCharmed && pillarPrefab != null)
                Instantiate(pillarPrefab, enemy.transform.position, Quaternion.identity);
            if (interval > 0) yield return new WaitForSeconds(interval);
            else yield return null;
        }
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        foreach (GameObject cross in crosses)
            if (cross != null) Destroy(cross);
    }
}
