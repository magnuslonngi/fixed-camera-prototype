using System;
using UnityEngine;

namespace FixedCameraPrototype.Shared
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private float m_maxHealth = 100.0f;

        private float m_currentHealth;

        public float CurrentHealth => m_currentHealth;
        public float MaxHealth => m_maxHealth;
        public bool IsDead => m_currentHealth <= 0.0f;

        public event Action<float, float> OnHealthChanged;
        public event Action OnDied;

        private void Awake()
        {
            m_currentHealth = m_maxHealth;
        }

        public void ApplyDamage(float damageAmount)
        {
            if (IsDead || damageAmount <= 0.0f)
            {
                return;
            }

            m_currentHealth = Mathf.Max(0.0f, m_currentHealth - damageAmount);
            OnHealthChanged?.Invoke(m_currentHealth, m_maxHealth);

            if (IsDead)
            {
                OnDied?.Invoke();
            }
        }
    }
}
