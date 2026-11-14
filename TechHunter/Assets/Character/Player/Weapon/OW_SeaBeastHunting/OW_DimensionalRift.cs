using UnityEngine;

public class OW_DimensionalRift : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] GameObject AttackArea1;
    [SerializeField] GameObject AttackArea2;
    [SerializeField] AudioClip clip;

    AudioManager audioManager => AudioManager.instance;

    // Enemy rift variants are also driven by this script. The player-only
    // AttackPool cannot produce an EnemyAttack from its PlayerAttack template.
    EnemyBase enemyAttackOwner;
    int phase;
    float attackCount;

    public void SetEnemyAttackOwner(EnemyBase owner)
    {
        enemyAttackOwner = owner;
    }

    void FixedUpdate()
    {
        attackCount += Time.fixedDeltaTime;
        if (phase == 0 && attackCount > 1f)
        {
            if (animator != null)
                animator.Play("”j—ô", 0, 0);

            SpawnAttackArea(AttackArea1, 1f);
            SpawnAttackArea(AttackArea2, 0.8f);

            attackCount = 0f;
            if (clip != null && audioManager != null)
                audioManager.PlaySE(clip);
            phase = 1;
        }
        else if (phase == 1 && attackCount > 2f)
        {
            Destroy(gameObject);
        }
    }

    void SpawnAttackArea(GameObject prefab, float lifetime)
    {
        if (prefab == null) return;

        // Boss/Enemy prefabs must retain EnemyAttack and their faction rules.
        // Do not pass them through AttackPool (which always instantiates a
        // PlayerAttack, irrespective of its templatePrefab argument).
        if (prefab.TryGetComponent<EnemyAttack>(out var enemyTemplate))
        {
            GameObject hitbox = Instantiate(prefab, transform.position,
                Quaternion.identity);
            EnemyAttack attack = hitbox.GetComponent<EnemyAttack>();
            if (attack != null && enemyAttackOwner != null)
            {
                Boss_Controller boss = enemyAttackOwner as Boss_Controller;
                if (boss != null)
                    boss.ConfigureAttackTarget(attack);
                else
                    attack.ConfigureAttack(enemyAttackOwner,
                        attack.AttackPowerPercent);
            }
            Destroy(hitbox, lifetime);
            return;
        }

        // Original player-owned dimensional rifts still use their pool.
        if (AttackPool.Instance != null)
            AttackPool.Instance.Get(prefab, transform.position,
                Quaternion.identity, lifetime);
        else
            Debug.LogWarning("OW_DimensionalRift: AttackPool is missing for player rift.", this);
    }
}
