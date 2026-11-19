using System;
using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Enemy
{
    public class EnemyHitZone : MonoBehaviour, IDamageable
    {
        [SerializeField] private HitZoneType m_zoneType;
        [SerializeField] private float m_damageMultiplier = 1.0f;
        [SerializeField] private Health m_health;

        public HitZoneType ZoneType => m_zoneType;
        public Vector3 AimPoint => transform.position;

        public event Action OnHit;

        public void ApplyDamage(float damageAmount)
        {
            m_health.ApplyDamage(damageAmount * m_damageMultiplier);
            OnHit?.Invoke();
        }
    }
}
