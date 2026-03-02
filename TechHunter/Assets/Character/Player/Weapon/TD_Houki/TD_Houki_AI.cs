using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TD_Houki_AI : PlayerSkillBase
{
    PlayerSkillBase MYskillBase;
    public int ConboNumber = 0;
    [SerializeField] GameObject Slash;
    [SerializeField] GameObject BigAttackArea;
    [SerializeField] RelicData relicData;
    void Start()
    {
        MYskillBase = GetComponent<PlayerSkillBase>();
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

            if (ConboNumber == 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    StartCoroutine(PlaceObject((float)i / 4,i));
                }
            }
            else
            {
                GameObject CL_Weapon = SkillSpawn(SkillWeapon, TargetObject);
                playerController.isCursorDirection();

                CL_Weapon.transform.up = playerController.CursorDirection;
                if (ConboNumber == 0)
                    CL_Weapon.transform.Rotate(0, 0, 90);

                SaveComponent save = CL_Weapon.GetComponent<SaveComponent>();
                save.animator.SetInteger("Anim", ConboNumber);
                save.animator.SetBool("Set", true);

                if (ConboNumber == 3) 
                {
                    GameObject CL_Slash = Instantiate(Slash, transform.position, Quaternion.identity);
                    Destroy(CL_Slash,1);
                    GameObject CL_AttackArea = Instantiate(BigAttackArea, transform.position, Quaternion.identity);
                    Destroy(CL_AttackArea, 0.2f);
                }

                Destroy(CL_Weapon, 5);
            }


            Att = ATT.A2;
        }
        if (Att == ATT.A2)
        {
            PlayerMove(0.4f);
            if (skillcount > 0.5)
            {
                end();
            }

        }
    }

    private IEnumerator PlaceObject(float index,int number)
    {
        yield return new WaitForSeconds(index);
        GameObject CL_Weapon = SkillSpawn(SkillWeapon, TargetObject);
        playerController.isCursorDirection();

        CL_Weapon.transform.up = playerController.CursorDirection;
        if (ConboNumber == 0)
            CL_Weapon.transform.Rotate(0, 0, 90);

        if (number == 0)
        {
            CL_Weapon.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            CL_Weapon.transform.localScale = new Vector3(-1, 1, 1);
        }

        SaveComponent save = CL_Weapon.GetComponent<SaveComponent>();
        save.animator.SetInteger("Anim", ConboNumber);
        save.animator.SetBool("Set", true);

        Destroy(CL_Weapon, 5);
    }
}
