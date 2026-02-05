using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_DK_Sword : MonoBehaviour
{
    [SerializeField] GameObject AttackArea;
    [SerializeField] float AttackDestroyTime = 0.1f;
    [SerializeField] float DestroyTime = 0.2f;
    [SerializeField] float AttackTime = 0.2f;
    [SerializeField] GameObject Effect;
    [SerializeField] Animator animator;
  
    float Count = 0;
    int Phase = 0;

    Vector2 Direction = Vector2.zero;

    PlayerController playerController =>PlayerController.Instance;
    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;

    private void FixedUpdate()
    {
        Count += Time.fixedDeltaTime;
        if (Phase == 0) 
        {
            Direction = playerController.CursorDirection.normalized;
            transform.up = Direction.normalized;

            animator.Play("‘Ò‹@");
            if (Count > AttackTime) 
            {
                Count = 0;
                Phase++;
            }
        }
        if (Phase == 1) 
        {
            GameObject CL_Effect = Instantiate(Effect, transform.position, Quaternion.identity);
            AttackPool.Instance.Get(AttackArea, transform.position, Direction.normalized, AttackDestroyTime);
            CL_Effect.transform.up = Direction.normalized;
            CL_Effect.transform.Rotate(0,0,90);

            ALL_System.camera_Controller.Shake(0.1f, 0.1f);
            Destroy(CL_Effect, AttackDestroyTime*3);
            animator.Play("Ø‚é");

            Count = 0;
            Phase++;
        }
        if (Phase == 2) 
        {
            if (Count > DestroyTime) 
            {
                Destroy(gameObject);
            }
        }
    }
}
