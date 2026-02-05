
using UnityEngine;

public class OW_HarpoonVer2 : MonoBehaviour
{

    PlayerController playerController => PlayerController.Instance;
    [SerializeField] GameObject SkillWeapon;
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float Speed = 6f;
    [SerializeField] charaDamage saveDamage;
    public Vector2 Direction = Vector2.zero;
    [SerializeField] PlayerAttack capsuleCollider;

    [SerializeField] int MaxBounus = 10;

    [SerializeField] float BounusAttackTime = 0.5f;
    float BouusAttackCount = 0;

    [SerializeField] float dieTimer = 7;
    float dieCount = 0;

    bool Attacked = false;
    // Start is called before the first frame update
    void Start()
    {
        transform.up = Direction.normalized;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        BouusAttackCount += Time.fixedDeltaTime;
        if (Attacked)
        {
            transform.localPosition = Vector2.zero;
            dieCount += Time.fixedDeltaTime;
            if(dieCount > dieTimer) Destroy(gameObject);
        }
        else
        {
            rb.velocity = Direction.normalized * Speed;
        }
        
    }
    void Shot()
    {
        Attacked = true;
        float angle = 40;
        //animator.Play("ê[Ç≠ä—Ç≠", 0, 0);
        for (int i = 0; i < 5; i++)
        {
            GameObject CL_Harpoon = Instantiate(SkillWeapon, transform.position, Quaternion.identity);
            
            Rigidbody2D rb = CL_Harpoon.GetComponent<Rigidbody2D>();
            Vector2 bulletDirection = Quaternion.Euler(0, 0, angle - 20 * i) * Direction.normalized;
            CL_Harpoon.transform.up = bulletDirection;
            rb.velocity = bulletDirection * 20;
            Destroy(CL_Harpoon, 0.8f);
        }
        //Destroy(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyArea"))
        {
            if (Attacked)
            {
               
            }
            else
            {
                capsuleCollider.enabled = false;
               
                saveDamage = other.GetComponent<charaDamage>();
                if (!saveDamage) return;
                rb.velocity = Vector2.zero;
                //Destroy(rb);
                transform.parent = saveDamage.transform;
                Shot();
            }
        }
        else
        {
            if (other == SkillWeapon || BounusAttackTime > BouusAttackCount || MaxBounus > 10) return;

            
            MaxBounus++;
            BouusAttackCount = 0;
            PlayerAttack playerAttack = other.GetComponent<PlayerAttack>();
            if(playerAttack)
            if (Attacked)
            {
                Shot();
            }
        }
    }
}
