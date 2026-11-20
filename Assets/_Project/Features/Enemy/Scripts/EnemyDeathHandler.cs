using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Enemy
{
    // Leaves the body in the world as an inert corpse: no AI, no navigation, no blocking, no bullet hits.
    public class EnemyDeathHandler : MonoBehaviour
    {
        [SerializeField] private Health m_health;
        [SerializeField] private Behaviour[] m_behavioursDisabledOnDeath;
        [SerializeField] private Collider[] m_collidersDisabledOnDeath;

        private void OnEnable()
        {
            m_health.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            m_health.OnDied -= HandleDied;
        }

        private void HandleDied()
        {
            foreach (Behaviour behaviour in m_behavioursDisabledOnDeath)
            {
                behaviour.enabled = false;
            }

            foreach (Collider bodyCollider in m_collidersDisabledOnDeath)
            {
                bodyCollider.enabled = false;
            }
        }
    }
}
