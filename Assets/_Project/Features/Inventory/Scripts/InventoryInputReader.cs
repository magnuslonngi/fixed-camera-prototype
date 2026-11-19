using System;
using FixedCameraPrototype.Shared;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FixedCameraPrototype.Inventory
{
    public class InventoryInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference m_openInventoryAction;
        [SerializeField] private CharacterControlLock m_controlLock;

        public event Action OnOpenInventoryPressed;

        private void OnEnable()
        {
            m_openInventoryAction.action.performed += HandleOpenInventoryPerformed;
            m_controlLock.OnLockStateChanged += HandleControlLockStateChanged;
            SetActionEnabled(!m_controlLock.IsLocked);
        }

        private void OnDisable()
        {
            m_openInventoryAction.action.performed -= HandleOpenInventoryPerformed;
            m_controlLock.OnLockStateChanged -= HandleControlLockStateChanged;
            SetActionEnabled(false);
        }

        private void HandleOpenInventoryPerformed(InputAction.CallbackContext context)
        {
            OnOpenInventoryPressed?.Invoke();
        }

        private void HandleControlLockStateChanged(bool isLocked)
        {
            SetActionEnabled(!isLocked);
        }

        private void SetActionEnabled(bool isEnabled)
        {
            if (isEnabled)
            {
                m_openInventoryAction.action.Enable();
            }
            else
            {
                m_openInventoryAction.action.Disable();
            }
        }
    }
}
