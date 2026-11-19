using System;
using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Combat
{
    public class PlayerAim : MonoBehaviour, IAimStateSource
    {
        private const float k_inputThreshold = 0.5f;

        [Header("References")]
        [SerializeField] private CombatInputReader m_inputReader;
        [SerializeField] private MovementLock m_movementLock;
        [SerializeField] private AimAssist m_aimAssist;
        [SerializeField] private Transform m_aimOrigin;

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
        public Vector3 AimDirection => Quaternion.AngleAxis(-ResolvePitchDegrees(), m_aimOrigin.right) * m_aimOrigin.forward;

        public event Action<bool> OnAimingChanged;

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
            OnAimingChanged?.Invoke(m_isAiming);

            if (!m_isAiming)
            {
                m_movementLock.RemoveLock();
                return;
            }

            m_movementLock.AddLock();
            BeginSnapToNearestTarget();
        }

        public bool TryFindSnapTarget(out IAimAssistTarget target)
        {
            return m_aimAssist.TryFindAcquisitionTarget(transform.position, transform.forward, out target);
        }

        // NOTE(Design): The aim origin sits off the body's center and points away from body forward
        // in the pistol pose, so facing the target with the body would leave the hands aiming beside it.
        // Solves the body yaw that places the target on the aim origin's line of fire.
        public bool TryCalculateSnapRotation(Vector3 targetPosition, out Quaternion snapRotation)
        {
            snapRotation = transform.rotation;

            Vector3 flatToTarget = Vector3.ProjectOnPlane(targetPosition - transform.position, Vector3.up);
            Vector3 localOrigin = Vector3.ProjectOnPlane(transform.InverseTransformPoint(m_aimOrigin.position), Vector3.up);
            Vector3 localDirection = Vector3.ProjectOnPlane(transform.InverseTransformDirection(m_aimOrigin.forward), Vector3.up).normalized;
            Vector3 localPerpendicular = Vector3.Cross(Vector3.up, localDirection);

            float lateralOffset = Vector3.Dot(localOrigin, localPerpendicular);
            float targetDistance = flatToTarget.magnitude;
            if (targetDistance <= Mathf.Abs(lateralOffset))
            {
                return false;
            }

            float distanceAlongLine = Mathf.Sqrt((targetDistance * targetDistance) - (lateralOffset * lateralOffset));
            bool isTargetBehindOrigin = distanceAlongLine <= Vector3.Dot(localOrigin, localDirection);
            if (isTargetBehindOrigin)
            {
                return false;
            }

            Vector3 localTargetPoint = (localPerpendicular * lateralOffset) + (localDirection * distanceAlongLine);
            snapRotation = Quaternion.LookRotation(flatToTarget, Vector3.up) * Quaternion.Inverse(Quaternion.LookRotation(localTargetPoint, Vector3.up));
            return true;
        }

        private void BeginSnapToNearestTarget()
        {
            if (TryFindSnapTarget(out IAimAssistTarget target))
            {
                m_isSnapping = TryCalculateSnapRotation(target.Position, out m_snapRotation);
            }
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
