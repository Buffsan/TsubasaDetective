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
    [SerializeField] GameObject chantEffectPrefab;
    [SerializeField] AudioClip chantSE;
    [SerializeField] AudioClip strikeSE;
    [SerializeField, Min(0f)] float pillarLeadTime = 1f;
    [SerializeField, Range(0f, 1f)] float chantMoveMultiplier = 0.1f;
    [SerializeField] GameObject impactEffectPrefab;
    [SerializeField, Min(0f)] float impactScatterRadius = 1f;
    [SerializeField, Min(0f)] float strikeInterval = 0.12f;
    [SerializeField, Min(0f)] float crownRadius = 0.65f;
    [SerializeField, Min(0f)] float crownHeight = 0.8f;

    readonly List<Mark> marks = new List<Mark>();
    float elapsed;
    int issued;
    bool fired;
    PlayerSkillBase ownSkill;
    GameObject chantEffectInstance;

    public class Mark
    {
        public EnemyBase target;
        public GameObject visual;
        public GameObject pillar;
        public Vector3 impactPosition;
        public bool hasImpactPosition;
    }

    void Start()
    {
        ownSkill = this;

        // Spawn one chanting effect at the player and keep it attached during charging.
        if (chantEffectPrefab != null && playerController != null)
        {
            Transform owner = playerController.transform;
            chantEffectInstance = Instantiate(chantEffectPrefab, owner.position, Quaternion.identity);
            chantEffectInstance.transform.SetParent(owner, true);
            chantEffectInstance.transform.localPosition = Vector3.zero;
        }
        else if (chantEffectPrefab == null)
            Debug.LogWarning("神罰の詠唱エフェクトが未設定です。", this);

        // This is a dedicated, inspector-configurable chant SE, played once on start.
        if (chantSE != null && AudioManager.instance != null)
            AudioManager.instance.PlaySE(chantSE);
    }

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
        // Consistent with existing chant skills (e.g. SS_StarSmog).
        PlayerMove(chantMoveMultiplier);
        elapsed = Mathf.Min(chantDuration, elapsed + Time.deltaTime);
        float x = elapsed / chantDuration;
        int desired = Mathf.Min(maxMarks, Mathf.FloorToInt(maxMarks * 0.5f * (x * x + x * x * x) + 0.00001f));
        while (issued < desired)
        {
            EnemyBase target = FindNearestEnemy();
            if (target == null) break; // 敵がいない間は付与せず、次のフレームで再試行する。
            issued++;
            AddMark(target);
        }
        PrewarmPillars();
        if (elapsed >= chantDuration) Fire();
    }

    // Each strike keeps its original damage time (chant ends + index * interval).
    // The beam starts one second earlier whenever the mark already exists.
    // A newly assigned mark or an early interruption may have less preparation time.
    void PrewarmPillars()
    {
        if (lightPillarAttackPrefab == null) return;
        for (int i = 0; i < marks.Count; i++)
        {
            Mark mark = marks[i];
            if (mark.pillar != null || mark.target == null || mark.target.HP <= 0f ||
                mark.target.status == EnemyBase.Status.Die || mark.target.IsCharmed)
                continue;

            float beamStartTime = chantDuration + i * strikeInterval - pillarLeadTime;
            if (elapsed < beamStartTime) continue;

            Vector2 scatter = Random.insideUnitCircle * impactScatterRadius;
            Vector3 targetPosition = mark.target.transform.position;
            mark.impactPosition = targetPosition + new Vector3(scatter.x, scatter.y, 0f);
            mark.hasImpactPosition = true;
            mark.pillar = Instantiate(lightPillarAttackPrefab, mark.impactPosition, Quaternion.identity);
            Destroy(mark.pillar, 5f);
        }
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
        runner.Begin(marks, lightPillarAttackPrefab, impactEffectPrefab,
            impactScatterRadius, strikeInterval, playerController, SkillLevel,
            SkillSlotIndex, strikeSE);
        end();
    }

    void OnDestroy()
    {
        if (chantEffectInstance != null) Destroy(chantEffectInstance);
        if (fired) return;
        foreach (Mark mark in marks)
            if (mark.visual != null) Destroy(mark.visual);
    }
}

/// <summary>神罰を詠唱オブジェクトの寿命から独立して連続生成する。</summary>
public class DivineJudgementStrikeRunner : MonoBehaviour
{
    readonly List<DivineJudgement_AI.Mark> marks = new List<DivineJudgement_AI.Mark>();
    GameObject pillarPrefab;
    GameObject impactPrefab;
    AudioClip strikeSE;
    float scatterRadius;
    float interval;
    PlayerController player;
    int skillLevel;
    int slot;
    [SerializeField] float baseDamage = 20f;

