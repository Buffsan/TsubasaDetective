using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GC_BulletKnife : MonoBehaviour
{
    bool Hit = false;

    public CircleCollider2D circle;
    //public Animator animator;
    public Rigidbody2D rb;

    float Timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    
    private void FixedUpdate()
    {
        if (Hit) 
        {
            Timer += Time.deltaTime;
            if (Timer > 0.1) 
            { 
            Destroy(gameObject);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy")&& !Hit) 
        { 
        Hit = true;
            circle.enabled = false;
            //animator.Play("Attack");
            rb.velocity = Vector2.zero;
        }
    }
}
