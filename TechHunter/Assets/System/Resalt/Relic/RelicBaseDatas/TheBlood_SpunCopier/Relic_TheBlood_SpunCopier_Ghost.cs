using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic_TheBlood_SpunCopier_Ghost : MonoBehaviour
{
    [SerializeField] Animator animator;
    public GameObject playerskillBase;
    public GameObject SaveSkillObject = null;
    PlayerSkillBase playerSkillBase = null;

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
            if (Cout > 5) 
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
        Destroy(gameObject);
    }
}
