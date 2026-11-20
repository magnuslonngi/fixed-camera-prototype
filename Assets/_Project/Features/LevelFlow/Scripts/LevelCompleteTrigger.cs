using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.LevelFlow
{
    // Ends the level when the player walks in: holds the character still, shows the closing text,
    // then returns to the main menu once the text is dismissed.
    [RequireComponent(typeof(BoxCollider))]
    public class LevelCompleteTrigger : MonoBehaviour
    {
        [SerializeField]
        [TextArea(4, 12)]
        private string m_completionText;

        [SerializeField] private SceneReference m_mainMenuScene;

        private bool m_hasCompleted;
        private IDocumentReader m_documentReader;

        public bool HasCompleted => m_hasCompleted;

        private void Reset()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }

        private void OnDisable()
        {
            if (m_documentReader != null)
            {
                m_documentReader.OnDocumentClosed -= HandleCompletionTextClosed;
            }
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

            if (!CapabilityRoot.TryFindCapability(other.gameObject, out m_documentReader))
            {
                m_mainMenuScene.Load();
                return;
            }

            m_documentReader.OnDocumentClosed += HandleCompletionTextClosed;
            m_documentReader.OpenDocument(m_completionText);
        }

        private void HandleCompletionTextClosed()
        {
            m_documentReader.OnDocumentClosed -= HandleCompletionTextClosed;
            m_mainMenuScene.Load();
        }
    }
}
