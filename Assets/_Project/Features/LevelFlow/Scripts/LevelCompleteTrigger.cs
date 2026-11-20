using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.LevelFlow
{
    // Ends the level when the player walks in: shows the closing text, then keeps the character still for good.
    [RequireComponent(typeof(BoxCollider))]
    public class LevelCompleteTrigger : MonoBehaviour
    {
        [SerializeField]
        [TextArea(4, 12)]
        private string m_completionText;

        private bool m_hasCompleted;

        public bool HasCompleted => m_hasCompleted;

        private void Reset()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (m_hasCompleted || !other.TryGetComponent(out CharacterController _))
            {
                return;
            }

            m_hasCompleted = true;

            if (CapabilityRoot.TryFindCapability(other.gameObject, out CharacterControlLock controlLock))
            {
                controlLock.AddLock();
            }

            if (CapabilityRoot.TryFindCapability(other.gameObject, out IDocumentReader documentReader))
            {
                documentReader.OpenDocument(m_completionText);
            }
        }
    }
}
