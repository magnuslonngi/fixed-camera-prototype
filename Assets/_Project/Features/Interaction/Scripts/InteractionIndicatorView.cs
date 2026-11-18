using UnityEngine;
using UnityEngine.UIElements;

namespace FixedCameraPrototype.Interaction
{
    // One on-screen indicator for one interactable. Holds no indication state of its own:
    // it mirrors the interactable's state into USS classes.
    public class InteractionIndicatorView
    {
        private const string k_noticedClassName = "indicator--noticed";
        private const string k_focusedClassName = "indicator--focused";
        private const string k_indicatorName = "indicator";
        private const string k_promptLabelName = "indicator-prompt-label";

        private readonly Interactable m_interactable;
        private readonly VisualElement m_rootElement;
        private readonly VisualElement m_indicatorElement;

        public InteractionIndicatorView(Interactable interactable, VisualTreeAsset indicatorTemplate, VisualElement parentElement, string promptText)
        {
            m_interactable = interactable;
            m_rootElement = indicatorTemplate.Instantiate();
            m_rootElement.pickingMode = PickingMode.Ignore;
            m_rootElement.style.position = Position.Absolute;
            m_indicatorElement = m_rootElement.Q(k_indicatorName);
            m_rootElement.Q<Label>(k_promptLabelName).text = promptText;
            parentElement.Add(m_rootElement);

            m_interactable.OnIndicationStateChanged += ApplyIndicationState;
            ApplyIndicationState(m_interactable.IndicationState);
        }

        public Interactable Interactable => m_interactable;
        public bool IsVisible => m_interactable.IndicationState != InteractionIndicationState.Hidden;

        public void SetPanelPosition(Vector2 panelPosition, bool isInFrontOfCamera)
        {
            m_rootElement.style.left = panelPosition.x;
            m_rootElement.style.top = panelPosition.y;
            m_rootElement.style.visibility = isInFrontOfCamera ? Visibility.Visible : Visibility.Hidden;
        }

        public void Dispose()
        {
            m_interactable.OnIndicationStateChanged -= ApplyIndicationState;
            m_rootElement.RemoveFromHierarchy();
        }

        private void ApplyIndicationState(InteractionIndicationState indicationState)
        {
            m_indicatorElement.EnableInClassList(k_noticedClassName, indicationState == InteractionIndicationState.Noticed);
            m_indicatorElement.EnableInClassList(k_focusedClassName, indicationState == InteractionIndicationState.Focused);
        }
    }
}
