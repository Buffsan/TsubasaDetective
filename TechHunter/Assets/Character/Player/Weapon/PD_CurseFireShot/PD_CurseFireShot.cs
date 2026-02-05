using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PD_CurseFireShot : MonoBehaviour
{

    float AttackCount = 0;
    int P = 0;
    [SerializeField] Animator animator;
    [SerializeField] GameObject Effect;
    [SerializeField] GameObject AttackArea;
    [SerializeField] GameObject AttackArea2;
    [SerializeField] GameObject AttackArea3;
    [SerializeField] GameObject AttackPoint;
    [SerializeField] AudioClip Clip;

    ALL_SystemManager systemManager => ALL_SystemManager.Instance;

    AudioManager audiomanager => AudioManager.instance;

    PlayerController playerController => PlayerController.Instance;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        AttackCount += Time.deltaTime;
        if (P == 0) 
        {
            animator.Play("‘Ò‹@",0,0);
            P++;
        }
        if (AttackCount > 0.5 && P == 1)
        {
            P++;
            AttackCount = 0;
            animator.Play("U‚è‰º‚ë‚·", 0, 0);


        }
        if (P == 1)
        {
            playerController.isCursorDirection();
            transform.up = (Vector2)playerController.CursorDirection;
            transform.Rotate(0, 0, 90);
        }
        if (AttackCount > 0.1 && P == 2) 
        {
            P++;
            AttackPool.Instance.Get(AttackArea, transform.position, Quaternion.identity,0.1f);
            AttackPool.Instance.Get(AttackArea2, transform.position, Quaternion.identity, 0.2f);
            AttackPool.Instance.Get(AttackArea3, transform.position, Quaternion.identity, 0.2f);
            GameObject CL_Effect = Instantiate(Effect, AttackPoint.transform.position, Quaternion.identity);
            
            systemManager.camera_Controller.Shake(0.2f, 0.2f);
            Destroy(CL_Effect,5);
            
            audiomanager.PlaySE(Clip);
            AttackCount=0;
        }
        if (AttackCount > 0.5 && P == 3) 
        {
            AttackCount = 0;
            P++;
            animator.Play("Á‚¦‚é");
            
        }
        if (AttackCount > 2 && P == 4) 
        { 
        Destroy(gameObject);
        }
    }
}
