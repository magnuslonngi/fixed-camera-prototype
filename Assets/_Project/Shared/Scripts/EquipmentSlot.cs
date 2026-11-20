using System;
using UnityEngine;

namespace FixedCameraPrototype.Shared
{
    // Sole authority for the item the character holds. Shared so Inventory can equip and Combat can
    // check its weapon without either feature referencing the other.
    public class EquipmentSlot : MonoBehaviour
    {
        private ItemDefinition m_equippedItem;

        public ItemDefinition EquippedItem => m_equippedItem;

        public event Action<ItemDefinition> OnEquippedItemChanged;

        public bool IsEquipped(ItemDefinition item)
        {
            return item != null && m_equippedItem == item;
        }

        public void Equip(ItemDefinition item)
        {
            SetEquippedItem(item);
        }

        public void Unequip()
        {
            SetEquippedItem(null);
        }

        private void SetEquippedItem(ItemDefinition item)
        {
            if (m_equippedItem == item)
            {
                return;
            }

            m_equippedItem = item;
            OnEquippedItemChanged?.Invoke(m_equippedItem);
        }
    }
}
