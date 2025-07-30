using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlay : MonoBehaviour
{
    public AudioSource Asource;

    public float PlayCount = 0;
    public string AudioName;
    public bool Dell =false;
    void Update()
    {
        if (PlayCount < 0.2) 
        {
            PlayCount += Time.deltaTime;
        }

        if (Asource != null)
        {
            if (!Asource.isPlaying)
            {
                Destroy(gameObject);
            }
            if (Dell) 
            {
                Destroy(gameObject);
            }
        }
        else { Debug.Log("音がねぇ"); }
    }
    public void isCL_PlaySE(AudioClip Clip)
    {
        //Debug.Log(Clip);
        Asource.clip = Clip;
        Asource.Play();
    }

    public AudioClip GetCurrentClip()
    {
        return Asource.clip; // 現在再生中のクリップを返す
    }

    public bool IsPlaying()
    {
        return Asource.isPlaying; // 再生中かどうかを確認
    }
    public void Stop() 
    { 
    Asource.Stop();
    }
}
