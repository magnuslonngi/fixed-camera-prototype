using FixedCameraPrototype.Shared;
using UnityEngine;
using UnityEngine.UIElements;

namespace FixedCameraPrototype.Inventory
{
    // Stateless view: every refresh rebuilds from PlayerInventory and ItemSelectionSession.
    [RequireComponent(typeof(UIDocument))]
    public class InventoryHud : MonoBehaviour
    {
        private const string k_panelName = "inventory-panel";
        private const string k_itemListName = "item-list";
        private const string k_emptyLabelName = "empty-label";
        private const string k_feedbackLabelName = "feedback-label";
        private const string k_descriptionLabelName = "description-label";
        private const string k_hintLabelName = "hint-label";
        private const string k_visibleDescriptionClassName = "inventory-panel__description--visible";
        private const string k_equippedSuffix = " (equipped)";
        private const string k_quantitySuffixFormat = " x{0}";
        private const string k_statusSuffixFormat = " {0}";
        private const string k_targetUseHintText = "W/S select   Enter use   Esc cancel";
        private const string k_browseHintFormat = "W/S select   Enter {0}   Esc close";
        private const string k_equipVerb = "equip";
        private const string k_unequipVerb = "unequip";
        private const string k_readVerb = "read";
        private const string k_inspectVerb = "inspect";
        private const string k_hiddenPanelClassName = "inventory-panel--hidden";
        private const string k_selectingPanelClassName = "inventory-panel--selecting";
        private const string k_itemClassName = "inventory-item";
        private const string k_selectedItemClassName = "inventory-item--selected";
        private const string k_visibleFeedbackClassName = "inventory-panel__feedback--visible";
        private const long k_rejectionFeedbackDurationMilliseconds = 1500;

        [SerializeField] private PlayerInventory m_inventory;
        [SerializeField] private ItemSelectionSession m_selectionSession;
        [SerializeField] private ItemActionPerformer m_actionPerformer;
        [SerializeField] private EquipmentSlot m_equipmentSlot;
        [SerializeField] private string m_rejectionFeedbackText = "It doesn't work here.";

        private VisualElement m_panel;
        private VisualElement m_itemList;
        private Label m_emptyLabel;
        private Label m_feedbackLabel;
        private Label m_descriptionLabel;
        private Label m_hintLabel;
        private IVisualElementScheduledItem m_hideFeedbackScheduledItem;

        // Optional: only present when another feature (e.g. Combat) owns live item state on the character.
        private IItemStatusSource m_itemStatusSource;

        private void OnEnable()
        {
            VisualElement rootElement = GetComponent<UIDocument>().rootVisualElement;
            m_panel = rootElement.Q(k_panelName);
            m_itemList = rootElement.Q(k_itemListName);
            m_emptyLabel = rootElement.Q<Label>(k_emptyLabelName);
            m_feedbackLabel = rootElement.Q<Label>(k_feedbackLabelName);
            m_descriptionLabel = rootElement.Q<Label>(k_descriptionLabelName);
            m_hintLabel = rootElement.Q<Label>(k_hintLabelName);
            m_hideFeedbackScheduledItem = m_feedbackLabel.schedule.Execute(HideRejectionFeedback);
            m_hideFeedbackScheduledItem.Pause();

            m_inventory.OnItemsChanged += Refresh;
            m_selectionSession.OnSelectionStarted += Refresh;
            m_selectionSession.OnSelectionEnded += HandleSelectionEnded;
            m_selectionSession.OnSelectedIndexChanged += HandleSelectedIndexChanged;
            m_selectionSession.OnItemRejected += ShowRejectionFeedback;
            m_actionPerformer.OnDescriptionRequested += ShowDescription;
            m_equipmentSlot.OnEquippedItemChanged += HandleEquippedItemChanged;

            if (m_inventory.TryGetComponent(out m_itemStatusSource))
            {
                m_itemStatusSource.OnStatusChanged += Refresh;
            }

            Refresh();
        }

