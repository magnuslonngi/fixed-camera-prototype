namespace FixedCameraPrototype.Shared
{
    // Lets any feature show a full-screen document without knowing the Reading feature.
    public interface IDocumentReader
    {
        void OpenDocument(string documentText);
    }
}
