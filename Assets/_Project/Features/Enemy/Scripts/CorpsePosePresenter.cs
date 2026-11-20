using UnityEngine;

namespace FixedCameraPrototype.Enemy
{
    // Shows the body already lying dead from the first frame, skipping the fall that the death state plays.
    public class CorpsePosePresenter : MonoBehaviour
    {
        private const int k_baseLayerIndex = 0;
        private const float k_finalNormalizedTime = 1.0f;
        private static readonly int s_isDeadHash = Animator.StringToHash("IsDead");
        private static readonly int s_deathStateHash = Animator.StringToHash("Death");

        [SerializeField] private Animator m_animator;

        private void Start()
        {
            m_animator.SetBool(s_isDeadHash, true);
            m_animator.Play(s_deathStateHash, k_baseLayerIndex, k_finalNormalizedTime);
        }
    }
}
