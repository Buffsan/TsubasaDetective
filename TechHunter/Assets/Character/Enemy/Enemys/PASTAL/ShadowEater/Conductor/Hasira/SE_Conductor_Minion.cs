using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SE_Conductor_Minion : MonoBehaviour
{

    public bool PlayerHide = false;
    public GameObject MainObject;
    // Start is called before the first frame update
    
    private void FixedUpdate()
    {
        if(!MainObject)Destroy(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        { 
        PlayerHide = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHide = false;
        }
    }
}
