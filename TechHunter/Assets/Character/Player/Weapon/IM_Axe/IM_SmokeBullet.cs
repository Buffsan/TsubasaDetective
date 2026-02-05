using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IM_SmokeBullet : MonoBehaviour
{
    public Rigidbody2D rb;

    bool Attack = false;
    [SerializeField] ParticleSystem particleSystem;
    [SerializeField] GameObject Body;
    [SerializeField] GameObject AttackArea;
    [SerializeField] GameObject AttackArea1;
    [SerializeField] GameObject SmogEffect;
    [SerializeField] float AttackCountNumber = 5;
    [SerializeField] float AttackTime = 0.4f;
    [SerializeField] float DestroyTime = 5;

    float AttackCount = 0;
    int AttackNumber = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (Attack)
        {
            AttackCount += Time.fixedDeltaTime;
            if (AttackCount > AttackTime)
            {
                GameObject CL_AttackArea = Instantiate(AttackArea1, transform.position, Quaternion.identity);
                Destroy(CL_AttackArea, 0.1f);
                AttackCount = 0;
                AttackNumber ++;
                if (AttackNumber > AttackCountNumber) 
                {
                    Destroy(gameObject);
                }
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyArea") && !Attack) 
        {
            charaDamage charadamage = other.GetComponent<charaDamage>();
            if (!charadamage) return;
            Attack = true;
            GameObject CL_AttackArea = Instantiate(AttackArea,transform.position,Quaternion.identity);
            Destroy(CL_AttackArea,0.1f);
            GameObject CL_SmogEffect = Instantiate(SmogEffect, transform.position, Quaternion.identity);
            //Destroy(SmogEffect, 4f);
            Destroy(Body);
            Destroy(gameObject, DestroyTime);
            particleSystem.emissionRate = 0;
            rb.velocity = Vector3.zero;
        }
    }
}
