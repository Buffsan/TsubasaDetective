using UnityEngine;

public class Skill_PD_FlameDance_AI : PlayerSkillBase
{
    public override void SkillPlay()
    {
        if (Att != ATT.A1) return;
        Att = ATT.A2;

        // Buff affects the player only. Do not let ghost casts alter player dodge.
        if (TargetObject == playerController.gameObject && SkillWeapon != null)
        {
            Skill_PD_FlameDance_Controller current =
                Skill_PD_FlameDance_Controller.Active;

            if (current != null)
            {
                current.Refresh();
            }
            else
            {
                GameObject effect = Instantiate(SkillWeapon, TargetObject.transform.position,
                    Quaternion.identity);
                Skill_PD_FlameDance_Controller controller =
                    effect.GetComponent<Skill_PD_FlameDance_Controller>();
                if (controller != null)
                    controller.Initialize(playerController);
                else
                {
                    Debug.LogError("FlameDance: SkillWeapon requires Skill_PD_FlameDance_Controller.");
                    Destroy(effect);
                }
            }
        }
        end();
    }
}
