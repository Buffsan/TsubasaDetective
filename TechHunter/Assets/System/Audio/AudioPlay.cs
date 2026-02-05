using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlay : MonoBehaviour
{
    public AudioSource Asource;
    public float PlayCount = 0;
    public string AudioName;
    public bool Dell =false;
    public void Play(AudioClip clip)
    {
        Asource.clip = clip;
        Asource.Play();
        StartCoroutine(ReleaseAfterPlay());
    }
    IEnumerator ReleaseAfterPlay()
    {
        yield return new WaitWhile(() => Asource.isPlaying);
        AudioPool.Instance.Release(Asource);
    }
    public void Stop() 
    { 
        Asource.Stop();
    }
}
