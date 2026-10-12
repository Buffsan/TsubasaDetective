using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class charaDamage : MonoBehaviour, IDamageable
{
    [SerializeField] EnemyBase enemyBase;
    public EnemyBase OwnerEnemyBase => enemyBase;
    public Faction MyFactions => enemyBase != null ? enemyBase.Factions : Faction.None;
    [SerializeField] GameObject Coin;
    CoinManager coinManager;

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


    private void Start()
    {
        DF = enemyBase.ATKDF;
        Stan = enemyBase.StanHP;
        ConfusionHP = enemyBase.ConfusionHP;

        coinManager = Coin.GetComponent<CoinManager>();
        enemyBase.DefaltColor = enemyBase.spriteRenderer.color;
    }

    private void Update()
    {
        UpdateBuffs();
    }

    public void SpawnBuff(StatusEffectAttack buffData)
    {
        foreach (var active in buffs)
        {
            if (active.statuseffectdata == null) continue;

            if (active.statuseffectdata.effecttype == buffData.effecttype)
            {
                // 効果時間だけ延長
                active.remainTime += buffData.Duration;
                return;
            }

        }

        ActiveBuff activeBuff = new ActiveBuff(buffData);
        if (activeBuff.statuseffectdata.effecttype != StatusEffect.StatusEffectType.None)
        {
            activeBuff.audioClips = buffData.audioClips;
            switch (activeBuff.statuseffectdata.effecttype)
            {
                case StatusEffect.StatusEffectType.Poison:
                    activeBuff.effectObject = Instantiate(activeBuff.statuseffectdata.PoisonEffectPrefab, transform.position, Quaternion.identity);
                    activeBuff.effectObject.transform.parent = transform;
                    break;

                case StatusEffect.StatusEffectType.ElectricShock:
                    activeBuff.effectObject = Instantiate(activeBuff.statuseffectdata.EffectObject, transform.position, Quaternion.identity);
                    activeBuff.effectObject.transform.parent = transform;
                    break;
            }
        }

        // 初めてなら追加
        buffs.Add(activeBuff);
    }

    public void Damage(float Attackvalue, float Stanvalue, float ConfusionPower, bool Critical)
    {
        if (isDead) return;

        if (DamageType != 0 && passDamageAreas.Count == 0)
        {
            Debug.Log("ダメージを押し付ける処理なのに押しつけ先がいない　ひどいよのなか");
            return;
        }
        switch (DamageType)
        {

            case 0:
                receiveDamage(Attackvalue, Stanvalue, ConfusionPower, Critical);
                break;
            case 1:
                passDamageAreas[0].Damage(Attackvalue, Stanvalue, ConfusionPower, Critical);
                break;
            case 2:
                foreach (var d in passDamageAreas)
                {
                    d.Damage(Attackvalue, Stanvalue, ConfusionPower, Critical);
                }
                break;
        }



    }

    void receiveDamage(float Attackvalue, float Stanvalue, float ConfusionPower, bool Critical)
    {
        float AddDamage = Attackvalue - DF;
        if (AddDamage < 1)
        {
            AddDamage = 1;
        }

        if (enemyBase.status == EnemyBase.Status.ConfusionResistance)
        {
            TakeDamage(-AddDamage * 2);
            isDamageTextPop(AddDamage * 2, 2, Critical);
        }
        else
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

            buff.remainTime -= Time.deltaTime;
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
            }

            if (buff.remainTime <= 0)
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
    private void FixedUpdate()
    {

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
            else
            {
                Instantiate(enemyBase.DieEffect, transform.position, Quaternion.identity);
                Destroy(enemyBase.gameObject);
            }
        }
    }

    private bool isDead = false;
    public void Death()
    {
        enemyBase.status = EnemyBase.Status.Die;
        //Debug.Log("しんだ");
        if (isDead) return;   // 既に死亡処理中なら何もしない
        isDead = true;        // ここで最初にロックをかける


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
        text.isChangeText(value);
        text.Criticul = Criticul;

    }
    void CoinDrop()
    {
        int remain = enemyBase.GOLD;

        int[] values = {
        coinManager.CrystalValue,
        coinManager.GoaldValue,
        coinManager.SilverValue,
        coinManager.CopperValue
    };

        CoinManager.CoinType[] types = {
        CoinManager.CoinType.CrystalCoin,
        CoinManager.CoinType.GoaldCoin,
        CoinManager.CoinType.SilverCoin,
        CoinManager.CoinType.CopperCoin
    };

        for (int i = 0; i < values.Length; i++)
        {
            int count = remain / values[i];
            remain %= values[i];

            for (int j = 0; j < count; j++)
            {
                // ▼ Instantiate の代わりにプールから取得する
                GameObject coinObj = CoinPool.Instance.GetCoin(transform.position);

                CoinManager coin = coinObj.GetComponent<CoinManager>();
                coin.coinType = types[i];

                // SystemManager の coinManagers 登録（あなたの仕様に合わせる）
                enemyBase.ALL_System.systemManager.coinManagers.Add(coin);
            }
        }

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