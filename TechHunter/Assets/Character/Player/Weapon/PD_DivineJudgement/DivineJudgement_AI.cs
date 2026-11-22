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
    [Header("攻撃ステータス")]
    [Tooltip("光柱1発の攻撃力、スタン、混乱、状態異常を管理する既存のweapondata SO")]
    public weapondata weapondata;

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
    [Tooltip("光柱の着弾地点を中心とした矩形攻撃範囲（Unity単位）")]
    [SerializeField] Vector2 strikeAreaSize = new Vector2(1.5f, 1.5f);
    [SerializeField, Min(0f)] float strikeInterval = 0.12f;
    [SerializeField, Min(0f)] float crownRadius = 0.65f;
    [SerializeField, Min(0f)] float crownHeight = 0.8f;

    [Header("十字架の付与演出")]
    [SerializeField] AudioClip markSE;
    [SerializeField, Min(0.01f)] float crossAppearDuration = 0.3f;
    [SerializeField, Min(1f)] float crossStartScale = 1.6f;
    [SerializeField, Range(0f, 1f)] float crossFinalOpacity = 0.5f;

    [Header("光柱のサイズ")]
    [Tooltip("1 = プレハブ本来の高さ、2 = 2倍、0.5 = 半分。光柱の幅は変更しません。")]
    [SerializeField, Min(0.01f)] float pillarHeightMultiplier = 1f;

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

        // Preserve the last valid world position after the target dies,
        // is charmed, despawns or is destroyed. The reservation outlives it.
        public Vector3 lastTargetPosition;
        public bool hasLastTargetPosition;

        public void RememberTargetPosition()
        {
            if (target == null || !target.isActiveAndEnabled ||
                target.HP <= 0f || target.status == EnemyBase.Status.Die ||
                target.IsCharmed)
                return;

            lastTargetPosition = target.transform.position;
            hasLastTargetPosition = true;
        }
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
            if (mark.pillar != null) continue;
            mark.RememberTargetPosition();
            if (!mark.hasLastTargetPosition) continue;

            float beamStartTime = chantDuration + i * strikeInterval - pillarLeadTime;
            if (elapsed < beamStartTime) continue;

            // Even if the original enemy has died, its reserved location
            // still gets a pillar, and nearby enemies can take AoE damage.
            Vector2 scatter = Random.insideUnitCircle * impactScatterRadius;
            mark.impactPosition = mark.lastTargetPosition +
                new Vector3(scatter.x, scatter.y, 0f);
            mark.hasImpactPosition = true;
            mark.pillar = DivineJudgementVisual.SpawnPillar(
                lightPillarAttackPrefab, mark.impactPosition, pillarHeightMultiplier);
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
        // Cache the position immediately, before death/despawn can happen.
        Mark mark = new Mark
        {
            target = target,
            lastTargetPosition = target.transform.position,
            hasLastTargetPosition = true
        };
        marks.Add(mark);
        mark.visual = crossPrefab != null ? Instantiate(crossPrefab) : DivineJudgementVisual.CreateCross();
        if (mark.visual != null)
        {
            // Start larger and fully transparent. Over 0.3s shrink into the
            // regular crown placement while fading to half opacity.
            DivineJudgementCrossAppear animation = mark.visual.GetComponent<DivineJudgementCrossAppear>();
            if (animation == null) animation = mark.visual.AddComponent<DivineJudgementCrossAppear>();
            animation.Initialize(crossAppearDuration, crossStartScale, crossFinalOpacity);
        }
        UpdateCrown(target);
        // Up to 20 marks are placed in 4 seconds, and some occur less than
        // AudioManager.minInterval apart. Play each assignment through the
        // existing audio pool so rapid marks do not lose their paper-tear SE.
        if (markSE != null)
        {
            if (AudioPool.Instance != null)
            {
                AudioSource source = AudioPool.Instance.GetSource();
                AudioPlay sound = source != null ? source.GetComponent<AudioPlay>() : null;
                if (sound != null) sound.Play(markSE);
                else if (source != null) AudioPool.Instance.Release(source);
            }
            else if (AudioManager.instance != null)
                AudioManager.instance.PlaySE(markSE);
        }
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
        {
            mark.RememberTargetPosition();
            if (mark.target != null && mark.target.isActiveAndEnabled &&
                mark.target.HP > 0f &&
                mark.target.status != EnemyBase.Status.Die &&
                !mark.target.IsCharmed &&
                mark.visual != null)
                targets.Add(mark.target);
        }
        // Only living targets move their crosses; marks on dead targets
        // remain at the last position until the scheduled pillar lands.
        foreach (EnemyBase target in targets) UpdateCrown(target);
    }

    void Fire()
    {
        if (fired) return;
        foreach (Mark mark in marks) mark.RememberTargetPosition();
        fired = true;
        // スキルオブジェクトが破棄されても神罰が最後まで発生するように独立させる。
        GameObject runnerObject = new GameObject("DivineJudgement_StrikeRunner");
        DivineJudgementStrikeRunner runner = runnerObject.AddComponent<DivineJudgementStrikeRunner>();
        runner.Begin(marks, lightPillarAttackPrefab, impactEffectPrefab,
            impactScatterRadius, strikeInterval, playerController, SkillLevel,
            SkillSlotIndex, strikeSE, pillarHeightMultiplier, weapondata,
            strikeAreaSize);
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
    float pillarHeightMultiplier;
    PlayerController player;
    int skillLevel;
    int slot;
    weapondata attackData;
    Vector2 strikeAreaSize;

    public void Begin(List<DivineJudgement_AI.Mark> assigned, GameObject prefab,
        GameObject impactEffect, float radius, float delay, PlayerController owner,
        int level, int sourceSlot, AudioClip firingSound, float heightMultiplier,
        weapondata skillWeaponData, Vector2 hitAreaSize)
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
        pillarHeightMultiplier = Mathf.Max(0.01f, heightMultiplier);
        attackData = skillWeaponData;
        strikeAreaSize = new Vector2(Mathf.Max(0.01f, hitAreaSize.x),
            Mathf.Max(0.01f, hitAreaSize.y));
        if (attackData == null)
            Debug.LogError("神罰のweapondataが未設定です。DivineJudgement_AIプレハブに武器データSOを割り当ててください。", this);
        if (pillarPrefab == null)
            Debug.LogError("神罰の光柱プレハブ参照がありません。", this);
        StartCoroutine(Strike());
    }

    void Update()
    {
        // Casting has ended, but later reserved strikes must continue to
        // remember moving targets until their death or despawn.
        foreach (DivineJudgement_AI.Mark mark in marks)
            mark.RememberTargetPosition();
    }

    System.Collections.IEnumerator Strike()
    {
        for (int i = 0; i < marks.Count; i++)
        {
            DivineJudgement_AI.Mark mark = marks[i];
            if (mark.visual != null) Destroy(mark.visual);

            // The reservation is independent of the target's lifetime.
            // A dead/destroyed target still produces its scheduled pillar
            // at the last known location, preserving the 1.5m AoE hit.
            if (!mark.hasImpactPosition)
            {
                mark.RememberTargetPosition();
                if (mark.hasLastTargetPosition)
                {
                    Vector2 scatter = Random.insideUnitCircle * scatterRadius;
                    mark.impactPosition = mark.lastTargetPosition +
                        new Vector3(scatter.x, scatter.y, 0f);
                    mark.hasImpactPosition = true;
                }
            }

            if (mark.hasImpactPosition)
            {
                if (mark.pillar == null && pillarPrefab != null)
                {
                    mark.pillar = DivineJudgementVisual.SpawnPillar(
                        pillarPrefab, mark.impactPosition, pillarHeightMultiplier);
                }

                // Once per pillar, independent of how many enemies it hits.
                if (impactPrefab != null)
                {
                    GameObject impact = Instantiate(impactPrefab, mark.impactPosition,
                        Quaternion.identity);
                    Destroy(impact, 5f);
                }
                if (strikeSE != null && AudioManager.instance != null)
                    AudioManager.instance.PlaySE(strikeSE);

                ApplyAreaDamage(mark.impactPosition);
            }

            if (interval > 0f) yield return new WaitForSeconds(interval);
            else yield return null;
        }
        Destroy(gameObject);
    }

    void ApplyAreaDamage(Vector3 impactPosition)
    {
        if (attackData == null || player == null || player.playerBuff == null) return;

        // Query the same EnemyArea trigger colliders used by PlayerAttack.
        // The box is centered on the scattered pillar impact, NOT on the
        // originally assigned enemy. Multiple enemy colliders count once.
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            (Vector2)impactPosition, strikeAreaSize, 0f);
        HashSet<EnemyBase> damaged = new HashSet<EnemyBase>();

        foreach (Collider2D hit in hits)
        {
            if (hit == null || !hit.gameObject.CompareTag("EnemyArea")) continue;
            charaDamage receiver = hit.GetComponent<charaDamage>();
            if (receiver == null) continue;

            EnemyBase enemy = receiver.OwnerEnemyBase;
            if (enemy == null || enemy.status == EnemyBase.Status.Die ||
                enemy.HP <= 0f || enemy.IsCharmed || !enemy.isActiveAndEnabled)
                continue;
            if (!damaged.Add(enemy)) continue;

            // Existing weapondata-based skill damage formula and effects.
            float attack = PlayerAttack.CalculateWeaponDamage(
                attackData, player, Mathf.Max(0, skillLevel - 1), true);
            bool critical = player.AddCritical + player.Critical >
                Random.Range(0f, 100f);

            GlobalEvents.InvokePlayerSkillHit();
            GlobalEvents.InvokePlayerAttack(enemy.gameObject, attack,
                critical ? 3f : 1f, critical);
            if (attackData.HitAudio != null && AudioManager.instance != null &&
                attackData.HitAudio != strikeSE)
                AudioManager.instance.PlaySE(attackData.HitAudio);
            if (attackData.statusEffects != null)
            {
                foreach (StatusEffectAttack effect in attackData.statusEffects)
                    if (effect != null) receiver.SpawnBuff(effect);
            }

            float actualDamage = critical
                ? (attack * 3f + 5f) *
                  Mathf.Max(0f, 1f + player.AddLuckyDamagePercent / 100f)
                : attack;
            bool charm = player.HandInHandRelicActive && slot == 2;
            if (charm)
                receiver.DamageFromPlayerAsCharm(actualDamage,
                    attackData.StaggerPower, attackData.ConfusionPower, critical,
                    player.AddConfusionDamagePercent, player.HandInHandCharmEffect);
            else
                receiver.DamageFromPlayer(actualDamage,
                    attackData.StaggerPower, attackData.ConfusionPower, critical,
                    player.AddConfusionDamagePercent);
            player.HitEffectSpawn(hit.transform.position);
        }
    }

    void OnDestroy()
    {
        foreach (DivineJudgement_AI.Mark mark in marks)
            if (mark.visual != null) Destroy(mark.visual);
    }
}


