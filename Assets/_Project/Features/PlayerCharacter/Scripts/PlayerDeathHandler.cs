using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.PlayerCharacter
{
    // Death is permanent until a restart flow exists, so the locks are never released.
    public class PlayerDeathHandler : MonoBehaviour
    {
        [SerializeField] private Health m_health;
        [SerializeField] private CharacterControlLock m_controlLock;
        [SerializeField] private MovementLock m_movementLock;

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
            m_controlLock.AddLock();
            m_movementLock.AddLock();
        }
    }
}
