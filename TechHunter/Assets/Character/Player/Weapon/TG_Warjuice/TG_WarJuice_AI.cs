using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TG_WarJuice_AI : PlayerSkillBase
{
    PlayerSkillBase MYskillBase;
    [SerializeField]BuffData BuffData;
    [SerializeField] AudioClip audioClip;

    [SerializeField] GameObject Effect;

    AudioManager audioManager => AudioManager.instance;

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
            GameObject CL_Weapon = Instantiate(SkillWeapon, playerController.transform.position, Quaternion.identity);
            Att = ATT.A2;

            

            CL_Weapon.transform.parent = playerController.MainAnimBody.transform;
            playerController.playerBuff.SpawnBuff(BuffData);

            GameObject CL_Effect = Instantiate(Effect, playerController.transform.position,Quaternion.identity);
            CL_Effect.transform.position = new Vector2(playerController.transform.position.x, playerController.transform.position.y - 0.5f);
            CL_Effect.transform.parent = playerController.transform;
            Destroy(CL_Effect , BuffData.BuffTime);

            audioManager.isPlaySE(audioClip);

            Destroy( CL_Weapon ,2);
        }
        else if (Att == ATT.A2) 
        {
            end();
        }
    }
}
