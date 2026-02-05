using UnityEngine;

public class EffectDespawn : MonoBehaviour
{
    EffectPool pool => EffectPool.Instance;
    float timer;

    public void Init(float lifetime, EffectPool pool)
    {
     
        timer = lifetime;
        
        // Particle Çí‚é~ÇµÇƒÇ©ÇÁçƒê∂ÇµíºÇ∑
        var ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Stop();
            ps.Clear();
            ps.Play();
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            pool.Release(gameObject);
        }
    }
}
