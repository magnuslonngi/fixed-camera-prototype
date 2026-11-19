namespace FixedCameraPrototype.Shared
{
    // A full-screen modal that takes input only while it is the top of the ModalStack.
    public interface IModalScreen
    {
        void SetInputFocus(bool hasFocus);
    }
}
