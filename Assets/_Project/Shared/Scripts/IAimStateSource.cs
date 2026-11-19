using System;

namespace FixedCameraPrototype.Shared
{
    public interface IAimStateSource
    {
        bool IsAiming { get; }

        event Action<bool> OnAimingChanged;
    }
}
