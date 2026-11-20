using UnityEngine;

namespace FixedCameraPrototype.Enemy
{
    // Stands at its spawn point, returning there and to its spawn facing after losing a target.
    public class StationaryIdleBehavior : MonoBehaviour, IEnemyIdleBehavior
    {
        [SerializeField] private EnemyLocomotion m_locomotion;

        private Vector3 m_spawnPosition;
        private Vector3 m_spawnForward;

        private void Awake()
        {
            m_spawnPosition = transform.position;
            m_spawnForward = transform.forward;
        }

        public void BeginIdle()
        {
            m_locomotion.MoveTo(m_spawnPosition);
        }

        public void TickIdle(float deltaTime)
        {
            if (m_locomotion.HasArrived)
            {
                m_locomotion.Stop();
                m_locomotion.FaceTowards(transform.position + m_spawnForward, deltaTime);
            }
        }
    }
}
