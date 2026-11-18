using UnityEngine;

namespace FixedCameraPrototype.Shared
{
    public interface IAimAssistTarget
    {
        bool IsTargetable { get; }
        Vector3 Position { get; }
        Vector3 GetAimPoint(HitZoneType zoneType);
    }
}
