using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Enemy
{
    public class EnemyAimTarget : MonoBehaviour, IAimAssistTarget
    {
        [SerializeField] private Health m_health;
        [SerializeField] private EnemyHitZone m_headZone;
        [SerializeField] private EnemyHitZone m_torsoZone;
        [SerializeField] private EnemyHitZone m_legsZone;

        public bool IsTargetable => !m_health.IsDead;
        public Vector3 Position => m_torsoZone.AimPoint;

        public Vector3 GetAimPoint(HitZoneType zoneType)
        {
            switch (zoneType)
            {
                case HitZoneType.Head:
                    return m_headZone.AimPoint;
                case HitZoneType.Legs:
                    return m_legsZone.AimPoint;
                default:
                    return m_torsoZone.AimPoint;
            }
        }
    }
}
