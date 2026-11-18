using UnityEngine;

namespace FixedCameraPrototype.Interaction
{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private InteractionInputReader m_inputReader;
        [SerializeField] private InteractionTargetSelector m_targetSelector;

        private void OnEnable()
        {
            m_inputReader.OnInteractPressed += InteractWithFocusedTarget;
        }

        private void OnDisable()
        {
            m_inputReader.OnInteractPressed -= InteractWithFocusedTarget;
        }

        private void InteractWithFocusedTarget()
        {
            Interactable focusedInteractable = m_targetSelector.FocusedInteractable;
            if (focusedInteractable != null)
            {
                focusedInteractable.Interact(gameObject);
            }
        }
    }
}
