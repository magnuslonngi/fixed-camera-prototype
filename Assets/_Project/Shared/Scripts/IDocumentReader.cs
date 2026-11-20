using System;

namespace FixedCameraPrototype.Shared
{
    // Lets any feature show a full-screen document without knowing the Reading feature.
    public interface IDocumentReader
    {
        event Action OnDocumentClosed;

        void OpenDocument(string documentText);
    }
}
