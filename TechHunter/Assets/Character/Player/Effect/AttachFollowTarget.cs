using UnityEngine;

public class AttachFollowTarget : MonoBehaviour
{
    Transform target;
    Vector3 offset;
    EffectPool pool => EffectPool.Instance;

    bool followEnabled = false; // © ’Ç]‚·‚é‚©‚Ç‚¤‚©

    public void Init(Transform followTarget)
    {
        target = followTarget;

        if (followTarget != null)
        {
            followEnabled = true;
            offset = transform.position - followTarget.position;
        }
        else
        {
            followEnabled = false; // © ’Ç]‚µ‚È‚¢ƒ‚[ƒh
        }
    }

    void LateUpdate()
    {
        if (!followEnabled)
            return; // followTarget ‚ª null ‚Ìê‡‚ÍˆêØ’Ç]‚µ‚È‚¢

        if (target == null)
        {
            pool.Release(gameObject); // ’Ç]‘ÎÛ‚ª€‚ñ‚¾ê‡‚Ì‚İÁ‚·
            return;
        }

        transform.position = target.position + offset;
    }
}
