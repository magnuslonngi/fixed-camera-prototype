using UnityEngine;

namespace FixedCameraPrototype.Interaction
{
    public class DoorOpenPresenter : MonoBehaviour
    {
        [SerializeField] private ItemLockInteraction m_itemLock;
        [SerializeField] private GameObject m_doorObject;

        private void OnEnable()
        {
            m_itemLock.OnUnlocked += HideDoor;
        }

        private void OnDisable()
        {
            m_itemLock.OnUnlocked -= HideDoor;
        }

        private void HideDoor()
        {
            m_doorObject.SetActive(false);
        }
    }
}
