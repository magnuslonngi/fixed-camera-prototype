using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FixedCameraPrototype.Inventory
{
    // Reads UI-map actions, which no gameplay reader uses, so they keep working while CharacterControlLock
    // has the gameplay actions disabled. Enabled by ItemSelectionSession only while a selection is open.
    public class ItemSelectionInputReader : MonoBehaviour
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
            if (IsOpeningPress())
            {
                return;
            }

            OnSubmitPressed?.Invoke();
        }

        private void HandleCancelPerformed(InputAction.CallbackContext context)
        {
            if (IsOpeningPress())
            {
                return;
            }

            OnCancelPressed?.Invoke();
        }

        // NOTE(Engine): Tab both opens the inventory (Player map) and cancels it (UI map). Enabling the UI
        // actions from inside the opening press's callback makes the Input System re-resolve bindings and
        // replay that same press to Cancel (seen on the first open per session), closing it instantly.
        private bool IsOpeningPress()
        {
            return Time.frameCount == m_enabledFrame;
        }
    }
}
