using UnityEngine;

namespace FixedCameraPrototype.Shared
{
    // Marks the top of an entity whose components are grouped on child objects by feature.
    // Features find each other's capabilities through it by interface, so they stay decoupled
    // and keep working no matter which child a component is moved to.
    public class CapabilityRoot : MonoBehaviour
    {
        private const bool k_includeInactiveChildren = true;

        public bool TryGetCapability<T>(out T capability) where T : class
        {
            capability = GetComponentInChildren<T>(k_includeInactiveChildren);
            return capability != null;
        }

        public static bool TryFindCapability<T>(GameObject memberObject, out T capability) where T : class
        {
            CapabilityRoot capabilityRoot = memberObject.GetComponentInParent<CapabilityRoot>(k_includeInactiveChildren);
            if (capabilityRoot == null)
            {
                capability = null;
                return false;
            }

            return capabilityRoot.TryGetCapability(out capability);
        }
    }
}
