using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField] GameObject AudioPlayObj;
    [SerializeField] AudioClip BGM;
    public AudioSource BgmSource;
    AudioPlay audioPlay;

    public List<AudioPlay> audioPlays = new List<AudioPlay>();

    public Dictionary<AudioClip, float> lastPlayTimes = new Dictionary<AudioClip, float>();
    public float minInterval = 0.1f; // 効果音を再生する間隔（秒）

    void Start()
    {
        BgmSource = GetComponent<AudioSource>();
        if (BGM != null)
        {
            BgmSource.clip = BGM;
            BgmSource.Play();
        }

        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
    public void PlaySE(AudioClip Clip)
    {
        // 再生間隔チェック
        if (lastPlayTimes.ContainsKey(Clip))
        {
            if (Time.time - lastPlayTimes[Clip] < minInterval) return;
        }
        // Pool から AudioSource を取得して再生
        AudioSource src = AudioPool.Instance.GetSource();
        src.GetComponent<AudioPlay>().Play(Clip);
        lastPlayTimes[Clip] = Time.time;
    }
    public void StopAllAudio()
    {
        // すべての再生中の音を停止する
        foreach (var audio in audioPlays)
        {
            audio.Stop();
        }
        audioPlays.Clear();
    }
}
