namespace FixedCameraPrototype.Shared
{
    public interface IItemReceiver
    {
        bool TryAddItem(ItemDefinition item, int quantity);
    }
}
