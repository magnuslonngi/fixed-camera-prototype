using UnityEngine;

namespace FixedCameraPrototype.Enemy
{
    public class EnemyAnimationPresenter : MonoBehaviour
    {
        private static readonly int s_speedHash = Animator.StringToHash("Speed");
        private static readonly int s_attackHash = Animator.StringToHash("Attack");
        private static readonly int s_hurtHash = Animator.StringToHash("Hurt");
        private static readonly int s_isDeadHash = Animator.StringToHash("IsDead");

        [SerializeField] private Animator m_animator;
        [SerializeField] private EnemyBrain m_brain;
        [SerializeField] private EnemyLocomotion m_locomotion;
        [SerializeField] private float m_speedDampSeconds = 0.1f;

        private void OnEnable()
        {
            m_brain.OnStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            m_brain.OnStateChanged -= HandleStateChanged;
        }

        private void Update()
        {
            m_animator.SetFloat(s_speedHash, m_locomotion.NormalizedSpeed, m_speedDampSeconds, Time.deltaTime);
        }

        private void HandleStateChanged(EnemyState newState)
        {
            switch (newState)
            {
                case EnemyState.Attack:
                    m_animator.SetTrigger(s_attackHash);
                    break;
                case EnemyState.Hurt:
                    m_animator.ResetTrigger(s_attackHash);
                    m_animator.SetTrigger(s_hurtHash);
                    break;
                case EnemyState.Dead:
                    m_animator.ResetTrigger(s_attackHash);
                    m_animator.ResetTrigger(s_hurtHash);
                    m_animator.SetBool(s_isDeadHash, true);
                    break;
            }
        }
    }
}
