using FixedCameraPrototype.Shared;
using UnityEngine;
using UnityEngine.UIElements;

namespace FixedCameraPrototype.PlayerCharacter
{
    [RequireComponent(typeof(UIDocument))]
    public class PlayerHealthHud : MonoBehaviour
    {
        private const string k_fillName = "health-fill";
        private const float k_fullPercent = 100.0f;

        [SerializeField] private Health m_health;

        private VisualElement m_fill;

        private void OnEnable()
        {
            m_fill = GetComponent<UIDocument>().rootVisualElement.Q(k_fillName);
            m_health.OnHealthChanged += UpdateFill;
        }

        private void Start()
        {
            UpdateFill(m_health.CurrentHealth, m_health.MaxHealth);
        }

        private void OnDisable()
        {
            m_health.OnHealthChanged -= UpdateFill;
        }

        private void UpdateFill(float currentHealth, float maxHealth)
        {
            float healthRatio = maxHealth > 0.0f ? currentHealth / maxHealth : 0.0f;
            m_fill.style.width = Length.Percent(healthRatio * k_fullPercent);
        }
    }
}
