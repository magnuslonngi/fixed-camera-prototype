using System;
using FixedCameraPrototype.Shared;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FixedCameraPrototype.Interaction
{
    public class InteractionInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference m_interactAction;
        [SerializeField] private CharacterControlLock m_controlLock;

        public event Action OnInteractPressed;

        private void OnEnable()
        {
            m_interactAction.action.performed += HandleInteractPerformed;
            m_controlLock.OnLockStateChanged += HandleControlLockStateChanged;
            SetActionEnabled(!m_controlLock.IsLocked);
        }

        private void OnDisable()
        {
            m_interactAction.action.performed -= HandleInteractPerformed;
            m_controlLock.OnLockStateChanged -= HandleControlLockStateChanged;
            SetActionEnabled(false);
        }

        private void HandleInteractPerformed(InputAction.CallbackContext context)
        {
            OnInteractPressed?.Invoke();
        }

        private void HandleControlLockStateChanged(bool isLocked)
        {
            SetActionEnabled(!isLocked);
        }

        private void SetActionEnabled(bool isEnabled)
        {
            if (isEnabled)
            {
                m_interactAction.action.Enable();
            }
            else
            {
                m_interactAction.action.Disable();
            }
        }
    }
}
