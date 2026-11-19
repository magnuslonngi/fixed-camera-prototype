using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Combat
{
    public class WeaponVisibilityPresenter : MonoBehaviour
    {
        [SerializeField] private PlayerWeapon m_weapon;
        [SerializeField] private EquipmentSlot m_equipmentSlot;
        [SerializeField] private GameObject m_weaponVisual;

        private void OnEnable()
        {
            m_equipmentSlot.OnEquippedItemChanged += HandleEquippedItemChanged;
            UpdateVisibility();
        }

        private void OnDisable()
        {
            m_equipmentSlot.OnEquippedItemChanged -= HandleEquippedItemChanged;
        }

        private void HandleEquippedItemChanged(ItemDefinition equippedItem)
        {
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            m_weaponVisual.SetActive(m_weapon.IsEquipped);
        }
    }
}
