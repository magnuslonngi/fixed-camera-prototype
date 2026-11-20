using FixedCameraPrototype.Shared;
using UnityEngine;
using UnityEngine.UIElements;

namespace FixedCameraPrototype.Inventory
{
    // Stateless view of the full-screen inventory: every refresh rebuilds from PlayerInventory and
    // ItemSelectionSession. The 3D preview itself is driven by ItemPreviewPresenter.
    [RequireComponent(typeof(UIDocument))]
    public class InventoryScreenHud : MonoBehaviour
    {
        private const string k_screenName = "inventory-screen";
        private const string k_titleLabelName = "title-label";
        private const string k_itemListName = "item-list";
        private const string k_emptyLabelName = "empty-label";
        private const string k_feedbackLabelName = "feedback-label";
        private const string k_previewImageName = "preview-image";
        private const string k_descriptionLabelName = "description-label";
        private const string k_hintLabelName = "hint-label";
        private const string k_visibleScreenClassName = "inventory-screen--visible";
        private const string k_itemClassName = "inventory-item";
        private const string k_selectedItemClassName = "inventory-item--selected";
        private const string k_visibleFeedbackClassName = "inventory-screen__feedback--visible";
        private const string k_browseTitleText = "Inventory";
        private const string k_targetTitleText = "Use which item?";
        private const string k_browseHintText = "W/S ↑/↓  select     Enter  examine     Esc/Tab  close";
        private const string k_targetHintText = "W/S ↑/↓  select     Enter  use     Esc/Tab  cancel";
        private const string k_equippedSuffix = "  (equipped)";
        private const string k_quantitySuffixFormat = "  x{0}";
        private const string k_statusSuffixFormat = "  {0}";
        private const long k_rejectionFeedbackDurationMilliseconds = 1500;

        [SerializeField] private PlayerInventory m_inventory;
        [SerializeField] private ItemSelectionSession m_selectionSession;
        [SerializeField] private EquipmentSlot m_equipmentSlot;
        [SerializeField] private ItemPreviewStage m_previewStage;
        [SerializeField] private string m_rejectionFeedbackText = "It doesn't work here.";

        private VisualElement m_screen;
        private Label m_titleLabel;
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
            m_screen = rootElement.Q(k_screenName);
            m_titleLabel = rootElement.Q<Label>(k_titleLabelName);
            m_itemList = rootElement.Q(k_itemListName);
            m_emptyLabel = rootElement.Q<Label>(k_emptyLabelName);
            m_feedbackLabel = rootElement.Q<Label>(k_feedbackLabelName);
            m_descriptionLabel = rootElement.Q<Label>(k_descriptionLabelName);
            m_hintLabel = rootElement.Q<Label>(k_hintLabelName);
            rootElement.Q(k_previewImageName).style.backgroundImage = Background.FromRenderTexture(m_previewStage.PreviewTexture);
            m_hideFeedbackScheduledItem = m_feedbackLabel.schedule.Execute(HideRejectionFeedback);
            m_hideFeedbackScheduledItem.Pause();

            m_inventory.OnItemsChanged += Refresh;
            m_selectionSession.OnSelectionStarted += Refresh;
            m_selectionSession.OnSelectionEnded += HandleSelectionEnded;
            m_selectionSession.OnSelectedIndexChanged += HandleSelectedIndexChanged;
            m_selectionSession.OnItemRejected += ShowRejectionFeedback;
            m_equipmentSlot.OnEquippedItemChanged += HandleEquippedItemChanged;

            if (CapabilityRoot.TryFindCapability(m_inventory.gameObject, out m_itemStatusSource))
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
            m_equipmentSlot.OnEquippedItemChanged -= HandleEquippedItemChanged;
            m_hideFeedbackScheduledItem.Pause();

            if (m_itemStatusSource != null)
            {
                m_itemStatusSource.OnStatusChanged -= Refresh;
            }
        }

        private void Refresh()
        {
            bool isActive = m_selectionSession.IsActive;
            m_screen.EnableInClassList(k_visibleScreenClassName, isActive);
            if (!isActive)
            {
                return;
            }

            bool isBrowsing = m_selectionSession.IsBrowsing;
            m_titleLabel.text = isBrowsing ? k_browseTitleText : k_targetTitleText;
            m_hintLabel.text = isBrowsing ? k_browseHintText : k_targetHintText;
            m_emptyLabel.style.display = m_inventory.ItemCount > 0 ? DisplayStyle.None : DisplayStyle.Flex;
            m_descriptionLabel.text = ResolveDescriptionText(isBrowsing);
            RebuildItemList();
        }

        // Target mode previews the target, so an item description would describe the wrong thing.
        private string ResolveDescriptionText(bool isBrowsing)
        {
            ItemDefinition selectedItem = m_selectionSession.SelectedItem;
            return isBrowsing && selectedItem != null ? selectedItem.DescriptionText : string.Empty;
        }

        private void RebuildItemList()
        {
            m_itemList.Clear();
            for (int i = 0; i < m_inventory.ItemCount; i++)
            {
                Label itemLabel = new Label(BuildItemLabelText(m_inventory.GetItemAt(i)));
                itemLabel.AddToClassList(k_itemClassName);
                itemLabel.EnableInClassList(k_selectedItemClassName, i == m_selectionSession.SelectedIndex);
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

        private void HandleEquippedItemChanged(ItemDefinition equippedItem)
        {
            Refresh();
        }

        private void HandleSelectedIndexChanged(int selectedIndex)
        {
            Refresh();
        }

        private void HandleSelectionEnded()
        {
            HideRejectionFeedback();
            Refresh();
        }

        private void ShowRejectionFeedback(ItemDefinition rejectedItem)
        {
            m_feedbackLabel.text = m_rejectionFeedbackText;
            m_feedbackLabel.AddToClassList(k_visibleFeedbackClassName);
            m_hideFeedbackScheduledItem.ExecuteLater(k_rejectionFeedbackDurationMilliseconds);
        }

        private void HideRejectionFeedback()
        {
            m_hideFeedbackScheduledItem.Pause();
            m_feedbackLabel.RemoveFromClassList(k_visibleFeedbackClassName);
        }
    }
}
