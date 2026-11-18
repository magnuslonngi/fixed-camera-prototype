using UnityEngine;
using FixedCameraPrototype.Shared;

namespace FixedCameraPrototype.PlayerCharacter
{
    // Authority for the screen-relative movement basis (forward/right on the ground plane).
    public class CameraRelativeInputBasis : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader m_inputReader;
        [SerializeField] private CameraCutEventChannel m_cameraCutChannel;
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float m_inputDeadzone = 0.2f;
        [SerializeField]
        [Range(1.0f, 180.0f)]
        private float m_latchReleaseAngleDegrees = 35.0f;

        private Transform m_viewCameraTransform;

        private InputBasisLatch m_latch;
        private Vector3 m_liveForward = Vector3.forward;
        private Vector3 m_liveRight = Vector3.right;

        public bool IsLatched => m_latch.IsLatched;
        public float InputDeadzone => m_inputDeadzone;

        private void Awake()
        {
            m_latch = new InputBasisLatch(m_inputDeadzone, m_latchReleaseAngleDegrees);
            m_viewCameraTransform = Camera.main.transform;
        }

        private void OnEnable()
        {
            m_cameraCutChannel.OnCameraCut += HandleCameraCut;
        }

        private void OnDisable()
        {
            m_cameraCutChannel.OnCameraCut -= HandleCameraCut;
        }

        // Update runs before the CinemachineBrain's LateUpdate, so the sampled basis still belongs to
        // the pre-cut shot when the cut event fires. That is the basis worth latching.
        private void Update()
        {
            m_latch.UpdateRelease(m_inputReader.MoveInput);

            if (!m_latch.IsLatched)
            {
                SampleLiveBasisFromCamera();
            }
        }

        public Vector3 ToWorldDirection(Vector2 input)
        {
            Vector3 forward = m_latch.IsLatched ? m_latch.LatchedForward : m_liveForward;
            Vector3 right = m_latch.IsLatched ? m_latch.LatchedRight : m_liveRight;
            return (forward * input.y) + (right * input.x);
        }

        private void HandleCameraCut()
        {
            m_latch.TryLatch(m_liveForward, m_liveRight, m_inputReader.MoveInput);
        }

        private void SampleLiveBasisFromCamera()
        {
            Vector3 flatForward = Vector3.ProjectOnPlane(m_viewCameraTransform.forward, Vector3.up);
            if (flatForward.sqrMagnitude < Mathf.Epsilon)
            {
                return;
            }

            m_liveForward = flatForward.normalized;
            m_liveRight = Vector3.Cross(Vector3.up, m_liveForward);
        }
    }
}
