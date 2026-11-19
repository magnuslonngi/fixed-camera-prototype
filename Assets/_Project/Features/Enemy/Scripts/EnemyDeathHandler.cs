using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Enemy
{
    public class EnemyDeathHandler : MonoBehaviour
    {
        [SerializeField] private Health m_health;

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
            Destroy(gameObject);
        }
    }
}
