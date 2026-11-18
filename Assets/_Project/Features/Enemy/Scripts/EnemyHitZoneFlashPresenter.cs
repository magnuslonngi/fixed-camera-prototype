using System.Collections;
using UnityEngine;

namespace FixedCameraPrototype.Enemy
{
    public class EnemyHitZoneFlashPresenter : MonoBehaviour
    {
        private static readonly int s_baseColorPropertyId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private EnemyHitZone m_hitZone;
        [SerializeField] private Renderer m_zoneRenderer;
        [SerializeField] private Color m_flashColor = Color.red;
        [SerializeField] private float m_flashSeconds = 0.2f;

        private MaterialPropertyBlock m_propertyBlock;
        private Coroutine m_flashRoutine;

        private void Awake()
        {
            m_propertyBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            m_hitZone.OnHit += HandleHit;
        }

        private void OnDisable()
        {
            m_hitZone.OnHit -= HandleHit;
        }

        private void HandleHit()
        {
            if (m_flashRoutine != null)
            {
                StopCoroutine(m_flashRoutine);
            }

            m_flashRoutine = StartCoroutine(FlashZoneColor());
        }

        private IEnumerator FlashZoneColor()
        {
            m_propertyBlock.SetColor(s_baseColorPropertyId, m_flashColor);
            m_zoneRenderer.SetPropertyBlock(m_propertyBlock);

            yield return new WaitForSeconds(m_flashSeconds);

            m_zoneRenderer.SetPropertyBlock(null);
            m_flashRoutine = null;
        }
    }
}
