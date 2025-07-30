using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CG_Crossbow : MonoBehaviour
{
    [SerializeField] GameObject Arrow;
    [SerializeField] AudioClip Clip;
    [SerializeField] GameObject AttackPont;

    AudioManager manager => AudioManager.instance;

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
        controller.isCursorDirection();
        transform.up = controller.CursorDirection;
        transform.Rotate(0, 0, 90);

        AttackCount += Time.deltaTime;
        if (ArrowCount == 0) 
        { 
            ArrowCount = 1;
            GameObject CL_Arrow = Instantiate(Arrow, AttackPont.transform.position, Quaternion.identity);
            Rigidbody2D rb = CL_Arrow.GetComponent<Rigidbody2D>();

            controller.isCursorDirection();

            CL_Arrow.transform.up = controller.CursorDirection.normalized;
            CL_Arrow.transform.Rotate(0,0,90);

            manager.isPlaySE(Clip);
            Destroy(CL_Arrow, 10);
            rb.velocity = controller.CursorDirection.normalized * 4;
        }
        if (AttackCount > 1) 
        { 
        Destroy(gameObject);
        }
        
    }
}
