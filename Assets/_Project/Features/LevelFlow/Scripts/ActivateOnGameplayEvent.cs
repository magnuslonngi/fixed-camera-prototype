using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.LevelFlow
{
    // Swaps level objects in or out when a gameplay moment happens (a gate opens, a corpse gets up).
    public class ActivateOnGameplayEvent : MonoBehaviour
    {
        [SerializeField] private GameplayEventChannel m_triggerEventChannel;
        [SerializeField] private GameObject[] m_objectsToActivate;
        [SerializeField] private GameObject[] m_objectsToDeactivate;

        private void OnEnable()
        {
            m_triggerEventChannel.OnRaised += HandleTriggerRaised;
        }

        private void OnDisable()
        {
            m_triggerEventChannel.OnRaised -= HandleTriggerRaised;
        }

        private void HandleTriggerRaised()
        {
            foreach (GameObject objectToDeactivate in m_objectsToDeactivate)
            {
                objectToDeactivate.SetActive(false);
            }

            foreach (GameObject objectToActivate in m_objectsToActivate)
            {
                objectToActivate.SetActive(true);
            }
        }
    }
}
