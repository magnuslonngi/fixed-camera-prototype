using System;
using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Interaction
{
    // Hands the item to the instigator's IItemReceiver. The object only leaves the world once the
    // item is actually stored, so an instigator without an inventory cannot make it vanish. The
    // instigator's IItemExaminePrompt, if any, then shows what was picked up.
    [RequireComponent(typeof(Interactable))]
    public class PickUpInteraction : MonoBehaviour
    {
        [SerializeField] private ItemDefinition m_item;
        [SerializeField] private int m_quantity = 1;
        [Tooltip("Optional. Raised after the item is stored, for level scripting that reacts to this pickup.")]
        [SerializeField] private GameplayEventChannel m_pickedUpEventChannel;

        private Interactable m_interactable;

        public event Action<GameObject> OnPickedUp;

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

        private void HandleInteracted(GameObject instigator)
        {
            if (!CapabilityRoot.TryFindCapability(instigator, out IItemReceiver itemReceiver) || !itemReceiver.TryAddItem(m_item, m_quantity))
            {
                return;
            }

            OnPickedUp?.Invoke(instigator);
            if (m_pickedUpEventChannel != null)
            {
                m_pickedUpEventChannel.Raise();
            }

            gameObject.SetActive(false);

            if (CapabilityRoot.TryFindCapability(instigator, out IItemExaminePrompt itemExaminePrompt))
            {
                itemExaminePrompt.ExamineItem(m_item);
            }
        }
    }
}
