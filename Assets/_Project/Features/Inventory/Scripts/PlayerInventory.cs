using System;
using System.Collections.Generic;
using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Inventory
{
    // Sole authority for carried items and their quantities. Stackable items share one entry;
    // every other pickup gets its own entry.
    public class PlayerInventory : MonoBehaviour, IItemReceiver, IItemStock
    {
        private readonly List<InventoryEntry> m_entries = new List<InventoryEntry>();

        public int ItemCount => m_entries.Count;

        public event Action OnItemsChanged;

        public ItemDefinition GetItemAt(int index)
        {
            return m_entries[index].Item;
        }

        public int GetQuantity(ItemDefinition item)
        {
            int totalQuantity = 0;
            foreach (InventoryEntry entry in m_entries)
            {
                if (entry.Item == item)
                {
                    totalQuantity += entry.Quantity;
                }
            }

            return totalQuantity;
        }

        public bool TryAddItem(ItemDefinition item, int quantity)
        {
            if (item == null || quantity <= 0)
            {
                return false;
            }

            InventoryEntry stackEntry = item.IsStackable ? FindEntry(item) : null;
            if (stackEntry != null)
            {
                stackEntry.Quantity += quantity;
            }
            else
            {
                m_entries.Add(new InventoryEntry(item, quantity));
            }

            OnItemsChanged?.Invoke();
            return true;
        }

        public bool TryRemoveItem(ItemDefinition item)
        {
            InventoryEntry entry = FindEntry(item);
            if (entry == null)
            {
                return false;
            }

            m_entries.Remove(entry);
            OnItemsChanged?.Invoke();
            return true;
        }

        public int RemoveQuantity(ItemDefinition item, int requestedQuantity)
        {
            InventoryEntry entry = FindEntry(item);
            if (entry == null || requestedQuantity <= 0)
            {
                return 0;
            }

            int removedQuantity = Mathf.Min(entry.Quantity, requestedQuantity);
            entry.Quantity -= removedQuantity;
            if (entry.Quantity == 0)
            {
                m_entries.Remove(entry);
            }

            OnItemsChanged?.Invoke();
            return removedQuantity;
        }

        private InventoryEntry FindEntry(ItemDefinition item)
        {
            return m_entries.Find(entry => entry.Item == item);
        }

        private class InventoryEntry
        {
            public InventoryEntry(ItemDefinition item, int quantity)
            {
                Item = item;
                Quantity = quantity;
            }

            public ItemDefinition Item { get; }
            public int Quantity { get; set; }
        }
    }
}
