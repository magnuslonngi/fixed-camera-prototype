using System;
using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Inventory
{
    // Sole authority for the inventory screen. Browsing (opened with the inventory key) hands the chosen
    // item to the item window; target mode (opened by an IItemUseTarget) uses the chosen item on that target.
    public class ItemSelectionSession : MonoBehaviour, IItemSelectionPrompt, IModalScreen
    {
        private const int k_noSelectionIndex = -1;

        [SerializeField] private PlayerInventory m_inventory;
        [SerializeField] private MenuInputReader m_inputReader;
        [SerializeField] private ModalStack m_modalStack;
        [SerializeField] private InventoryInputReader m_inventoryInputReader;
        [SerializeField] private ItemExamineSession m_examineSession;
        [SerializeField] private EquipmentSlot m_equipmentSlot;

        private IItemUseTarget m_activeTarget;
        private int m_selectedIndex = k_noSelectionIndex;

        public bool IsActive => m_modalStack.IsOpen(this);
        public bool IsBrowsing => IsActive && m_activeTarget == null;
        public IItemUseTarget ActiveTarget => m_activeTarget;
        public int SelectedIndex => m_selectedIndex;
        public ItemDefinition SelectedItem => HasValidSelection() ? m_inventory.GetItemAt(m_selectedIndex) : null;

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
            m_inventoryInputReader.OnOpenInventoryPressed += OpenInventory;
        }

        private void OnDisable()
        {
            m_inputReader.OnNavigateUpPressed -= SelectPrevious;
            m_inputReader.OnNavigateDownPressed -= SelectNext;
            m_inputReader.OnSubmitPressed -= UseSelectedItem;
            m_inputReader.OnCancelPressed -= EndSelection;
            m_inventoryInputReader.OnOpenInventoryPressed -= OpenInventory;
            EndSelection();
        }

        public void BeginItemSelection(IItemUseTarget target)
        {
            if (target == null)
            {
                return;
            }

            Open(target);
        }

        public void OpenInventory()
        {
            Open(null);
        }

        public void EndSelection()
        {
            if (!IsActive)
            {
                return;
            }

            m_modalStack.Pop(this);
            m_activeTarget = null;
            SetSelectedIndex(k_noSelectionIndex);
            OnSelectionEnded?.Invoke();
        }

        public void SetInputFocus(bool hasFocus)
        {
            m_inputReader.enabled = hasFocus;
        }

        private void Open(IItemUseTarget target)
        {
            if (IsActive)
            {
                return;
            }

            m_activeTarget = target;
            m_modalStack.Push(this);
            OnSelectionStarted?.Invoke();
            SetSelectedIndex(m_inventory.ItemCount > 0 ? 0 : k_noSelectionIndex);
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
            int itemCount = m_inventory.ItemCount;
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

            if (IsBrowsing)
            {
                m_examineSession.ExamineItem(selectedItem);
                return;
            }

            if (!m_activeTarget.TryUseItem(selectedItem))
            {
                OnItemRejected?.Invoke(selectedItem);
                return;
            }

            if (m_equipmentSlot.IsEquipped(selectedItem))
            {
                m_equipmentSlot.Unequip();
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
            return m_selectedIndex >= 0 && m_selectedIndex < m_inventory.ItemCount;
        }
    }
}
