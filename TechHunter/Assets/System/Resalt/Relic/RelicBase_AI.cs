using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicBase_AI : MonoBehaviour
{
    public GameObject AttackObject;
    public PlayerController playerController => PlayerController.Instance;
    public virtual void BaseAction()
    {

    }

}
