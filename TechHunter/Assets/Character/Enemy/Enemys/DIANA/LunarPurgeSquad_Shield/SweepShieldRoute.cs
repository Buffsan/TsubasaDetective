using UnityEngine;

// Stage-specific route settings populated before EnemyContoroller.Start.
[DisallowMultipleComponent]
public class SweepShieldRoute : MonoBehaviour
{
    public float LaneY { get; private set; }
    public float ExitX { get; private set; }
    public bool Initialized { get; private set; }

    public void Initialize(float laneY, float exitX)
    {
        LaneY = laneY;
        ExitX = exitX;
        Initialized = true;
    }
}
