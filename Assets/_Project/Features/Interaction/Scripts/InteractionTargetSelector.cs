using System;
using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Interaction
{
    // Sole authority for which interactable the player is focused on. Distances are horizontal so
    // object height (shelf vs floor) does not change priority.
    public class InteractionTargetSelector : MonoBehaviour
    {
        [SerializeField] private CharacterControlLock m_controlLock;

        private Interactable m_focusedInteractable;

        public Interactable FocusedInteractable => m_focusedInteractable;

        public event Action<Interactable> OnFocusChanged;

        // While another system owns the character (e.g. inventory selection), nothing is focusable,
        // so indicators hide and stale focus cannot be interacted with on unlock.
        private void Update()
        {
            if (m_controlLock.IsLocked)
            {
                ClearFocusAndIndications();
                return;
            }

            UpdateFocusedInteractable(FindClosestInteractableInInteractRange());
            UpdateIndicationStates();
        }

        private void OnDisable()
        {
            ClearFocusAndIndications();
        }

        private void ClearFocusAndIndications()
        {
            UpdateFocusedInteractable(null);
            foreach (Interactable interactable in InteractableRegistry.RegisteredInteractables)
            {
                interactable.SetIndicationState(InteractionIndicationState.Hidden);
            }
        }

        private Interactable FindClosestInteractableInInteractRange()
        {
            Interactable closestInteractable = null;
            float closestDistance = float.MaxValue;

            foreach (Interactable interactable in InteractableRegistry.RegisteredInteractables)
            {
                float distance = CalculateHorizontalDistanceTo(interactable);
                if (distance <= interactable.InteractRadiusMeters && distance < closestDistance)
                {
                    closestInteractable = interactable;
                    closestDistance = distance;
                }
            }

            return closestInteractable;
        }

        private void UpdateIndicationStates()
        {
            foreach (Interactable interactable in InteractableRegistry.RegisteredInteractables)
            {
                interactable.SetIndicationState(DetermineIndicationState(interactable));
            }
        }

        private InteractionIndicationState DetermineIndicationState(Interactable interactable)
        {
            if (interactable == m_focusedInteractable)
            {
                return InteractionIndicationState.Focused;
            }

            bool isWithinNoticeRange = CalculateHorizontalDistanceTo(interactable) <= interactable.NoticeRadiusMeters;
            return isWithinNoticeRange ? InteractionIndicationState.Noticed : InteractionIndicationState.Hidden;
        }

        private void UpdateFocusedInteractable(Interactable focusedInteractable)
        {
            if (focusedInteractable == m_focusedInteractable)
            {
                return;
            }

            m_focusedInteractable = focusedInteractable;
            OnFocusChanged?.Invoke(m_focusedInteractable);
        }

        private float CalculateHorizontalDistanceTo(Interactable interactable)
        {
            Vector3 offset = Vector3.ProjectOnPlane(interactable.transform.position - transform.position, Vector3.up);
            return offset.magnitude;
        }
    }
}
