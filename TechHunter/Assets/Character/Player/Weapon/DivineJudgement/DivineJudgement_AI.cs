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

    void Start() => ownSkill = this;

    void Update()
    {
        if (fired) return;
        if (playerController == null) return;
        // 別のスキルや回避により中断された場合も、それまでの指定を発射する。
        if (playerController.SaveSkillBase != ownSkill)
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
        mark.visual = crossPrefab != null ? Instantiate(crossPrefab) : DivineJudgementVisual.CreateCross();
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
            lightPillarAttackPrefab, strikeInterval, playerController, SkillLevel, SkillSlotIndex);
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
    PlayerController player;
    int skillLevel;
    int slot;
    [SerializeField] float baseDamage = 20f;

    public void Begin(List<EnemyBase> assigned, List<GameObject> visuals, GameObject prefab, float delay, PlayerController owner, int level, int sourceSlot)
    {
        targets.AddRange(assigned);
        crosses.AddRange(visuals);
        pillarPrefab = prefab;
        interval = delay;
        player = owner;
        skillLevel = level;
        slot = sourceSlot;
        StartCoroutine(Strike());
    }

    System.Collections.IEnumerator Strike()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (crosses[i] != null) Destroy(crosses[i]);
            EnemyBase enemy = targets[i];
            if (enemy != null && enemy.HP > 0 && enemy.status != EnemyBase.Status.Die && !enemy.IsCharmed)
            {
                if (pillarPrefab != null) Instantiate(pillarPrefab, enemy.transform.position, Quaternion.identity);
                else DivineJudgementVisual.CreateImpact(enemy.transform.position);
                charaDamage receiver = enemy.CharaDamage != null ? enemy.CharaDamage : enemy.CharmStatusReceiver;
                if (receiver != null && player != null)
                {
                    float levelBonus = Mathf.Max(0, skillLevel - 1);
                    float buff = player.playerBuff != null ? player.playerBuff.MultiplyAttack_AllBuff + player.playerBuff.SkillOnlyAttackMultiplierBonus : 1f;
                    float attack = ((baseDamage + player.ATK + 0.5f * levelBonus) *
                        ((100f + player.AddATK + 50f * levelBonus) / 100f) +
                        (player.playerBuff != null ? player.playerBuff.Attack_AllBuff : 0f)) * player.AddMultiplierATK * buff;
                    bool critical = player.AddCritical + player.Critical > Random.Range(0f, 100f);
                    if (critical) attack = (attack * 3f + 5f) * Mathf.Max(0f, 1f + player.AddLuckyDamagePercent / 100f);
                    GlobalEvents.InvokePlayerSkillHit();
                    GlobalEvents.InvokePlayerAttack(enemy.gameObject, attack, critical ? 3f : 1f, critical);
                    bool charm = player.HandInHandRelicActive && slot == 2;
                    if (charm) receiver.DamageFromPlayerAsCharm(attack, 0f, 0f, critical, player.AddConfusionDamagePercent, player.HandInHandCharmEffect);
                    else receiver.DamageFromPlayer(attack, 0f, 0f, critical, player.AddConfusionDamagePercent);
                }
            }
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


// Fallback visuals are generated at runtime; artists can replace both prefab slots.
public static class DivineJudgementVisual
{
    static Sprite white;
    static Sprite White
    {
        get
        {
            if (white != null) return white;
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            return white;
        }
    }

    static GameObject Rect(string name, Transform parent, Vector3 position, Vector3 scale, Color color, int order)
    {
        GameObject go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = White;
        sr.color = color;
        sr.sortingOrder = order;
        return go;
    }

    public static GameObject CreateCross()
    {
        GameObject root = new GameObject("DivineJudgement_Cross");
        Color gold = new Color(1f, 0.88f, 0.35f, 0.95f);
        Rect("VerticalGlow", root.transform, Vector3.zero, new Vector3(0.16f, 0.72f, 1f), new Color(1f, 0.75f, 0.15f, 0.35f), 201);
        Rect("Vertical", root.transform, Vector3.zero, new Vector3(0.065f, 0.65f, 1f), gold, 202);
        Rect("Horizontal", root.transform, new Vector3(0f, 0.10f, 0f), new Vector3(0.35f, 0.07f, 1f), gold, 202);
        return root;
    }

    public static void CreateImpact(Vector3 position)
    {
        GameObject root = new GameObject("DivineJudgement_Impact");
        root.transform.position = position;
        Rect("OuterBeam", root.transform, new Vector3(0, 2.3f, 0), new Vector3(1.35f, 5f, 1), new Color(1f, 0.75f, 0.15f, 0.38f), 190);
        Rect("Beam", root.transform, new Vector3(0, 2.3f, 0), new Vector3(0.85f, 5f, 1), new Color(1f, 0.93f, 0.55f, 0.8f), 191);
        Rect("Core", root.transform, new Vector3(0, 2.3f, 0), new Vector3(0.35f, 5f, 1), Color.white, 192);
        Rect("Impact", root.transform, Vector3.zero, new Vector3(1.8f, 0.28f, 1), new Color(1f, 0.87f, 0.4f, 0.8f), 193);
        Object.Destroy(root, 0.22f);
    }
}
