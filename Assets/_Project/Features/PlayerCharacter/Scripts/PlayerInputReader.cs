using FixedCameraPrototype.Shared;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FixedCameraPrototype.PlayerCharacter
{
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference m_moveAction;
        [SerializeField] private InputActionReference m_tankMoveAction;
        [SerializeField] private InputActionReference m_sprintAction;
        [SerializeField] private CharacterControlLock m_controlLock;

        public Vector2 MoveInput => m_moveAction.action.ReadValue<Vector2>();
        public float TankTurnInput => m_tankMoveAction.action.ReadValue<Vector2>().x;
        public float TankThrottleInput => m_tankMoveAction.action.ReadValue<Vector2>().y;
        public bool IsSprintHeld => m_sprintAction.action.IsPressed();

        private void OnEnable()
        {
            m_controlLock.OnLockStateChanged += HandleControlLockStateChanged;
            SetActionsEnabled(!m_controlLock.IsLocked);
        }

        private void OnDisable()
        {
            m_controlLock.OnLockStateChanged -= HandleControlLockStateChanged;
            SetActionsEnabled(false);
        }

        private void HandleControlLockStateChanged(bool isLocked)
        {
            SetActionsEnabled(!isLocked);
        }

        private void SetActionsEnabled(bool isEnabled)
        {
            if (isEnabled)
            {
                m_moveAction.action.Enable();
                m_tankMoveAction.action.Enable();
                m_sprintAction.action.Enable();
            }
            else
            {
                m_moveAction.action.Disable();
                m_tankMoveAction.action.Disable();
                m_sprintAction.action.Disable();
            }
        }
    }
}
