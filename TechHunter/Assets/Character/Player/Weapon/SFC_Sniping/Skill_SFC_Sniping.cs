using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_SFC_Sniping : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] GameObject Bullet;
    [SerializeField] GameObject AttackArea;
    [SerializeField] GameObject Inpact;
    [SerializeField] float BulletSpeed = 20;

    int DirectionMove = 1;
    float Count = 0;

    [SerializeField] int Phase = 0;

    [SerializeField] BuffData Debuff;

    [SerializeField] List<AudioClip> audioClips = new List<AudioClip>();
    [SerializeField] List<float> PhaseTimes = new List<float>();
    [SerializeField] List<AnimationClip> animationClips = new List<AnimationClip>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Count += Time.deltaTime;
        if (Phase == 0) 
        {
            if (Count >= PhaseTimes[0])
            {
                PlayerController.Instance.playerBuff.SpawnBuff(Debuff);
                PlayerController.Instance.isCursorDirection();
                if (PlayerController.Instance.CursorDirection.x > 0)
                {
                    DirectionMove = 1;
                }
                else 
                {
                    DirectionMove = -1;
                }
                transform.localScale = new Vector3(transform.localScale.x * DirectionMove, transform.localScale.y, 1);
                animator.Play(animationClips[0].name, 0, 0);
                Phase++;
                Count = 0;
            }
        }
        if (Phase == 1) 
        {
            if (Count >= PhaseTimes[1])
            {


                animator.Play(animationClips[1].name, 0, 0);
                Phase++;
                Count = 0;
            }
        }
        if (Phase == 2)
        {
            if (Count >= PhaseTimes[2])
            {
                GameObject CL_Attack = Instantiate(Inpact, transform.position, Quaternion.identity);
                Destroy(CL_Attack, 1);
                GameObject CL_Attack2 = Instantiate(AttackArea, transform.position, Quaternion.identity);
                Destroy(CL_Attack2, 0.1f);

                ALL_SystemManager.Instance.camera_Controller.Shake(0.2f, 0.2f);
                AudioManager.instance.PlaySE(audioClips[0]);
                animator.Play(animationClips[2].name, 0, 0);
                Phase++;
                Count = 0;
            }
        }
        if (Phase == 3)
        {
            if (Count >= PhaseTimes[3])
            {
                
                Phase++;
                Count = 0;
            }
        }
        if (Phase == 4)
        {
            if (Count >= PhaseTimes[4])
            {
                GameObject CL_Attack = Instantiate(Bullet, transform.position, Quaternion.identity);
                Rigidbody2D rb = CL_Attack.GetComponent<Rigidbody2D>();
                rb.velocity = Vector2.right.normalized * BulletSpeed *DirectionMove;
                animator.Play(animationClips[3].name, 0, 0);
                ALL_SystemManager.Instance.camera_Controller.Shake(0.2f, 0.4f);
                Destroy(CL_Attack, 3);
                AudioManager.instance.PlaySE(audioClips[1]);
                Count = 0;
                Phase++;
            }
        }
        if (Phase == 5)
        {
            if (Count >= PhaseTimes[5])
            {
                
                Destroy(gameObject, 4);
                Phase++;
                Count = 0;
            }
        }

    }
}
