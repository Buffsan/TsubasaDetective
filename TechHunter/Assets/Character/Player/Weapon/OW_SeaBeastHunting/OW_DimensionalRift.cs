using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OW_DimensionalRift : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] GameObject AttackArea1;
    [SerializeField] GameObject AttackArea2;

    [SerializeField] AudioClip clip;

    AudioManager audioManager => AudioManager.instance;

    int Phase = 0;
    float AttackCount =0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        AttackCount += Time.fixedDeltaTime;
        if (Phase == 0)
        {
            if (AttackCount > 1)
            {
                animator.Play("”j—ô", 0,0);
                GameObject CL_AttackArea = Instantiate(AttackArea1,transform.position,Quaternion.identity);
                Destroy(CL_AttackArea,1f);
                if (AttackArea2)
                {
                    GameObject CL_AttackArea2 = Instantiate(AttackArea2, transform.position, Quaternion.identity);
                    Destroy(CL_AttackArea2, 0.8f);
                }
                AttackCount = 0;
                audioManager.isPlaySE(clip);
                Phase++; 
            }
        }
        if (Phase == 1) 
        {
            if (AttackCount > 2) 
            {
            Destroy(gameObject);
            }
        }
    }
}
