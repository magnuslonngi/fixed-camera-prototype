using System;
using UnityEngine;
using FixedCameraPrototype.Shared;

namespace FixedCameraPrototype.PlayerCharacter
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        private const float k_maximumInputMagnitude = 1.0f;
        private const float k_idleBlendValue = 0.0f;
        // Must match the Walk threshold in the PlayerCharacter Locomotion blend tree.
        private const float k_walkBlendValue = 0.5f;
        private const float k_runBlendValue = 1.0f;

        [Header("References")]
        [SerializeField] private PlayerInputReader m_inputReader;
        [SerializeField] private CameraRelativeInputBasis m_inputBasis;
        [SerializeField] private MovementLock m_movementLock;

        [Header("Speed (meters per second)")]
        [SerializeField] private float m_walkSpeed = 1.8f;
        [SerializeField] private float m_runSpeed = 4.0f;
        [SerializeField] private float m_backwardWalkSpeed = 1.2f;
        [SerializeField] private float m_accelerationRate = 20.0f;
        [SerializeField] private float m_decelerationRate = 30.0f;
        [SerializeField] private float m_gravity = 20.0f;

        private CharacterController m_characterController;
        private Vector3 m_horizontalVelocity;
        private float m_verticalVelocity;

        public Vector3 DesiredDirection { get; private set; }
        public float TankTurnInput { get; private set; }
        public float TankThrottleInput { get; private set; }
        public float NormalizedSpeed => IsMovingBackward
            ? -CalculateBackwardNormalizedSpeed(m_horizontalVelocity.magnitude)
            : CalculateNormalizedSpeed(m_horizontalVelocity.magnitude);
        public bool IsRunning => m_inputReader.IsSprintHeld && (DesiredDirection.sqrMagnitude > 0.0f || TankThrottleInput > 0.0f);

        // Camera-relative input faces the player along its movement, so only tank reversing walks backward.
        private bool IsMovingBackward => DesiredDirection.sqrMagnitude <= 0.0f
            && Vector3.Dot(m_horizontalVelocity, transform.forward) < 0.0f;

        public event Action<float, bool> OnLocomotionChanged;

        private void Awake()
        {
            m_characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            UpdateDesiredDirection();
            UpdateTankInput();
            UpdateHorizontalVelocity(Time.deltaTime);
            UpdateVerticalVelocity(Time.deltaTime);

            Vector3 displacement = (m_horizontalVelocity + (Vector3.up * m_verticalVelocity)) * Time.deltaTime;
            m_characterController.Move(displacement);

            OnLocomotionChanged?.Invoke(NormalizedSpeed, IsRunning);
        }

        private float CalculateNormalizedSpeed(float currentSpeed)
        {
            if (currentSpeed < m_walkSpeed)
            {
                return Mathf.Lerp(k_idleBlendValue, k_walkBlendValue, Mathf.InverseLerp(0.0f, m_walkSpeed, currentSpeed));
            }

            return Mathf.Lerp(k_walkBlendValue, k_runBlendValue, Mathf.InverseLerp(m_walkSpeed, m_runSpeed, currentSpeed));
        }

        private float CalculateBackwardNormalizedSpeed(float currentSpeed)
        {
            return Mathf.Lerp(k_idleBlendValue, k_walkBlendValue, Mathf.InverseLerp(0.0f, m_backwardWalkSpeed, currentSpeed));
        }

        private void UpdateTankInput()
        {
            bool isLocked = m_movementLock.IsLocked;
            TankTurnInput = isLocked ? 0.0f : m_inputReader.TankTurnInput;
            TankThrottleInput = isLocked ? 0.0f : m_inputReader.TankThrottleInput;
        }

        private bool HasMovementInput()
        {
            return DesiredDirection.sqrMagnitude > 0.0f || !Mathf.Approximately(TankThrottleInput, 0.0f);
        }

        private Vector3 CalculateTargetVelocity()
        {
            float forwardSpeed = m_inputReader.IsSprintHeld ? m_runSpeed : m_walkSpeed;
            float tankSpeed = TankThrottleInput >= 0.0f ? forwardSpeed : m_backwardWalkSpeed;
            Vector3 cameraRelativeVelocity = DesiredDirection * forwardSpeed;
            Vector3 tankVelocity = transform.forward * (TankThrottleInput * tankSpeed);
            return Vector3.ClampMagnitude(cameraRelativeVelocity + tankVelocity, forwardSpeed);
        }

        private void UpdateDesiredDirection()
        {
            if (m_movementLock.IsLocked)
            {
                DesiredDirection = Vector3.zero;
                return;
            }

            Vector2 input = m_inputReader.MoveInput;
            bool isBelowDeadzone = input.magnitude < m_inputBasis.InputDeadzone;
            DesiredDirection = isBelowDeadzone
                ? Vector3.zero
                : Vector3.ClampMagnitude(m_inputBasis.ToWorldDirection(input), k_maximumInputMagnitude);
        }

        private void UpdateHorizontalVelocity(float deltaTime)
        {
            float changeRate = HasMovementInput() ? m_accelerationRate : m_decelerationRate;
            m_horizontalVelocity = Vector3.MoveTowards(m_horizontalVelocity, CalculateTargetVelocity(), changeRate * deltaTime);
        }

        private void UpdateVerticalVelocity(float deltaTime)
        {
            // Small downward bias keeps the controller grounded on slopes and steps.
            m_verticalVelocity = m_characterController.isGrounded
                ? -m_gravity * deltaTime
                : m_verticalVelocity - (m_gravity * deltaTime);
        }
    }
}
