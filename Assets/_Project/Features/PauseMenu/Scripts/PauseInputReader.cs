using System;
using FixedCameraPrototype.Shared;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FixedCameraPrototype.PauseMenu
{
    // Pause is a gameplay action: it is only enabled while the character is controllable, so it cannot
    // open over the inventory, an examined item, a document, or after death or level completion.
    public class PauseInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference m_pauseAction;
        [SerializeField] private CharacterControlLock m_controlLock;

        private int m_enabledFrame;

        public event Action OnPausePressed;

        private void OnEnable()
        {
            m_pauseAction.action.performed += HandlePausePerformed;
            m_controlLock.OnLockStateChanged += HandleControlLockStateChanged;
            SetActionEnabled(!m_controlLock.IsLocked);
        }

        private void OnDisable()
        {
            m_pauseAction.action.performed -= HandlePausePerformed;
            m_controlLock.OnLockStateChanged -= HandleControlLockStateChanged;
            SetActionEnabled(false);
        }

        // NOTE(Engine): Esc also closes modal screens (UI Cancel). Closing one releases the control lock and
        // re-enables this action inside that press's callback, which replays the press here; ignore it.
        private void HandlePausePerformed(InputAction.CallbackContext context)
        {
            if (Time.frameCount == m_enabledFrame)
            {
                return;
            }

            OnPausePressed?.Invoke();
        }

        private void HandleControlLockStateChanged(bool isLocked)
        {
            SetActionEnabled(!isLocked);
        }

        private void SetActionEnabled(bool isEnabled)
        {
            if (isEnabled)
            {
                m_enabledFrame = Time.frameCount;
                m_pauseAction.action.Enable();
            }
            else
            {
                m_pauseAction.action.Disable();
            }
        }
    }
}
