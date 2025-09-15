using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousDamage : MonoBehaviour
{
    [SerializeField] float AttackTime = 0.2f;
    float AttackTimeCount = 0;

    [SerializeField] int AttackNumber = 0;
    int AttackNumberCount = 0;

    [SerializeField] GameObject AttackArea;
    // Start is called before the first frame update
    private void FixedUpdate()
    {
        AttackTimeCount += Time.fixedDeltaTime;
        if (AttackTimeCount > AttackTime) 
        {
            AttackNumberCount++;
            AttackTimeCount = 0;
            
            GameObject CL_AttackArea = Instantiate(AttackArea,transform.position,Quaternion.identity);
            Destroy(CL_AttackArea,0.1f);
            if (AttackNumberCount > AttackNumber) 
            {
                Destroy(gameObject);
            }
        }
    }
}
