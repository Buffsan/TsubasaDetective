using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OW_HarpoonVer2_AI : PlayerSkillBase
{
    PlayerSkillBase MYskillBase;
    [SerializeField] Animator animator;
    [SerializeField] GameObject Harpoon;

    bool Attacked = false;

    void Start()
    {
        throwHarpoon();
        MYskillBase = GetComponent<PlayerSkillBase>();
    }
    private void FixedUpdate()
    {
        if (playerController.SaveSkillBase != MYskillBase || playerController.SaveSkillBase == null)
        {
            Destroy(this.gameObject);
        }
    }
    public override void SkillPlay()
    {
        skillcount += Time.deltaTime;
        if (Att == ATT.A1)
        {
            PlayerMove(2.5f);
            playerController.isCursorDirection();

            transform.position = transform.position;
            transform.up = new Vector2(0,1);

            if (skillcount > 5) 
            {
                Att = ATT.A2;
                
                end();
            }
            //Att = ATT.A2;
        }
        if (Att == ATT.A2)
        {
           
            if (skillcount > 0.6f)
            {
                end();
            }

        }
    }
    public void throwHarpoon() 
    {
        GameObject CL_Weapon = SpawnObject(Harpoon, TargetObject,5);
        OW_HarpoonVer2 _HarpoonVer2 = CL_Weapon.GetComponent<OW_HarpoonVer2>();

        _HarpoonVer2.Direction = (Vector2)playerController.CursorDirection;
        

    }
    
}
