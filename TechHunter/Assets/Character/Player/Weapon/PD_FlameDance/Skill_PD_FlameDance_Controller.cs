using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class Skill_PD_FlameDance_Controller : MonoBehaviour
{
    public static Skill_PD_FlameDance_Controller Active { get; private set; }

    [Header("Duration")]
    [SerializeField, Min(0.01f)] float Duration = 5f;

    [Header("Prefabs (visual explosion MUST NOT deal damage itself)")]
    [SerializeField] GameObject FirePrefab;
    [SerializeField] GameObject ExplosionPrefab;
    [SerializeField] AudioClip clip;

    [SerializeField] AudioManager audioManager => AudioManager.instance;
    ALL_SystemManager aLL_System => ALL_SystemManager.Instance;
    

    [Header("Active skill effect (ParticleSystem prefab)")]
    [SerializeField] GameObject ActiveEffectPrefab;

    [Header("Trail")]
    [SerializeField, Min(0.05f)] float FireSpacing = 0.35f;
    [SerializeField, Min(0f)] float FireSpawnInterval = 0.04f;
    [SerializeField, Min(1)] int MaxFireCount = 80;

    [Header("Final explosion - one hit per enemy")]
    [SerializeField, Min(0.01f)] float ExplosionRadius = 0.9f;
    [SerializeField] LayerMask ExplosionTargetLayers = ~0;
    [SerializeField, Min(0f)] float ExplosionDamage = 20f;
    [SerializeField, Min(0f)] float ExplosionStun = 0f;
    [SerializeField, Min(0f)] float ExplosionConfusion = 0f;
    [SerializeField] bool ExplosionCritical = false;
    [SerializeField, Min(0f)] float ExplosionLifeTime = 1f;
    [SerializeField] SkillCardData skillCardData;

    PlayerController player;
    Skill_PD_FlameDance_EffectCleanup activeEffect;
    float remaining;
    bool previousDodge;
    int previousDodgeSerial;
    float timeSinceFire;
    Vector2 lastFirePosition;
    readonly List<Vector2> firePositions = new List<Vector2>();
    readonly List<GameObject> fires = new List<GameObject>();
    bool initialized;
    bool finished;

    public void Initialize(PlayerController target)
    {
        if (target == null) { Destroy(gameObject); return; }
        if (Active != null && Active != this)
        {
            Active.Refresh();
            Destroy(gameObject);
            return;
        }

        Active = this;
        player = target;
        initialized = true;
        previousDodge = false;
        previousDodgeSerial = player.FlameDanceDodgeSerial;
        Refresh();
        SpawnActiveEffect();
    }

    void SpawnActiveEffect()
    {
        if (ActiveEffectPrefab == null || player == null) return;

        GameObject effect = Instantiate(ActiveEffectPrefab, player.transform);
        effect.transform.localPosition = Vector3.zero;
        effect.transform.localRotation = Quaternion.identity;

        activeEffect = effect.GetComponent<Skill_PD_FlameDance_EffectCleanup>();
        if (activeEffect == null)
            activeEffect = effect.AddComponent<Skill_PD_FlameDance_EffectCleanup>();

        activeEffect.Play();
    }

    void StopActiveEffect()
    {
        if (activeEffect == null) return;
        activeEffect.StopAndDestroy();
        activeEffect = null;
    }

    public void Refresh()
    {
        remaining = Mathf.Max(0.01f, Duration);
        if (player != null) player.FlameDanceActive = true;
    }

    void Update()
    {
        if (!initialized || finished) return;
        if (player == null) { Finish(false); return; }

        remaining -= Time.deltaTime;
        if (remaining <= 0f) Finish(true);
    }

    void FixedUpdate()
    {
        if (!initialized || finished || player == null) return;

        bool dodging = player.movetype == PlayerController.MoveType.Dodge;
        int dodgeSerial = player.FlameDanceDodgeSerial;
        Vector2 now = player.transform.position;

        // A second dodge can begin before the first one ends. The serial detects that.
        if (dodging && (!previousDodge || dodgeSerial != previousDodgeSerial))
        {
            timeSinceFire = 0f;
            lastFirePosition = now;
            SpawnFire(now);
        }
        else if (dodging)
        {
            timeSinceFire += Time.fixedDeltaTime;
            if (timeSinceFire >= FireSpawnInterval &&
                Vector2.Distance(lastFirePosition, now) >= Mathf.Max(0.05f, FireSpacing))
            {
                SpawnFire(now);
                lastFirePosition = now;
                timeSinceFire = 0f;
            }
        }

        previousDodge = dodging;
        previousDodgeSerial = dodgeSerial;
    }

    void SpawnFire(Vector2 position)
    {
        if (FirePrefab == null || firePositions.Count >= MaxFireCount) return;

        GameObject flame = Instantiate(FirePrefab, position, Quaternion.identity);
        firePositions.Add(position);
        fires.Add(flame);
    }

    void Finish(bool explode)
    {
        if (finished) return;
        finished = true;

        if (Active == this)
        {
            Active = null;
            if (player != null) player.FlameDanceActive = false;
        }

        StopActiveEffect();

        if (explode) ExplodeAll();

        // Remove all trail flames when the skill ends, alongside the final explosions.
        foreach (GameObject fire in fires)
            if (fire != null) Destroy(fire);
        fires.Clear();
        Destroy(gameObject);
    }

    void ExplodeAll()
    {
        // All explosion circles share this hit history. Damage is applied only once
        // per enemy, regardless of the number of overlapping explosion circles.
        var hitEnemies = new HashSet<EnemyBase>();
        var hitOtherTargets = new HashSet<Component>();

        foreach (Vector2 position in firePositions)
        {
            if (ExplosionPrefab != null)
            {
                GameObject visual = Instantiate(ExplosionPrefab, position, Quaternion.identity);
                if (ExplosionLifeTime > 0f) Destroy(visual, ExplosionLifeTime);
            }

            Collider2D[] overlaps = Physics2D.OverlapCircleAll(
                position, ExplosionRadius, ExplosionTargetLayers);

            foreach (Collider2D targetCollider in overlaps)
            {
                if (!targetCollider.CompareTag("EnemyArea")) continue;

                IDamageable damageable = targetCollider.GetComponent<IDamageable>();
                if (damageable == null) continue;

                // Multiple hurtboxes on one enemy count as a single target.
                EnemyBase enemy = targetCollider.GetComponentInParent<EnemyBase>();
                if (enemy != null)
                {
                    if (!hitEnemies.Add(enemy)) continue;
                }
                else
                {
                    Component component = damageable as Component;
                    if (component == null || !hitOtherTargets.Add(component)) continue;
                }

                audioManager.PlaySE(clip);
                aLL_System.camera_Controller.Shake(0.3f,0.1f);
               

                damageable.Damage(AttackMathf(), ExplosionStun,
                    ExplosionConfusion, ExplosionCritical);
            }
        }
    }
    float AttackMathf()
    {
        float AttackAll;
        float Level_BONUS_Damage = 0;
        float Level_BONUS_Damage_multiply = 0;
        int AttackLevel = 0;

        if (skillCardData != null)
        {
            foreach (var skillinfo in aLL_System.playerController.skillINFO)
            {
                if (skillinfo.skillDATA == skillCardData)
                {
                    AttackLevel = skillinfo.SkillLevel - 1;

                    Level_BONUS_Damage = 0.5f * AttackLevel;
                    Level_BONUS_Damage_multiply = 50 * AttackLevel;

                    break;
                }
            }
        }
        AttackAll = ((ExplosionDamage + aLL_System.playerController.ATK + Level_BONUS_Damage) * ((100 + aLL_System.playerController.AddATK + Level_BONUS_Damage_multiply) / 100) + aLL_System.playerController.playerBuff.Attack_AllBuff) * aLL_System.playerController.AddMultiplierATK * aLL_System.playerController.playerBuff.MultiplyAttack_AllBuff;
        return AttackAll;
    }
    void OnDestroy()
    {
        StopActiveEffect();

        // Also clean up flames if the controller is forcibly destroyed.
        foreach (GameObject fire in fires)
            if (fire != null) Destroy(fire);
        fires.Clear();

        if (Active == this)
        {
            Active = null;
            if (player != null) player.FlameDanceActive = false;
        }
    }
}
