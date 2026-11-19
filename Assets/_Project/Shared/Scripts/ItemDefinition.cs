using UnityEngine;

namespace FixedCameraPrototype.Shared
{
    // Item identity is the asset reference itself, so locks and pickups compare assets, never names.
    [CreateAssetMenu(menuName = "FixedCameraPrototype/Item Definition", fileName = "ItemDefinition")]
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string m_displayName;
        [SerializeField] private ItemUseAction m_useAction = ItemUseAction.Inspect;

        [SerializeField]
        [TextArea]
        private string m_descriptionText;

        public string DisplayName => m_displayName;
        public ItemUseAction UseAction => m_useAction;
        public string DescriptionText => m_descriptionText;
    }
}
