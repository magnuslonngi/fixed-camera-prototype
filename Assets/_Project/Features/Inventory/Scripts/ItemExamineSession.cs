using System;
using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Inventory
{
    // Sole authority for the item window: one item shown full screen with its own action (read or equip).
    // Closing only pops this screen, so it returns to the inventory when opened from there and to
    // gameplay when opened by a pickup.
    public class ItemExamineSession : MonoBehaviour, IItemExaminePrompt, IModalScreen
    {
        [SerializeField] private ModalStack m_modalStack;
        [SerializeField] private MenuInputReader m_inputReader;
        [SerializeField] private EquipmentSlot m_equipmentSlot;

        private ItemDefinition m_examinedItem;

        // Optional: only present when another feature (e.g. Reading) can show documents on this character.
        private IDocumentReader m_documentReader;

        public bool IsOpen => m_modalStack.IsOpen(this);
        public ItemDefinition ExaminedItem => m_examinedItem;

        public event Action<ItemDefinition> OnExamineOpened;
        public event Action OnExamineClosed;

        private void Awake()
        {
            m_inputReader.enabled = false;
            CapabilityRoot.TryFindCapability(gameObject, out m_documentReader);
        }

        private void OnEnable()
        {
            m_inputReader.OnSubmitPressed += PerformExaminedItemAction;
            m_inputReader.OnCancelPressed += CloseExamine;
        }

        private void OnDisable()
        {
            m_inputReader.OnSubmitPressed -= PerformExaminedItemAction;
            m_inputReader.OnCancelPressed -= CloseExamine;
            CloseExamine();
        }

        public void ExamineItem(ItemDefinition item)
        {
            if (item == null || IsOpen)
            {
                return;
            }

            m_examinedItem = item;
            m_modalStack.Push(this);
            OnExamineOpened?.Invoke(m_examinedItem);
        }

        public void CloseExamine()
        {
            if (!IsOpen)
            {
                return;
            }

            m_modalStack.Pop(this);
            m_examinedItem = null;
            OnExamineClosed?.Invoke();
        }

        public void SetInputFocus(bool hasFocus)
        {
            m_inputReader.enabled = hasFocus;
        }

        private void PerformExaminedItemAction()
        {
            switch (m_examinedItem.UseAction)
            {
                case ItemUseAction.Read:
                    ReadExaminedItem();
                    break;
                case ItemUseAction.Equip:
                    ToggleExaminedItemEquipped();
                    CloseExamine();
                    break;
            }
        }

        private void ReadExaminedItem()
        {
            if (m_documentReader != null)
            {
                m_documentReader.OpenDocument(m_examinedItem.ReadableText);
            }
        }

        private void ToggleExaminedItemEquipped()
        {
            if (m_equipmentSlot.IsEquipped(m_examinedItem))
            {
                m_equipmentSlot.Unequip();
            }
            else
            {
                m_equipmentSlot.Equip(m_examinedItem);
            }
        }
    }
}
