using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FixedCameraPrototype.Interaction
{
    public class InteractionInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference m_interactAction;

        public event Action OnInteractPressed;

        private void OnEnable()
        {
            m_interactAction.action.performed += HandleInteractPerformed;
            m_interactAction.action.Enable();
        }

        private void OnDisable()
        {
            m_interactAction.action.performed -= HandleInteractPerformed;
            m_interactAction.action.Disable();
        }

        private void HandleInteractPerformed(InputAction.CallbackContext context)
        {
            OnInteractPressed?.Invoke();
        }
    }
}
