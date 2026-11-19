using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Combat
{
    public class HitBloodPresenter : MonoBehaviour
    {
        [SerializeField] private PlayerWeapon m_weapon;
        [SerializeField] private ParticleSystem m_bloodSplatterPrefab;

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
            if (!shot.DidHit || !shot.HitCollider.TryGetComponent(out IDamageable _))
            {
                return;
            }

            ParticleSystem splatter = Instantiate(m_bloodSplatterPrefab, shot.EndPoint, Quaternion.LookRotation(shot.HitNormal));
            splatter.Play(true);
        }
    }
}
