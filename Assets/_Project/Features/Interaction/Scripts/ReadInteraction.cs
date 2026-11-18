using System;
using UnityEngine;

namespace FixedCameraPrototype.Interaction
{
    // Placeholder: a document reader UI will subscribe to OnReadRequested later.
    [RequireComponent(typeof(Interactable))]
    public class ReadInteraction : MonoBehaviour
    {
        [SerializeField]
        [TextArea]
        private string m_documentText;

        private Interactable m_interactable;

        public event Action<string> OnReadRequested;

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
            OnReadRequested?.Invoke(m_documentText);
            Debug.Log($"[Placeholder] Read {name}: {m_documentText}", this);
        }
    }
}
