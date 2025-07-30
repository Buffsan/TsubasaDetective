using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GS_KIRIKAESI_AI : PlayerSkillBase
{
    PlayerSkillBase MYskillBase;
    [SerializeField] BuffData Muteki;
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
            GameObject CL_Weapon = Instantiate(SkillWeapon,playerController.transform.position,Quaternion.identity); 
            CL_Weapon.transform.parent = playerController.MainAnimBody.transform;

            //playerController.playerDamage.SetInvincible(0.4f);
            playerController.playerBuff.SpawnBuff(Muteki);
            Att = ATT.A2;
        }
        if (Att == ATT.A2)
        {
            if (skillcount > 1)
            {
                end();
            }
            else 
            {
                playerController.isMove(0.2f);
            }
        }
        

    }
}
