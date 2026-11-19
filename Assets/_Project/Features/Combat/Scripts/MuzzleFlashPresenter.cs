using UnityEngine;

namespace FixedCameraPrototype.Combat
{
    public class MuzzleFlashPresenter : MonoBehaviour
    {
        [SerializeField] private PlayerWeapon m_weapon;
        [SerializeField] private ParticleSystem m_muzzleFlash;

        private void OnEnable()
        {
            m_weapon.OnShotFired += HandleShotFired;
        }

        private void OnDisable()
        {
            m_weapon.OnShotFired -= HandleShotFired;
        }

        private void HandleShotFired(ShotResult shot)
        {
            m_muzzleFlash.Play(true);
        }
    }
}
