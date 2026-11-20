using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Enemy
{
    // Announces the first time this enemy starts chasing, so level scripting can react off-camera.
    public class EnemyAlertBroadcaster : MonoBehaviour
    {
        [SerializeField] private EnemyBrain m_brain;
        [SerializeField] private GameplayEventChannel m_firstChaseEventChannel;

        private bool m_hasBroadcast;

        private void OnEnable()
        {
            m_brain.OnStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            m_brain.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(EnemyState newState)
        {
            if (m_hasBroadcast || newState != EnemyState.Chase)
            {
                return;
            }

            m_hasBroadcast = true;
            m_firstChaseEventChannel.Raise();
        }
    }
}
