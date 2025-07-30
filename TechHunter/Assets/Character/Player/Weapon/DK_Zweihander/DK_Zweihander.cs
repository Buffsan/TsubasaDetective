using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DK_Zweihander : MonoBehaviour
{
    [SerializeField] GameObject AttackArea;

    float AttackCount = 0.2f;
    int AttackNumber = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        
        AttackCount += Time.deltaTime;
        if (AttackCount > 0.2 && AttackNumber < 2) 
        {
            AttackNumber++;
            AttackCount = 0;
            GameObject CL_AttackArea = Instantiate(AttackArea, transform.position, Quaternion.identity);
            Destroy( CL_AttackArea ,0.05f);
        }
        if (AttackCount > 1f) 
        {
            Destroy(gameObject);
        }

    }
}
