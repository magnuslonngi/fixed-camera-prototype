using UnityEngine;

namespace FixedCameraPrototype.PlayerCharacter
{
    // Drives the locomotion blend tree. Root motion turns the blended clip into actual movement,
    // so the damping here doubles as the acceleration and deceleration of the character.
    public class PlayerLocomotionAnimator : MonoBehaviour
    {
        private const float k_walkBlendValue = 0.5f;
        private const float k_runBlendValue = 1.0f;

        private static readonly int s_speedParameterHash = Animator.StringToHash("Speed");

        [SerializeField] private PlayerMovement m_playerMovement;
        [SerializeField] private Animator m_animator;
        [SerializeField] private float m_speedDampSeconds = 0.1f;

        private void Update()
        {
            m_animator.SetFloat(s_speedParameterHash, CalculateTargetBlendValue(), m_speedDampSeconds, Time.deltaTime);
        }

        // Stick tilt scales walk (idle -> walk); sprint jumps straight to run.
        private float CalculateTargetBlendValue()
        {
            if (m_playerMovement.IsRunning)
            {
                return k_runBlendValue;
            }

            return m_playerMovement.DesiredDirection.magnitude * k_walkBlendValue;
        }
    }
}
