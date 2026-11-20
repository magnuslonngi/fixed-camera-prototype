using System.Collections;
using FixedCameraPrototype.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FixedCameraPrototype.LevelFlow
{
    // Restarts the level after the player dies, once the death has had time to read on screen.
    public class ReloadSceneOnPlayerDeath : MonoBehaviour
    {
        [SerializeField] private Health m_playerHealth;
        [SerializeField] private float m_reloadDelaySeconds = 3.0f;

        private void OnEnable()
        {
            m_playerHealth.OnDied += HandlePlayerDied;
        }

        private void OnDisable()
        {
            m_playerHealth.OnDied -= HandlePlayerDied;
        }

        private void HandlePlayerDied()
        {
            StartCoroutine(ReloadAfterDelay());
        }

        // Realtime so a modal pause opened at the moment of death cannot stall the restart.
        private IEnumerator ReloadAfterDelay()
        {
            yield return new WaitForSecondsRealtime(m_reloadDelaySeconds);
            Scene activeScene = SceneManager.GetActiveScene();
#if UNITY_EDITOR
            // Sandbox scenes are not in Build Settings, so they can only be reloaded by path in the Editor.
            if (activeScene.buildIndex < 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(activeScene.path, new LoadSceneParameters(LoadSceneMode.Single));
                yield break;
            }
#endif
            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}
