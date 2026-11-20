using System;
using FixedCameraPrototype.Shared;
using UnityEngine;
using UnityEngine.UIElements;

namespace FixedCameraPrototype.MainMenu
{
    // Stateless view of the start menu: redraws the highlight from MenuOptionCursor.
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuHud : MonoBehaviour
    {
        private const string k_optionListName = "option-list";
        private const string k_playGameText = "Play Game";
        private const string k_exitGameText = "Exit Game";

        [SerializeField] private MenuOptionCursor m_optionCursor;

        private MenuOptionListView m_optionListView;

        private void OnEnable()
        {
            VisualElement rootElement = GetComponent<UIDocument>().rootVisualElement;
            m_optionListView = new MenuOptionListView(rootElement.Q(k_optionListName), BuildOptionTexts());
            m_optionCursor.OnSelectedIndexChanged += m_optionListView.ShowSelection;
            m_optionListView.ShowSelection(m_optionCursor.SelectedIndex);
        }

        private void OnDisable()
        {
            m_optionCursor.OnSelectedIndexChanged -= m_optionListView.ShowSelection;
        }

        private static string[] BuildOptionTexts()
        {
            MainMenuOption[] options = (MainMenuOption[])Enum.GetValues(typeof(MainMenuOption));
            return Array.ConvertAll(options, ResolveOptionText);
        }

        private static string ResolveOptionText(MainMenuOption option)
        {
            switch (option)
            {
                case MainMenuOption.PlayGame:
                    return k_playGameText;
                case MainMenuOption.ExitGame:
                    return k_exitGameText;
                default:
                    return option.ToString();
            }
        }
    }
}
