using System.Collections.Generic;
using UnityEngine.UIElements;

namespace FixedCameraPrototype.Shared
{
    // Draws a menu's options into a UI Toolkit container with the same highlight style as the inventory list.
    // Holds no selection state: callers pass the cursor's index on every change.
    public class MenuOptionListView
    {
        private const string k_optionClassName = "menu-option";
        private const string k_selectedOptionClassName = "menu-option--selected";

        private readonly List<Label> m_optionLabels = new List<Label>();

        public MenuOptionListView(VisualElement optionContainer, IReadOnlyList<string> optionTexts)
        {
            optionContainer.Clear();
            foreach (string optionText in optionTexts)
            {
                Label optionLabel = new Label(optionText);
                optionLabel.AddToClassList(k_optionClassName);
                optionContainer.Add(optionLabel);
                m_optionLabels.Add(optionLabel);
            }
        }

        public void ShowSelection(int selectedIndex)
        {
            for (int i = 0; i < m_optionLabels.Count; i++)
            {
                m_optionLabels[i].EnableInClassList(k_selectedOptionClassName, i == selectedIndex);
            }
        }
    }
}
