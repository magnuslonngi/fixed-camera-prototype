using System;
using UnityEngine;

namespace FixedCameraPrototype.Interaction
{
    // Core interaction authority for one object. Behaviours (pick up, inspect, read) compose on top
    // by listening to OnInteracted; presentation listens to OnIndicationStateChanged.
    public class Interactable : MonoBehaviour
    {
        [SerializeField]
        [Min(0.0f)]
        private float m_noticeRadiusMeters = 4.0f;
        [SerializeField]
        [Min(0.0f)]
        private float m_interactRadiusMeters = 1.5f;
        [Tooltip("Where the indicator is drawn. Falls back to this transform.")]
        [SerializeField] private Transform m_indicatorAnchor;

        private InteractionIndicationState m_indicationState = InteractionIndicationState.Hidden;

        public event Action<InteractionIndicationState> OnIndicationStateChanged;
        public event Action<GameObject> OnInteracted;

        public float NoticeRadiusMeters => m_noticeRadiusMeters;
        public float InteractRadiusMeters => m_interactRadiusMeters;
        public InteractionIndicationState IndicationState => m_indicationState;
        public Vector3 IndicatorAnchorPosition => m_indicatorAnchor != null ? m_indicatorAnchor.position : transform.position;

        private void OnEnable()
        {
            InteractableRegistry.Register(this);
        }

        private void OnDisable()
        {
            SetIndicationState(InteractionIndicationState.Hidden);
            InteractableRegistry.Unregister(this);
        }

        private void OnValidate()
        {
            m_noticeRadiusMeters = Mathf.Max(m_noticeRadiusMeters, m_interactRadiusMeters);
        }

        public void Interact(GameObject instigator)
        {
            OnInteracted?.Invoke(instigator);
        }

        internal void SetIndicationState(InteractionIndicationState indicationState)
        {
            if (indicationState == m_indicationState)
            {
                return;
            }

            m_indicationState = indicationState;
            OnIndicationStateChanged?.Invoke(m_indicationState);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_noticeRadiusMeters);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, m_interactRadiusMeters);
        }
    }
}
