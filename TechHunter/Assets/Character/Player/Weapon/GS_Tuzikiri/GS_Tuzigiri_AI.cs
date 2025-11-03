using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GS_Tuzigiri_AI : PlayerSkillBase
{
    PlayerSkillBase MYskillBase;

    [SerializeField] BuffData buffdata;
    [SerializeField] BuffData LastBuff;
    public GameObject SaveTarget;
    
    Vector2 EnemyDirection;

    Vector2 SaveDirection;

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
           
            GameObject CL_Weapon = SkillSpawn(SkillWeapon, TargetObject);
            Att = ATT.A2;
            
            Destroy(CL_Weapon, 2);

            playerController.playerBuff.SpawnBuff(buffdata);

            EnemyDirection = playerController.CursorDirection.normalized;
            

        }
        else if (Att == ATT.A2)
        {
            
                if (skillcount >= 0.2 )
                {
                    playerController.mode = PlayerController.ModeType.M1;
                    playerController.movetype = PlayerController.MoveType.Nomal;

                playerController.playerBuff.SpawnBuff(LastBuff);
                    //Debug.Log("AAAAA");
                    Destroy(this.gameObject);
                }
                else
                {

                    PlayerVelocityMove(20,EnemyDirection);

                }
        }
            
        
    }
}
