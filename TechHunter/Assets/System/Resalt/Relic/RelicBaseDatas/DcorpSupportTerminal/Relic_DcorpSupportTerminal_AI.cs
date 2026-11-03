using UnityEngine;

public class Relic_DcorpSupportTerminal_AI : RelicBase_AI
{
    [SerializeField, Min(0.01f)] float BuffTime = 0.5f;
    [SerializeField] BuffData buffdata;
    [SerializeField] Animator RelicObject;

    float activeUntil = -1f;
    bool showing;

    void OnEnable()
    {
        GlobalEvents.OnPlayerSkillHit += OnSkillHit;
        activeUntil = -1f;
        showing = false;
    }

    void OnDisable()
    {
        GlobalEvents.OnPlayerSkillHit -= OnSkillHit;
    }

    void Update()
    {
        if (!showing || Time.time < activeUntil) return;
        showing = false;
        if (RelicObject != null)
            RelicObject.Play("消える", 0, 0f);
    }

    void OnSkillHit()
    {
        PlayerController owner = playerController;
        if (owner == null || owner.playerBuff == null || buffdata == null) return;

        // Refresh on every direct skill hit. The event is invoked before
        // the triggering attack calculates its own damage, so that hit
        // also receives the skill-only attack increase.
        owner.playerBuff.SpawnOrRefreshBuff(buffdata);
        activeUntil = Time.time + Mathf.Max(0.01f, BuffTime);

        if (!showing)
        {
            showing = true;
            if (RelicObject != null)
                RelicObject.Play("出現", 0, 0f);
        }
    }
}
