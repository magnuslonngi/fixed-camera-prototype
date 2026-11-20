using System.Collections.Generic;

namespace FixedCameraPrototype.Shared
{
    // Lets enemies enumerate pursuit targets without physics layers or scene lookups.
    public static class PursuitTargetRegistry
    {
        private static readonly List<IPursuitTarget> s_registeredTargets = new List<IPursuitTarget>();

        public static IReadOnlyList<IPursuitTarget> RegisteredTargets => s_registeredTargets;

        public static void Register(IPursuitTarget target)
        {
            if (!s_registeredTargets.Contains(target))
            {
                s_registeredTargets.Add(target);
            }
        }

        public static void Unregister(IPursuitTarget target)
        {
            s_registeredTargets.Remove(target);
        }
    }
}
