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
        private const string k_hiddenPanelClassName = "inventory-panel--hidden";
        private const string k_selectingPanelClassName = "inventory-panel--selecting";
        private const string k_itemClassName = "inventory-item";
        private const string k_selectedItemClassName = "inventory-item--selected";
        private const string k_visibleFeedbackClassName = "inventory-panel__feedback--visible";
        private const long k_rejectionFeedbackDurationMilliseconds = 1500;

        [SerializeField] private PlayerInventory m_inventory;
        [SerializeField] private ItemSelectionSession m_selectionSession;
        [SerializeField] private string m_rejectionFeedbackText = "It doesn't work here.";

        private VisualElement m_panel;
        private VisualElement m_itemList;
        private Label m_emptyLabel;
        private Label m_feedbackLabel;
        private IVisualElementScheduledItem m_hideFeedbackScheduledItem;

        private void OnEnable()
        {
            VisualElement rootElement = GetComponent<UIDocument>().rootVisualElement;
            m_panel = rootElement.Q(k_panelName);
            m_itemList = rootElement.Q(k_itemListName);
            m_emptyLabel = rootElement.Q<Label>(k_emptyLabelName);
            m_feedbackLabel = rootElement.Q<Label>(k_feedbackLabelName);
            m_hideFeedbackScheduledItem = m_feedbackLabel.schedule.Execute(HideRejectionFeedback);
            m_hideFeedbackScheduledItem.Pause();

            m_inventory.OnItemsChanged += Refresh;
            m_selectionSession.OnSelectionStarted += Refresh;
            m_selectionSession.OnSelectionEnded += HandleSelectionEnded;
            m_selectionSession.OnSelectedIndexChanged += HandleSelectedIndexChanged;
            m_selectionSession.OnItemRejected += ShowRejectionFeedback;
            Refresh();
        }

        private void OnDisable()
        {
            m_inventory.OnItemsChanged -= Refresh;
            m_selectionSession.OnSelectionStarted -= Refresh;
            m_selectionSession.OnSelectionEnded -= HandleSelectionEnded;
            m_selectionSession.OnSelectedIndexChanged -= HandleSelectedIndexChanged;
            m_selectionSession.OnItemRejected -= ShowRejectionFeedback;
            m_hideFeedbackScheduledItem.Pause();
        }

        private void Refresh()
        {
            bool isSelecting = m_selectionSession.IsActive;
            bool hasItems = m_inventory.Items.Count > 0;

            m_panel.EnableInClassList(k_hiddenPanelClassName, !isSelecting && !hasItems);
            m_panel.EnableInClassList(k_selectingPanelClassName, isSelecting);
            m_emptyLabel.style.display = hasItems ? DisplayStyle.None : DisplayStyle.Flex;
            RebuildItemList(isSelecting);
        }

        private void RebuildItemList(bool isSelecting)
        {
            m_itemList.Clear();
            for (int i = 0; i < m_inventory.Items.Count; i++)
            {
                Label itemLabel = new Label(m_inventory.Items[i].DisplayName);
                itemLabel.AddToClassList(k_itemClassName);
                itemLabel.EnableInClassList(k_selectedItemClassName, isSelecting && i == m_selectionSession.SelectedIndex);
                m_itemList.Add(itemLabel);
            }
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
