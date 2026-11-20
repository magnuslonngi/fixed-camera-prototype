using UnityEngine;
using UnityEngine.UIElements;

namespace FixedCameraPrototype.PlayerCharacter
{
    [RequireComponent(typeof(UIDocument))]
    public class PlayerStaminaHud : MonoBehaviour
    {
        private const string k_fillName = "stamina-fill";
        private const string k_exhaustedFillClassName = "vitals-hud__stamina-fill--exhausted";
        private const float k_fullPercent = 100.0f;

        [SerializeField] private PlayerStamina m_stamina;

        private VisualElement m_fill;

        private void OnEnable()
        {
            m_fill = GetComponent<UIDocument>().rootVisualElement.Q(k_fillName);
            m_stamina.OnStaminaChanged += UpdateFill;
            m_stamina.OnExhaustionChanged += UpdateExhaustedStyle;
        }

        private void Start()
        {
            UpdateFill(m_stamina.CurrentStamina, m_stamina.MaxStamina);
            UpdateExhaustedStyle(m_stamina.IsExhausted);
        }

        private void OnDisable()
        {
            m_stamina.OnStaminaChanged -= UpdateFill;
            m_stamina.OnExhaustionChanged -= UpdateExhaustedStyle;
        }

        private void UpdateFill(float currentStamina, float maxStamina)
        {
            float staminaRatio = maxStamina > 0.0f ? currentStamina / maxStamina : 0.0f;
            m_fill.style.width = Length.Percent(staminaRatio * k_fullPercent);
        }

        private void UpdateExhaustedStyle(bool isExhausted)
        {
            m_fill.EnableInClassList(k_exhaustedFillClassName, isExhausted);
        }
    }
}
