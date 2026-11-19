using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FixedCameraPrototype.Shared
{
    // Reads UI-map actions, which no gameplay reader uses, so they keep working while CharacterControlLock
    // has the gameplay actions disabled. Each modal screen owns one; ModalStack enables only the focused one.
    public class MenuInputReader : MonoBehaviour
    {
        private const float k_navigateThreshold = 0.5f;

        [SerializeField] private InputActionReference m_navigateAction;
        [SerializeField] private InputActionReference m_submitAction;
        [SerializeField] private InputActionReference m_cancelAction;

        private bool m_isNavigateHeld;
        private int m_enabledFrame;

        public event Action OnNavigateUpPressed;
        public event Action OnNavigateDownPressed;
        public event Action OnSubmitPressed;
        public event Action OnCancelPressed;

        private void OnEnable()
        {
            // Treat navigate as held on open so a direction already held (e.g. W while walking) needs
            // a release before it steps the selection.
            m_isNavigateHeld = true;
            m_enabledFrame = Time.frameCount;
            m_submitAction.action.performed += HandleSubmitPerformed;
            m_cancelAction.action.performed += HandleCancelPerformed;
            m_navigateAction.action.Enable();
            m_submitAction.action.Enable();
            m_cancelAction.action.Enable();
        }

        // NOTE(Engine): The actions are shared assets. ModalStack always disables the losing reader before
        // enabling the gaining one, so this Disable never switches off the actions of the newly focused screen.
        private void OnDisable()
        {
            m_submitAction.action.performed -= HandleSubmitPerformed;
            m_cancelAction.action.performed -= HandleCancelPerformed;
            m_navigateAction.action.Disable();
            m_submitAction.action.Disable();
            m_cancelAction.action.Disable();
        }

        // Navigate is a PassThrough value action, so steps come from polling its vertical axis edge.
        private void Update()
        {
            float verticalInput = m_navigateAction.action.ReadValue<Vector2>().y;
            bool isPastThreshold = Mathf.Abs(verticalInput) >= k_navigateThreshold;

            if (isPastThreshold && !m_isNavigateHeld)
            {
                if (verticalInput > 0.0f)
                {
                    OnNavigateUpPressed?.Invoke();
                }
                else
                {
                    OnNavigateDownPressed?.Invoke();
                }
            }

            m_isNavigateHeld = isPastThreshold;
        }

        private void HandleSubmitPerformed(InputAction.CallbackContext context)
        {
            if (IsFocusingPress())
            {
                return;
            }

            OnSubmitPressed?.Invoke();
        }

        private void HandleCancelPerformed(InputAction.CallbackContext context)
        {
            if (IsFocusingPress())
            {
                return;
            }

            OnCancelPressed?.Invoke();
        }

        // NOTE(Engine): Enabling actions from inside a press's callback makes the Input System re-resolve
        // bindings and replay that same press. Tab opening the inventory would replay as Cancel, and Enter or
        // Esc that switches screens would replay into the newly focused one, so presses in the focus frame are ignored.
        private bool IsFocusingPress()
        {
            return Time.frameCount == m_enabledFrame;
        }
    }
}
