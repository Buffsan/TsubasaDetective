using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Skill_DK_Sword_AI : PlayerSkillBase
{

    PlayerSkillBase MYskillBase;

    [SerializeField] float AttackWaitTime = 1.2f;
    [SerializeField] float PlayerMoveSpeed = 0.1f;
    [SerializeField] BuffData buffData;

    Vector2 Direction = Vector2.zero;

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
            GameObject CL_Weapon = SkillSpawn(SkillWeapon, TargetObject);
            Direction = playerController.CursorDirection.normalized;
            playerController.playerBuff.SpawnBuff(buffData);
            Att = ATT.A2;
            skillcount = 0;
        }
        else if (Att == ATT.A2)
        {
            PlayerVelocityMove(PlayerMoveSpeed, Direction);
            if (skillcount > AttackWaitTime)
            {
                playerController.Dash = true;
                end();
            }

        }


    }
}