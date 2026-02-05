using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic_TheBlood_SpunCopier_Ghost : MonoBehaviour
{
    [SerializeField] Animator animator;
    public float WaitTime = 4;
    public GameObject playerskillBase;
    public GameObject SaveSkillObject = null;
    PlayerSkillBase playerSkillBase = null;
    public SkillCardData skillCardData;
    public int HP = 1;

    GameObject SaveWeapon = null;
    int Phase = 0;
    float Cout = 0;
    // Start is called before the first frame update
    private void FixedUpdate()
    {
        
        if (playerskillBase == null && Phase ==0) return;

        if (Phase == 0) 
        {
            Cout += Time.fixedDeltaTime;
            if (Cout > WaitTime) 
            {
                Phase++;
            }
            
        }
        if (Phase == 1) 
        { 
            GameObject CL_Skill = Instantiate(playerskillBase, transform.position, Quaternion.identity);
            CL_Skill.transform.parent = transform;
            playerSkillBase = CL_Skill.GetComponent<PlayerSkillBase>();
            playerSkillBase.TargetObject = gameObject;

            int i = 0;
            foreach (var playerskill in PlayerController.Instance.skillINFO) 
            {
                if (playerskill.skillDATA == skillCardData) 
                {
                    playerskill.SkillCardManager.isCoolTime_add_Over(-skillCardData.CoolTime * 0.3f);
                }
                i++;
            }
            
            Phase++;
        }
        if (Phase == 2) 
        {
            if (!playerSkillBase)
            {
                Phase++;
            }
            else 
            {
                playerSkillBase.SkillPlay();
            }
            
        }
        if (Phase == 3) 
        {
            Phase++;
            Destroy(gameObject, 15);
            animator.Play("è¡Ç¶ÇÈ");
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyAttack enemyAttack = other.GetComponent<EnemyAttack>();
        if (!enemyAttack) return;
        HP--;
        if (HP > 0) return;
        Destroy(gameObject);
    }
}
