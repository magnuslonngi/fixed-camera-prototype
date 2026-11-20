using FixedCameraPrototype.Shared;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace FixedCameraPrototype.Inventory
{
    // Always-on reminder of the inventory key during gameplay; hidden while any full-screen modal is open.
    [RequireComponent(typeof(UIDocument))]
    public class InventoryPromptHud : MonoBehaviour
    {
        private const string k_promptName = "inventory-prompt";
        private const string k_keyLabelName = "key-label";
        private const string k_hiddenPromptClassName = "inventory-prompt--hidden";

        [SerializeField] private ModalStack m_modalStack;
        [SerializeField] private InputActionReference m_openInventoryAction;
        [Tooltip("Control scheme group whose binding is shown in the prompt.")]
        [SerializeField] private string m_promptBindingGroup = "Keyboard&Mouse";

        private VisualElement m_prompt;

        private void OnEnable()
        {
            VisualElement rootElement = GetComponent<UIDocument>().rootVisualElement;
            m_prompt = rootElement.Q(k_promptName);
            rootElement.Q<Label>(k_keyLabelName).text = m_openInventoryAction.action.GetBindingDisplayString(InputBinding.MaskByGroup(m_promptBindingGroup));

            m_modalStack.OnStackChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            m_modalStack.OnStackChanged -= Refresh;
        }

        private void Refresh()
        {
            m_prompt.EnableInClassList(k_hiddenPromptClassName, m_modalStack.HasOpenScreen);
        }
    }
}
