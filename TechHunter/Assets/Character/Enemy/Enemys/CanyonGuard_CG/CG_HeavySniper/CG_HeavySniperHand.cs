using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CG_HeavySniperHand : MonoBehaviour
{

    [SerializeField] EnemyBase owner;

    void Awake()
    {
        if (owner == null) owner = GetComponentInParent<EnemyBase>();
    }

    private void FixedUpdate()
    {
        Transform target = null;
        if (owner is EnemyContoroller controller)
        {
            target = controller.CurrentTarget;
            if (target == null && controller.TargetMode == EnemyContoroller.TargetType.PlayerOnly)
                target = owner.Player != null ? owner.Player.transform : null;
        }
        else if (owner is Boss_Controller boss)
        {
            target = boss.CurrentTarget;
            if (target == null && boss.TargetMode == EnemyContoroller.TargetType.PlayerOnly)
                target = owner.Player != null ? owner.Player.transform : null;
        }
        else if (owner is CG_Knght knight)
        {
            target = knight.CurrentTarget;
        }
        else if (PlayerController.Instance != null)
        {
            target = PlayerController.Instance.transform;
        }

        if (target == null) return;
        Vector2 direction = (Vector2)target.position - (Vector2)transform.position;
        if (direction.sqrMagnitude < 0.000001f) return;
        transform.up = direction;
        transform.Rotate(0, 0, 90);
    }
}
