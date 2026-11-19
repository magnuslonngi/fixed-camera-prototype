using System;
using System.Collections.Generic;
using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Inventory
{
    public class PlayerInventory : MonoBehaviour, IItemReceiver
    {
        private readonly List<ItemDefinition> m_items = new List<ItemDefinition>();

        public IReadOnlyList<ItemDefinition> Items => m_items;

        public event Action OnItemsChanged;

        public bool TryAddItem(ItemDefinition item)
        {
            if (item == null)
            {
                return false;
            }

            m_items.Add(item);
            OnItemsChanged?.Invoke();
            return true;
        }

        public bool TryRemoveItem(ItemDefinition item)
        {
            if (!m_items.Remove(item))
            {
                return false;
            }

            OnItemsChanged?.Invoke();
            return true;
        }
    }
}
