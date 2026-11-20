using UnityEngine;

namespace FixedCameraPrototype.Shared
{
    // Something hostile AI can detect, chase and hurt, without the AI referencing the feature that owns it.
    public interface IPursuitTarget
    {
        bool IsTargetable { get; }
        Vector3 Position { get; }
        Vector3 LineOfSightPoint { get; }
        IDamageable Damageable { get; }
    }
}
