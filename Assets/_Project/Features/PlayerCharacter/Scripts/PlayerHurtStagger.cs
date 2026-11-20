using System.Collections;
using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.PlayerCharacter
{
    public class PlayerHurtStagger : MonoBehaviour
    {
        [SerializeField] private Health m_health;
        [SerializeField] private MovementLock m_movementLock;
        [Tooltip("How long a hit stops movement; matches the hurt animation length.")]
        [SerializeField] private float m_staggerSeconds = 0.8f;

        private Coroutine m_staggerRoutine;

        private void OnEnable()
        {
            m_health.OnHealthChanged += HandleHealthChanged;
        }

        private void OnDisable()
        {
            m_health.OnHealthChanged -= HandleHealthChanged;
            EndStagger();
        }

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            if (m_health.IsDead)
            {
                return;
            }

            EndStagger();
            m_staggerRoutine = StartCoroutine(StaggerForDuration());
        }

        private IEnumerator StaggerForDuration()
        {
            m_movementLock.AddLock();
            yield return new WaitForSeconds(m_staggerSeconds);
            m_movementLock.RemoveLock();
            m_staggerRoutine = null;
        }

        private void EndStagger()
        {
            if (m_staggerRoutine == null)
            {
                return;
            }

            StopCoroutine(m_staggerRoutine);
            m_movementLock.RemoveLock();
            m_staggerRoutine = null;
        }
    }
}
