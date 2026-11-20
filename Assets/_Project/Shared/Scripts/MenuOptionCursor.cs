using System;
using UnityEngine;

namespace FixedCameraPrototype.Shared
{
    // Sole authority for which option of a fixed option menu is highlighted. Steps wrap around like the
    // inventory list; Submit reports the highlighted option. Menus map the index to their own option enum.
    public class MenuOptionCursor : MonoBehaviour
    {
        private const int k_firstOptionIndex = 0;

        [SerializeField] private MenuInputReader m_inputReader;

        private int m_optionCount;
        private int m_selectedIndex = k_firstOptionIndex;

        public int SelectedIndex => m_selectedIndex;

        public event Action<int> OnSelectedIndexChanged;
        public event Action<int> OnOptionSubmitted;
        public event Action OnCancelPressed;

        private void OnEnable()
        {
            m_inputReader.OnNavigateUpPressed += SelectPrevious;
            m_inputReader.OnNavigateDownPressed += SelectNext;
            m_inputReader.OnSubmitPressed += SubmitSelectedOption;
            m_inputReader.OnCancelPressed += HandleCancelPressed;
        }

        private void OnDisable()
        {
            m_inputReader.OnNavigateUpPressed -= SelectPrevious;
            m_inputReader.OnNavigateDownPressed -= SelectNext;
            m_inputReader.OnSubmitPressed -= SubmitSelectedOption;
            m_inputReader.OnCancelPressed -= HandleCancelPressed;
        }

        public void Configure(int optionCount)
        {
            m_optionCount = Mathf.Max(0, optionCount);
            ResetSelection();
        }

        public void ResetSelection()
        {
            SetSelectedIndex(k_firstOptionIndex);
        }

        private void SelectNext()
        {
            StepSelection(1);
        }

        private void SelectPrevious()
        {
            StepSelection(-1);
        }

        private void StepSelection(int indexOffset)
        {
            if (m_optionCount == 0)
            {
                return;
            }

            int wrappedIndex = ((m_selectedIndex + indexOffset) % m_optionCount + m_optionCount) % m_optionCount;
            SetSelectedIndex(wrappedIndex);
        }

        private void SubmitSelectedOption()
        {
            if (m_optionCount == 0)
            {
                return;
            }

            OnOptionSubmitted?.Invoke(m_selectedIndex);
        }

        private void HandleCancelPressed()
        {
            OnCancelPressed?.Invoke();
        }

        private void SetSelectedIndex(int selectedIndex)
        {
            if (selectedIndex == m_selectedIndex)
            {
                return;
            }

            m_selectedIndex = selectedIndex;
            OnSelectedIndexChanged?.Invoke(m_selectedIndex);
        }
    }
}
