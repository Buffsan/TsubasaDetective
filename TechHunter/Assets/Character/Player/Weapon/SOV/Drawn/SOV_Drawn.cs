using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SOV_Drawn : MonoBehaviour
{
    [SerializeField] GameObject Slash;
    [SerializeField] GameObject AttackArea;

    Rigidbody2D rb;

    bool Right = false;
    float AttackCount = 0;
    float MoveCount = 0;

    PlayerController controller => PlayerController.Instance;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.velocity = controller.LastMoveInput.normalized*5;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {MoveCount += Time.deltaTime;
        if (MoveCount < 3)
        {
            
            if (MoveCount > 0.3)
            {
                rb.velocity = new Vector2(rb.velocity.x * 0.7f, rb.velocity.y * 0.7f);
            }
            if (AttackCount > 0.1)
            {
                if (Right)
                {
                    GameObject CL_Slash = Instantiate(Slash, transform.position, Quaternion.identity);
                    CL_Slash.transform.localScale = new Vector2(4, 4);
                    Destroy(CL_Slash, 0.5f);
                    Right = false;

                    GameObject CL_AttackArea = Instantiate(AttackArea, new Vector2(transform.position.x + 0.9f, transform.position.y), Quaternion.identity);
                    Destroy(CL_AttackArea, 0.05f);
                }
                else
                {
                    GameObject CL_Slash = Instantiate(Slash, transform.position, Quaternion.identity);
                    CL_Slash.transform.localScale = new Vector2(-4, -4);
                    Destroy(CL_Slash, 0.5f);
                    Right = true;
                    GameObject CL_AttackArea = Instantiate(AttackArea, new Vector2(transform.position.x - 0.9f, transform.position.y), Quaternion.identity);
                    Destroy(CL_AttackArea, 0.05f);
                }
                AttackCount = 0;
            }
            else
            {
                AttackCount += Time.deltaTime;
            }
        }
        else 
        {
            rb.velocity = new Vector2(0, rb.velocity.y + 3);
            if (MoveCount > 7) 
            {
                Destroy(gameObject);
            }
        }
    }
}
