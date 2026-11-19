using UnityEngine;

namespace FixedCameraPrototype.Combat
{
    public class WeaponAudioPresenter : MonoBehaviour
    {
        [SerializeField] private PlayerWeapon m_weapon;
        [SerializeField] private PlayerReload m_reload;
        [SerializeField] private AudioSource m_shotAudioSource;
        [SerializeField] private AudioSource m_reloadAudioSource;
        [SerializeField] private AudioClip m_reloadAudioClip;

        private void OnEnable()
        {
            m_weapon.OnShotFired += HandleShotFired;
            m_reload.OnReloadStarted += HandleReloadStarted;
        }

        private void OnDisable()
        {
            m_weapon.OnShotFired -= HandleShotFired;
            m_reload.OnReloadStarted -= HandleReloadStarted;
        }

        // Shot source plays an Audio Random Container resource, which picks a clip on each Play.
        private void HandleShotFired(ShotResult shot)
        {
            m_shotAudioSource.Play();
        }

        private void HandleReloadStarted()
        {
            m_reloadAudioSource.PlayOneShot(m_reloadAudioClip);
        }
    }
}
