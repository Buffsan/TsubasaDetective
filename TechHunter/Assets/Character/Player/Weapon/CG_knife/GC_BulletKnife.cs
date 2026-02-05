using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GC_BulletKnife : MonoBehaviour
{
    bool Hit = false;

    public CircleCollider2D circle;
    //public Animator animator;
    public Rigidbody2D rb;
    int Phase = 0;
    [SerializeField]float DestroyTime = 1;
    float Timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy")) 
        { 
            if(!Hit)Destroy(gameObject, DestroyTime);
            Hit = true;
            
        }
    }
}
