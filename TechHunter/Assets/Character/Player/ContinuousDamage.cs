using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ContinuousDamage : MonoBehaviour
{
    [SerializeField] float AttackTime = 0.2f;
    [SerializeField] float StartCount = 0;

    [SerializeField] GameObject BaseRotateObject;
    [SerializeField] float AttackDieTime = 0.1f;
    float AttackTimeCount = 0;

    [SerializeField] int AttackNumber = 0;
    int AttackNumberCount = 0;

    [SerializeField] GameObject AttackArea;
    private void Start()
    {
        AttackTimeCount = StartCount;
    }
    // Start is called before the first frame update
    private void FixedUpdate()
    {
        AttackTimeCount += Time.fixedDeltaTime;
        if (AttackTimeCount > AttackTime) 
        {
            AttackNumberCount++;
            AttackTimeCount = 0;

            if (BaseRotateObject)
            {
                AttackPool.Instance.Get(AttackArea, transform.position, Quaternion.Euler(BaseRotateObject.transform.eulerAngles), AttackDieTime);
            }
            else 
            { 
                AttackPool.Instance.Get(AttackArea, transform.position, Quaternion.identity, AttackDieTime);
            }

           
            if (AttackNumberCount > AttackNumber) 
            {
                Destroy(gameObject);
            }
        }
    }
}
