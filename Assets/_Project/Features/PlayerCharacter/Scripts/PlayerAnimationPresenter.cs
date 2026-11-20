using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.PlayerCharacter
{
    public class PlayerAnimationPresenter : MonoBehaviour
    {
        private static readonly int s_moveSpeedHash = Animator.StringToHash("MoveSpeed");
        private static readonly int s_isAimingHash = Animator.StringToHash("IsAiming");
        private static readonly int s_hurtHash = Animator.StringToHash("Hurt");
        private static readonly int s_isDeadHash = Animator.StringToHash("IsDead");

        [Header("References")]
        [SerializeField] private Animator m_animator;
        [SerializeField] private PlayerMovement m_playerMovement;
        [SerializeField] private Health m_health;

        [Header("Blending (seconds)")]
        [SerializeField] private float m_moveSpeedDampSeconds = 0.1f;

        // Optional: the base Player prefab has no combat feature providing aim state.
        private IAimStateSource m_aimStateSource;

        private void Awake()
        {
            CapabilityRoot.TryFindCapability(gameObject, out m_aimStateSource);
        }

        private void OnEnable()
        {
            m_playerMovement.OnLocomotionChanged += HandleLocomotionChanged;
            m_health.OnHealthChanged += HandleHealthChanged;
            m_health.OnDied += HandleDied;

            if (m_aimStateSource != null)
            {
                m_aimStateSource.OnAimingChanged += HandleAimingChanged;
                HandleAimingChanged(m_aimStateSource.IsAiming);
            }
        }

        private void OnDisable()
        {
            m_playerMovement.OnLocomotionChanged -= HandleLocomotionChanged;
            m_health.OnHealthChanged -= HandleHealthChanged;
            m_health.OnDied -= HandleDied;

            if (m_aimStateSource != null)
            {
                m_aimStateSource.OnAimingChanged -= HandleAimingChanged;
            }
        }

        private void HandleLocomotionChanged(float normalizedSpeed, bool isRunning)
        {
            m_animator.SetFloat(s_moveSpeedHash, normalizedSpeed, m_moveSpeedDampSeconds, Time.deltaTime);
        }

        private void HandleAimingChanged(bool isAiming)
        {
            m_animator.SetBool(s_isAimingHash, isAiming);
        }

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            if (!m_health.IsDead)
            {
                m_animator.SetTrigger(s_hurtHash);
            }
        }

        private void HandleDied()
        {
            m_animator.ResetTrigger(s_hurtHash);
            m_animator.SetBool(s_isDeadHash, true);
        }
    }
}
