using System;
using UnityEngine;

namespace FixedCameraPrototype.Interaction
{
    // Sole authority for which interactable the player is focused on. Distances are horizontal so
    // object height (shelf vs floor) does not change priority.
    public class InteractionTargetSelector : MonoBehaviour
    {
        private Interactable m_focusedInteractable;

        public event Action<Interactable> OnFocusChanged;

        public Interactable FocusedInteractable => m_focusedInteractable;

        private void Update()
        {
            UpdateFocusedInteractable(FindClosestInteractableInInteractRange());
            UpdateIndicationStates();
        }

        private void OnDisable()
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
