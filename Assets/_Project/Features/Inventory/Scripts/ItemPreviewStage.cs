using UnityEngine;

namespace FixedCameraPrototype.Inventory
{
    // Isolated mini-scene that renders one model into a RenderTexture for the UI. It sits far outside
    // the level on its own layer, and its camera renders only that layer and only while a model is shown.
    public class ItemPreviewStage : MonoBehaviour
    {
        private const int k_renderTextureDepthBits = 24;
        private const float k_halfAngleDivisor = 2.0f;

        [SerializeField] private Camera m_previewCamera;
        [SerializeField] private Transform m_modelPivot;
        [SerializeField] private Vector2Int m_textureSizePixels = new Vector2Int(1024, 1024);
        [SerializeField] private float m_spinDegreesPerSecond = 40.0f;
        [Tooltip("Extra space around the model's bounding sphere. 1 = touching the frame edges.")]
        [SerializeField] private float m_framingPaddingMultiplier = 1.25f;

        private RenderTexture m_previewTexture;
        private GameObject m_shownPrefab;
        private GameObject m_modelInstance;

        // Created lazily so views can bind it in OnEnable regardless of Awake order.
        public RenderTexture PreviewTexture
        {
            get
            {
                if (m_previewTexture == null)
                {
                    m_previewTexture = new RenderTexture(m_textureSizePixels.x, m_textureSizePixels.y, k_renderTextureDepthBits);
                    m_previewTexture.name = name;
                }

                return m_previewTexture;
            }
        }

        private void Awake()
        {
            m_previewCamera.targetTexture = PreviewTexture;
            m_previewCamera.cullingMask = 1 << gameObject.layer;
            m_previewCamera.enabled = false;
        }

        private void OnDestroy()
        {
            if (m_previewTexture != null)
            {
                m_previewTexture.Release();
                Destroy(m_previewTexture);
            }
        }

        // Unscaled so the model keeps turning while the game is paused behind the menus.
        private void Update()
        {
            if (m_modelInstance != null)
            {
                m_modelPivot.Rotate(Vector3.up, m_spinDegreesPerSecond * Time.unscaledDeltaTime, Space.World);
            }
        }

        public void Show(GameObject modelPrefab)
        {
            if (modelPrefab == m_shownPrefab)
            {
                return;
            }

            Clear();
            if (modelPrefab == null)
            {
                return;
            }

            m_shownPrefab = modelPrefab;
            m_modelInstance = Instantiate(modelPrefab, m_modelPivot);
            m_modelInstance.transform.localPosition = Vector3.zero;
            m_modelInstance.transform.localRotation = Quaternion.identity;
            PrepareModelForDisplay(m_modelInstance);
            FrameModel(m_modelInstance);
            m_previewCamera.enabled = true;
        }

        public void Clear()
        {
            if (m_modelInstance != null)
            {
                Destroy(m_modelInstance);
            }

            m_modelInstance = null;
            m_shownPrefab = null;
            m_modelPivot.localRotation = Quaternion.identity;
            m_previewCamera.enabled = false;
        }

        // The preview is a picture, not a world object: no physics, no gameplay scripts, stage layer only.
        private void PrepareModelForDisplay(GameObject modelInstance)
        {
            foreach (Collider modelCollider in modelInstance.GetComponentsInChildren<Collider>(true))
            {
                modelCollider.enabled = false;
            }

            foreach (MonoBehaviour modelBehaviour in modelInstance.GetComponentsInChildren<MonoBehaviour>(true))
            {
                modelBehaviour.enabled = false;
            }

            foreach (Transform modelTransform in modelInstance.GetComponentsInChildren<Transform>(true))
            {
                modelTransform.gameObject.layer = gameObject.layer;
            }
        }

        private void FrameModel(GameObject modelInstance)
        {
            Renderer[] modelRenderers = modelInstance.GetComponentsInChildren<Renderer>();
            if (modelRenderers.Length == 0)
            {
                return;
            }

            Bounds modelBounds = modelRenderers[0].bounds;
            for (int i = 1; i < modelRenderers.Length; i++)
            {
                modelBounds.Encapsulate(modelRenderers[i].bounds);
            }

            modelInstance.transform.position += m_modelPivot.position - modelBounds.center;

            float boundingSphereRadius = modelBounds.extents.magnitude;
            float halfFieldOfViewRadians = m_previewCamera.fieldOfView * Mathf.Deg2Rad / k_halfAngleDivisor;
            float cameraDistance = boundingSphereRadius * m_framingPaddingMultiplier / Mathf.Sin(halfFieldOfViewRadians);
            Transform cameraTransform = m_previewCamera.transform;
            cameraTransform.position = m_modelPivot.position - cameraTransform.forward * cameraDistance;
        }
    }
}
