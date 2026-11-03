using UnityEngine;

// No count indicator is intentionally provided.
public sealed class Relic_TwelfthCandle : RelicBase_AI
{
    [SerializeField] ParticleSystem Effect_Relic;
    int count;
    void OnEnable() { GlobalEvents.OnSkillCooldownStarted += CountSkill; }
    void OnDisable() { GlobalEvents.OnSkillCooldownStarted -= CountSkill; }

    void CountSkill(int slotIndex)
    {
        count++;
        if (count < 12) return;
        count = 0;
        PlayerController owner = playerController;
        if (owner == null || slotIndex < 0 || slotIndex >= owner.skillINFO.Count) return;
        SkillInfo skill = owner.skillINFO[slotIndex];
        if (skill != null && skill.SkillCardManager != null)
        {
            skill.SkillCardManager.RefreshCooldownImmediately();
            PlayRelicImage(owner);
        }
    }

    void PlayRelicImage(PlayerController owner)
    {
        if (Effect_Relic == null) return;
        // The relic controller stays where it was first instantiated,
        // so position the particles where the player is on activation.
        Effect_Relic.transform.position = owner.transform.position;
        Effect_Relic.gameObject.SetActive(true);
        Effect_Relic.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        Effect_Relic.Play(true);
    }
}
