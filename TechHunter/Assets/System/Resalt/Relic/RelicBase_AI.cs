using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicBase_AI : MonoBehaviour
{
    public GameObject AttackObject;
    public PlayerController playerController => PlayerController.Instance;
    public AudioManager audioManager => AudioManager.instance;
    public void AudioPlay(AudioClip clip) 
    {
        audioManager.isPlaySE(clip);
    }
    public virtual void BaseAction()
    {

    }

}
