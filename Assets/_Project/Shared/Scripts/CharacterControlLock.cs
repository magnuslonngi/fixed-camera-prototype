using System;
using UnityEngine;

namespace FixedCameraPrototype.Shared
{
    // Lets a feature take all gameplay input away from the character (e.g. a modal UI) without
    // referencing the features that read that input. Separate from MovementLock, which only stops movement.
    public class CharacterControlLock : MonoBehaviour
    {
        private int m_activeLockCount;

        public bool IsLocked => m_activeLockCount > 0;

        public event Action<bool> OnLockStateChanged;

        public void AddLock()
        {
            m_activeLockCount++;
            if (m_activeLockCount == 1)
            {
                OnLockStateChanged?.Invoke(true);
            }
        }

        public void RemoveLock()
        {
            if (m_activeLockCount == 0)
            {
                return;
            }

            m_activeLockCount--;
            if (m_activeLockCount == 0)
            {
                OnLockStateChanged?.Invoke(false);
            }
        }
    }
}
