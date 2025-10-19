using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Add_BuffArea : MonoBehaviour
{
    [SerializeField] bool SendBuff = false;
    [SerializeField] BuffData buffData;
    PlayerController controller => PlayerController.Instance;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("PlayerArea") && !SendBuff) 
        { 
            SendBuff = true;
            controller.playerBuff.SpawnBuff(buffData);
        }
    }
}
