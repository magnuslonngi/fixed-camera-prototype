using FixedCameraPrototype.Shared;
using UnityEngine;
using UnityEngine.UIElements;

namespace FixedCameraPrototype.Inventory
{
    // Stateless view of the item window. The 3D preview itself is driven by ItemPreviewPresenter.
    [RequireComponent(typeof(UIDocument))]
    public class ItemExamineHud : MonoBehaviour
    {
        private const string k_overlayName = "examine-overlay";
        private const string k_previewImageName = "preview-image";
        private const string k_nameLabelName = "name-label";
        private const string k_descriptionLabelName = "description-label";
        private const string k_hintLabelName = "hint-label";
        private const string k_visibleOverlayClassName = "examine-overlay--visible";
        private const string k_actionHintFormat = "Enter  {0}     Esc/Tab  close";
        private const string k_closeOnlyHintText = "Esc/Tab  close";
        private const string k_readVerb = "read";
        private const string k_equipVerb = "equip";
        private const string k_unequipVerb = "unequip";

        [SerializeField] private ItemExamineSession m_examineSession;
        [SerializeField] private EquipmentSlot m_equipmentSlot;
        [SerializeField] private ItemPreviewStage m_previewStage;

        private VisualElement m_overlay;
        private Label m_nameLabel;
        private Label m_descriptionLabel;
        private Label m_hintLabel;

        private void OnEnable()
        {
            VisualElement rootElement = GetComponent<UIDocument>().rootVisualElement;
            m_overlay = rootElement.Q(k_overlayName);
            m_nameLabel = rootElement.Q<Label>(k_nameLabelName);
            m_descriptionLabel = rootElement.Q<Label>(k_descriptionLabelName);
            m_hintLabel = rootElement.Q<Label>(k_hintLabelName);
            rootElement.Q(k_previewImageName).style.backgroundImage = Background.FromRenderTexture(m_previewStage.PreviewTexture);

            m_examineSession.OnExamineOpened += ShowItem;
            m_examineSession.OnExamineClosed += Hide;

            if (m_examineSession.IsOpen)
            {
                ShowItem(m_examineSession.ExaminedItem);
            }
            else
            {
                Hide();
            }
        }

        private void OnDisable()
        {
            m_examineSession.OnExamineOpened -= ShowItem;
            m_examineSession.OnExamineClosed -= Hide;
        }

        private void ShowItem(ItemDefinition item)
        {
            m_nameLabel.text = item.DisplayName;
            m_descriptionLabel.text = item.DescriptionText;
            m_hintLabel.text = BuildHintText(item);
            m_overlay.AddToClassList(k_visibleOverlayClassName);
        }

        private void Hide()
        {
            m_overlay.RemoveFromClassList(k_visibleOverlayClassName);
        }

        private string BuildHintText(ItemDefinition item)
        {
            switch (item.UseAction)
            {
                case ItemUseAction.Read:
                    return string.Format(k_actionHintFormat, k_readVerb);
                case ItemUseAction.Equip:
                    return string.Format(k_actionHintFormat, m_equipmentSlot.IsEquipped(item) ? k_unequipVerb : k_equipVerb);
                default:
                    return k_closeOnlyHintText;
            }
        }
    }
}
