using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GS_YARI_AI : PlayerSkillBase
{
    PlayerSkillBase MYskillBase;

    [SerializeField] GameObject SkillWeapon1;
    [SerializeField] GameObject SkillWeapon2;
    [SerializeField] RelicData relicData;
    public int ConboNumber = 0;
    void Start()
    {
        MYskillBase = GetComponent<PlayerSkillBase>();
        if (playerController.relicDatas.Count != 0)
        {
            foreach (var relic in playerController.relicDatas)
            {
                if (relic == null) continue;
                if (relic.RelicDataObject == null) continue;
                if (relicData == null) continue;
                if (relicData.RelicDataObject == null) continue;

                if (relic.RelicDataObject.name == relicData.RelicDataObject.name)
                {
                    var kit = relic.RelicDataObject
                        .GetComponent<Relic_EightsPortableExpeditionKit>();

                    if (kit == null) continue;

                    ConboNumber = kit.weaponConbos[0];
                    kit.AddConbo(0);
                    break;
                }
            }
        }
    }
    private void FixedUpdate()
    {
        if (playerController.SaveSkillBase != MYskillBase)
        {
            Destroy(this.gameObject);
        }
    }
    public override void SkillPlay()
    {
        skillcount += Time.deltaTime;
        if (Att == ATT.A1)
        {
            GameObject weapon = SkillWeapon;

            if (ConboNumber == 3) 
            {
                weapon = SkillWeapon2;
            }

            GameObject CL_Weapon = SkillSpawn(weapon, TargetObject);
            playerController.isCursorDirection();

            CL_Weapon.transform.up = playerController.CursorDirection;
            
            if(ConboNumber == 1) 
            {
                for (int i = 0; i < 2; i++) 
                {
                    StartCoroutine(RateAttack((float)i / 5));
                }
            }
            if (ConboNumber == 2) 
            {
                for (int i = 0; i < 4; i++)
                {
                    StartCoroutine(RateAttack((float)i / 8));
                }
            }
            if (ConboNumber == 3)
            {
                for (int i = 0; i < 7; i++)
                {
                    StartCoroutine(RateAttack((float)i / 12));
                }
            }

            Destroy(CL_Weapon, 3);
            Att = ATT.A2;
        }
        if (Att == ATT.A2)
        {
            PlayerMove(0.6f);
            if (skillcount > 0.8)
            {
                end();
            }

        }
    }

    IEnumerator RateAttack(float index) 
    {
        yield return new WaitForSeconds(index);

        GameObject CL_Weapon = SkillSpawn(SkillWeapon1, TargetObject);
        Animator animator = CL_Weapon.GetComponent<Animator>();
        animator.Play("çUåÇÇQ");
        playerController.isCursorDirection();

        CL_Weapon.transform.up = playerController.CursorDirection;

        CL_Weapon.transform.Rotate(0, 0, UnityEngine.Random.Range(-20, 20));

        Destroy(CL_Weapon, 3);
    }
}
