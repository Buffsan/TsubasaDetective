using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DK_BattleFlag : MonoBehaviour
{


    public float AttackCount = 0;
    [SerializeField] List<GameObject> Attacks = new List<GameObject>();
    [SerializeField] GameObject AttackPoint;

    [SerializeField] GameObject Effect;
    [SerializeField] GameObject SlashEffect;
   
    [SerializeField] GameObject Bullet;

    [SerializeField] BuffData buffData;

    [SerializeField] Animator animator;

    [SerializeField] AudioClip clip1;
    [SerializeField] AudioClip clip2;
    [SerializeField] AudioClip clip3;

    float AttackCount1 = 0;

    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;
    PlayerController playerController => PlayerController.Instance;
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
                playerController.playerBuff.SpawnBuff(buffData);
                audioManager.isPlaySE(clip3);
                GameObject CL_Attack = Instantiate(Attacks[0],transform.position, Quaternion.identity);
                GameObject CL_effect = Instantiate(SlashEffect, transform.position, Quaternion.identity);
                Destroy(CL_Attack, 0.15f);
                Destroy(CL_effect, 1);

                

                AttackCount = 0;
                animator.Play("Œf‚°‚é", 0,0); 
                phase = 1;
            }
        }
        if (phase == 1)
        {
            if (AttackCount > 1)
            {
                GameObject CL_Attack = Instantiate(Attacks[1],transform.position, Quaternion.identity);
                Destroy(CL_Attack, 0.1f);
                transform.SetParent(null);
                audioManager.isPlaySE(clip1);
                animator.Play("“Ë‚«‚³‚·");
                ALL_System.camera_Controller.Shake(0.2f, 0.2f);
                AttackCount = 0;
                phase = 2;
                GameObject CL_effect = Instantiate(Effect, transform.position, Quaternion.identity);
                Destroy(CL_effect, 1);
            }
        }
        if (phase == 2)
        {
            
            AttackCount1 += Time.fixedDeltaTime;
            if (AttackCount > 10)
            {
                animator.Play("Á‚¦‚é");
                AttackCount = 0;
                phase = 3;
            }
            if (AttackCount1 > 1) 
            {
                AttackCount1 = 0;
                GameObject CL_Attack = Instantiate(Attacks[2],transform.position, Quaternion.identity);
                Destroy( CL_Attack ,0.1f);
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
