using System;
using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Inventory
{
    // Runs an item's own action when it is used from the inventory without a world target.
    public class ItemActionPerformer : MonoBehaviour
    {
        [SerializeField] private EquipmentSlot m_equipmentSlot;

        public event Action<ItemDefinition> OnDescriptionRequested;

        public void Perform(ItemDefinition item)
        {
            if (item.UseAction == ItemUseAction.Equip)
            {
                ToggleEquipped(item);
                return;
            }

            OnDescriptionRequested?.Invoke(item);
        }

        private void ToggleEquipped(ItemDefinition item)
        {
            if (m_equipmentSlot.IsEquipped(item))
            {
                m_equipmentSlot.Unequip();
            }
            else
            {
                m_equipmentSlot.Equip(item);
            }
        }
    }
}
