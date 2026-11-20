using UnityEngine;
using UnityEngine.AI;

namespace FixedCameraPrototype.Enemy
{
    public class EnemyLocomotion : MonoBehaviour
    {
        private const float k_arrivalToleranceMeters = 0.2f;

        [SerializeField] private NavMeshAgent m_agent;
        [SerializeField] private float m_turnSpeedDegreesPerSecond = 240.0f;

        private bool IsAgentActive => m_agent.enabled && m_agent.isOnNavMesh;

        public float NormalizedSpeed => IsAgentActive ? m_agent.velocity.magnitude / m_agent.speed : 0.0f;

        public bool HasArrived =>
            !IsAgentActive ||
            (!m_agent.pathPending && m_agent.remainingDistance <= Mathf.Max(m_agent.stoppingDistance, k_arrivalToleranceMeters));

        public void MoveTo(Vector3 destination)
        {
            if (!IsAgentActive)
            {
                return;
            }

            m_agent.updateRotation = true;
            m_agent.isStopped = false;
            m_agent.SetDestination(destination);
        }

        public void Stop()
        {
            if (!IsAgentActive)
            {
                return;
            }

            m_agent.isStopped = true;
            m_agent.ResetPath();
            m_agent.velocity = Vector3.zero;
        }

        // The agent only turns along its path, so facing a point while standing still is done manually.
        public void FaceTowards(Vector3 worldPosition, float deltaTime)
        {
            m_agent.updateRotation = false;
            Vector3 flatDirection = Vector3.ProjectOnPlane(worldPosition - transform.position, Vector3.up);
            if (flatDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(flatDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, m_turnSpeedDegreesPerSecond * deltaTime);
        }
    }
}
