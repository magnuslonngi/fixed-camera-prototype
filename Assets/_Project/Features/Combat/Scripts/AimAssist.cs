using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Combat
{
    public class AimAssist : MonoBehaviour
    {
        private const int k_maximumOverlapResults = 32;

        private static readonly Collider[] s_overlapResults = new Collider[k_maximumOverlapResults];

        [SerializeField] private bool m_isAssistEnabled = true;

        [Header("Cones (half angle in degrees, measured on yaw only)")]
        [SerializeField]
        [Range(0.0f, 180.0f)]
        private float m_acquisitionHalfAngleDegrees = 60.0f;
        [SerializeField]
        [Range(0.0f, 90.0f)]
        private float m_shotAssistHalfAngleDegrees = 20.0f;

        [Header("Targeting")]
        [SerializeField] private float m_rangeMeters = 15.0f;
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float m_assistStrength = 1.0f;
        [SerializeField] private LayerMask m_targetLayers;
        [SerializeField] private LayerMask m_obstructionLayers;

        public bool IsAssistEnabled => m_isAssistEnabled;
        public float AcquisitionHalfAngleDegrees => m_acquisitionHalfAngleDegrees;
        public float ShotAssistHalfAngleDegrees => m_shotAssistHalfAngleDegrees;
        public float RangeMeters => m_rangeMeters;

        public bool TryFindAcquisitionTarget(Vector3 origin, Vector3 direction, out IAimAssistTarget target)
        {
            return TryFindNearestTarget(origin, direction, m_acquisitionHalfAngleDegrees, HitZoneType.Torso, out target);
        }

        public Vector3 ResolveShotDirection(Vector3 origin, Vector3 rawDirection, HitZoneType zoneType)
        {
            if (!TryFindNearestTarget(origin, rawDirection, m_shotAssistHalfAngleDegrees, zoneType, out IAimAssistTarget target))
            {
                return rawDirection;
            }

            Vector3 assistedDirection = (target.GetAimPoint(zoneType) - origin).normalized;
            return Vector3.Slerp(rawDirection, assistedDirection, m_assistStrength).normalized;
        }

        private bool TryFindNearestTarget(
            Vector3 origin,
            Vector3 direction,
            float halfAngleDegrees,
            HitZoneType zoneType,
            out IAimAssistTarget nearestTarget)
        {
            nearestTarget = null;
            if (!m_isAssistEnabled)
            {
                return false;
            }

            float smallestYawDegrees = float.MaxValue;
            int overlapCount = Physics.OverlapSphereNonAlloc(
                origin,
                m_rangeMeters,
                s_overlapResults,
                m_targetLayers,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < overlapCount; i++)
            {
                IAimAssistTarget candidate = s_overlapResults[i].GetComponentInParent<IAimAssistTarget>();
                if (candidate == null || !candidate.IsTargetable)
                {
                    continue;
                }

                float yawDegrees = CalculateYawDegrees(direction, candidate.Position - origin);
                if (yawDegrees > halfAngleDegrees || yawDegrees >= smallestYawDegrees)
                {
                    continue;
                }

                if (HasLineOfSight(origin, candidate.GetAimPoint(zoneType)))
                {
                    smallestYawDegrees = yawDegrees;
                    nearestTarget = candidate;
                }
            }

            return nearestTarget != null;
        }

        private bool HasLineOfSight(Vector3 origin, Vector3 aimPoint)
        {
            return !Physics.Linecast(origin, aimPoint, m_obstructionLayers, QueryTriggerInteraction.Ignore);
        }

        private static float CalculateYawDegrees(Vector3 fromDirection, Vector3 toDirection)
        {
            return Vector3.Angle(
                Vector3.ProjectOnPlane(fromDirection, Vector3.up),
                Vector3.ProjectOnPlane(toDirection, Vector3.up));
        }
    }
}
