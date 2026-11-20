using System;
using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Combat
{
    // Owns the timed reload. Holds the movement lock for its duration; rounds are drawn only on
    // completion, so a cancelled reload costs no ammo.
    public class PlayerReload : MonoBehaviour
    {
        [SerializeField] private CombatInputReader m_inputReader;
        [SerializeField] private PlayerWeapon m_weapon;
        [SerializeField] private WeaponMagazine m_magazine;
        [SerializeField] private EquipmentSlot m_equipmentSlot;
        [SerializeField] private MovementLock m_movementLock;
        [SerializeField] private float m_reloadSeconds = 1.5f;

        private float m_reloadEndTime;
        private bool m_isReloading;

        public bool IsReloading => m_isReloading;

        public event Action OnReloadStarted;
        public event Action OnReloadFinished;
        public event Action OnReloadCancelled;

        private void OnEnable()
        {
            m_inputReader.OnReloadPressed += HandleReloadPressed;
            m_equipmentSlot.OnEquippedItemChanged += HandleEquippedItemChanged;
        }

        private void OnDisable()
        {
            m_inputReader.OnReloadPressed -= HandleReloadPressed;
            m_equipmentSlot.OnEquippedItemChanged -= HandleEquippedItemChanged;
            CancelReload();
        }

        private void Update()
        {
            if (m_isReloading && Time.time >= m_reloadEndTime)
            {
                FinishReload();
            }
        }

        private void HandleReloadPressed()
        {
            if (m_isReloading || !m_weapon.IsEquipped || !m_magazine.CanReload)
            {
                return;
            }

            m_isReloading = true;
            m_reloadEndTime = Time.time + m_reloadSeconds;
            m_movementLock.AddLock();
            OnReloadStarted?.Invoke();
        }

        private void HandleEquippedItemChanged(ItemDefinition equippedItem)
        {
            if (!m_weapon.IsEquipped)
            {
                CancelReload();
            }
        }

        private void FinishReload()
        {
            EndReload();
            m_magazine.LoadFromReserve();
            OnReloadFinished?.Invoke();
        }

        private void CancelReload()
        {
            if (!m_isReloading)
            {
                return;
            }

            EndReload();
            OnReloadCancelled?.Invoke();
        }

        private void EndReload()
        {
            m_isReloading = false;
            m_movementLock.RemoveLock();
        }
    }
}
