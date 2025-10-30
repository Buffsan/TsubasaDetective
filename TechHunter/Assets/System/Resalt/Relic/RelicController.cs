using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicController : RelicBase
{

    public RelicBase_AI relicBase;
    private void Start()
    {
        if (relicBase == null) 
        {
            if (relicData == null) { Debug.Log("データがないね"); return; }

            GameObject Relic = Instantiate(relicData.RelicDataObject,transform.position,Quaternion.identity);
            Relic.transform.parent = transform;
            relicBase = Relic.GetComponent<RelicBase_AI>();
        }
    }
    private void FixedUpdate()
    {
        if(playerController.movetype != PlayerController.MoveType.NoAction && playerController.movetype != PlayerController.MoveType.Wait)
        relicBase.BaseAction();//行動の別途挿入
    }
}
