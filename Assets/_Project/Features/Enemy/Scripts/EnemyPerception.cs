using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Enemy
{
    public class EnemyPerception : MonoBehaviour
    {
        [SerializeField] private Transform m_eyeTransform;
        [SerializeField] private float m_detectionRangeMeters = 12.0f;
        [SerializeField] private float m_fieldOfViewDegrees = 120.0f;
        [SerializeField] private LayerMask m_lineOfSightBlockers = 1;
        [SerializeField] private float m_scanIntervalSeconds = 0.2f;
        [Tooltip("Seconds without line of sight before the target is forgotten.")]
        [SerializeField] private float m_loseTargetSeconds = 4.0f;

        private IPursuitTarget m_currentTarget;
        private Vector3 m_lastKnownTargetPosition;
        private bool m_hasLineOfSightToTarget;
        private float m_secondsSinceTargetSeen;
        private float m_secondsUntilNextScan;

        public bool HasTarget => m_currentTarget != null;
        public IPursuitTarget CurrentTarget => m_currentTarget;
        public bool HasLineOfSightToTarget => m_hasLineOfSightToTarget;
        public Vector3 LastKnownTargetPosition => m_lastKnownTargetPosition;

        private void OnDisable()
        {
            ForgetTarget();
        }

        private void Update()
        {
            m_secondsSinceTargetSeen += Time.deltaTime;
            m_secondsUntilNextScan -= Time.deltaTime;
            if (m_secondsUntilNextScan > 0.0f)
            {
                return;
            }

            m_secondsUntilNextScan = m_scanIntervalSeconds;
            if (HasTarget)
            {
                TrackCurrentTarget();
            }
            else
            {
                ScanForTarget();
            }
        }

        // Being hurt reveals the attacker even if it is behind the enemy.
        public void AlertToNearestTarget()
        {
            IPursuitTarget nearestTarget = null;
            float nearestDistance = float.MaxValue;
            foreach (IPursuitTarget target in PursuitTargetRegistry.RegisteredTargets)
            {
                float distance = Vector3.Distance(transform.position, target.Position);
                if (target.IsTargetable && distance < nearestDistance)
                {
                    nearestTarget = target;
                    nearestDistance = distance;
                }
            }

            if (nearestTarget != null)
            {
                AcquireTarget(nearestTarget);
            }
        }

        private void ScanForTarget()
        {
            foreach (IPursuitTarget target in PursuitTargetRegistry.RegisteredTargets)
            {
                if (IsInDetectionRange(target) && IsInFieldOfView(target) && HasClearLineOfSight(target))
                {
                    AcquireTarget(target);
                    return;
                }
            }
        }

        // Once engaged the enemy keeps tracking without a view cone, since it turns to follow the target.
        private void TrackCurrentTarget()
        {
            if (!m_currentTarget.IsTargetable)
            {
                ForgetTarget();
                return;
            }

            m_hasLineOfSightToTarget = HasClearLineOfSight(m_currentTarget);
            if (m_hasLineOfSightToTarget)
            {
                m_lastKnownTargetPosition = m_currentTarget.Position;
                m_secondsSinceTargetSeen = 0.0f;
            }
            else if (m_secondsSinceTargetSeen >= m_loseTargetSeconds)
            {
                ForgetTarget();
            }
        }

        private void AcquireTarget(IPursuitTarget target)
        {
            m_currentTarget = target;
            m_lastKnownTargetPosition = target.Position;
            m_hasLineOfSightToTarget = true;
            m_secondsSinceTargetSeen = 0.0f;
        }

        private void ForgetTarget()
        {
            m_currentTarget = null;
            m_hasLineOfSightToTarget = false;
        }

        private bool IsInDetectionRange(IPursuitTarget target)
        {
            return target.IsTargetable && Vector3.Distance(transform.position, target.Position) <= m_detectionRangeMeters;
        }

        private bool IsInFieldOfView(IPursuitTarget target)
        {
            Vector3 directionToTarget = Vector3.ProjectOnPlane(target.Position - transform.position, Vector3.up);
            return Vector3.Angle(transform.forward, directionToTarget) <= m_fieldOfViewDegrees * 0.5f;
        }

        private bool HasClearLineOfSight(IPursuitTarget target)
        {
            Vector3 eyePosition = m_eyeTransform.position;
            bool isBlocked = Physics.Linecast(eyePosition, target.LineOfSightPoint, out RaycastHit hit, m_lineOfSightBlockers, QueryTriggerInteraction.Ignore);
            return !isBlocked || IsColliderOfTarget(hit.collider, target);
        }

        private static bool IsColliderOfTarget(Collider hitCollider, IPursuitTarget target)
        {
            return CapabilityRoot.TryFindCapability(hitCollider.gameObject, out IPursuitTarget hitTarget) && hitTarget == target;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_detectionRangeMeters);
        }
    }
}
