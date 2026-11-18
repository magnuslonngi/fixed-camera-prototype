using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Combat
{
    public class PlayerAim : MonoBehaviour
    {
        private const float k_inputThreshold = 0.5f;

        [Header("References")]
        [SerializeField] private CombatInputReader m_inputReader;
        [SerializeField] private MovementLock m_movementLock;
        [SerializeField] private AimAssist m_aimAssist;

        [Header("Aiming (degrees)")]
        [SerializeField] private float m_turnSpeedDegreesPerSecond = 120.0f;
        [SerializeField] private float m_snapTurnSpeedDegreesPerSecond = 720.0f;
        [SerializeField] private float m_upPitchDegrees = 30.0f;
        [SerializeField] private float m_downPitchDegrees = 30.0f;

        private bool m_isAiming;
        private bool m_isSnapping;
        private Quaternion m_snapRotation;

        public bool IsAiming => m_isAiming;
        public AimTier CurrentTier => ResolveTier(m_inputReader.AimAdjustInput.y);
        public Vector3 AimDirection => Quaternion.AngleAxis(-ResolvePitchDegrees(), transform.right) * transform.forward;

        private void Update()
        {
            if (m_inputReader.IsAimHeld != m_isAiming)
            {
                SetAiming(m_inputReader.IsAimHeld);
            }

            if (m_isAiming)
            {
                UpdateFacing(Time.deltaTime);
            }
        }

        private void OnDisable()
        {
            SetAiming(false);
        }

        private void SetAiming(bool isAimRequested)
        {
            if (m_isAiming == isAimRequested)
            {
                return;
            }

            m_isAiming = isAimRequested;
            m_isSnapping = false;

            if (!m_isAiming)
            {
                m_movementLock.RemoveLock();
                return;
            }

            m_movementLock.AddLock();
            BeginSnapToNearestTarget();
        }

        private void BeginSnapToNearestTarget()
        {
            if (!m_aimAssist.TryFindAcquisitionTarget(transform.position, transform.forward, out IAimAssistTarget target))
            {
                return;
            }

            Vector3 flatDirection = Vector3.ProjectOnPlane(target.Position - transform.position, Vector3.up);
            if (flatDirection.sqrMagnitude < Mathf.Epsilon)
            {
                return;
            }

            m_snapRotation = Quaternion.LookRotation(flatDirection, Vector3.up);
            m_isSnapping = true;
        }

        private void UpdateFacing(float deltaTime)
        {
            float turnInput = m_inputReader.AimAdjustInput.x;
            if (Mathf.Abs(turnInput) >= k_inputThreshold)
            {
                m_isSnapping = false;
                transform.Rotate(Vector3.up, Mathf.Sign(turnInput) * m_turnSpeedDegreesPerSecond * deltaTime, Space.World);
                return;
            }

            if (m_isSnapping)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    m_snapRotation,
                    m_snapTurnSpeedDegreesPerSecond * deltaTime);
                m_isSnapping = transform.rotation != m_snapRotation;
            }
        }

        private float ResolvePitchDegrees()
        {
            switch (CurrentTier)
            {
                case AimTier.Up:
                    return m_upPitchDegrees;
                case AimTier.Down:
                    return -m_downPitchDegrees;
                default:
                    return 0.0f;
            }
        }

        private static AimTier ResolveTier(float verticalInput)
        {
            if (verticalInput >= k_inputThreshold)
            {
                return AimTier.Up;
            }

            return verticalInput <= -k_inputThreshold ? AimTier.Down : AimTier.Center;
        }
    }
}
