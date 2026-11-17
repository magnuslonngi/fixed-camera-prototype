using NUnit.Framework;
using UnityEngine;

namespace FixedCameraPrototype.PlayerCharacter.Tests
{
    public class InputBasisLatchTests
    {
        private const float k_deadzone = 0.2f;
        private const float k_releaseAngleDegrees = 35.0f;

        private InputBasisLatch m_latch;

        [SetUp]
        public void SetUp()
        {
            m_latch = new InputBasisLatch(k_deadzone, k_releaseAngleDegrees);
        }

        [Test]
        public void TryLatch_WithStickHeld_Latches()
        {
            m_latch.TryLatch(Vector3.forward, Vector3.right, Vector2.up);

            Assert.IsTrue(m_latch.IsLatched);
            Assert.AreEqual(Vector3.forward, m_latch.LatchedForward);
        }

        [Test]
        public void TryLatch_WithStickInDeadzone_DoesNotLatch()
        {
            m_latch.TryLatch(Vector3.forward, Vector3.right, new Vector2(0.05f, 0.05f));

            Assert.IsFalse(m_latch.IsLatched);
        }

        [Test]
        public void UpdateRelease_WithSmallStickChange_StaysLatched()
        {
            m_latch.TryLatch(Vector3.forward, Vector3.right, Vector2.up);

            m_latch.UpdateRelease(new Vector2(0.2f, 1.0f));

            Assert.IsTrue(m_latch.IsLatched);
        }

        [Test]
        public void UpdateRelease_WithLargeStickChange_Releases()
        {
            m_latch.TryLatch(Vector3.forward, Vector3.right, Vector2.up);

            m_latch.UpdateRelease(Vector2.right);

            Assert.IsFalse(m_latch.IsLatched);
        }

        [Test]
        public void UpdateRelease_WithStickReleased_Releases()
        {
            m_latch.TryLatch(Vector3.forward, Vector3.right, Vector2.up);

            m_latch.UpdateRelease(Vector2.zero);

            Assert.IsFalse(m_latch.IsLatched);
        }
    }
}
