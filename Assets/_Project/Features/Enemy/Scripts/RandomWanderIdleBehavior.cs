using UnityEngine;
using UnityEngine.AI;

namespace FixedCameraPrototype.Enemy
{
    public class RandomWanderIdleBehavior : MonoBehaviour, IEnemyIdleBehavior
    {
        private const float k_navMeshSampleDistanceMeters = 2.0f;
        private const int k_maxDestinationAttempts = 8;

        [SerializeField] private EnemyLocomotion m_locomotion;
        [SerializeField] private float m_wanderRadiusMeters = 6.0f;
        [SerializeField] private float m_minimumWaitSeconds = 1.5f;
        [SerializeField] private float m_maximumWaitSeconds = 4.0f;

        private Vector3 m_wanderCenter;
        private float m_remainingWaitSeconds;

        private void Awake()
        {
            m_wanderCenter = transform.position;
        }

        public void BeginIdle()
        {
            m_locomotion.Stop();
            m_remainingWaitSeconds = Random.Range(m_minimumWaitSeconds, m_maximumWaitSeconds);
        }

        public void TickIdle(float deltaTime)
        {
            if (!m_locomotion.HasArrived)
            {
                return;
            }

            m_remainingWaitSeconds -= deltaTime;
            if (m_remainingWaitSeconds > 0.0f)
            {
                return;
            }

            m_remainingWaitSeconds = Random.Range(m_minimumWaitSeconds, m_maximumWaitSeconds);
            if (TryPickWanderDestination(out Vector3 destination))
            {
                m_locomotion.MoveTo(destination);
            }
        }

        private bool TryPickWanderDestination(out Vector3 destination)
        {
            for (int attempt = 0; attempt < k_maxDestinationAttempts; attempt++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * m_wanderRadiusMeters;
                Vector3 candidate = m_wanderCenter + new Vector3(randomOffset.x, 0.0f, randomOffset.y);
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, k_navMeshSampleDistanceMeters, NavMesh.AllAreas))
                {
                    destination = hit.position;
                    return true;
                }
            }

            destination = m_wanderCenter;
            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(Application.isPlaying ? m_wanderCenter : transform.position, m_wanderRadiusMeters);
        }
    }
}
