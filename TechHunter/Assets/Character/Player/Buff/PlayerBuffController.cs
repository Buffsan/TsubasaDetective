using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBuffController : MonoBehaviour
{
    public List<ActiveBuff> buffs = new();

    PlayerController playerController => PlayerController.Instance;

    const float DEFALT_NUM = 0;
    const float DEFALT_MULTIPLY_NUM = 1;


    public float Attack_AllBuff;
    public float MultiplyAttack_AllBuff;
    public float DEF_AllBuff = 0;
    public float MultiplyDEF_AllBuff = 1;

    public float Speed_AllBuff = 0;
    public float MultiplySpeed_AllBuff = 1;

    public float Multiply_CoolTimeBuff = 0;


    private void Update()
    {
        UpdateBuffs();
    }

    public void SpawnBuff(BuffData buffData)
    {
        buffs.Add(new ActiveBuff(buffData));
    }
    public void SpawnOrRefreshBuff(BuffData buffData)
    {
        for (int i = 0; i < buffs.Count; i++)
        {
            // 同じバフを発見
            if (buffs[i].data == buffData)
            {
                // 効果時間を更新
                buffs[i].remainTime = buffData.BuffTime;

                return;
            }
        }

        // 無ければ新規追加
        buffs.Add(new ActiveBuff(buffData));
    }
    public bool HasBuff(BuffData buffData)
    {
        for (int i = 0; i < buffs.Count; i++)
        {
            if (buffs[i].data == buffData)
            {
                return true;
            }
        }

        return false;
    }

    void UpdateBuffs()
    {
        playerController.playerDamage.mode =
                    PlayerDamage.Mode.Nomal;
        Attack_AllBuff = DEFALT_NUM;
        DEF_AllBuff = DEFALT_NUM;
        Speed_AllBuff = DEFALT_NUM;

        MultiplyAttack_AllBuff = DEFALT_MULTIPLY_NUM;
        MultiplyDEF_AllBuff = DEFALT_MULTIPLY_NUM;
        MultiplySpeed_AllBuff = DEFALT_MULTIPLY_NUM;
        Multiply_CoolTimeBuff = DEFALT_NUM;

        for (int i = buffs.Count - 1; i >= 0; i--)
        {
            ActiveBuff buff = buffs[i];

            buff.Tick(Time.deltaTime);

            if (buff.IsExpired())
            {
                buffs.RemoveAt(i);
                continue;
            }

            Attack_AllBuff += buff.data.ATK;
            MultiplyAttack_AllBuff += buff.data.MultiplyATK;
            DEF_AllBuff += buff.data.ATKDF;
            MultiplyDEF_AllBuff += buff.data.MultiplyATKDF;
            Speed_AllBuff += buff.data.SPEED;
            MultiplySpeed_AllBuff += buff.data.MultiplySPEED;
            Multiply_CoolTimeBuff += buff.data.Multiply_CoolTimeBuff;

            if (buff.data.Invincible)
            {
                playerController.playerDamage.mode =
                    PlayerDamage.Mode.Invincible;
            }
        }
    }
    /*
    public List<PlayerBuffsBase> playerBuffs = new List<PlayerBuffsBase>();
    PlayerController playerController => PlayerController.Instance;

    public float Attack_AllBuff = 0;
    public float MultiplyAttack_AllBuff = 1;

    public float DEF_AllBuff = 0;
    public float MultiplyDEF_AllBuff = 1;

    public float Speed_AllBuff = 0;
    public float MultiplySpeed_AllBuff = 1;

    public float Multiply_CoolTimeBuff = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void SpawnBuff(BuffData buffdata) 
    {
        GameObject buffObject = new GameObject("BuffObject:" + buffdata.BuffNAME);
        buffObject.AddComponent<PlayerBuffsBase>();
        buffObject.transform.parent = playerController.gameObject.transform;

        PlayerBuffsBase Object_Buff = buffObject.GetComponent<PlayerBuffsBase>();
        Object_Buff.buffData = buffdata;

        playerBuffs.Add(Object_Buff);
    }

    private void FixedUpdate()
    {
        AddBuff();
    }
    // Update is called once per frame
    public void AddBuff() 
    {
        playerController.playerDamage.mode = PlayerDamage.Mode.Nomal;
        Attack_AllBuff = 0;
        MultiplyAttack_AllBuff = 1;

        DEF_AllBuff = 0;
        MultiplyDEF_AllBuff = 1;

        Speed_AllBuff = 0;
        MultiplySpeed_AllBuff = 1;

        Multiply_CoolTimeBuff = 0;

        foreach (PlayerBuffsBase buffs in playerBuffs) 
        {

            if (buffs.BuffTime < buffs.BuffCount)
            {
                //playerBuffs.Remove(buffs);
                buffs.Die();
                
            }
            else 
            {
                if (buffs.buffData.Invincible)
                {
                    playerController.playerDamage.mode = PlayerDamage.Mode.Invincible;
                }

            Attack_AllBuff += buffs.ATK;
            MultiplyAttack_AllBuff += buffs.MultiplyATK;

            DEF_AllBuff += buffs.ATKDF;
            MultiplyDEF_AllBuff += buffs.MultiplyATKDF;

            Speed_AllBuff += buffs.SPEED;
            MultiplySpeed_AllBuff+= buffs.MultiplySPEED;

                Multiply_CoolTimeBuff += buffs.Multiply_CoolTimeBuff;

            }
            //playerBuffs.RemoveAll(buffs => buffs == null || buffs.My == null);

        }
        playerBuffs.RemoveAll(buffs => buffs == null);
    }
    */
}
