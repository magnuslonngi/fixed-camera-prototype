using System;
using UnityEngine;

namespace FixedCameraPrototype.PlayerCharacter
{
    public class PlayerStamina : MonoBehaviour
    {
        [SerializeField] private float m_maxStamina = 100.0f;
        [SerializeField] private float m_sprintDrainPerSecond = 20.0f;
        [SerializeField] private float m_regenerationPerSecond = 8.0f;
        [Tooltip("Seconds after the last sprint before stamina starts regenerating.")]
        [SerializeField] private float m_regenerationDelaySeconds = 1.5f;
        [Tooltip("Once exhausted, sprinting stays blocked until stamina refills to this fraction of max.")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float m_exhaustionRecoveryRatio = 0.4f;

        private float m_currentStamina;
        // Hysteresis flag, not derivable from the current value alone: the same stamina level
        // allows sprinting on the way down but blocks it while recovering from empty.
        private bool m_isExhausted;
        private float m_lastDrainTime = float.NegativeInfinity;

        public float CurrentStamina => m_currentStamina;
        public float MaxStamina => m_maxStamina;
        public bool IsExhausted => m_isExhausted;
        public bool CanSprint => !m_isExhausted && m_currentStamina > 0.0f;

        public event Action<float, float> OnStaminaChanged;
        public event Action<bool> OnExhaustionChanged;

        private void Awake()
        {
            m_currentStamina = m_maxStamina;
        }

        private void Update()
        {
            if (IsRegenerating())
            {
                SetStamina(m_currentStamina + (m_regenerationPerSecond * Time.deltaTime));
            }
        }

        public void DrainForSprint(float deltaTime)
        {
            if (!CanSprint)
            {
                return;
            }

            m_lastDrainTime = Time.time;
            SetStamina(m_currentStamina - (m_sprintDrainPerSecond * deltaTime));
        }

        private bool IsRegenerating()
        {
            return m_currentStamina < m_maxStamina && Time.time - m_lastDrainTime >= m_regenerationDelaySeconds;
        }

        private void SetStamina(float newStamina)
        {
            float clampedStamina = Mathf.Clamp(newStamina, 0.0f, m_maxStamina);
            if (Mathf.Approximately(clampedStamina, m_currentStamina))
            {
                return;
            }

            m_currentStamina = clampedStamina;
            OnStaminaChanged?.Invoke(m_currentStamina, m_maxStamina);
            UpdateExhaustion();
        }

        private void UpdateExhaustion()
        {
            bool shouldBeExhausted = m_isExhausted
                ? m_currentStamina < m_maxStamina * m_exhaustionRecoveryRatio
                : m_currentStamina <= 0.0f;
            if (shouldBeExhausted == m_isExhausted)
            {
                return;
            }

            m_isExhausted = shouldBeExhausted;
            OnExhaustionChanged?.Invoke(m_isExhausted);
        }
    }
}
