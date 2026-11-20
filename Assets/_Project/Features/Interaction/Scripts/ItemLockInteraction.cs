using System;
using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Interaction
{
    // Interacting opens the instigator's item selection; the chosen item comes back through TryUseItem.
    [RequireComponent(typeof(Interactable))]
    public class ItemLockInteraction : MonoBehaviour, IItemUseTarget
    {
        [SerializeField] private ItemDefinition m_requiredItem;
        [Tooltip("Model shown in the inventory while choosing an item to use here.")]
        [SerializeField] private GameObject m_previewPrefab;
        [Tooltip("Brief description shown under the preview while choosing an item to use here.")]
        [SerializeField, TextArea] private string m_descriptionText;

        private Interactable m_interactable;
        private bool m_isUnlocked;

        public bool IsUnlocked => m_isUnlocked;
        public GameObject PreviewPrefab => m_previewPrefab;
        public string DescriptionText => m_descriptionText;

        public event Action OnUnlocked;
        public event Action<ItemDefinition> OnUnlockRejected;

        private void Awake()
        {
            m_interactable = GetComponent<Interactable>();
        }

        private void OnEnable()
        {
            m_interactable.OnInteracted += HandleInteracted;
        }

        private void OnDisable()
        {
            m_interactable.OnInteracted -= HandleInteracted;
        }

        public bool TryUseItem(ItemDefinition item)
        {
            if (m_isUnlocked || item != m_requiredItem)
            {
                OnUnlockRejected?.Invoke(item);
                return false;
            }

            m_isUnlocked = true;
            m_interactable.enabled = false;
            OnUnlocked?.Invoke();
            return true;
        }

        private void HandleInteracted(GameObject instigator)
        {
            if (CapabilityRoot.TryFindCapability(instigator, out IItemSelectionPrompt itemSelectionPrompt))
            {
                itemSelectionPrompt.BeginItemSelection(this);
            }
        }
    }
}
