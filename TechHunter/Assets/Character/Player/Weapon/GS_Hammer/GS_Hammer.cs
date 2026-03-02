using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GS_Hammer : MonoBehaviour
{
    float AttackCont = 0;
    int Phase = 0;
    PlayerController playerController => PlayerController.Instance;
    [SerializeField] RelicData relicData;
    [SerializeField] Animator animator;
    [SerializeField] GameObject Bullet;
    [SerializeField] AudioClip AudioClip;
    [SerializeField] GameObject AttackPoint;
    [SerializeField] GameObject AttackPoint1;
    [SerializeField] GameObject Bullet1;
    [SerializeField] GameObject Bullet2;
    [SerializeField] GameObject Bullet3;
    [SerializeField] GameObject Smoke;
    [SerializeField] GameObject SmokeAttack;
    [SerializeField] AudioClip audioClip;
    [SerializeField] ParticleSystem particleSystem;
    [SerializeField] PlayerAttack playerAttack;
    [SerializeField] weapondata weapondata2;
    [SerializeField] float MIN_KIRI_SPEED = 1;
    [SerializeField] float MAX_KIRI_SPEED = 1;
    public int ConboNumber = 0;
    PlayerController controller => PlayerController.Instance;
    AudioManager audioManager => AudioManager.instance;
    // Start is called before the first frame update
    void Start()
    {
        if (playerController.relicDatas.Count != 0)
        {
            foreach (var relic in playerController.relicDatas)
            {
                if (relic == null) continue;
                if (relic.RelicDataObject == null) continue;
                if (relicData == null) continue;
                if (relicData.RelicDataObject == null) continue;

                if (relic.RelicDataObject.name == relicData.RelicDataObject.name)
                {
                    var kit = relic.RelicDataObject
                        .GetComponent<Relic_EightsPortableExpeditionKit>();

                    if (kit == null) continue;

                    ConboNumber = kit.weaponConbos[0];
                    kit.AddConbo(0);
                    break;
                }
            }
        }
    }

    // Update is called once per frame
    private void FixedUpdate()
    {

        AttackCont += Time.fixedDeltaTime;
       

        if (Phase == 0)
        {
            

            if (ConboNumber != 0) 
            {
                particleSystem.emissionRate = 50;
                if (ConboNumber == 1) 
                { 
                transform.localScale = new Vector3 (1, -1, 1);
                }
            }

            transform.up = controller.CursorDirection;
            if (ConboNumber == 3)
            {
                animator.Play("ç\Ç¶ÇP");
            }
            else 
            { 
            transform.Rotate(0, 0, 90);
            animator.Play("ç\Ç¶");
            }
            
            if (AttackCont > 0.4)
            {
                audioManager.PlaySE(AudioClip);
                
                Phase = 1;
                AttackCont = 0;

                GameObject AttackArea = Bullet;
                Vector3 pos = AttackPoint.transform.position;
                if (ConboNumber == 0)
                {
                    animator.Play("çUåÇ");
                    pos = AttackPoint.transform.position;
                }
                else 
                {
                    for (int i = 0; i < ConboNumber; i++) 
                    {
                        SpawnKIRI();
                    }
                    
                }
                if (ConboNumber == 1) 
                { 
                    AttackArea = Bullet1;
                    animator.Play("çUåÇ");
                    pos = AttackPoint.transform.position;
                }
                if (ConboNumber == 2) 
                {
                    AttackArea = Bullet2;
                    animator.Play("çUåÇÇP");
                    pos = transform.position;
                }
                if (ConboNumber == 3)
                {
                    AttackArea = Bullet3;
                    animator.Play("çUåÇÇQ");
                    pos = AttackPoint1.transform.position;

                    GameObject cl_Smog = Instantiate(SmokeAttack, pos, Quaternion.identity);
                    ALL_SystemManager.Instance.camera_Controller.Shake(0.2f,0.2f);
                    AudioManager.instance.PlaySE(audioClip);

                    cl_Smog.transform.up = controller.CursorDirection;
                    Destroy(cl_Smog, 3);
                }

                GameObject CL_Bullet = Instantiate(AttackArea, pos, Quaternion.identity);
           
                CL_Bullet.transform.up = controller.CursorDirection;
                Destroy(CL_Bullet, 0.2f);
            }

        }
        if (Phase == 1)
        {
            if (AttackCont > 0.3)
            {

                particleSystem.emissionRate = 0;
                if (ConboNumber == 0)
                {
                    animator.Play("è¡Ç¶ÇÈ");
                }
                else 
                {
                    animator.Play("è¡é∏");
                }
                Phase = 2;
                AttackCont = 0;

               
                
            }
        }
        if (Phase == 2)
        {

            if (AttackCont > 0.8)
            {
                Destroy(gameObject);
            }

        }

    }

    void SpawnKIRI()
    {
        GameObject CL_kiri = Instantiate(Smoke, transform.position, Quaternion.identity);
        GS_KIRISAKI_KIRI kiri = CL_kiri.GetComponent<GS_KIRISAKI_KIRI>();
        kiri.MainObject = gameObject;
        Rigidbody2D rb = CL_kiri.GetComponent<Rigidbody2D>();
        Vector2 randomDirection = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
        rb.velocity = randomDirection.normalized * Random.Range(MIN_KIRI_SPEED, MAX_KIRI_SPEED);
    }
}
