using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DE_BigSword_AI : PlayerSkillBase
{

    PlayerSkillBase MYskillBase;

    [SerializeField] float AttackWaitTime = 1.2f;
    [SerializeField] float PlayerMoveSpeed = 0.1f;
    [SerializeField] bool isDash = false;

    void Start()
    {
        MYskillBase = GetComponent<PlayerSkillBase>();
    }
    private void FixedUpdate()
    {
        if (playerController.SaveSkillBase != MYskillBase)
        {
            Dash();
            Destroy(this.gameObject);
        }
    }
    // Update is called once per frame
    public override void SkillPlay()
    {
        skillcount += Time.deltaTime;
        if (Att == ATT.A1)
        {
            GameObject CL_Weapon = SkillSpawn(SkillWeapon, TargetObject);
            Att = ATT.A2;
        }
        else if (Att == ATT.A2)
        {
            if (skillcount > AttackWaitTime)
            {
                playerController.mode = PlayerController.ModeType.M1;
                playerController.movetype = PlayerController.MoveType.Nomal;

                //Debug.Log("AAAAA");
                Dash();
                Destroy(this.gameObject);
            }
            else
            {
                PlayerMove(PlayerMoveSpeed);
            }

        }


    }
    void Dash() 
    {
        if (!isDash) return;
        playerController.Dash = true;
    }
}
