using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GS_KIRISAKI_KIRI : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] GameObject AttackArea;
    public GameObject MainObject;
    bool one = false;

    private void FixedUpdate()
    {
        if (MainObject) return;
        if (one) return;
        KIRISAKI_finish();
        one = true;
    }
    public void KIRISAKI_finish() 
    {

        animator.Play("ñ∂çUåÇ");
        rb.velocity = Vector3.zero;
        GameObject CL_Attack = Instantiate(AttackArea, transform.position, Quaternion.identity);
        Destroy(CL_Attack, 0.8f);
        Destroy(gameObject, 2);

    }
}
