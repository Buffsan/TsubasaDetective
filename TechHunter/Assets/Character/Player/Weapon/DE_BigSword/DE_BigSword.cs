using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DE_BigSword : MonoBehaviour
{

    public float AttackCount = 0;
    [SerializeField] GameObject Attack;
    [SerializeField] GameObject AttackPoint;

    bool AttackBool = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        transform.localScale = new Vector3(1, 1, 1);
        if (AttackCount > 1.2)
        {

            if (!AttackBool)
            {
                AttackBool = true;

                GameObject CL_Attack = Instantiate(Attack, AttackPoint.transform.position, Quaternion.identity);
                Destroy(CL_Attack, 0.15f);
            }
        }
        if (AttackCount > 1.9) 
        {
            Destroy(gameObject);
        }

            AttackCount += Time.deltaTime;
    }
}
