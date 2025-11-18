using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static PlayerSkillBase;

public class OW_HarpoonVer2 : MonoBehaviour
{

    PlayerController playerController => PlayerController.Instance;
    [SerializeField] GameObject SkillWeapon;
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float Speed = 6f;
    public Vector2 Direction = Vector2.zero;

    bool Attacked = false;
    // Start is called before the first frame update
    void Start()
    {
        transform.up = Direction.normalized;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        rb.velocity = Direction.normalized * Speed;
    }
    void Shot()
    {
        Attacked = true;
        float angle = 40;
        animator.Play("ê[Ç≠ä—Ç≠", 0, 0);
        for (int i = 0; i < 5; i++)
        {
            GameObject CL_Harpoon = Instantiate(SkillWeapon, transform.position, Quaternion.identity);
            Rigidbody2D rb = CL_Harpoon.GetComponent<Rigidbody2D>();
            Vector2 bulletDirection = Quaternion.Euler(0, 0, angle - 20 * i) * Direction.normalized;
            CL_Harpoon.transform.up = bulletDirection;
            rb.velocity = bulletDirection * 20;
            Destroy(CL_Harpoon, 0.8f);
        }
        Destroy(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyArea") && !Attacked)
        {
            Shot();
        }
    }
}
