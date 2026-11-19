using UnityEngine;

namespace FixedCameraPrototype.Shared
{
    // Lets other features stop character movement without referencing the movement feature.
    public class MovementLock : MonoBehaviour
    {
        private int m_activeLockCount;

        public bool IsLocked => m_activeLockCount > 0;

        public void AddLock()
        {
            m_activeLockCount++;
        }

        public void RemoveLock()
        {
            m_activeLockCount = Mathf.Max(0, m_activeLockCount - 1);
        }
    }
}
