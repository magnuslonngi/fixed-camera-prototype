using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace FixedCameraPrototype.Interaction
{
    // Screen-space indicator layer. Spawns one view per registered interactable and projects it
    // onto the panel each frame. Runs late so it reads the camera after CinemachineBrain has moved it
    // for this frame; otherwise indicators lag one frame behind every camera cut.
    [DefaultExecutionOrder(k_afterCinemachineBrainExecutionOrder)]
    [RequireComponent(typeof(UIDocument))]
    public class InteractionIndicatorHud : MonoBehaviour
    {
        private const int k_afterCinemachineBrainExecutionOrder = 1000;
        private const string k_indicatorLayerName = "indicator-layer";

        [SerializeField] private VisualTreeAsset m_indicatorTemplate;
        [SerializeField] private InputActionReference m_interactAction;
        [Tooltip("Control scheme group whose binding is shown in the prompt.")]
        [SerializeField] private string m_promptBindingGroup = "Keyboard&Mouse";

        private readonly Dictionary<Interactable, InteractionIndicatorView> m_viewsByInteractable = new Dictionary<Interactable, InteractionIndicatorView>();

        private UIDocument m_document;
        private VisualElement m_indicatorLayer;
        private Camera m_viewCamera;

        private void Awake()
        {
            m_document = GetComponent<UIDocument>();
            m_viewCamera = Camera.main;
        }

        private void OnEnable()
        {
            m_indicatorLayer = m_document.rootVisualElement.Q(k_indicatorLayerName);

            InteractableRegistry.OnRegistered += CreateView;
            InteractableRegistry.OnUnregistered += DisposeView;
            foreach (Interactable interactable in InteractableRegistry.RegisteredInteractables)
            {
                CreateView(interactable);
            }
        }

        private void OnDisable()
        {
            InteractableRegistry.OnRegistered -= CreateView;
            InteractableRegistry.OnUnregistered -= DisposeView;
            foreach (InteractionIndicatorView view in m_viewsByInteractable.Values)
            {
                view.Dispose();
            }
            m_viewsByInteractable.Clear();
        }

        private void LateUpdate()
        {
            IPanel panel = m_indicatorLayer.panel;
            if (panel == null)
            {
                return;
            }

            foreach (InteractionIndicatorView view in m_viewsByInteractable.Values)
            {
                if (view.IsVisible)
                {
                    PositionView(view, panel);
                }
            }
        }

        private void PositionView(InteractionIndicatorView view, IPanel panel)
        {
            Vector3 anchorWorldPosition = view.Interactable.IndicatorAnchorPosition;
            bool isInFrontOfCamera = m_viewCamera.WorldToViewportPoint(anchorWorldPosition).z > 0.0f;
            Vector2 panelPosition = RuntimePanelUtils.CameraTransformWorldToPanel(panel, anchorWorldPosition, m_viewCamera);
            view.SetPanelPosition(panelPosition, isInFrontOfCamera);
        }

        private void CreateView(Interactable interactable)
        {
            if (m_viewsByInteractable.ContainsKey(interactable))
            {
                return;
            }

            string promptText = m_interactAction.action.GetBindingDisplayString(InputBinding.MaskByGroup(m_promptBindingGroup));
            m_viewsByInteractable.Add(interactable, new InteractionIndicatorView(interactable, m_indicatorTemplate, m_indicatorLayer, promptText));
        }

        private void DisposeView(Interactable interactable)
        {
            if (m_viewsByInteractable.Remove(interactable, out InteractionIndicatorView view))
            {
                view.Dispose();
            }
        }
    }
}
