using System;
using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Combat
{
    // Sole authority for the rounds loaded in the weapon. Reserve rounds stay in the character's
    // IItemStock and are only drawn when a reload completes.
    public class WeaponMagazine : MonoBehaviour, IItemStatusSource
    {
        private const string k_loadedRoundsStatusFormat = "{0}/{1}";

        [SerializeField] private ItemDefinition m_weaponItem;
        [SerializeField] private ItemDefinition m_ammoItem;
        [SerializeField] private int m_capacity = 10;

        private int m_loadedRounds;

        // Optional: a character without an inventory simply has no reserve to reload from.
        private IItemStock m_reserveStock;

        public int LoadedRounds => m_loadedRounds;
        public int Capacity => m_capacity;
        public int ReserveRounds => m_reserveStock != null ? m_reserveStock.GetQuantity(m_ammoItem) : 0;
        public bool IsEmpty => m_loadedRounds <= 0;
        public bool CanReload => m_loadedRounds < m_capacity && ReserveRounds > 0;

        public event Action OnStatusChanged;

        private void Awake()
        {
            m_loadedRounds = m_capacity;
            CapabilityRoot.TryFindCapability(gameObject, out m_reserveStock);
        }

        public bool TryConsumeRound()
        {
            if (IsEmpty)
            {
                return false;
            }

            SetLoadedRounds(m_loadedRounds - 1);
            return true;
        }

        public void LoadFromReserve()
        {
            if (!CanReload)
            {
                return;
            }

            int drawnRounds = m_reserveStock.RemoveQuantity(m_ammoItem, m_capacity - m_loadedRounds);
            SetLoadedRounds(m_loadedRounds + drawnRounds);
        }

        public bool TryGetStatusText(ItemDefinition item, out string statusText)
        {
            bool isWeaponItem = item != null && item == m_weaponItem;
            statusText = isWeaponItem ? string.Format(k_loadedRoundsStatusFormat, m_loadedRounds, m_capacity) : null;
            return isWeaponItem;
        }

        private void SetLoadedRounds(int loadedRounds)
        {
            int clampedRounds = Mathf.Clamp(loadedRounds, 0, m_capacity);
            if (clampedRounds == m_loadedRounds)
            {
                return;
            }

            m_loadedRounds = clampedRounds;
            OnStatusChanged?.Invoke();
        }
    }
}
