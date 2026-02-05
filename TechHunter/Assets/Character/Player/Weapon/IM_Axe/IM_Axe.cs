using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IM_Axe : MonoBehaviour
{
    

    public float AttackCount = 0;
    [SerializeField] GameObject Attack;
    [SerializeField] GameObject AttackPoint;

    [SerializeField] GameObject Effect;
    [SerializeField] GameObject Bullet;

    [SerializeField] Animator animator;

    [SerializeField] AudioClip clip1;
    [SerializeField] AudioClip clip2;
    [SerializeField] AudioClip clip3;
    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;
    PlayerController playerController=> PlayerController.Instance;
    AudioManager audioManager => AudioManager.instance;

    bool AttackBool = false;

    int phase = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        transform.localScale = new Vector3(1, 1, 1);
        if (phase == 0)
        {
            if (AttackCount > 0.15)
            {
                audioManager.PlaySE(clip3);
                GameObject CL_Attack = Instantiate(Attack, AttackPoint.transform.position, Quaternion.identity);
                GameObject CL_effect = Instantiate(Effect, transform.position, Quaternion.identity);
                Destroy(CL_Attack, 0.15f);
                Destroy(CL_effect, 1);

                ALL_System.camera_Controller.Shake(0.1f, 0.1f);

                AttackCount = 0;
                animator.Play("‰ñ“]Ø‚è");phase = 1;
            }
        }
        if (phase == 1) 
        {
            if (AttackCount > 0.4) 
            {
                audioManager.PlaySE(clip1);
                animator.Play("•ÏŒ`");
                AttackCount = 0;
                phase = 2;
            }
        }
        if (phase == 2) 
        {
            transform.up = playerController.CursorDirection.normalized;
            if (AttackCount > 0.3)
            {
                ALL_System.camera_Controller.Shake(0.1f, 0.2f);
                audioManager.PlaySE(clip2);
                GameObject CL_Bullet = Instantiate(Bullet, transform.position, Quaternion.identity);
                IM_SmokeBullet iM_Smoke = CL_Bullet.GetComponent<IM_SmokeBullet>();
                iM_Smoke.rb.velocity = playerController.CursorDirection.normalized *15;
                AttackCount = 0;
                phase = 3;
            }
        }
        if (phase == 3) 
        {
            if (AttackCount > 0.5)
            {
                Destroy(gameObject);
            }
        }
        
        

        AttackCount += Time.deltaTime;
    }
}

