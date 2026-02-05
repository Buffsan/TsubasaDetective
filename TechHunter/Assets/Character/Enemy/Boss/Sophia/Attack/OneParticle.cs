using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneParticle : MonoBehaviour
{

    ParticleSystem particle;

    public float EffectLimit =0;
    public float DieLimit = 10;
    float EffectCount = 0;
    float DieCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        particle = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        EffectCount += Time.deltaTime;
        if (EffectCount > EffectLimit) 
        {

            particle.emissionRate = 0;

            DieCount += Time.deltaTime;
            if (DieCount > DieLimit) 
            { 
            Destroy(gameObject);
            }
        
        }
    }
}
