using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DK_LancethatPiercesHeresy : MonoBehaviour
{
    int Phase = 0;
    float Count = 0;
    Vector2 Direction = Vector2.zero;

    public GameObject player;

    [SerializeField] float Speed = 5;
    [SerializeField] GameObject AsultArea;
    [SerializeField] GameObject AttackArea;
    [SerializeField] Animator animator;
    [SerializeField] bool Guard = true;

    PlayerController playerController => PlayerController.Instance;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Count += Time.deltaTime;
        if (Phase == 0) 
        {
            if (playerController.gameObject == player)
            {
                Direction = playerController.CursorDirection;
            }
            else 
            {
                Direction = playerController.cursorController.MousePos - transform.position;
            }
            transform.up = Direction;
            if (Count > 0.5) 
            {
                NextPhase();
                Guard = false;
            }
        }
        if (Phase == 1) 
        {

            NextPhase();
            animator.Play("ŠÑ‚­");
            AsultArea.SetActive(true);
        }
        if (Phase == 2) 
        {
            playerController.isSpecialMove(Speed, Direction, player);
            if (Count > 0.25) 
            {
                Destroy(gameObject);
            }
        }
    }
    void NextPhase() 
    {
        Phase++;
        Count = 0;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Guard) return;
        EnemyAttack enemyAttack = other.GetComponent<EnemyAttack>();
        //Debug.Log("1");
        if (!enemyAttack) return;
        //Debug.Log("2");
        GlobalEvents.InvokeOnPlayerDamage(0, true);
        GameObject CL_AttackArea = Instantiate(AttackArea, transform.position,Quaternion.identity);
        Destroy(CL_AttackArea, 0.1f);
    }
}
