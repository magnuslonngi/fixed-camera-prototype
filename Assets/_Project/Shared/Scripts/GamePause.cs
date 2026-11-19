using UnityEngine;

namespace FixedCameraPrototype.Shared
{
    // Sole owner of Time.timeScale. Ref-counted so nested pausers never resume the game early.
    public class GamePause : MonoBehaviour
    {
        private const float k_pausedTimeScale = 0.0f;

        private int m_activePauseCount;
        private float m_timeScaleBeforePause = 1.0f;

        public bool IsPaused => m_activePauseCount > 0;

        private void OnDisable()
        {
            if (IsPaused)
            {
                Time.timeScale = m_timeScaleBeforePause;
                m_activePauseCount = 0;
            }
        }

        public void AddPause()
        {
            m_activePauseCount++;
            if (m_activePauseCount == 1)
            {
                m_timeScaleBeforePause = Time.timeScale;
                Time.timeScale = k_pausedTimeScale;
            }
        }

        public void RemovePause()
        {
            if (m_activePauseCount == 0)
            {
                return;
            }

            m_activePauseCount--;
            if (m_activePauseCount == 0)
            {
                Time.timeScale = m_timeScaleBeforePause;
            }
        }
    }
}
