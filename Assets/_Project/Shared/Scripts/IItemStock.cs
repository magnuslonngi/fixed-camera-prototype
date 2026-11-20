namespace FixedCameraPrototype.Shared
{
    // Lets a feature draw consumables (e.g. ammo) from the character's storage without knowing the Inventory feature.
    public interface IItemStock
    {
        int GetQuantity(ItemDefinition item);
        int RemoveQuantity(ItemDefinition item, int requestedQuantity);
    }
}
