using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Interaction
{
    // Reading happens in place: the document is handed to the instigator's IDocumentReader.
    [RequireComponent(typeof(Interactable))]
    public class ReadInteraction : MonoBehaviour
    {
        [SerializeField]
        [TextArea(4, 12)]
        private string m_documentText;

        private Interactable m_interactable;

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
            if (instigator.TryGetComponent(out IDocumentReader documentReader))
            {
                documentReader.OpenDocument(m_documentText);
            }
        }
    }
}
