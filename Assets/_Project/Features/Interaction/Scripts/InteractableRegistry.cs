using System;
using System.Collections.Generic;
using UnityEngine;

namespace FixedCameraPrototype.Interaction
{
    // Interactables self-register so the selector and HUD can find them without layers, colliders or scene lookups.
    public static class InteractableRegistry
    {
        private static readonly HashSet<Interactable> s_registeredInteractables = new HashSet<Interactable>();

        public static event Action<Interactable> OnRegistered;
        public static event Action<Interactable> OnUnregistered;

        public static IReadOnlyCollection<Interactable> RegisteredInteractables => s_registeredInteractables;

        public static void Register(Interactable interactable)
        {
            if (s_registeredInteractables.Add(interactable))
            {
                OnRegistered?.Invoke(interactable);
            }
        }

        public static void Unregister(Interactable interactable)
        {
            if (s_registeredInteractables.Remove(interactable))
            {
                OnUnregistered?.Invoke(interactable);
            }
        }

        // Static state survives Play Mode when domain reload is disabled; clear it on each run.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            s_registeredInteractables.Clear();
            OnRegistered = null;
            OnUnregistered = null;
        }
    }
}
