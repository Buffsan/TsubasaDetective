using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KARAKAZE_AI : MonoBehaviour
{

    [SerializeField] GameObject Slash;
    [SerializeField] GameObject Bourei;
    [SerializeField] Animator animator;
    [SerializeField] bool SlashSpawn = false;

    [SerializeField] float SlashRange = 3;

    PlayerController playerController => PlayerController.Instance;

    int Phase = 0;
    float Count = 0;
    Vector2 Direction = Vector2.zero;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Count += Time.fixedDeltaTime;
        if (Phase == 0) 
        {
            Direction = playerController.cursorController.MousePos - transform.position;
            transform.up = playerController.CursorDirection.normalized;

            if (Count > 1) 
            { 
                Phase = 1;
                Count = 0;
                

                if (SlashSpawn)
                {
                    GameObject CL_Slash = Instantiate(Slash, transform.position, Quaternion.identity);
                    CL_Slash.transform.up = Direction.normalized;
                    
                    
                    Rigidbody2D rb = CL_Slash.GetComponent<Rigidbody2D>();
                    rb.velocity = Direction.normalized * 12;
                    Destroy(CL_Slash,SlashRange);
                }
                
                

                animator.Play("çUåÇ");
                Destroy(gameObject, 3);
            }
        }
        
    }
}
