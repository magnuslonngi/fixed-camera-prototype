using System;
using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Combat
{
    public class PlayerWeapon : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CombatInputReader m_inputReader;
        [SerializeField] private PlayerAim m_playerAim;
        [SerializeField] private AimAssist m_aimAssist;
        [SerializeField] private Transform m_muzzle;

        [Header("Weapon")]
        [SerializeField] private float m_damage = 10.0f;
        [SerializeField] private float m_rangeMeters = 20.0f;
        [SerializeField] private float m_secondsBetweenShots = 0.4f;
        [SerializeField] private LayerMask m_hitLayers;

        private float m_nextShotTime;

        public Vector3 MuzzlePosition => m_muzzle.position;
        public float RangeMeters => m_rangeMeters;

        public event Action<ShotResult> OnShotFired;

        private void OnEnable()
        {
            m_inputReader.OnFirePressed += HandleFirePressed;
        }

        private void OnDisable()
        {
            m_inputReader.OnFirePressed -= HandleFirePressed;
        }

        private void HandleFirePressed()
        {
            if (!m_playerAim.IsAiming || Time.time < m_nextShotTime)
            {
                return;
            }

            m_nextShotTime = Time.time + m_secondsBetweenShots;
            FireShot();
        }

        private void FireShot()
        {
            Vector3 origin = m_muzzle.position;
            Vector3 rawDirection = m_playerAim.AimDirection;
            Vector3 finalDirection = m_aimAssist.ResolveShotDirection(origin, rawDirection, m_playerAim.CurrentTier.ToHitZone());

            bool didHit = Physics.Raycast(
                origin,
                finalDirection,
                out RaycastHit hit,
                m_rangeMeters,
                m_hitLayers,
                QueryTriggerInteraction.Ignore);

            if (didHit && hit.collider.TryGetComponent(out IDamageable damageable))
            {
                damageable.ApplyDamage(m_damage);
            }

            Vector3 endPoint = didHit ? hit.point : origin + (finalDirection * m_rangeMeters);
            OnShotFired?.Invoke(new ShotResult(origin, rawDirection, finalDirection, endPoint, didHit));
        }
    }
}
