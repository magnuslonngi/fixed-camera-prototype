using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FixedCameraPrototype.Combat
{
    public class CombatInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference m_aimAction;
        [SerializeField] private InputActionReference m_fireAction;
        [SerializeField] private InputActionReference m_aimAdjustAction;

        public bool IsAimHeld => m_aimAction.action.IsPressed();
        public Vector2 AimAdjustInput => m_aimAdjustAction.action.ReadValue<Vector2>();

        public event Action OnFirePressed;

        private void OnEnable()
        {
            m_aimAction.action.Enable();
            m_fireAction.action.Enable();
            m_aimAdjustAction.action.Enable();
            m_fireAction.action.performed += HandleFirePerformed;
        }

        private void OnDisable()
        {
            m_fireAction.action.performed -= HandleFirePerformed;
            m_aimAction.action.Disable();
            m_fireAction.action.Disable();
            m_aimAdjustAction.action.Disable();
        }

        private void HandleFirePerformed(InputAction.CallbackContext context)
        {
            OnFirePressed?.Invoke();
        }
    }
}
