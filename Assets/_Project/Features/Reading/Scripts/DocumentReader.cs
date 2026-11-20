using System;
using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Reading
{
    // Sole authority for the open document. Opening pushes a paused full-screen modal; Cancel closes it
    // and hands input back to whatever screen opened it (or to gameplay).
    public class DocumentReader : MonoBehaviour, IDocumentReader, IModalScreen
    {
        [SerializeField] private ModalStack m_modalStack;
        [SerializeField] private MenuInputReader m_inputReader;

        private string m_documentText;

        public bool IsOpen => m_modalStack.IsOpen(this);
        public string DocumentText => m_documentText;

        public event Action<string> OnDocumentOpened;
        public event Action OnDocumentClosed;

        private void Awake()
        {
            m_inputReader.enabled = false;
        }

        private void OnEnable()
        {
            m_inputReader.OnCancelPressed += CloseDocument;
        }

        private void OnDisable()
        {
            m_inputReader.OnCancelPressed -= CloseDocument;
            CloseDocument();
        }

        public void OpenDocument(string documentText)
        {
            if (IsOpen)
            {
                return;
            }

            m_documentText = documentText;
            m_modalStack.Push(this);
            OnDocumentOpened?.Invoke(m_documentText);
        }

        public void CloseDocument()
        {
            if (!IsOpen)
            {
                return;
            }

            m_modalStack.Pop(this);
            m_documentText = null;
            OnDocumentClosed?.Invoke();
        }

        public void SetInputFocus(bool hasFocus)
        {
            m_inputReader.enabled = hasFocus;
        }
    }
}
