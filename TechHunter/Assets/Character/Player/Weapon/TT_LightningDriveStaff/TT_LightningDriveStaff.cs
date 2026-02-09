using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TT_LightningDriveStaff : MonoBehaviour
{

    float AttackCont = 0;
    int Phase = 0;

    [SerializeField] Animator animator;
    [SerializeField] GameObject Bullet;
    [SerializeField] GameObject Bullet2;
    [SerializeField] AudioClip AudioClip;
    [SerializeField] RelicData bioBattery;
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
        transform.up = controller.CursorDirection;

        if (Phase == 0) 
        {
            animator.Play("‘Ò‹@");
            if (AttackCont > 0.2) 
            {
                audioManager.PlaySE(AudioClip);
                animator.Play("”­ŽË‘Ò‹@");
                Phase = 1;
                AttackCont =0;
            }

        }
        if (Phase == 1) 
        {
            if (AttackCont > 0.2) 
            {


                animator.Play("”­ŽË");
                Phase = 2;
                AttackCont =0;

                bool bio_Battery = false;
                foreach (var relic in controller.relicDatas) 
                {
                    if (relic.RelicName == bioBattery.RelicName) 
                    { 
                    bio_Battery = true;
                    }
                }

                if (bio_Battery)
                {
                    GameObject CL_Bullet = Instantiate(Bullet2, transform.position, Quaternion.identity);
                    Rigidbody2D rb = CL_Bullet.GetComponent<Rigidbody2D>();

                    CL_Bullet.transform.up = controller.CursorDirection;
                    rb.velocity = controller.CursorDirection.normalized * 22;
                }
                else 
                { 
                    GameObject CL_Bullet = Instantiate(Bullet,transform.position,Quaternion.identity);
                    Rigidbody2D rb = CL_Bullet.GetComponent<Rigidbody2D>();

                    CL_Bullet.transform.up = controller.CursorDirection;
                    rb.velocity = controller.CursorDirection.normalized * 15;
                }
                
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
