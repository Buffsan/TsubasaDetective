using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicBase_AI : MonoBehaviour
{
    public GameObject AttackObject;
    public PlayerController playerController => PlayerController.Instance;
    public AudioManager audioManager => AudioManager.instance;

    public void SpawnObjectNow(GameObject SpawnObject, Vector2 Target, float DestroyTime)
    {
        GameObject CL_Object = Instantiate(SpawnObject, Target, Quaternion.identity);
        Destroy(CL_Object, DestroyTime);
    }
    public GameObject SpawnObjectNow_Input(GameObject SpawnObject, Vector2 Target, float DestroyTime)
    {
        GameObject CL_Object = Instantiate(SpawnObject, Target, Quaternion.identity);
        Destroy(CL_Object, DestroyTime);
        return CL_Object;
    }
    public Vector2 PlayerDirection(Vector2 target)  
    { 
        Vector2 playerDirection = (Vector2)playerController.gameObject.transform.position - target;
        return playerDirection.normalized;
    }
    public void AudioPlay(AudioClip clip) 
    {
        audioManager.PlaySE(clip);
    }
    public virtual void BaseAction()
    {

    }

}
