using UnityEngine;

namespace FixedCameraPrototype.Shared
{
    // Item identity is the asset reference itself, so locks and pickups compare assets, never names.
    [CreateAssetMenu(menuName = "FixedCameraPrototype/Item Definition", fileName = "ItemDefinition")]
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string m_displayName;
        [SerializeField] private ItemUseAction m_useAction = ItemUseAction.Inspect;
        [SerializeField] private bool m_isStackable;
        [Tooltip("Model shown in the item window and inventory preview. Never placed in the world.")]
        [SerializeField] private GameObject m_previewPrefab;

        [Tooltip("Brief description shown under the item preview.")]
        [SerializeField]
        [TextArea]
        private string m_descriptionText;

        [Tooltip("Full document text, only used when the use action is Read.")]
        [SerializeField]
        [TextArea(4, 12)]
        private string m_readableText;

        public string DisplayName => m_displayName;
        public ItemUseAction UseAction => m_useAction;
        public bool IsStackable => m_isStackable;
        public GameObject PreviewPrefab => m_previewPrefab;
        public string DescriptionText => m_descriptionText;
        public string ReadableText => m_readableText;
    }
}
