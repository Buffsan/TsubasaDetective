using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DK_Needle : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] CircleCollider2D AttackArea;
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;

    PlayerController playerController => PlayerController.Instance;

    [SerializeField] BuffData buffData;

    public GameObject Target;
    public GameObject Player;

    float TargetDistance = 0;

    float DieTimer = 0;

    public float Timer = 10;

    public charaDamage charaDamage;

    public bool Hit = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (Target != null && !Hit) 
        {

            Vector2 velo = Target.transform.position - transform.position;

            TargetDistance = Vector2.Distance(Target.transform.position, transform.position);

            transform.up = velo;
            rb.velocity = velo.normalized * 40 ;

        }
        if (Hit) 
        { 
        DieTimer += Time.deltaTime;
            if (DieTimer > Timer) 
            {
                DieObject();
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyArea")&&!Hit&& TargetDistance < 2) 
        {
            charaDamage = other.GetComponent<charaDamage>();
            if (!charaDamage) return;

            Vector2 targetPos = other.transform.position;
            playerController.playerBuff.SpawnBuff(buffData);



            AttackArea.enabled = false;
            animator.Play("Attack");
            rb.velocity = Vector3.zero;
            Hit = true;
            transform.parent = other.transform;
            Destroy(rb);
            
        }
    }
    public void DieObject()
    {
        Destroy(gameObject);
    }
}
