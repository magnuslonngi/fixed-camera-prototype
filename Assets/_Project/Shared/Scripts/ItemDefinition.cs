using UnityEngine;

namespace FixedCameraPrototype.Shared
{
    // Item identity is the asset reference itself, so locks and pickups compare assets, never names.
    [CreateAssetMenu(menuName = "FixedCameraPrototype/Item Definition", fileName = "ItemDefinition")]
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string m_displayName;

        public string DisplayName => m_displayName;
    }
}
