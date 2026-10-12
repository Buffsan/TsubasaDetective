using System.Collections;
using UnityEngine;

// Keeps the effect alive until particles already emitted have disappeared.
public class Skill_PD_FlameDance_EffectCleanup : MonoBehaviour
{
    ParticleSystem[] particles;
    bool stopping;

    public void Play()
    {
        particles = GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem particle in particles)
            if (particle != null) particle.Play();
    }

    public void StopAndDestroy()
    {
        if (stopping) return;
        stopping = true;

        // Detach before the skill controller or player hierarchy changes.
        transform.SetParent(null, true);

        if (particles == null)
            particles = GetComponentsInChildren<ParticleSystem>(true);

        if (particles.Length == 0)
        {
            Destroy(gameObject);
            return;
        }

        // Stop emission without clearing particles already on screen.
        foreach (ParticleSystem particle in particles)
            if (particle != null)
                particle.Stop(false, ParticleSystemStopBehavior.StopEmitting);

        StartCoroutine(WaitForParticles());
    }

    IEnumerator WaitForParticles()
    {
        while (true)
        {
            bool alive = false;
            foreach (ParticleSystem particle in particles)
            {
                if (particle != null && particle.IsAlive(true))
                {
                    alive = true;
                    break;
                }
            }

            if (!alive) break;
            yield return null;
        }

        Destroy(gameObject);
    }
}
