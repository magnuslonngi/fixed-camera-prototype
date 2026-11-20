using System;
using System.Collections;
using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Enemy
{
    public class EnemyAttack : MonoBehaviour
    {
        [SerializeField] private float m_attackRangeMeters = 1.3f;
        [Tooltip("Extra reach when the blow lands, so a target that just stepped back is still hit.")]
        [SerializeField] private float m_hitReachToleranceMeters = 0.4f;
        [Tooltip("The enemy must face the target within this angle to start an attack or land the blow.")]
        [SerializeField] private float m_maxAttackAngleDegrees = 25.0f;
        [SerializeField] private float m_damage = 20.0f;
        [Tooltip("Delay from attack start to the moment the blow lands in the attack animation.")]
        [SerializeField] private float m_hitDelaySeconds = 0.7f;
        [SerializeField] private float m_cooldownSeconds = 1.8f;

        private float m_nextAttackTime;
        private Coroutine m_pendingHitRoutine;

        public event Action OnAttackStarted;

        public bool IsInRange(IPursuitTarget target)
        {
            return FlatDistanceTo(target) <= m_attackRangeMeters;
        }

        public bool IsFacing(IPursuitTarget target)
        {
            Vector3 directionToTarget = Vector3.ProjectOnPlane(target.Position - transform.position, Vector3.up);
            return Vector3.Angle(transform.forward, directionToTarget) <= m_maxAttackAngleDegrees;
        }

        public bool TryStartAttack(IPursuitTarget target)
        {
            if (!CanStartAttack(target))
            {
                return false;
            }

            m_nextAttackTime = Time.time + m_cooldownSeconds;
            m_pendingHitRoutine = StartCoroutine(LandBlowAfterDelay(target));
            OnAttackStarted?.Invoke();
            return true;
        }

        public void CancelAttack()
        {
            if (m_pendingHitRoutine != null)
            {
                StopCoroutine(m_pendingHitRoutine);
                m_pendingHitRoutine = null;
            }
        }

        private bool CanStartAttack(IPursuitTarget target)
        {
            return Time.time >= m_nextAttackTime && target.IsTargetable && IsInRange(target) && IsFacing(target);
        }

        private IEnumerator LandBlowAfterDelay(IPursuitTarget target)
        {
            yield return new WaitForSeconds(m_hitDelaySeconds);

            m_pendingHitRoutine = null;
            if (IsBlowConnecting(target))
            {
                target.Damageable.ApplyDamage(m_damage);
            }
        }

        private bool IsBlowConnecting(IPursuitTarget target)
        {
            return target.IsTargetable &&
                   FlatDistanceTo(target) <= m_attackRangeMeters + m_hitReachToleranceMeters &&
                   IsFacing(target);
        }

        private float FlatDistanceTo(IPursuitTarget target)
        {
            return Vector3.ProjectOnPlane(target.Position - transform.position, Vector3.up).magnitude;
        }
    }
}
