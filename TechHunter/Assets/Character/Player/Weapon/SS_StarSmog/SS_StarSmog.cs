using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SS_StarSmog : MonoBehaviour
{
    [SerializeField] GameObject Arrow;
    [SerializeField] AudioClip Clip;
    [SerializeField] GameObject AttackPont;

    GameObject Save_Laser;
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
        //transform.Rotate(0, 0, 90);

        AttackCount += Time.deltaTime;
        if (Save_Laser != null) 
        {
            Save_Laser.transform.position = AttackPont.transform.position;
            Save_Laser.transform.up = controller.CursorDirection.normalized;
            Save_Laser.transform.Rotate(0, 0, 180);
        }
        if (ArrowCount == 0)
        {
            ArrowCount = 1;
            GameObject CL_Arrow = Instantiate(Arrow, AttackPont.transform.position, Quaternion.identity);
            Save_Laser = CL_Arrow;
            //Rigidbody2D rb = CL_Arrow.GetComponent<Rigidbody2D>();

            controller.isCursorDirection();

            Save_Laser.transform.up = controller.CursorDirection.normalized;
            Save_Laser.transform.Rotate(0, 0, 180);

            manager.isPlaySE(Clip);
            Destroy(Save_Laser, 10);
            //rb.velocity = controller.CursorDirection.normalized * 4;
        }
        if (AttackCount > 5)
        {
            Destroy(gameObject);
        }

    }
}
