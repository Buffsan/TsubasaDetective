using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TG_ShotGun_AI : PlayerSkillBase
{

    PlayerSkillBase MYskillBase;

    int Phase = 0;

    [SerializeField] GameObject Attack;
    [SerializeField] GameObject Effect;

    [SerializeField] AudioClip clip0;
    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;
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
            audioManager.isPlaySE(clip0);

            ALL_System.camera_Controller.Shake(0.2f, 0.1f);
            GameObject CL_Weapon = Instantiate(Attack, transform.position, Quaternion.identity);
            GameObject CL_Effect = Instantiate(Effect, transform.position, Quaternion.identity);
            Destroy(CL_Weapon,0.1f);
            Destroy(CL_Effect, 1f);

            Att = ATT.A2;
            transform.up = playerController.CursorDirection;
            CL_Weapon.transform.up = playerController.CursorDirection;
            CL_Effect.transform.up = playerController.CursorDirection;
            CL_Effect.transform.Rotate(0, 0, 180);

        }
        else if (Att == ATT.A2)
        {
            playerController.rb.velocity = playerController.CursorDirection.normalized * -20;
            
            if (skillcount > 0.1)
            {
                Att = ATT.A3;
                skillcount = 0;
            }

        }
        else if (Att == ATT.A3) 
        {
            playerController.rb.velocity = playerController.rb.velocity*0.5f;
            if (skillcount > 0.2)
            {
           
                end();
                Destroy(this.gameObject);
            }
        }


    }
}