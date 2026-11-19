using UnityEngine;
using UnityEngine.InputSystem;

namespace FixedCameraPrototype.PlayerCharacter
{
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference m_moveAction;
        [SerializeField] private InputActionReference m_tankMoveAction;
        [SerializeField] private InputActionReference m_sprintAction;

        public Vector2 MoveInput => m_moveAction.action.ReadValue<Vector2>();
        public float TankTurnInput => m_tankMoveAction.action.ReadValue<Vector2>().x;
        public float TankThrottleInput => m_tankMoveAction.action.ReadValue<Vector2>().y;
        public bool IsSprintHeld => m_sprintAction.action.IsPressed();

        private void OnEnable()
        {
            m_moveAction.action.Enable();
            m_tankMoveAction.action.Enable();
            m_sprintAction.action.Enable();
        }

        private void OnDisable()
        {
            m_moveAction.action.Disable();
            m_tankMoveAction.action.Disable();
            m_sprintAction.action.Disable();
        }
    }
}
