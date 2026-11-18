using System;
using UnityEngine;

namespace FixedCameraPrototype.Interaction
{
    // Placeholder: an inspect viewer will subscribe to OnInspectRequested later.
    [RequireComponent(typeof(Interactable))]
    public class InspectInteraction : MonoBehaviour
    {
        private Interactable m_interactable;

        public event Action<GameObject> OnInspectRequested;

        private void Awake()
        {
            m_interactable = GetComponent<Interactable>();
        }

        private void OnEnable()
        {
            m_interactable.OnInteracted += HandleInteracted;
        }

        private void OnDisable()
        {
            m_interactable.OnInteracted -= HandleInteracted;
        }

        private void HandleInteracted(GameObject instigator)
        {
            OnInspectRequested?.Invoke(instigator);
            Debug.Log($"[Placeholder] Inspect {name}", this);
        }
    }
}
