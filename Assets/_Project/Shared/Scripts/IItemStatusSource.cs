using System;

namespace FixedCameraPrototype.Shared
{
    // Lets the owner of an item's live state (e.g. loaded rounds) describe it to UI that cannot reference that owner.
    public interface IItemStatusSource
    {
        event Action OnStatusChanged;

        bool TryGetStatusText(ItemDefinition item, out string statusText);
    }
}
