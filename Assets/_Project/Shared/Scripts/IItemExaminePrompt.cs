namespace FixedCameraPrototype.Shared
{
    // Lets a pickup show the item it just handed over without knowing the Inventory feature.
    public interface IItemExaminePrompt
    {
        void ExamineItem(ItemDefinition item);
    }
}
