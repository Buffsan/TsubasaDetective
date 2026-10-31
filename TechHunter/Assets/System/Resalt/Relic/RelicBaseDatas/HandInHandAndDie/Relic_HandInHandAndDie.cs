using UnityEngine;

public sealed class Relic_HandInHandAndDie : RelicBase_AI
{
    PlayerController owner;

    private void Start()
    {
        owner = PlayerController.Instance;
        if (owner != null) owner.RegisterHandInHandRelic();
    }

    private void OnDestroy()
    {
        if (owner != null) owner.UnregisterHandInHandRelic();
    }
}
