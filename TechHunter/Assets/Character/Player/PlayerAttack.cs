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

    // 攻撃判定1回につき敵本体単位で重複命中を防ぐ。
    // AgeinAttack が true の攻撃は、従来どおり再入場で再ヒット可能。
    readonly HashSet<GameObject> EnemyData = new HashSet<GameObject>();

    private void OnEnable()
    {
        // AttackPool の再利用時に前の攻撃の命中履歴を引き継がない。
        EnemyData.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
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
                float attackValue = AttackMathf(); // 同一命中では1回だけ計算
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

    float AttackMathf() 
    {
        float AttackAll;
        float Level_BONUS_Damage = 0;
        float Level_BONUS_Damage_multiply = 0;

        if (skillcarddata != null) 
        {
            foreach (var skillinfo in controller.skillINFO) 
            {
                if (skillinfo.skillDATA == skillcarddata) 
                {
                    AttackLevel = skillinfo.SkillLevel -1;

                    Level_BONUS_Damage = Level_AttackBONUS * AttackLevel;
                    Level_BONUS_Damage_multiply = Level_AttackBONUS_multiplier * AttackLevel;

                    break;
                }
            }
        }

        AttackAll = ((weapondata.Attack + controller.ATK + Level_BONUS_Damage) * ((100 + controller.AddATK + Level_BONUS_Damage_multiply) / 100) + controller.playerBuff.Attack_AllBuff) * controller.AddMultiplierATK * controller.playerBuff.MultiplyAttack_AllBuff;
       return AttackAll;
    }

}
