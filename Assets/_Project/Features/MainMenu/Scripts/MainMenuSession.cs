using System;
using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.MainMenu
{
    // Runs the start menu: the only screen in its scene, so its menu input stays enabled for its whole life.
    public class MainMenuSession : MonoBehaviour
    {
        [SerializeField] private MenuOptionCursor m_optionCursor;
        [SerializeField] private MenuInputReader m_inputReader;
        [SerializeField] private SceneReference m_gameplayScene;

        public MainMenuOption SelectedOption => (MainMenuOption)m_optionCursor.SelectedIndex;

        private void Awake()
        {
            m_optionCursor.Configure(Enum.GetValues(typeof(MainMenuOption)).Length);
        }

        private void OnEnable()
        {
            m_optionCursor.OnOptionSubmitted += HandleOptionSubmitted;
            m_inputReader.enabled = true;
        }

        private void OnDisable()
        {
            m_optionCursor.OnOptionSubmitted -= HandleOptionSubmitted;
            m_inputReader.enabled = false;
        }

        private void HandleOptionSubmitted(int optionIndex)
        {
            switch ((MainMenuOption)optionIndex)
            {
                case MainMenuOption.PlayGame:
                    m_gameplayScene.Load();
                    break;
                case MainMenuOption.ExitGame:
                    ExitGame();
                    break;
            }
        }

        private static void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
