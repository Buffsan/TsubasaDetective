using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletFire : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] Animator anim;
    [SerializeField] float LiveTime = 3;
    float liveCount = 0;
    bool Die = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (liveCount < LiveTime)
        {
            liveCount += Time.deltaTime;
        }
        else 
        {
            anim.SetInteger("Anim", 1);
            
            if (!Die) 
            { 
            Die = true;
                Destroy(gameObject,0.5f);
            }
        }
    }
}
