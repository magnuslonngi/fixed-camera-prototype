using System;
using Unity.Cinemachine;
using UnityEngine;

namespace FixedCameraPrototype.FixedCamera
{
    [RequireComponent(typeof(BoxCollider))]
    public class CameraZone : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera m_shotCamera;

        public CinemachineCamera ShotCamera => m_shotCamera;

        public event Action<CameraZone> OnTargetEntered;
        public event Action<CameraZone> OnTargetExited;

        private void Reset()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<CharacterController>(out _))
            {
                OnTargetEntered?.Invoke(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<CharacterController>(out _))
            {
                OnTargetExited?.Invoke(this);
            }
        }

        private void OnDrawGizmos()
        {
            if (!TryGetComponent<BoxCollider>(out BoxCollider box))
            {
                return;
            }

            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
            Gizmos.matrix = Matrix4x4.identity;

            if (m_shotCamera != null)
            {
                Gizmos.DrawLine(box.bounds.center, m_shotCamera.transform.position);
            }
        }
    }
}
