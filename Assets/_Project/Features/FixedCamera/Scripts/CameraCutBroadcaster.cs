using Unity.Cinemachine;
using UnityEngine;
using FixedCameraPrototype.Shared;

namespace FixedCameraPrototype.FixedCamera
{
    public class CameraCutBroadcaster : MonoBehaviour
    {
        [SerializeField] private CameraCutEventChannel m_cameraCutChannel;

        private void OnEnable()
        {
            CinemachineCore.CameraActivatedEvent.AddListener(HandleCameraActivated);
        }

        private void OnDisable()
        {
            CinemachineCore.CameraActivatedEvent.RemoveListener(HandleCameraActivated);
        }

        private void HandleCameraActivated(ICinemachineCamera.ActivationEventParams activation)
        {
            if (activation.IsCut)
            {
                m_cameraCutChannel.RaiseCameraCut();
            }
        }
    }
}
