using UnityEngine;

namespace FixedCameraPrototype.Shared
{
    // The target decides whether an item works on it; the caller consumes the item only on success.
    public interface IItemUseTarget
    {
        // Model shown while choosing which item to use on this target. May be null.
        GameObject PreviewPrefab { get; }

        bool TryUseItem(ItemDefinition item);
    }
}
