using UnityEngine;

namespace FixedCameraPrototype.Enemy
{
    public class PatrolIdleBehavior : MonoBehaviour, IEnemyIdleBehavior
    {
        [SerializeField] private EnemyLocomotion m_locomotion;
        [SerializeField] private Transform[] m_waypoints;
        [SerializeField] private float m_waitAtWaypointSeconds = 2.0f;

        // Captured once because the waypoints may be children of the moving enemy.
        private Vector3[] m_waypointPositions;
        private int m_currentWaypointIndex;
        private float m_remainingWaitSeconds;

        private void Awake()
        {
            m_waypointPositions = new Vector3[m_waypoints.Length];
            for (int i = 0; i < m_waypoints.Length; i++)
            {
                m_waypointPositions[i] = m_waypoints[i].position;
            }
        }

        public void BeginIdle()
        {
            m_remainingWaitSeconds = 0.0f;
            MoveToCurrentWaypoint();
        }

        public void TickIdle(float deltaTime)
        {
            if (m_waypointPositions.Length == 0 || !m_locomotion.HasArrived)
            {
                return;
            }

            m_remainingWaitSeconds -= deltaTime;
            if (m_remainingWaitSeconds > 0.0f)
            {
                return;
            }

            m_currentWaypointIndex = (m_currentWaypointIndex + 1) % m_waypointPositions.Length;
            m_remainingWaitSeconds = m_waitAtWaypointSeconds;
            MoveToCurrentWaypoint();
        }

        private void MoveToCurrentWaypoint()
        {
            if (m_waypointPositions.Length > 0)
            {
                m_locomotion.MoveTo(m_waypointPositions[m_currentWaypointIndex]);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (m_waypoints == null)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            for (int i = 0; i < m_waypoints.Length; i++)
            {
                Transform waypoint = m_waypoints[i];
                Transform nextWaypoint = m_waypoints[(i + 1) % m_waypoints.Length];
                if (waypoint != null && nextWaypoint != null)
                {
                    Gizmos.DrawLine(waypoint.position, nextWaypoint.position);
                }
            }
        }
    }
}
