using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBuffsBase : MonoBehaviour
{

    public GameObject My;

    public float BuffTime =1;
    public float BuffCount = 0;
    public BuffData buffData;

    public string BuffNAME;

    public float Regen;

    public float SPEED;
    public float MultiplySPEED;

    public float ATK;
    public float MultiplyATK;

    public float ATKDF;
    public float MultiplyATKDF;

    // Start is called before the first frame update
    void Start()
    {
        My = gameObject;

     BuffNAME = buffData.BuffNAME;

        BuffTime = buffData.BuffTime;

     Regen = buffData.Regen;

     SPEED = buffData.SPEED;
     MultiplySPEED = buffData.MultiplySPEED;

     ATK = buffData.ATK;
     MultiplyATK = buffData.MultiplyATK;

     ATKDF = buffData.ATKDF;
     MultiplyATKDF = buffData.MultiplyATKDF;

    }

    // Update is called once per frame
    private void FixedUpdate()
    {

        BuffCount += Time.deltaTime;
        /*
        if (BuffTime < BuffCount) 
        { 
        Destroy(gameObject);
        }*/
    }
    public void Die() 
    { 
    
        Destroy(My);

    }
}
