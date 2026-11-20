using System;
using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Enemy
{
    // Authority over the enemy's behavior state. Drives perception, locomotion and attack; never presentation.
    public class EnemyBrain : MonoBehaviour
    {
        [SerializeField] private Health m_health;
        [SerializeField] private EnemyPerception m_perception;
        [SerializeField] private EnemyLocomotion m_locomotion;
        [SerializeField] private EnemyAttack m_attack;
        [Tooltip("How long an attack commits the enemy; matches the attack animation length.")]
        [SerializeField] private float m_attackDurationSeconds = 1.4f;
        [Tooltip("How long a hit staggers the enemy; matches the hurt animation length.")]
        [SerializeField] private float m_staggerSeconds = 0.6f;

        private IEnemyIdleBehavior m_idleBehavior;
        private EnemyState m_currentState;
        private float m_remainingStateSeconds;

        public EnemyState CurrentState => m_currentState;

        public event Action<EnemyState> OnStateChanged;

        private void Awake()
        {
            m_idleBehavior = GetComponent<IEnemyIdleBehavior>();
        }

        private void OnEnable()
        {
            m_health.OnHealthChanged += HandleHealthChanged;
            m_health.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            m_health.OnHealthChanged -= HandleHealthChanged;
            m_health.OnDied -= HandleDied;
        }

        private void Start()
        {
            EnterState(EnemyState.Idle);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            switch (m_currentState)
            {
                case EnemyState.Idle:
                    UpdateIdle(deltaTime);
                    break;
                case EnemyState.Chase:
                    UpdateChase(deltaTime);
                    break;
                case EnemyState.Attack:
                case EnemyState.Hurt:
                    UpdateTimedState(deltaTime);
                    break;
            }
        }

        private void UpdateIdle(float deltaTime)
        {
            if (m_perception.HasTarget)
            {
                EnterState(EnemyState.Chase);
                return;
            }

            m_idleBehavior?.TickIdle(deltaTime);
        }

        private void UpdateChase(float deltaTime)
        {
            if (!m_perception.HasTarget)
            {
                EnterState(EnemyState.Idle);
                return;
            }

            IPursuitTarget target = m_perception.CurrentTarget;
            if (!m_attack.IsInRange(target))
            {
                m_locomotion.MoveTo(m_perception.HasLineOfSightToTarget ? target.Position : m_perception.LastKnownTargetPosition);
                return;
            }

            m_locomotion.Stop();
            m_locomotion.FaceTowards(target.Position, deltaTime);
            if (m_attack.TryStartAttack(target))
            {
                EnterState(EnemyState.Attack);
            }
        }

        private void UpdateTimedState(float deltaTime)
        {
            m_remainingStateSeconds -= deltaTime;
            if (m_remainingStateSeconds <= 0.0f)
            {
                EnterState(m_perception.HasTarget ? EnemyState.Chase : EnemyState.Idle);
            }
        }

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            if (m_health.IsDead)
            {
                return;
            }

            m_attack.CancelAttack();
            m_locomotion.Stop();
            m_perception.AlertToNearestTarget();
            EnterState(EnemyState.Hurt);
        }

        private void HandleDied()
        {
            m_attack.CancelAttack();
            m_locomotion.Stop();
            EnterState(EnemyState.Dead);
        }

        private void EnterState(EnemyState newState)
        {
            m_currentState = newState;
            switch (newState)
            {
                case EnemyState.Idle:
                    m_idleBehavior?.BeginIdle();
                    break;
                case EnemyState.Attack:
                    m_remainingStateSeconds = m_attackDurationSeconds;
                    break;
                case EnemyState.Hurt:
                    m_remainingStateSeconds = m_staggerSeconds;
                    break;
            }

            OnStateChanged?.Invoke(newState);
        }
    }
}
