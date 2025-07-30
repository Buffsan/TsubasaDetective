using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyPlayerAttack : MonoBehaviour
{
    // Start is called before the first frame update
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("EnemyArea")) 
        {

            charaDamage charaDamage = other.GetComponent<charaDamage>();

            if (charaDamage != null)
            {
                Destroy(gameObject);
            }
        }
    }
}
