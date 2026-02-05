using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_GS_Flamethrower : MonoBehaviour
{

    [SerializeField] GameObject fire;
    [SerializeField] GameObject Gun;
    [SerializeField] Animator animator;
    [SerializeField] float fireSpeed = 3f;
    [SerializeField] float fireDestroTime = 1;
    [SerializeField] float AttackTime  = 0.1f;
    [SerializeField] float DieTime = 3;

    [SerializeField] GameObject Effect;
    [SerializeField] GameObject AttackArea;
    [SerializeField] AudioClip audioClip;

    float dieCount = 0;
    float AttackCount = 0;
    int Phase = 0;
    PlayerController playerController => PlayerController.Instance;
    AudioManager audioManager => AudioManager.instance;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (Phase == 0) 
        {
            Phase++;
            GameObject CL_Effect = Instantiate(Effect, transform.position, Quaternion.identity);
            GameObject CL_Attack = Instantiate(AttackArea, transform.position, Quaternion.identity);
            audioManager.PlaySE(audioClip);
            Destroy(CL_Effect, 2f);
            Destroy(CL_Attack, 0.1f);
        }
        if (Phase == 1)
        {
            dieCount += Time.fixedDeltaTime;
            AttackCount += Time.fixedDeltaTime;
            Gun.transform.up = playerController.CursorDirection.normalized;
            Gun.transform.Rotate(0, 0, 90);

            if (AttackCount > AttackTime)
            {

                GameObject CL_Fire = Instantiate(fire, transform.position, Quaternion.identity);
                Rigidbody2D rb = CL_Fire.GetComponent<Rigidbody2D>();
                rb.velocity = playerController.CursorDirection.normalized * fireSpeed;
                Destroy(CL_Fire, fireDestroTime);
                AttackCount = 0;
            }
            if (dieCount > DieTime)
            {

               
                animator.Play("è¡Ç¶ÇÈ");
                Phase ++;
            }
        }
        if (Phase == 2) 
        {
            Destroy(gameObject, 1);
            Phase++;
        }
    }
}
