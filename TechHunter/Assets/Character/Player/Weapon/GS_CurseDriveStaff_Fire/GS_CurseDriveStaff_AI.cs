using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GS_CurseDriveStaff_AI : PlayerSkillBase
{

    PlayerSkillBase MYskillBase;
    [SerializeField] GameObject Effect;
    [SerializeField] GameObject Bullet;
    [SerializeField] AudioClip audioClip;
    [SerializeField] bool LookWeapon = false;
    [SerializeField] GameObject SaveSkill;
    AudioManager audioManager => AudioManager.instance;
    
    void Start()
    {
        MYskillBase = GetComponent<PlayerSkillBase>();
    }
    private void FixedUpdate()
    {
        if (playerController.SaveSkillBase != MYskillBase && TargetObject == playerController.gameObject)
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
            GameObject CL_Effect = SkillSpawn(Effect, TargetObject);
            
            CL_Effect.transform.parent = playerController.transform;
            audioManager.isPlaySE(audioClip);
            Destroy(CL_Effect,5);
            Att = ATT.A2;
        }
        if (Att == ATT.A2)
        {
            PlayerMove(0.1f);
            if (skillcount > 1.5)
            {
                skillcount = 0;
                Att = ATT.A3;
            }
        }
        if (Att == ATT.A3)
        {

            GameObject CL_Weapon = SkillSpawn(SkillWeapon, TargetObject);
            
            GameObject CL_Attack = Instantiate(Bullet,transform.position, Quaternion.identity);
            if (LookWeapon) { SaveSkill = CL_Weapon;Destroy(CL_Attack, 0.1f); }
            CL_Attack.transform.up = playerController.CursorDirection.normalized;
            Rigidbody2D CL_rb = CL_Attack.GetComponent<Rigidbody2D>();
            if (!LookWeapon) CL_rb.velocity = playerController.CursorDirection.normalized * 10;

            Destroy(CL_Weapon, 2);

            Att = ATT.A4;

        }
        if (Att == ATT.A4)
        {
            if (SaveSkill) SaveSkill.transform.up = playerController.CursorDirection.normalized;
            if (skillcount > 2)
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