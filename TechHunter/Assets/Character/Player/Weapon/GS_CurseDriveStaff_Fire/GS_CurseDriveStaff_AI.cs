using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GS_CurseDriveStaff_AI : PlayerSkillBase
{

    PlayerSkillBase MYskillBase;
    [SerializeField] GameObject Effect;
    [SerializeField] GameObject Bullet;
    [SerializeField] AudioClip audioClip;
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
            GameObject CL_Effect = Instantiate(Effect, playerController.gameObject.transform.position, Quaternion.identity);
            CL_Effect.transform.parent = playerController.transform;
            audioManager.isPlaySE(audioClip);
            Destroy(CL_Effect,5);
            Att = ATT.A2;
        }
        if (Att == ATT.A2)
        {
            playerController.isMove(0.3f);
            if (skillcount > 1.5)
            {
                skillcount = 0;
                Att = ATT.A3;
            }
        }
        if (Att == ATT.A3)
        {

            GameObject CL_Skill = Instantiate(SkillWeapon, playerController.transform.position, Quaternion.identity);
            CL_Skill.transform.parent = playerController.transform;

            GameObject CL_Attack = Instantiate(Bullet, playerController.transform.position, Quaternion.identity);
            CL_Attack.transform.up = playerController.CursorDirection.normalized;
            Rigidbody2D CL_rb = CL_Attack.GetComponent<Rigidbody2D>();
            CL_rb.velocity = playerController.CursorDirection.normalized * 10;

            Destroy(CL_Skill,2);

            Att = ATT.A4;

        }
        if (Att == ATT.A4)
        {
            if (skillcount > 2)
            {
                end();
            }
            else
            {
                playerController.isMove(0.3f);
            }
        }


    }
}