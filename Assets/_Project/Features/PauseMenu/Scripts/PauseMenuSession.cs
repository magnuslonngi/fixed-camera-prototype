using System;
using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.PauseMenu
{
    // Sole authority for the pause menu. Opening pushes a modal, which pauses the game and locks the
    // character; Resume or Cancel pops it, Return to Main Menu leaves the level.
    public class PauseMenuSession : MonoBehaviour, IModalScreen
    {
        [SerializeField] private ModalStack m_modalStack;
        [SerializeField] private MenuInputReader m_inputReader;
        [SerializeField] private MenuOptionCursor m_optionCursor;
        [SerializeField] private PauseInputReader m_pauseInputReader;
        [SerializeField] private SceneReference m_mainMenuScene;

        public bool IsOpen => m_modalStack.IsOpen(this);
        public PauseMenuOption SelectedOption => (PauseMenuOption)m_optionCursor.SelectedIndex;

        public event Action OnPauseMenuOpened;
        public event Action OnPauseMenuClosed;

        private void Awake()
        {
            m_inputReader.enabled = false;
            m_optionCursor.Configure(Enum.GetValues(typeof(PauseMenuOption)).Length);
        }

        private void OnEnable()
        {
            m_pauseInputReader.OnPausePressed += OpenPauseMenu;
            m_optionCursor.OnOptionSubmitted += HandleOptionSubmitted;
            m_optionCursor.OnCancelPressed += ClosePauseMenu;
        }

        private void OnDisable()
        {
            m_pauseInputReader.OnPausePressed -= OpenPauseMenu;
            m_optionCursor.OnOptionSubmitted -= HandleOptionSubmitted;
            m_optionCursor.OnCancelPressed -= ClosePauseMenu;
            ClosePauseMenu();
        }

        public void OpenPauseMenu()
        {
            if (IsOpen)
            {
                return;
            }

            m_optionCursor.ResetSelection();
            m_modalStack.Push(this);
            OnPauseMenuOpened?.Invoke();
        }

        public void ClosePauseMenu()
        {
            if (!IsOpen)
            {
                return;
            }

            m_modalStack.Pop(this);
            OnPauseMenuClosed?.Invoke();
        }

        public void SetInputFocus(bool hasFocus)
        {
            m_inputReader.enabled = hasFocus;
        }

        private void HandleOptionSubmitted(int optionIndex)
        {
            switch ((PauseMenuOption)optionIndex)
            {
                case PauseMenuOption.Resume:
                    ClosePauseMenu();
                    break;
                case PauseMenuOption.ReturnToMainMenu:
                    // Stays open while the scene unloads so gameplay never runs unpaused for a frame;
                    // GamePause restores the time scale when it is destroyed with the level.
                    m_mainMenuScene.Load();
                    break;
            }
        }
    }
}
