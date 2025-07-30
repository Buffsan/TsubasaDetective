using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBuffController : MonoBehaviour
{

    public List<PlayerBuffsBase> playerBuffs = new List<PlayerBuffsBase>();
    PlayerController playerController => PlayerController.Instance;

    public float Attack_AllBuff = 0;
    public float MultiplyAttack_AllBuff = 1;

    public float DEF_AllBuff = 0;
    public float MultiplyDEF_AllBuff = 1;

    public float Speed_AllBuff = 0;
    public float MultiplySpeed_AllBuff = 1;

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

            }
            //playerBuffs.RemoveAll(buffs => buffs == null || buffs.My == null);

        }
        playerBuffs.RemoveAll(buffs => buffs == null);
    }
}
