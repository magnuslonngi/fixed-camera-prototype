using UnityEngine;
using UnityEngine.InputSystem;

namespace FixedCameraPrototype.PlayerCharacter
{
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference m_moveAction;
        [SerializeField] private InputActionReference m_sprintAction;

        public Vector2 MoveInput => m_moveAction.action.ReadValue<Vector2>();
        public bool IsSprintHeld => m_sprintAction.action.IsPressed();

        private void OnEnable()
        {
            m_moveAction.action.Enable();
            m_sprintAction.action.Enable();
        }

        private void OnDisable()
        {
            m_moveAction.action.Disable();
            m_sprintAction.action.Disable();
        }
    }
}
