using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class charaDamage : MonoBehaviour, IDamageable
{
    [SerializeField] EnemyBase enemyBase;
    [SerializeField] GameObject Coin;
    CoinManager coinManager;

    

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

    // Start is called before the first frame update
    void Start()
    {
        //HP = enemyBase.MAXHP;
        DF = enemyBase.ATKDF;
        Stan = enemyBase.StanHP;
        ConfusionHP = enemyBase.ConfusionHP;

        coinManager = Coin.GetComponent<CoinManager>();
        enemyBase.DefaltColor = enemyBase.spriteRenderer.color;
    }
    public void Damage(float Attackvalue, float Stanvalue, float ConfusionPower,bool Critical)
    {
        float AddDamage = Attackvalue - DF;
        if (AddDamage > 0)
        {
            if (enemyBase.status == EnemyBase.Status.ConfusionResistance)
            {
                enemyBase.HP -= AddDamage * 2;
                isDamageTextPop(AddDamage * 2,2, Critical);
            }
            else
            {
                enemyBase.HP -= AddDamage;
                isDamageTextPop(AddDamage, 1 , Critical);
            }

            enemyBase.TakeDamageColor();
            ConfusionHP -= (Attackvalue * HP_percent) * ConfusionPower;

        }
        if (1 - (ConfusionHP / enemyBase.ConfusionHP) > enemyBase.HP / enemyBase.MAXHP && enemyBase.status != EnemyBase.Status.ConfusionResistance)
        {//混乱

            audioManager.isPlaySE(enemyBase.ConfusionAudio);
            enemyBase.status = EnemyBase.Status.ConfusionResistance;
            StanCount = 0;
            SavePos = enemyBase.gameObject.transform.position;

            GameObject CL_ConfusionText = Instantiate(enemyBase.ConfusionImageText,enemyBase.transform.position,Quaternion.identity);
            Destroy( CL_ConfusionText ,2);

        }
        else if (Stan < Stanvalue && enemyBase.status != EnemyBase.Status.ConfusionResistance)
        {
            enemyBase.status = EnemyBase.Status.Stan;
            StanCount = 0;
            SavePos = enemyBase.gameObject.transform.position;
        }


        //enemyBase.Confusion_Slider.value = ConfusionHP / enemyBase.ConfusionHP;
        if (enemyBase.HP <= 0 && enemyBase.status != EnemyBase.Status.Die)
        {
            Death();
        }

    }
    public void AllRecorverConfusionHP() 
    {
        ConfusionHP = enemyBase.ConfusionHP;
        enemyBase.Confusion_Slider.value = 1 - (ConfusionHP / enemyBase.ConfusionHP);
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
            if (StanCount < 3)
            {
                StanCount += Time.deltaTime;
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
            { StanCount += Time.deltaTime;
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
            if (DieCount < 0.3) {
                DieCount += Time.deltaTime;
            } else {
                Instantiate(enemyBase.DieEffect, transform.position, Quaternion.identity);
                Destroy(enemyBase.gameObject);
            }
        }
    }
    public void Death()
    {
        //Debug.Log("しんだ");
        enemyBase.status = EnemyBase.Status.Die;
        enemyBase.animator.SetBool("Damage", true);
        enemyBase.rb.velocity = Vector2.zero;
        CoinDrop();
        Vector2 Dir = enemyBase.Player.transform.position - enemyBase.gameObject.transform.position;
        enemyBase.rb.velocity = Dir.normalized * -5f;
        enemyBase.audioManager.isPlaySE(enemyBase.DieAudio);
    }

    // Update is called once per frame
    void Update()
    {

    }
    void isDamageTextPop(float value,int PP,bool Criticul) 
    {
        GameObject CL_DamageText = Instantiate(enemyBase.DamageTextCanvas, enemyBase.transform.position, Quaternion.identity);
        CL_DamageText.transform.position = new Vector2(CL_DamageText.transform.position.x, CL_DamageText.transform.position.y + 1);
        DamageText damageText = CL_DamageText.GetComponent<DamageText>();
        damageText.isChangeText(value);
        damageText.Criticul = Criticul;

        Destroy(CL_DamageText, 0.5f);

    }
    void CoinDrop() 
    {
        int CrystalCount = 0;
        int GoaldCount =0;
        int SilverCount = 0;
        int CopperCount = 0;

        CrystalCount = enemyBase.GOLD / coinManager.CrystalValue;
        GoaldCount = (enemyBase.GOLD - CrystalCount * coinManager.CrystalValue) / coinManager.GoaldValue;
        SilverCount = (enemyBase.GOLD - (GoaldCount * coinManager.GoaldValue + CrystalCount * coinManager.CrystalValue)) / coinManager.SilverValue;
        CopperCount = (enemyBase.GOLD - (GoaldCount * coinManager.GoaldValue + SilverCount* coinManager.SilverValue + CrystalCount * coinManager.CrystalValue)) / coinManager.CopperValue;
        //Debug.Log("ーー水晶：" + CrystalCount + "ーー金：" +GoaldCount+ "ーー銀："+SilverCount+"ーー銅："+CopperCount);
        
        for (int i = 0; i < CrystalCount; i++)
        {
            GameObject CL_Goald = Instantiate(Coin, transform.position, Quaternion.identity);
            CoinManager CL_coin = CL_Goald.GetComponent<CoinManager>();

            CL_coin.coinType = CoinManager.CoinType.CrystalCoin;
        }
        for (int i = 0; i < GoaldCount; i++) 
        {
            GameObject CL_Goald = Instantiate(Coin, transform.position, Quaternion.identity);
            CoinManager CL_coin = CL_Goald.GetComponent<CoinManager>();

            CL_coin.coinType = CoinManager.CoinType.GoaldCoin;
        }
        for (int i = 0; i < SilverCount; i++)
        {
            GameObject CL_Goald = Instantiate(Coin, transform.position, Quaternion.identity);
            CoinManager CL_coin = CL_Goald.GetComponent<CoinManager>();

            CL_coin.coinType = CoinManager.CoinType.SilverCoin;
        }
        for (int i = 0; i < CopperCount; i++)
        {
            GameObject CL_Goald = Instantiate(Coin, transform.position, Quaternion.identity);
            CoinManager CL_coin = CL_Goald.GetComponent<CoinManager>();

            CL_coin.coinType = CoinManager.CoinType.CopperCoin;
        }
    }
}
