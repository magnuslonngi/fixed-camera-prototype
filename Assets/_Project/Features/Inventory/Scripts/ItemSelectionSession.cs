using System;
using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Inventory
{
    // Sole authority for the modal "use an item on something" state. While open it holds the
    // character control lock, so gameplay input is off and only the selection input is live.
    public class ItemSelectionSession : MonoBehaviour, IItemSelectionPrompt
    {
        private const int k_noSelectionIndex = -1;

        [SerializeField] private PlayerInventory m_inventory;
        [SerializeField] private ItemSelectionInputReader m_inputReader;
        [SerializeField] private CharacterControlLock m_controlLock;

        private IItemUseTarget m_activeTarget;
        private int m_selectedIndex = k_noSelectionIndex;

        public bool IsActive => m_activeTarget != null;
        public int SelectedIndex => m_selectedIndex;
        public ItemDefinition SelectedItem => HasValidSelection() ? m_inventory.Items[m_selectedIndex] : null;

        public event Action OnSelectionStarted;
        public event Action OnSelectionEnded;
        public event Action<int> OnSelectedIndexChanged;
        public event Action<ItemDefinition> OnItemRejected;

        private void Awake()
        {
            m_inputReader.enabled = false;
        }

        private void OnEnable()
        {
            m_inputReader.OnNavigateUpPressed += SelectPrevious;
            m_inputReader.OnNavigateDownPressed += SelectNext;
            m_inputReader.OnSubmitPressed += UseSelectedItem;
            m_inputReader.OnCancelPressed += EndSelection;
        }

        private void OnDisable()
        {
            m_inputReader.OnNavigateUpPressed -= SelectPrevious;
            m_inputReader.OnNavigateDownPressed -= SelectNext;
            m_inputReader.OnSubmitPressed -= UseSelectedItem;
            m_inputReader.OnCancelPressed -= EndSelection;
            EndSelection();
        }

        public void BeginItemSelection(IItemUseTarget target)
        {
            if (IsActive || target == null)
            {
                return;
            }

            m_activeTarget = target;
            m_controlLock.AddLock();
            m_inputReader.enabled = true;
            OnSelectionStarted?.Invoke();
            SetSelectedIndex(m_inventory.Items.Count > 0 ? 0 : k_noSelectionIndex);
        }

        public void EndSelection()
        {
            if (!IsActive)
            {
                return;
            }

            m_activeTarget = null;
            m_inputReader.enabled = false;
            m_controlLock.RemoveLock();
            SetSelectedIndex(k_noSelectionIndex);
            OnSelectionEnded?.Invoke();
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
            int itemCount = m_inventory.Items.Count;
            if (itemCount == 0)
            {
                return;
            }

            int wrappedIndex = ((m_selectedIndex + indexOffset) % itemCount + itemCount) % itemCount;
            SetSelectedIndex(wrappedIndex);
        }

        private void UseSelectedItem()
        {
            ItemDefinition selectedItem = SelectedItem;
            if (selectedItem == null)
            {
                return;
            }

            if (!m_activeTarget.TryUseItem(selectedItem))
            {
                OnItemRejected?.Invoke(selectedItem);
                return;
            }

            m_inventory.TryRemoveItem(selectedItem);
            EndSelection();
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

        private bool HasValidSelection()
        {
            return m_selectedIndex >= 0 && m_selectedIndex < m_inventory.Items.Count;
        }
    }
}
