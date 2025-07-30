using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponAnim : WeaponBase
{
    [SerializeField] GameObject Player;
    [SerializeField] Animator animator;
    [SerializeField] PlayerController playerController;


    [SerializeField] float DieTimer = 1;
    [Space]
    [SerializeField] GameObject AttackArea;
    [SerializeField] Vector2 AttackAreaSize = Vector2.one;
    [SerializeField] GameObject AttackSpawnPoint;
    BoxCollider2D AttackAreaCollider;
    PlayerAttack playerAttack;

    float DieCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        playerController = Player.GetComponent<PlayerController>();

        gameObject.transform.SetParent(playerController.AnimatorBody.transform);

        animator = GetComponent<Animator>();

        GameObject CL_AttackArea = Instantiate(AttackArea,AttackSpawnPoint.transform.position,Quaternion.identity);
        AttackAreaCollider = CL_AttackArea.GetComponent<BoxCollider2D>();
        playerAttack = CL_AttackArea.GetComponent<PlayerAttack>();
        playerAttack.weapondata = weapondata;
        AttackAreaCollider.size = AttackAreaSize;
        Destroy(CL_AttackArea, 0.1f);
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
       
        DieCount += Time.deltaTime;
        if (DieCount > DieTimer) 
        {
            Destroy(gameObject);
        }
        if (playerController.mode == PlayerController.ModeType.M1) 
        {
            animator.SetInteger("Anim", 1);
        } else if (playerController.mode == PlayerController.ModeType.M2)
        {
            animator.SetInteger("Anim", 2);
        }
    }
}
