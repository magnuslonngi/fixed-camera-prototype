using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FixedCameraPrototype.Shared
{
    // Inspector-assigned scene, so code never names scenes by string or build index. The scene asset is
    // Editor-only; its path is copied into a serialized field so builds can still load it.
    [Serializable]
    public class SceneReference : ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        [SerializeField] private UnityEditor.SceneAsset m_sceneAsset;
#endif
        [SerializeField, HideInInspector] private string m_scenePath;

        public bool IsAssigned => !string.IsNullOrEmpty(m_scenePath);

        public void Load()
        {
            if (!IsAssigned)
            {
                Debug.LogError("SceneReference has no scene assigned.");
                return;
            }

#if UNITY_EDITOR
            // Sandbox scenes are not in Build Settings, so the Editor loads them by path directly.
            if (SceneUtility.GetBuildIndexByScenePath(m_scenePath) < 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(m_scenePath, new LoadSceneParameters(LoadSceneMode.Single));
                return;
            }
#endif
            SceneManager.LoadScene(m_scenePath);
        }

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            m_scenePath = m_sceneAsset != null ? UnityEditor.AssetDatabase.GetAssetPath(m_sceneAsset) : string.Empty;
#endif
        }

        public void OnAfterDeserialize()
        {
        }
    }
}