        private void OnDisable()
        {
            m_inventory.OnItemsChanged -= Refresh;
            m_selectionSession.OnSelectionStarted -= Refresh;
            m_selectionSession.OnSelectionEnded -= HandleSelectionEnded;
            m_selectionSession.OnSelectedIndexChanged -= HandleSelectedIndexChanged;
            m_selectionSession.OnItemRejected -= ShowRejectionFeedback;
            m_actionPerformer.OnDescriptionRequested -= ShowDescription;
            m_equipmentSlot.OnEquippedItemChanged -= HandleEquippedItemChanged;
            m_hideFeedbackScheduledItem.Pause();

            if (m_itemStatusSource != null)
            {
                m_itemStatusSource.OnStatusChanged -= Refresh;
            }
        }

        private void Refresh()
        {
            bool isSelecting = m_selectionSession.IsActive;
            bool hasItems = m_inventory.ItemCount > 0;

            m_panel.EnableInClassList(k_hiddenPanelClassName, !isSelecting && !hasItems);
            m_panel.EnableInClassList(k_selectingPanelClassName, isSelecting);
            m_emptyLabel.style.display = hasItems ? DisplayStyle.None : DisplayStyle.Flex;
            RebuildItemList(isSelecting);
            m_hintLabel.text = ResolveHintText();
        }

        private void RebuildItemList(bool isSelecting)
        {
            m_itemList.Clear();
            for (int i = 0; i < m_inventory.ItemCount; i++)
            {
                Label itemLabel = new Label(BuildItemLabelText(m_inventory.GetItemAt(i)));
                itemLabel.AddToClassList(k_itemClassName);
                itemLabel.EnableInClassList(k_selectedItemClassName, isSelecting && i == m_selectionSession.SelectedIndex);
                m_itemList.Add(itemLabel);
            }
        }

        private string BuildItemLabelText(ItemDefinition item)
        {
            string quantitySuffix = item.IsStackable ? string.Format(k_quantitySuffixFormat, m_inventory.GetQuantity(item)) : string.Empty;
            string statusSuffix = TryGetItemStatusText(item, out string statusText) ? string.Format(k_statusSuffixFormat, statusText) : string.Empty;
            string equippedSuffix = m_equipmentSlot.IsEquipped(item) ? k_equippedSuffix : string.Empty;
            return item.DisplayName + quantitySuffix + statusSuffix + equippedSuffix;
        }

        private bool TryGetItemStatusText(ItemDefinition item, out string statusText)
        {
            statusText = null;
            return m_itemStatusSource != null && m_itemStatusSource.TryGetStatusText(item, out statusText);
        }

        private string ResolveHintText()
        {
            if (!m_selectionSession.IsBrowsing)
            {
                return k_targetUseHintText;
            }

            ItemDefinition selectedItem = m_selectionSession.SelectedItem;
            return selectedItem == null ? k_targetUseHintText : string.Format(k_browseHintFormat, ResolveActionVerb(selectedItem));
        }

        private string ResolveActionVerb(ItemDefinition item)
        {
            switch (item.UseAction)
            {
                case ItemUseAction.Equip:
                    return m_equipmentSlot.IsEquipped(item) ? k_unequipVerb : k_equipVerb;
                case ItemUseAction.Read:
                    return k_readVerb;
                default:
                    return k_inspectVerb;
            }
        }

        private void HandleEquippedItemChanged(ItemDefinition equippedItem)
        {
            Refresh();
        }

        private void HandleSelectedIndexChanged(int selectedIndex)
        {
            HideDescription();
            Refresh();
        }

        private void HandleSelectionEnded()
        {
            HideRejectionFeedback();
            HideDescription();
            Refresh();
        }

        private void ShowRejectionFeedback(ItemDefinition rejectedItem)
        {
            m_feedbackLabel.text = m_rejectionFeedbackText;
            m_feedbackLabel.AddToClassList(k_visibleFeedbackClassName);
            m_hideFeedbackScheduledItem.ExecuteLater(k_rejectionFeedbackDurationMilliseconds);
        }

        private void ShowDescription(ItemDefinition item)
        {
            m_descriptionLabel.text = item.DescriptionText;
            m_descriptionLabel.AddToClassList(k_visibleDescriptionClassName);
        }

        private void HideDescription()
        {
            m_descriptionLabel.RemoveFromClassList(k_visibleDescriptionClassName);
        }

        private void HideRejectionFeedback()
        {
            m_hideFeedbackScheduledItem.Pause();
            m_feedbackLabel.RemoveFromClassList(k_visibleFeedbackClassName);
        }
    }
}
