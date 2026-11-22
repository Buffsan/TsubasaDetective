using UnityEngine;

public sealed class Relic_HandInHandAndDie : RelicBase_AI
{
    [SerializeField] StatusEffectAttack charmEffect;
    PlayerController owner;
    bool registered;

    private void Start()
    {
        owner = PlayerController.Instance;
        if (owner != null) registered = owner.RegisterHandInHandRelic(charmEffect);
    }

    private void OnDestroy()
    {
        if (registered && owner != null) owner.UnregisterHandInHandRelic();
    }
}
