using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OW_Harpoon : MonoBehaviour
{

    public float AttackCount = 0;
    public float AttackCount2 = 0;
    int Count = 0;
    [SerializeField] GameObject Attack;
    [SerializeField] GameObject AttackPoint;
    int AttackNumber = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        transform.localScale = new Vector3(1, 1, 1);
        AttackCount += Time.deltaTime;
        if (AttackCount >= 0.1) 
        {
            AttackCount2 += Time.deltaTime;
            if (AttackCount2 >= 0.03 && Count <=4) 
            {
                Count++;
                AttackCount2 = 0;
                GameObject CL_AttackArea = Instantiate(Attack, AttackPoint.transform.position, transform.rotation);

                Destroy(CL_AttackArea ,0.1f);
            }

        }
        if (AttackCount >= 1) 
        {
            Destroy(gameObject);
        }
    }
}
