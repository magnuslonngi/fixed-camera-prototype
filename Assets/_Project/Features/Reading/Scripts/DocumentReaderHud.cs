using UnityEngine;
using UnityEngine.UIElements;

namespace FixedCameraPrototype.Reading
{
    // Stateless view: shows the text DocumentReader passes in, hides on close.
    [RequireComponent(typeof(UIDocument))]
    public class DocumentReaderHud : MonoBehaviour
    {
        private const string k_overlayName = "reader-overlay";
        private const string k_documentLabelName = "document-label";
        private const string k_visibleOverlayClassName = "reader-overlay--visible";

        [SerializeField] private DocumentReader m_documentReader;

        private VisualElement m_overlay;
        private Label m_documentLabel;

        private void OnEnable()
        {
            VisualElement rootElement = GetComponent<UIDocument>().rootVisualElement;
            m_overlay = rootElement.Q(k_overlayName);
            m_documentLabel = rootElement.Q<Label>(k_documentLabelName);

            m_documentReader.OnDocumentOpened += ShowDocument;
            m_documentReader.OnDocumentClosed += HideDocument;

            if (m_documentReader.IsOpen)
            {
                ShowDocument(m_documentReader.DocumentText);
            }
            else
            {
                HideDocument();
            }
        }

        private void OnDisable()
        {
            m_documentReader.OnDocumentOpened -= ShowDocument;
            m_documentReader.OnDocumentClosed -= HideDocument;
        }

        private void ShowDocument(string documentText)
        {
            m_documentLabel.text = documentText;
            m_overlay.AddToClassList(k_visibleOverlayClassName);
        }

        private void HideDocument()
        {
            m_overlay.RemoveFromClassList(k_visibleOverlayClassName);
        }
    }
}
