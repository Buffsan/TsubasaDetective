using UnityEngine;

public class AttackDespawn : MonoBehaviour
{
    float timer;
    AttackPool pool;

    public void Init(float lifetime, AttackPool pool)
    {
        this.timer = lifetime;
        this.pool = pool;
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