// Fallback visuals are generated at runtime; artists can replace both prefab slots.
/// <summary>
/// On attachment, animate the entire cross from 0% opacity and oversize down
/// to its normal scale and 50% opacity. The crown controller owns rotation/position.
/// </summary>
public class DivineJudgementCrossAppear : MonoBehaviour
{
    SpriteRenderer[] sprites;
    float[] originalAlpha;
    Vector3 finalScale;
    float duration;
    float startingScale;
    float targetOpacity;
    float elapsed;
    bool initialized;

    public void Initialize(float seconds, float scale, float opacity)
    {
        duration = Mathf.Max(0.01f, seconds);
        startingScale = Mathf.Max(1f, scale);
        targetOpacity = Mathf.Clamp01(opacity);
        finalScale = transform.localScale;
        sprites = GetComponentsInChildren<SpriteRenderer>(true);
        originalAlpha = new float[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
            originalAlpha[i] = sprites[i].color.a;
        elapsed = 0f;
        initialized = true;
        Apply(0f);
    }

    void Update()
    {
        if (!initialized || elapsed >= duration) return;
        elapsed = Mathf.Min(duration, elapsed + Time.deltaTime);
        float t = elapsed / duration;
        Apply(t * t * (3f - 2f * t));
    }

    void Apply(float t)
    {
        transform.localScale = finalScale * Mathf.Lerp(startingScale, 1f, t);
        float opacity = Mathf.Lerp(0f, targetOpacity, t);
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] == null) continue;
            Color color = sprites[i].color;
            color.a = originalAlpha[i] * opacity;
            sprites[i].color = color;
        }
    }
}

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

    // Scale the beam's HEIGHT through an independent parent rather than the
    // animated prefab root, whose Animator may overwrite localScale.
    public static GameObject SpawnPillar(GameObject prefab, Vector3 position, float heightMultiplier)
    {
        if (prefab == null) return null;
        GameObject holder = new GameObject("DivineJudgement_Pillar");
        holder.transform.position = position;
        GameObject beam = Object.Instantiate(prefab, position, Quaternion.identity);
        beam.transform.SetParent(holder.transform, true);
        holder.transform.localScale = new Vector3(1f, Mathf.Max(0.01f, heightMultiplier), 1f);
        Object.Destroy(holder, 5f);
        return holder;
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
