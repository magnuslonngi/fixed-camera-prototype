using System;
using FixedCameraPrototype.Shared;
using UnityEngine;
using UnityEngine.UIElements;

namespace FixedCameraPrototype.PauseMenu
{
    // Stateless view of the pause menu: shown while PauseMenuSession is open, highlight from MenuOptionCursor.
    [RequireComponent(typeof(UIDocument))]
    public class PauseMenuHud : MonoBehaviour
    {
        private const string k_screenName = "pause-screen";
        private const string k_optionListName = "option-list";
        private const string k_visibleScreenClassName = "pause-screen--visible";
        private const string k_resumeText = "Resume";
        private const string k_returnToMainMenuText = "Back to Main Menu";

        [SerializeField] private PauseMenuSession m_pauseMenuSession;
        [SerializeField] private MenuOptionCursor m_optionCursor;

        private VisualElement m_screen;
        private MenuOptionListView m_optionListView;

        private void OnEnable()
        {
            VisualElement rootElement = GetComponent<UIDocument>().rootVisualElement;
            m_screen = rootElement.Q(k_screenName);
            m_optionListView = new MenuOptionListView(rootElement.Q(k_optionListName), BuildOptionTexts());

            m_pauseMenuSession.OnPauseMenuOpened += Refresh;
            m_pauseMenuSession.OnPauseMenuClosed += Refresh;
            m_optionCursor.OnSelectedIndexChanged += m_optionListView.ShowSelection;
            Refresh();
        }

        private void OnDisable()
        {
            m_pauseMenuSession.OnPauseMenuOpened -= Refresh;
            m_pauseMenuSession.OnPauseMenuClosed -= Refresh;
            m_optionCursor.OnSelectedIndexChanged -= m_optionListView.ShowSelection;
        }

        private void Refresh()
        {
            m_screen.EnableInClassList(k_visibleScreenClassName, m_pauseMenuSession.IsOpen);
            m_optionListView.ShowSelection(m_optionCursor.SelectedIndex);
        }

        private static string[] BuildOptionTexts()
        {
            PauseMenuOption[] options = (PauseMenuOption[])Enum.GetValues(typeof(PauseMenuOption));
            return Array.ConvertAll(options, ResolveOptionText);
        }

        private static string ResolveOptionText(PauseMenuOption option)
        {
            switch (option)
            {
                case PauseMenuOption.Resume:
                    return k_resumeText;
                case PauseMenuOption.ReturnToMainMenu:
                    return k_returnToMainMenuText;
                default:
                    return option.ToString();
            }
        }
    }
}
