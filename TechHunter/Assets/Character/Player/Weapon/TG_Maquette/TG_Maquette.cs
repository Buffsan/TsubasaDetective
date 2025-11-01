using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TG_Maquette : MonoBehaviour
{
    [SerializeField] GameObject Arrow;
    [SerializeField] AudioClip Clip;
    [SerializeField] GameObject AttackPont;
    [SerializeField] Animator animator;
    [SerializeField] AudioClip AttackTarget;
    public float WaitTime = 0;

    AudioManager manager => AudioManager.instance;
    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;

    int ArrowCount = 0;

    float AttackCount = 0;
    float AttackTime = 0.12f;

    PlayerController controller => PlayerController.Instance;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Vector2 Direction = controller.cursorController.MousePos - transform.position;
        transform.up = Direction;
        transform.Rotate(0, 0, 90);
        //transform.Rotate(0, 0, 90);

        AttackCount += Time.deltaTime;
        if (ArrowCount == 0 && WaitTime < AttackCount) 
        { 
            ArrowCount++;
            AttackCount = 0;
            animator.Play("oŒ»");
            manager.isPlaySE(AttackTarget);
        }
        if (ArrowCount == 1 && AttackCount > 1.5f)
        {
            ArrowCount ++;
            GameObject CL_Arrow = Instantiate(Arrow, AttackPont.transform.position, Quaternion.identity);
            Rigidbody2D rb = CL_Arrow.GetComponent<Rigidbody2D>();

            ALL_System.camera_Controller.Shake(0.2f,0.1f);
            CL_Arrow.transform.up = Direction.normalized;
            CL_Arrow.transform.Rotate(0, 0, 180);

            manager.isPlaySE(Clip);
            Destroy(CL_Arrow, 10);
            rb.velocity = Direction.normalized * 30;

            animator.Play("ŽËŒ‚");
        }
        if (AttackCount > 7)
        {
            Destroy(gameObject);
        }

    }
}
