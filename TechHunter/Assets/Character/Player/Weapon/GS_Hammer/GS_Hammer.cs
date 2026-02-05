using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GS_Hammer : MonoBehaviour
{
    float AttackCont = 0;
    int Phase = 0;

    [SerializeField] Animator animator;
    [SerializeField] GameObject Bullet;
    [SerializeField] AudioClip AudioClip;
    [SerializeField] GameObject AttackPoint;
    PlayerController controller => PlayerController.Instance;
    AudioManager audioManager => AudioManager.instance;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    private void FixedUpdate()
    {

        AttackCont += Time.fixedDeltaTime;
       

        if (Phase == 0)
        { transform.up = controller.CursorDirection;
        transform.Rotate(0, 0, 90);
            animator.Play("\‚¦");
            if (AttackCont > 0.4)
            {
                audioManager.PlaySE(AudioClip);
                animator.Play("UŒ‚");
                Phase = 1;
                AttackCont = 0; 
                
                GameObject CL_Bullet = Instantiate(Bullet, AttackPoint.transform.position, Quaternion.identity);
           
                CL_Bullet.transform.up = controller.CursorDirection;
                Destroy(CL_Bullet, 0.1f);
            }

        }
        if (Phase == 1)
        {
            if (AttackCont > 0.1)
            {


                animator.Play("Á‚¦‚é");
                Phase = 2;
                AttackCont = 0;

               
                
            }
        }
        if (Phase == 2)
        {

            if (AttackCont > 1)
            {
                Destroy(gameObject);
            }

        }

    }
}
