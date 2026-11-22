using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CG_AutomaticCrossbow : MonoBehaviour
{
    public GameObject SkillObject;
    [SerializeField] GameObject Arrow;
    [SerializeField] AudioClip Clip;
    [SerializeField] int DefaltArrows = 50;

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

        int DoubleArrowChance = 1;
        if (SkillObject) 
        {
            DoubleArrowChance = 3;
        }

        AttackCount += Time.deltaTime;
        if (AttackCount > AttackTime / DoubleArrowChance && ArrowCount < DefaltArrows * DoubleArrowChance) 
        {
            AttackCount = 0;
            ArrowCount++;

            manager.PlaySE(Clip);
            GameObject CL_Arrow = Instantiate(Arrow,transform.position,Quaternion.identity);
            Rigidbody2D rb = CL_Arrow.GetComponent<Rigidbody2D>();

            float A_Random = Random.Range(-8, 8) * Mathf.Deg2Rad;

            Vector2 RadomDirection = new Vector2(
            Mathf.Cos(A_Random) * controller.CursorDirection.x - Mathf.Sin(A_Random) * controller.CursorDirection.y,
            Mathf.Sin(A_Random) * controller.CursorDirection.x + Mathf.Cos(A_Random) * controller.CursorDirection.y);

            rb.velocity = RadomDirection.normalized * 6;
            CL_Arrow.transform.up = RadomDirection;
            CL_Arrow.transform.Rotate(0,0,90);

            Destroy( CL_Arrow ,8f);
        }
        if (ArrowCount >= DefaltArrows) 
        { 
        Destroy(gameObject);
        }

    }
}
