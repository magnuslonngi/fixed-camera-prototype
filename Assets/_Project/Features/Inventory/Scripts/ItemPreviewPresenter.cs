using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Inventory
{
    // Single place deciding which model the preview stage shows, derived from the open screens:
    // the item window wins, then the inventory's target (target mode) or selected item.
    public class ItemPreviewPresenter : MonoBehaviour
    {
        [SerializeField] private ItemPreviewStage m_previewStage;
        [SerializeField] private ItemSelectionSession m_selectionSession;
        [SerializeField] private ItemExamineSession m_examineSession;

        private void OnEnable()
        {
            m_selectionSession.OnSelectionStarted += Refresh;
            m_selectionSession.OnSelectionEnded += Refresh;
            m_selectionSession.OnSelectedIndexChanged += HandleSelectedIndexChanged;
            m_examineSession.OnExamineOpened += HandleExamineOpened;
            m_examineSession.OnExamineClosed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            m_selectionSession.OnSelectionStarted -= Refresh;
            m_selectionSession.OnSelectionEnded -= Refresh;
            m_selectionSession.OnSelectedIndexChanged -= HandleSelectedIndexChanged;
            m_examineSession.OnExamineOpened -= HandleExamineOpened;
            m_examineSession.OnExamineClosed -= Refresh;
        }

        private void Refresh()
        {
            m_previewStage.Show(ResolvePreviewPrefab());
        }

        private GameObject ResolvePreviewPrefab()
        {
            if (m_examineSession.IsOpen)
            {
                return m_examineSession.ExaminedItem.PreviewPrefab;
            }

            if (!m_selectionSession.IsActive)
            {
                return null;
            }

            if (!m_selectionSession.IsBrowsing)
            {
                return m_selectionSession.ActiveTarget.PreviewPrefab;
            }

            ItemDefinition selectedItem = m_selectionSession.SelectedItem;
            return selectedItem != null ? selectedItem.PreviewPrefab : null;
        }

        private void HandleSelectedIndexChanged(int selectedIndex)
        {
            Refresh();
        }

        private void HandleExamineOpened(ItemDefinition examinedItem)
        {
            Refresh();
        }
    }
}