    public void Begin(List<DivineJudgement_AI.Mark> assigned, GameObject prefab,
        GameObject impactEffect, float radius, float delay, PlayerController owner,
        int level, int sourceSlot, AudioClip firingSound)
    {
        marks.AddRange(assigned);
        pillarPrefab = prefab;
        impactPrefab = impactEffect;
        scatterRadius = Mathf.Max(0f, radius);
        interval = delay;
        player = owner;
        skillLevel = level;
        slot = sourceSlot;
        strikeSE = firingSound;
        if (pillarPrefab == null)
            Debug.LogError("神罰の光柱プレハブ参照がありません。", this);
        StartCoroutine(Strike());
    }

    System.Collections.IEnumerator Strike()
    {
        for (int i = 0; i < marks.Count; i++)
        {
            DivineJudgement_AI.Mark mark = marks[i];
            if (mark.visual != null) Destroy(mark.visual);
            EnemyBase enemy = mark.target;
            if (enemy != null && enemy.HP > 0 && enemy.status != EnemyBase.Status.Die &&
                !enemy.IsCharmed)
            {
                // Prewarmed pillars retain their actual impact position.
                // On early interruption, create missing pillars immediately,
                // rather than delaying damage or changing the original strike schedule.
                if (!mark.hasImpactPosition)
                {
                    Vector2 scatter = Random.insideUnitCircle * scatterRadius;
                    mark.impactPosition = enemy.transform.position +
                        new Vector3(scatter.x, scatter.y, 0f);
                    mark.hasImpactPosition = true;
                }
                if (mark.pillar == null && pillarPrefab != null)
                {
                    mark.pillar = Instantiate(pillarPrefab, mark.impactPosition,
                        Quaternion.identity);
                    Destroy(mark.pillar, 5f);
                }

                charaDamage receiver = ResolveDamageReceiver(enemy);
                if (receiver != null && player != null)
                {
                    // Impact visuals and gunshot SE are tied to the damage frame,
                    // not to the earlier beam appearance.
                    if (impactPrefab != null)
                    {
                        GameObject impact = Instantiate(impactPrefab, mark.impactPosition,
                            Quaternion.identity);
                        Destroy(impact, 5f);
                    }
                    if (strikeSE != null && AudioManager.instance != null)
                        AudioManager.instance.PlaySE(strikeSE);

                    float levelBonus = Mathf.Max(0, skillLevel - 1);
                    float buff = player.playerBuff != null
                        ? player.playerBuff.MultiplyAttack_AllBuff +
                          player.playerBuff.SkillOnlyAttackMultiplierBonus : 1f;
                    float attack = ((baseDamage + player.ATK + 0.5f * levelBonus) *
                        ((100f + player.AddATK + 50f * levelBonus) / 100f) +
                        (player.playerBuff != null
                            ? player.playerBuff.Attack_AllBuff : 0f)) *
                        player.AddMultiplierATK * buff;
                    bool critical = player.AddCritical + player.Critical >
                        Random.Range(0f, 100f);
                    if (critical)
                        attack = (attack * 3f + 5f) *
                            Mathf.Max(0f, 1f + player.AddLuckyDamagePercent / 100f);

                    GlobalEvents.InvokePlayerSkillHit();
                    GlobalEvents.InvokePlayerAttack(enemy.gameObject, attack,
                        critical ? 3f : 1f, critical);
                    bool charm = player.HandInHandRelicActive && slot == 2;
                    if (charm)
                        receiver.DamageFromPlayerAsCharm(attack, 0f, 0f, critical,
                            player.AddConfusionDamagePercent, player.HandInHandCharmEffect);
                    else
                        receiver.DamageFromPlayer(attack, 0f, 0f, critical,
                            player.AddConfusionDamagePercent);
                }
            }
            if (interval > 0f) yield return new WaitForSeconds(interval);
            else yield return null;
        }
        Destroy(gameObject);
    }

    static charaDamage ResolveDamageReceiver(EnemyBase enemy)
    {
        if (enemy == null) return null;
        if (enemy.CharaDamage != null && enemy.CharaDamage.OwnerEnemyBase == enemy)
            return enemy.CharaDamage;
        charaDamage charmReceiver = enemy.CharmStatusReceiver;
        if (charmReceiver != null && charmReceiver.OwnerEnemyBase == enemy)
            return charmReceiver;
        foreach (charaDamage area in enemy.GetComponentsInChildren<charaDamage>(true))
        {
            if (area != null && area.OwnerEnemyBase == enemy && area.DamageType == 0)
                return area;
        }
        foreach (charaDamage area in enemy.GetComponentsInChildren<charaDamage>(true))
        {
            if (area != null && area.OwnerEnemyBase == enemy)
                return area;
        }
        return null;
    }

    void OnDestroy()
    {
        foreach (DivineJudgement_AI.Mark mark in marks)
            if (mark.visual != null) Destroy(mark.visual);
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


}
