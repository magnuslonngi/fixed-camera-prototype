namespace FixedCameraPrototype.Shared
{
    // The target decides whether an item works on it; the caller consumes the item only on success.
    public interface IItemUseTarget
    {
        bool TryUseItem(ItemDefinition item);
    }
}
