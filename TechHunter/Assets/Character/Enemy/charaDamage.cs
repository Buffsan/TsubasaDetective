using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class charaDamage : MonoBehaviour, IDamageable
{
    [SerializeField] EnemyBase enemyBase;
    public EnemyBase OwnerEnemyBase => enemyBase;
    public Faction MyFactions => enemyBase != null ? enemyBase.Factions : Faction.None;
    [SerializeField] GameObject Coin;

    public List<charaDamage> passDamageAreas = new List<charaDamage>();
    public int DamageType = 0;
    //ダメージを自身に変換
    //ダメージをpassDamageAreas[0]に変換
    //ダメージをpassDamageAreas[x]全てに変換

    public List<ActiveBuff> buffs = new();

    //public float HP;
    public float ConfusionHP;
    public float DF;
    public float Stan;

    public float HP_percent;

    public float UP_Confusion;


    float StanCount = 0;
    float DieCount = 0;

    Vector2 SavePos;

    AudioManager audioManager => AudioManager.instance;

    float CameraShakeDertionNormal = 0.1f;
    float CameraShakeStrengthNormal = 0.1f;
    float CameraShakeDertionVeteran = 0.2f;
    float CameraShakeStrengthVeteran = 0.2f;
    float CameraShakeDertionElite = 0.2f;
    float CameraShakeStrengthElite = 0.4f;

    private void Start()
    {
        DF = enemyBase.ATKDF;
        Stan = enemyBase.StanHP;
        ConfusionHP = enemyBase.ConfusionHP;

        enemyBase.DefaltColor = enemyBase.spriteRenderer.color;
    }

    private void Update()
    {
        UpdateBuffs();
    }

    public void SpawnBuff(StatusEffectAttack buffData)
    {
        if (buffData == null) return;
        ApplyStatusEffect(buffData, buffData.number);
    }

    // 毒・感電・魅了すべてをActiveBuffで管理。amountは今回の魅了蓄積値。
    public void ApplyStatusEffect(StatusEffectAttack buffData, float amount)
    {
        if (buffData == null) return;
        bool isCharm = buffData.effecttype == StatusEffect.StatusEffectType.Charm;
        if (isCharm && enemyBase != null)
        {
            // 敵が複数のEnemyAreaを持っていても魅了値は本体の受け口へ一元化する。
            charaDamage primary = enemyBase.CharaDamage != null
                ? enemyBase.CharaDamage : enemyBase.charaDamage;
            if (primary != null && primary != this)
            {
                if (primary.enemyBase == null) primary.enemyBase = enemyBase;
                primary.ApplyStatusEffect(buffData, amount);
                return;
            }
        }
        if (isCharm && (enemyBase == null || enemyBase.IsCharmed ||
                        enemyBase.status == EnemyBase.Status.Die || enemyBase.HP <= 0f))
            return;

        foreach (ActiveBuff active in buffs)
        {
            if (active.statuseffectdata == null ||
                active.statuseffectdata.effecttype != buffData.effecttype)
                continue;

            if (isCharm)
            {
                active.accumulatedValue += Mathf.Max(0f, amount);
                TryCompleteCharm(active);
            }
            else
            {
                // 毒・感電は従来の時間加算方式。
                active.remainTime += buffData.Duration;
            }
            return;
        }

        ActiveBuff activeBuff = new ActiveBuff(buffData);
        if (buffData.effecttype != StatusEffect.StatusEffectType.None)
        {
            activeBuff.audioClips = buffData.audioClips;
            GameObject effectPrefab = null;
            switch (buffData.effecttype)
            {
                case StatusEffect.StatusEffectType.Poison:
                    effectPrefab = buffData.PoisonEffectPrefab;
                    break;
                case StatusEffect.StatusEffectType.ElectricShock:
                case StatusEffect.StatusEffectType.Charm:
                    effectPrefab = buffData.EffectObject;
                    break;
            }
            if (effectPrefab != null)
            {
                activeBuff.effectObject = Instantiate(effectPrefab, transform.position, Quaternion.identity);
                activeBuff.effectObject.transform.parent = transform;
            }
        }

        if (isCharm)
            activeBuff.accumulatedValue = Mathf.Max(0f, amount);
        buffs.Add(activeBuff);
        if (isCharm) TryCompleteCharm(activeBuff);
    }

    public float GetAccumulatedStatusValue(StatusEffect.StatusEffectType effectType)
    {
        foreach (ActiveBuff buff in buffs)
        {
            if (buff.statuseffectdata != null &&
                buff.statuseffectdata.effecttype == effectType)
                return buff.accumulatedValue;
        }
        return 0f;
    }

    void TryCompleteCharm(ActiveBuff charm)
    {
        if (enemyBase == null || enemyBase.IsCharmed || enemyBase.HP <= 0f)
            return;
        if (charm.accumulatedValue > enemyBase.HP)
            enemyBase.BecomeCharmedAlly();
    }

    // Each damage chain has a unique token: cycles cannot recurse indefinitely.
    static uint damageTraversalSerial;
    uint lastDamageTraversal;
    bool reportedDamageCycle;

    public void Damage(float Attackvalue, float Stanvalue, float ConfusionPower, bool Critical)
    {
        DamageWithModifier(Attackvalue, Stanvalue, ConfusionPower, Critical, 0f, null);
    }

    // Only player-owned hits provide this modifier; enemy-vs-enemy damage is unchanged.
    public void DamageFromPlayer(float attack, float stagger, float confusion,
        bool critical, float confusionDamagePercent)
    {
        DamageWithModifier(attack, stagger, confusion, critical, confusionDamagePercent, null);
    }

    // 遺物の魅了SOを使い、通常ダメージを状態異常の蓄積へ変換。
    public void DamageFromPlayerAsCharm(float attack, float stagger, float confusion,
        bool critical, float confusionDamagePercent, StatusEffectAttack charmEffect)
    {
        if (charmEffect == null ||
            charmEffect.effecttype != StatusEffect.StatusEffectType.Charm)
        {
            Debug.LogError("魅了用StatusEffectAttackが未設定です。", this);
            return;
        }
        DamageWithModifier(attack, stagger, confusion, critical, confusionDamagePercent, charmEffect);
    }

    void DamageWithModifier(float attack, float stagger, float confusion,
        bool critical, float confusionDamagePercent, StatusEffectAttack charmEffect)
    {
        uint token = ++damageTraversalSerial;
        if (token == 0) token = ++damageTraversalSerial;
        ForwardDamage(attack, stagger, confusion, critical, token,
            confusionDamagePercent, charmEffect);
    }

    void ForwardDamage(float attack, float stagger, float confusion, bool critical,
        uint token, float confusionDamagePercent, StatusEffectAttack charmEffect)
    {
        if (enemyBase == null || isDead || enemyBase.status == EnemyBase.Status.Die) return;
        if (lastDamageTraversal == token)
        {
            if (!reportedDamageCycle)
            {
                Debug.LogWarning("Damage forwarding cycle or duplicate path on " + name, this);
                reportedDamageCycle = true;
            }
            return;
        }
        lastDamageTraversal = token;

        switch (DamageType)
        {
            case 0:
                receiveDamage(attack, stagger, confusion, critical,
                    confusionDamagePercent, charmEffect);
                break;
            case 1:
                if (passDamageAreas.Count > 0 && passDamageAreas[0] != null)
                    passDamageAreas[0].ForwardDamage(attack, stagger, confusion, critical,
                        token, confusionDamagePercent, charmEffect);
                break;
            case 2:
                foreach (charaDamage receiver in passDamageAreas)
                    if (receiver != null)
                        receiver.ForwardDamage(attack, stagger, confusion, critical,
                            token, confusionDamagePercent, charmEffect);
                break;
        }
    }

    void isDieCameraShake(charadata data) 
    {

        switch (data.enemyStrength) 
        {

            case charadata.EnemyStrength.None:
                
                break;
            case charadata.EnemyStrength.Fodder:
                
                break;
            case charadata.EnemyStrength.Normal:
                enemyBase.ALL_System.camera_Controller.Shake(CameraShakeDertionNormal,CameraShakeStrengthNormal);
                break;
            case charadata.EnemyStrength.Veteran:
                enemyBase.ALL_System.camera_Controller.Shake(CameraShakeDertionNormal, CameraShakeStrengthNormal);
                break;
            case charadata.EnemyStrength.Elite:
                enemyBase.ALL_System.camera_Controller.Shake(CameraShakeDertionElite, CameraShakeStrengthElite);
                break;
            case charadata.EnemyStrength.Boss:
                
                break;

        }

    }

    void receiveDamage(float Attackvalue, float Stanvalue, float ConfusionPower, bool Critical,
        float confusionDamagePercent, StatusEffectAttack charmEffect)
    {
        float AddDamage = Attackvalue - DF;
        if (AddDamage < 1)
        {
            AddDamage = 1;
        }

        if (enemyBase.status == EnemyBase.Status.ConfusionResistance)
        {
            // Keep the existing 2x confused-state multiplier, then apply the
            // player's bonus after defense (not to the confusion buildup).
            float confusedDamage = AddDamage * 2f
                * Mathf.Max(0f, 1f + confusionDamagePercent / 100f);
            TakeDamage(-confusedDamage);
            isDamageTextPop(confusedDamage, 2, Critical);
        }
        else if (charmEffect == null)
        {
            TakeDamage(-AddDamage);
            isDamageTextPop(AddDamage, 1, Critical);
        }

        enemyBase.TakeDamageColor();
        ConfusionHP -= (Attackvalue * HP_percent) * ConfusionPower;
        if (1 - (ConfusionHP / enemyBase.ConfusionHP) > enemyBase.HP / enemyBase.MAXHP && enemyBase.status != EnemyBase.Status.ConfusionResistance)
        {//混乱

            audioManager.PlaySE(enemyBase.ConfusionAudio);
            enemyBase.status = EnemyBase.Status.ConfusionResistance;
            StanCount = 0;
            SavePos = enemyBase.gameObject.transform.position;

            GameObject CL_ConfusionText = Instantiate(enemyBase.ConfusionImageText, enemyBase.transform.position, Quaternion.identity);
            Destroy(CL_ConfusionText, 2);

        }
        else if (Stan < Stanvalue && enemyBase.status != EnemyBase.Status.ConfusionResistance)
        {
            enemyBase.status = EnemyBase.Status.Stan;
            StanCount = 0;
            SavePos = enemyBase.gameObject.transform.position;
        }

        if (charmEffect != null && enemyBase.HP > 0f)
            ApplyStatusEffect(charmEffect, AddDamage * Mathf.Max(0f, charmEffect.number));

        if (enemyBase.HP <= 0 && enemyBase.status != EnemyBase.Status.Die)
        {
            Death();
        }
    }

    void TakeDamage(float addDamage)
    {

        enemyBase.HP += addDamage;

    }

    void UpdateBuffs()
    {
        for (int i = buffs.Count - 1; i >= 0; i--)
        {
            ActiveBuff buff = buffs[i];

            buff.Tick(Time.deltaTime);
            buff.tickTimer += Time.deltaTime;

            switch (buff.statuseffectdata.effecttype)
            {
                case StatusEffect.StatusEffectType.Poison:
                    if (buff.tickTimer >= buff.statuseffectdata.TickTime)
                    {
                        buff.tickTimer = 0;

                        Damage(TakePoisonDamage(enemyBase.charadata), 0, 0, false);
                    }


                    break;

                case StatusEffect.StatusEffectType.ElectricShock:

                    // 感電処理
                    if (buff.tickTimer >= buff.statuseffectdata.TickTime)
                    {
                        buff.tickTimer = 0;

                        audioManager.PlaySE(buff.audioClips[0]);
                        Damage(0.1f, 9, 2, false);
                        //感電
                    }
                    break;

                case StatusEffect.StatusEffectType.Charm:
                    // 魅了値は回復・解除されない。別の攻撃でHPが下がった場合も判定。
                    TryCompleteCharm(buff);
                    break;
            }

            if (buff.IsExpired())
            {
                if (buff.effectObject) Destroy(buff.effectObject);
                buffs.RemoveAt(i);
            }
        }
    }

    public void AllRecorverConfusionHP()
    {
        ConfusionHP = enemyBase.ConfusionHP;
        enemyBase.Confusion_Slider.value = 1 - (ConfusionHP / enemyBase.ConfusionHP);
    }

    public void RecoveryConfusionHP(float Value)
    {
        if (enemyBase.ConfusionHP > ConfusionHP + Value)
        {
            //ConfusionHP += Time.deltaTime;
            ConfusionHP += Value;
        }
        else
        {
            ConfusionHP = enemyBase.ConfusionHP;
        }
    }
    string GetHierarchyPath()
    {
        string path = gameObject.name;
        Transform current = transform.parent;
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        return path;
    }

    bool loggedMissingEnemyBase;

    private void FixedUpdate()
    {
        if (enemyBase == null)
        {
            // 生成された判定が本体の参照を失った場合、親から復元する。
            enemyBase = GetComponentInParent<EnemyBase>();
            if (enemyBase == null)
            {
                // 同じエラーを毎FixedUpdateで出さず、原因のオブジェクトを報告する。
                if (!loggedMissingEnemyBase)
                {
                    Debug.LogError("charaDamage: EnemyBaseの参照がありません。Object: " +
                        gameObject.name + " / Hierarchy: " + GetHierarchyPath(), this);
                    loggedMissingEnemyBase = true;
                }
                return;
            }
            loggedMissingEnemyBase = false;
        }

        HP_percent = (enemyBase.HP / enemyBase.MAXHP);

        if (Stan != enemyBase.StanHP)
        {
            Stan = enemyBase.StanHP;
        }

        if (enemyBase.ConfusionHP > ConfusionHP)
        {
            //ConfusionHP += Time.deltaTime;
            ConfusionHP += (enemyBase.ConfusionHP / enemyBase.charadata.ConfusionResistanceRegen) * Time.deltaTime;
        }

        enemyBase.Confusion_Slider.value = 1 - (ConfusionHP / enemyBase.ConfusionHP);
        enemyBase.HP_Slider.value = enemyBase.HP / enemyBase.MAXHP;

        if (enemyBase.status == EnemyBase.Status.ConfusionResistance)
        {
            if (StanCount < 3 + enemyBase.playerController.ConfusionTime + enemyBase.ConfusionAddTime)
            {
                StanCount += Time.fixedDeltaTime;
                float randomY = Random.Range(-0.1f, 0.1f);
                enemyBase.gameObject.transform.position = new Vector2(SavePos.x, SavePos.y + randomY);
                enemyBase.rb.velocity = Vector2.zero;

                enemyBase.mode = EnemyBase.ModeType.M1;
                enemyBase.moveType = EnemyBase.MoveType.Move;
                enemyBase.AttackCount = 0;
                ConfusionHP = enemyBase.ConfusionHP;
            }
            else
            {
                Vector2 Dir = enemyBase.Player.transform.position - enemyBase.gameObject.transform.position;

                enemyBase.rb.velocity = Dir.normalized * -1.5f;
            }
        }

        if (enemyBase.status == EnemyBase.Status.Stan)
        {

            if (StanCount < 0.15)
            {
                StanCount += Time.fixedDeltaTime;
                float randomY = Random.Range(-0.1f, 0.1f);
                enemyBase.gameObject.transform.position = new Vector2(SavePos.x, SavePos.y + randomY);
                enemyBase.rb.velocity = Vector2.zero;

                enemyBase.mode = EnemyBase.ModeType.M1;
                enemyBase.moveType = EnemyBase.MoveType.Move;
                enemyBase.AttackCount = 0;
            }
            else
            {
                Vector2 Dir = enemyBase.Player.transform.position - enemyBase.gameObject.transform.position;

                enemyBase.rb.velocity = Dir.normalized * -1.5f;
            }



        }
        if (enemyBase.status == EnemyBase.Status.Die)
        {

            if (DieCount < 0.3)
            {
                DieCount += Time.fixedDeltaTime;
            }
            else if (enemyBase.TryStartDeathCleanup())
            {
                if (enemyBase.DieEffect != null)
                    Instantiate(enemyBase.DieEffect, enemyBase.transform.position, Quaternion.identity);
                Destroy(enemyBase.gameObject);
            }
        }
    }

    private bool isDead = false;
    public void Death()
    {
        if (isDead || !enemyBase.TryStartDeath()) return;
        isDead = true;
        isDieCameraShake(enemyBase.charadata);
        enemyBase.status = EnemyBase.Status.Die;


        enemyBase.animator.SetBool("Damage", true);
        enemyBase.rb.velocity = Vector2.zero;
        CoinDrop();
        Vector2 Dir = enemyBase.Player.transform.position - enemyBase.gameObject.transform.position;
        enemyBase.rb.velocity = Dir.normalized * -5f;
        enemyBase.audioManager.PlaySE(enemyBase.DieAudio);
    }


    void isDamageTextPop(float value, int PP, bool Criticul)
    {
        // プールから取得
        GameObject obj = DamageTextPool.instance.Get();

        // 位置調整
        Vector3 pos = enemyBase.transform.position;
        pos.y += 1;
        obj.transform.position = pos;

        // 情報セット
        DamageText text = obj.GetComponent<DamageText>();
        text.Configure(value, Criticul);

    }
    void CoinDrop()
    {
        // Defer coin creation until all deaths this frame have been collected.
        // Each request retains the dying enemy's location.
        if (CoinPool.Instance != null)
            CoinPool.Instance.RequestDrop(enemyBase.GOLD, enemyBase.transform.position);
    }

    float TakeElectricDamage()
    {
        float ElectricDamage = 0;

        return ElectricDamage;
    }
    float TakePoisonDamage(charadata data)
    {
        float PoisonDamage = 0;

        switch (data.enemyStrength)
        {

            case charadata.EnemyStrength.None:
                PoisonDamage = 10;
                break;
            case charadata.EnemyStrength.Fodder:
                PoisonDamage = data.MAXHP / 6;
                break;
            case charadata.EnemyStrength.Normal:
                PoisonDamage = data.MAXHP / 20;
                break;
            case charadata.EnemyStrength.Veteran:
                PoisonDamage = data.MAXHP / 60;
                break;
            case charadata.EnemyStrength.Elite:
                PoisonDamage = data.MAXHP / 140;
                break;
            case charadata.EnemyStrength.Boss:
                PoisonDamage = data.MAXHP / 600;
                break;

        }

        return PoisonDamage;
    }
}
public class DamagePack
{

    public float Attackvalue;
    public float Stanvalue;
    public float ConfusionPower;
    public bool Critical;

    public void receiveDamagePack(float Attackvalue, float Stanvalue, float ConfusionPower, bool Critical)
    {

        this.Attackvalue = Attackvalue;
        this.Stanvalue = Stanvalue;
        this.ConfusionPower = ConfusionPower;
        this.Critical = Critical;

    }

}