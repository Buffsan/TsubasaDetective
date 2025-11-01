using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TG_Maquette_AI : PlayerSkillBase
{

    PlayerSkillBase MYskillBase;
    [SerializeField] GameObject Effect;
    [SerializeField] List<GameObject> SpawnLists;
    [SerializeField] GameObject SkillWeapon2;
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
    // Update is called once per frame
    public override void SkillPlay()
    {
        skillcount += Time.deltaTime;
        if (Att == ATT.A1)
        {
            float i = 0;
            foreach (var spawn in SpawnLists)
            {
                if (i == SpawnLists.Count-1)
                {
                    GameObject CL_Weapon = SkillSpawn_Add(SkillWeapon2, TargetObject,spawn);
                    TG_Maquette _Maquette = CL_Weapon.GetComponent<TG_Maquette>();
                    _Maquette.WaitTime = i/5;
                }
                else 
                {
                    GameObject CL_Weapon = SkillSpawn_Add(SkillWeapon, TargetObject,spawn);
                    TG_Maquette _Maquette = CL_Weapon.GetComponent<TG_Maquette>();
                    _Maquette.WaitTime = i /5;
                }
                 i++;
            }
            Att = ATT.A2;
        }
        if (Att == ATT.A2)
        {
            PlayerMove(0.3f);
            if (skillcount > 2)
            {
                skillcount = 0;
                Att = ATT.A3;
            }
        }
        if (Att == ATT.A3)
        {

            if (skillcount > 1)
            {
                end();
            }
            else
            {
                PlayerMove(0.3f);
            }
           

        }
      

    }
}
