using UnityEngine;

namespace FixedCameraPrototype.Combat
{
    public class CombatAnimationPresenter : MonoBehaviour
    {
        private static readonly int s_shootHash = Animator.StringToHash("Shoot");
        private static readonly int s_reloadHash = Animator.StringToHash("Reload");
        private static readonly int s_reloadCancelledHash = Animator.StringToHash("ReloadCancelled");

        [SerializeField] private Animator m_animator;
        [SerializeField] private PlayerWeapon m_weapon;
        [SerializeField] private PlayerReload m_reload;

        private void OnEnable()
        {
            m_weapon.OnShotFired += HandleShotFired;
            m_reload.OnReloadStarted += HandleReloadStarted;
            m_reload.OnReloadCancelled += HandleReloadCancelled;
        }

        private void OnDisable()
        {
            m_weapon.OnShotFired -= HandleShotFired;
            m_reload.OnReloadStarted -= HandleReloadStarted;
            m_reload.OnReloadCancelled -= HandleReloadCancelled;
        }

        private void HandleShotFired(ShotResult shot)
        {
            m_animator.SetTrigger(s_shootHash);
        }

        private void HandleReloadStarted()
        {
            m_animator.ResetTrigger(s_reloadCancelledHash);
            m_animator.SetTrigger(s_reloadHash);
        }

        private void HandleReloadCancelled()
        {
            m_animator.ResetTrigger(s_reloadHash);
            m_animator.SetTrigger(s_reloadCancelledHash);
        }
    }
}
