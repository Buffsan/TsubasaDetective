using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static PlayerSkillBase;

public class GS_KIRIKAESI : MonoBehaviour
{
    PlayerSkillBase MYskillBase;
    [SerializeField] Animator animator;
    [SerializeField] AudioClip Clip;
    [SerializeField] AudioClip Clip1;

    [SerializeField] GameObject AttackArea;
    [SerializeField] GameObject AttackPoint;
    public bool Damage = false;

    float skillcount = 0;

    int Type = 0;

    AudioManager audioManager => AudioManager.instance;
    PlayerController playerController => PlayerController.Instance;
    
    void Start()
    {
        MYskillBase = GetComponent<PlayerSkillBase>();
        audioManager.isPlaySE(Clip1);
    }
    private void FixedUpdate()
    {
        transform.localScale = new Vector3(2.5f,2.5f,2.5f);
        skillcount += Time.deltaTime;
        if (Type == 0)
        {
            if (Damage)
            {
                animator.SetInteger("Anim", 1);
                GameObject CL_AttackArea = Instantiate(AttackArea,AttackPoint.transform.position,Quaternion.identity);
                Destroy(CL_AttackArea,0.1f);
                CL_AttackArea.transform.rotation = Quaternion.identity;
                audioManager.isPlaySE(Clip);
                Type = 1;
                skillcount = 0;
                playerController.playerDamage.SetInvincible(1f);
            }
            else
            {
                if (skillcount > 1)
                {
                    Destroy(gameObject);
                }
            }
        }
        if (Type == 1) 
        {

            if (skillcount > 1.5) 
            {
                Destroy(gameObject);
            }
        
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.gameObject.CompareTag("EnemyArea") || other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Untagged")) 
        { 
        EnemyAttack enemyAttack = other.GetComponent<EnemyAttack>();
            if (enemyAttack != null) 
            { 
            Damage = true;
            }
        }

    }
}
