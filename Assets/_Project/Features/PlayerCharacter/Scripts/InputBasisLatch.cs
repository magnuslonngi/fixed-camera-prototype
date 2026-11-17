using UnityEngine;

namespace FixedCameraPrototype.PlayerCharacter
{
    // Pure logic (no MonoBehaviour) so latch rules are unit-testable.
    // Holds the pre-cut camera basis while the stick keeps pushing the same way, so a camera cut
    // does not reverse the character. Released when the stick is let go or turns far enough.
    public class InputBasisLatch
    {
        private readonly float m_deadzone;
        private readonly float m_releaseAngleDegrees;

        private Vector3 m_latchedForward;
        private Vector3 m_latchedRight;
        private Vector2 m_latchedInputDirection;

        public InputBasisLatch(float deadzone, float releaseAngleDegrees)
        {
            m_deadzone = deadzone;
            m_releaseAngleDegrees = releaseAngleDegrees;
        }

        public bool IsLatched { get; private set; }
        public Vector3 LatchedForward => m_latchedForward;
        public Vector3 LatchedRight => m_latchedRight;

        public void TryLatch(Vector3 forward, Vector3 right, Vector2 input)
        {
            if (input.magnitude < m_deadzone)
            {
                return;
            }

            m_latchedForward = forward;
            m_latchedRight = right;
            m_latchedInputDirection = input.normalized;
            IsLatched = true;
        }

        public void UpdateRelease(Vector2 input)
        {
            if (!IsLatched)
            {
                return;
            }

            if (input.magnitude < m_deadzone || HasTurnedPastReleaseAngle(input))
            {
                IsLatched = false;
            }
        }

        private bool HasTurnedPastReleaseAngle(Vector2 input)
        {
            return Vector2.Angle(m_latchedInputDirection, input) > m_releaseAngleDegrees;
        }
    }
}
