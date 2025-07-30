using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OW_HarpoonVer2_AI : PlayerSkillBase
{
    PlayerSkillBase MYskillBase;
    [SerializeField] Animator animator;

    bool Attacked = false;

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
            playerController.isMove(2f);
            playerController.isCursorDirection();

            transform.position = playerController.transform.position;
            transform.up = (Vector2)playerController.CursorDirection;

            if (skillcount > 5) 
            {
                Shot();
            }
            //Att = ATT.A2;
        }
        if (Att == ATT.A2)
        {
            playerController.isMove(0.4f);
            if (skillcount > 0.6f)
            {
                end();
            }

        }
    }

    void Shot() 
    {
        Attacked = true;
        float angle = 40;
        animator.Play("ê[Ç≠ä—Ç≠",0,0);
        for (int i = 0; i < 5; i++)
        {
            GameObject CL_Harpoon = Instantiate(SkillWeapon, transform.position, Quaternion.identity);
            Rigidbody2D rb = CL_Harpoon.GetComponent<Rigidbody2D>();
            Vector2 bulletDirection = Quaternion.Euler(0, 0, angle -20*i) * playerController.CursorDirection.normalized;
            CL_Harpoon.transform.up = bulletDirection;
            rb.velocity = bulletDirection * 20;
            Destroy(CL_Harpoon, 0.8f);
        }
        Att = ATT.A2;
        skillcount = 0;
        //end();

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy")&& !Attacked) 
        {
            Shot();
        }
    }
}
