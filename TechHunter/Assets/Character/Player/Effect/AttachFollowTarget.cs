using UnityEngine;

public class AttachFollowTarget : MonoBehaviour
{
    Transform target;
    Vector3 offset;
    EffectPool pool => EffectPool.Instance;

    bool followEnabled = false; // ← 追従するかどうか

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
            followEnabled = false; // ← 追従しないモード
        }
    }

    void OnDisable()
    {
        // The next pooled use may be a non-following effect.
        followEnabled = false;
        target = null;
        offset = Vector3.zero;
    }

    void LateUpdate()
    {
        if (!followEnabled)
            return; // followTarget が null の場合は一切追従しない

        if (target == null)
        {
            pool.Release(gameObject); // 追従対象が死んだ場合のみ消す
            return;
        }

        transform.position = target.position + offset;
    }
}
