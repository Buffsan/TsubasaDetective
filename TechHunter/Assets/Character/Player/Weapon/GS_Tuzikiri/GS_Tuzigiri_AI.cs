using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GS_Tuzigiri_AI : PlayerSkillBase
{
    PlayerSkillBase MYskillBase;

    [SerializeField] BuffData buffdata;

    public GameObject SaveTarget;
    
    Vector2 EnemyDirection;

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
            GameObject CL_Weapon = Instantiate(SkillWeapon, playerController.transform.position, Quaternion.identity);
            Att = ATT.A2;
            CL_Weapon.transform.parent = playerController.MainAnimBody.transform;
            Destroy(CL_Weapon, 2);

            playerController.playerBuff.SpawnBuff(buffdata);

            EnemyDirection = playerController.MoveInput;
            

        }
        else if (Att == ATT.A2)
        {
            
                if (skillcount >= 0.2 )
                {
                    playerController.mode = PlayerController.ModeType.M1;
                    playerController.movetype = PlayerController.MoveType.Nomal;

                    //Debug.Log("AAAAA");
                    Destroy(this.gameObject);
                }
                else
                {

                    playerController.rb.velocity = EnemyDirection.normalized * 15;

                }
        }
            
        
    }
}
