using System.Collections.Generic;
using UnityEngine;

public class AudioPool : MonoBehaviour
{
    public static AudioPool Instance;

    [SerializeField] GameObject audioPrefab;
    [SerializeField] int initialCount = 16;

    Queue<AudioSource> pool = new Queue<AudioSource>();

    void Awake()
    {
        Instance = this;

        // ‰Šúƒv[ƒ‹¶¬
        for (int i = 0; i < initialCount; i++)
        {
            GameObject obj = Instantiate(audioPrefab);
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            pool.Enqueue(obj.GetComponent<AudioSource>());
        }
    }
    public AudioSource GetSource()
    {
        AudioSource src;
        if (pool.Count > 0)
        {
            src = pool.Dequeue();
        }
        else
        {
            // ‘«‚è‚È‚¢‚Í‘‚â‚·
            GameObject obj = Instantiate(audioPrefab);
            src = obj.GetComponent<AudioSource>();
            obj.transform.SetParent(transform);
        }
        src.gameObject.SetActive(true);
        return src;
    }
    public void Release(AudioSource src)
    {
        src.Stop();
        src.clip = null;
        src.gameObject.SetActive(false);
        pool.Enqueue(src);
    }
}
