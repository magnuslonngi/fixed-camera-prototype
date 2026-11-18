using System;
using UnityEngine;

namespace FixedCameraPrototype.Interaction
{
    // Placeholder: removes the object from the world. An inventory will subscribe to OnPickedUp later.
    [RequireComponent(typeof(Interactable))]
    public class PickUpInteraction : MonoBehaviour
    {
        private Interactable m_interactable;

        public event Action<GameObject> OnPickedUp;

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
            OnPickedUp?.Invoke(instigator);
            gameObject.SetActive(false);
        }
    }
}
