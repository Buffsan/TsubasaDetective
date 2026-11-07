using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public weapondata weapondata;
    public SkillCardData skillcarddata;
    public int AttackLevel = 0;
    [System.NonSerialized] public int SourceSkillSlotIndex = -1;

    


    public bool isCriticulUse = true;

     float Level_AttackBONUS_multiplier = 50;
     float Level_AttackBONUS = 0.5f;
    PlayerController controller => PlayerController.Instance;
    public AudioManager audioManager => AudioManager.instance;

    public bool AgeinAttack = false;

    // Optional buff granted once per valid enemy hit (including pooled attacks).
    [SerializeField] public BuffData buffOnEnemyHit;

    // 攻撃判定1回につき敵本体単位で重複命中を防ぐ。
    // AgeinAttack が true の攻撃は、従来どおり再入場で再ヒット可能。
    readonly HashSet<GameObject> EnemyData = new HashSet<GameObject>();

    private void OnEnable()
    {
        // AttackPool の再利用時に前の攻撃の命中履歴を引き継がない。
        EnemyData.Clear();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Player attacks can start while already overlapping the orbiting harpoon.
        TryChargeHarpoon(other);
    }

    bool TryChargeHarpoon(Collider2D other)
    {
        Relic_ReturnlessHarpoon harpoon = other.GetComponent<Relic_ReturnlessHarpoon>();
        if (harpoon == null) return false;
        if (controller == null || weapondata == null || !harpoon.CanCharge) return true;
        if (!AgeinAttack && EnemyData.Contains(harpoon.gameObject)) return true;

        float damage = AttackMathf();
        if (isCriticulUse && controller.AddCritical + controller.Critical > Random.Range(0f, 100f))
            damage = (damage * 3f + 5f) * Mathf.Max(0f, 1f + controller.AddLuckyDamagePercent / 100f);
        if (harpoon.ChargeFromPlayer(damage))
        {
            if (!AgeinAttack) EnemyData.Add(harpoon.gameObject);
            controller.HitEffectSpawn(other.transform.position);
        }
        return true;
    }

    bool IsPlayerSkillAttack()
    {
        if (SourceSkillSlotIndex >= 0) return true;
        // Legacy skill prefabs may not have the origin marker. Only accept
        // their card if it is currently equipped by this player.
        if (skillcarddata == null || controller == null) return false;
        foreach (SkillInfo info in controller.skillINFO)
        {
            if (info != null && info.skillDATA == skillcarddata)
                return true;
        }
        return false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (TryChargeHarpoon(other)) return;
        if (other.gameObject.CompareTag("EnemyArea")) 
        { 
        IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null && controller != null)
            {
                charaDamage damageArea = damageable as charaDamage;
                if (damageArea != null &&
                    (damageArea.OwnerEnemyBase == null ||
                     damageArea.OwnerEnemyBase.status == EnemyBase.Status.Die ||
                     damageArea.OwnerEnemyBase.HP <= 0f ||
                     damageArea.OwnerEnemyBase.IsCharmed))
                    return;

                if (!AgeinAttack)
                {
                    // 敵が複数の EnemyArea を持つ場合も同じ本体への1ヒットとして扱う。
                    charaDamage area = damageable as charaDamage;
                    GameObject target = area != null && area.OwnerEnemyBase != null
                        ? area.OwnerEnemyBase.gameObject : other.gameObject;
                    if (!EnemyData.Add(target)) return;
                }

                if(weapondata.HitAudio) audioManager.PlaySE(weapondata.HitAudio);
                float criticalRandom = Random.Range(0f, 100f);
                bool isSkillHit = IsPlayerSkillAttack();
                if (isSkillHit)
                    GlobalEvents.InvokePlayerSkillHit();
                float attackValue = AttackMathf(isSkillHit); // 同一命中では1回だけ計算
                // Qスロット(index 2)にセットしたカードからの攻撃を魅了攻撃に変える。
                bool applyCharm = controller.HandInHandRelicActive &&
                    (SourceSkillSlotIndex == 2 ||
                     (SourceSkillSlotIndex < 0 &&
                      skillcarddata != null && controller.skillINFO.Count > 2 &&
                      controller.skillINFO[2].skillDATA == skillcarddata));
                if (controller.AddCritical + controller.Critical <= criticalRandom || !isCriticulUse)
                {
                    GlobalEvents.InvokePlayerAttack(other.gameObject, attackValue, 1, false);
                    SpawnStatusEffect(damageable);
                    DealDamage(damageable, attackValue, false, applyCharm);
                }
                else
                {
                    GlobalEvents.InvokePlayerAttack(other.gameObject, attackValue, 3f, true);
                    SpawnStatusEffect(damageable);
                    float luckyMultiplier = Mathf.Max(0f, 1f + controller.AddLuckyDamagePercent / 100f);
                    // Apply luck after both the 3x attack scaling and +5 flat damage.
                    DealDamage(damageable, ((attackValue * 3f) + 5f) * luckyMultiplier, true, applyCharm);
                }
                if (buffOnEnemyHit != null)
                    controller.playerBuff.SpawnBuff(buffOnEnemyHit);
                controller.HitEffectSpawn(other.transform.position);
            }
        }
    }

    void DealDamage(IDamageable damageable, float damage, bool critical, bool applyCharm)
    {
        if (damageable is charaDamage enemy)
        {
            if (applyCharm)
                enemy.DamageFromPlayerAsCharm(damage, weapondata.StaggerPower, weapondata.ConfusionPower,
                    critical, controller.AddConfusionDamagePercent, controller.HandInHandCharmEffect);
            else
                enemy.DamageFromPlayer(damage, weapondata.StaggerPower, weapondata.ConfusionPower,
                    critical, controller.AddConfusionDamagePercent);
        }
        else if (!applyCharm)
            damageable.Damage(damage, weapondata.StaggerPower, weapondata.ConfusionPower, critical);
    }

    void SpawnStatusEffect(IDamageable damageable) 
    {
        if (weapondata.statusEffects.Count <= 0) return;

        foreach (var effect in weapondata.statusEffects) 
        {

            damageable.SpawnBuff(effect);
        
        }
    }

    float AttackMathf(bool applySkillOnlyBuff = false)
    {
        if (skillcarddata != null)
        {
            foreach (var skillinfo in controller.skillINFO)
            {
                if (skillinfo.skillDATA == skillcarddata)
                {
                    AttackLevel = skillinfo.SkillLevel - 1;
                    break;
                }
            }
        }
        return CalculateWeaponDamage(weapondata, controller, AttackLevel, applySkillOnlyBuff);
    }

    // Standard player-attack calculation, shared with targeted skill attacks
    // such as DivineJudgement (which do not create a trigger collider).
    // attackLevel is the number of upgrades after the first card level.
    public static float CalculateWeaponDamage(
        weapondata attackData, PlayerController player, int attackLevel, bool applySkillOnlyBuff)
    {
        if (attackData == null || player == null || player.playerBuff == null)
            return 0f;

        float levelBonusDamage = 0.5f * attackLevel;
        float levelBonusMultiplier = 50f * attackLevel;
        float buffMultiplier = player.playerBuff.MultiplyAttack_AllBuff;
        if (applySkillOnlyBuff)
            buffMultiplier += player.playerBuff.SkillOnlyAttackMultiplierBonus;

        return ((attackData.Attack + player.ATK + levelBonusDamage)
                * ((100f + player.AddATK + levelBonusMultiplier) / 100f)
                + player.playerBuff.Attack_AllBuff)
            * player.AddMultiplierATK * buffMultiplier;
    }

}
