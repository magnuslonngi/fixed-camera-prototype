using System;
using FixedCameraPrototype.Shared;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FixedCameraPrototype.Combat
{
    public class CombatInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference m_aimAction;
        [SerializeField] private InputActionReference m_fireAction;
        [SerializeField] private InputActionReference m_aimAdjustAction;
        [SerializeField] private InputActionReference m_reloadAction;
        [SerializeField] private CharacterControlLock m_controlLock;

        public bool IsAimHeld => m_aimAction.action.IsPressed();
        public Vector2 AimAdjustInput => m_aimAdjustAction.action.ReadValue<Vector2>();

        public event Action OnFirePressed;
        public event Action OnReloadPressed;

        private void OnEnable()
        {
            m_fireAction.action.performed += HandleFirePerformed;
            m_reloadAction.action.performed += HandleReloadPerformed;
            m_controlLock.OnLockStateChanged += HandleControlLockStateChanged;
            SetActionsEnabled(!m_controlLock.IsLocked);
        }

        private void OnDisable()
        {
            m_fireAction.action.performed -= HandleFirePerformed;
            m_reloadAction.action.performed -= HandleReloadPerformed;
            m_controlLock.OnLockStateChanged -= HandleControlLockStateChanged;
            SetActionsEnabled(false);
        }

        private void HandleFirePerformed(InputAction.CallbackContext context)
        {
            OnFirePressed?.Invoke();
        }

        private void HandleReloadPerformed(InputAction.CallbackContext context)
        {
            OnReloadPressed?.Invoke();
        }

        private void HandleControlLockStateChanged(bool isLocked)
        {
            SetActionsEnabled(!isLocked);
        }

        private void SetActionsEnabled(bool isEnabled)
        {
            if (isEnabled)
            {
                m_aimAction.action.Enable();
                m_fireAction.action.Enable();
                m_aimAdjustAction.action.Enable();
                m_reloadAction.action.Enable();
            }
            else
            {
                m_aimAction.action.Disable();
                m_fireAction.action.Disable();
                m_aimAdjustAction.action.Disable();
                m_reloadAction.action.Disable();
            }
        }
    }
}
