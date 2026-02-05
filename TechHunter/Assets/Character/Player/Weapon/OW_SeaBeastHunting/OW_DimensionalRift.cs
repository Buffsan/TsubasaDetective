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
                AttackPool.Instance.Get(AttackArea1, transform.position, Quaternion.identity, 1);
                
                if (AttackArea2)
                {
                    AttackPool.Instance.Get(AttackArea2, transform.position, Quaternion.identity, 0.8f);
                    
                }
                AttackCount = 0;
                audioManager.PlaySE(clip);
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
