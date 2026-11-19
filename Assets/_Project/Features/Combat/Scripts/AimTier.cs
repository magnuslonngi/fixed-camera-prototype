using FixedCameraPrototype.Shared;

namespace FixedCameraPrototype.Combat
{
    public enum AimTier
    {
        Up,
        Center,
        Down
    }

    public static class AimTierExtensions
    {
        public static HitZoneType ToHitZone(this AimTier aimTier)
        {
            switch (aimTier)
            {
                case AimTier.Up:
                    return HitZoneType.Head;
                case AimTier.Down:
                    return HitZoneType.Legs;
                default:
                    return HitZoneType.Torso;
            }
        }
    }
}
