using System;
using UnityEngine;

namespace FixedCameraPrototype.PlayerCharacter
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        private const float k_maximumInputMagnitude = 1.0f;

        [Header("References")]
        [SerializeField] private PlayerInputReader m_inputReader;
        [SerializeField] private CameraRelativeInputBasis m_inputBasis;

        [Header("Speed (meters per second)")]
        [SerializeField] private float m_walkSpeed = 1.8f;
        [SerializeField] private float m_runSpeed = 4.0f;
        [SerializeField] private float m_accelerationRate = 20.0f;
        [SerializeField] private float m_decelerationRate = 30.0f;
        [SerializeField] private float m_gravity = 20.0f;

        private CharacterController m_characterController;
        private Vector3 m_horizontalVelocity;
        private float m_verticalVelocity;

        public Vector3 DesiredDirection { get; private set; }
        public float NormalizedSpeed => m_runSpeed > 0.0f ? m_horizontalVelocity.magnitude / m_runSpeed : 0.0f;
        public bool IsRunning => m_inputReader.IsSprintHeld && DesiredDirection.sqrMagnitude > 0.0f;

        public event Action<float, bool> OnLocomotionChanged;

        private void Awake()
        {
            m_characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            UpdateDesiredDirection();
            UpdateHorizontalVelocity(Time.deltaTime);
            UpdateVerticalVelocity(Time.deltaTime);

            Vector3 displacement = (m_horizontalVelocity + (Vector3.up * m_verticalVelocity)) * Time.deltaTime;
            m_characterController.Move(displacement);

            OnLocomotionChanged?.Invoke(NormalizedSpeed, IsRunning);
        }

        private void UpdateDesiredDirection()
        {
            Vector2 input = m_inputReader.MoveInput;
            bool isBelowDeadzone = input.magnitude < m_inputBasis.InputDeadzone;
            DesiredDirection = isBelowDeadzone
                ? Vector3.zero
                : Vector3.ClampMagnitude(m_inputBasis.ToWorldDirection(input), k_maximumInputMagnitude);
        }

        private void UpdateHorizontalVelocity(float deltaTime)
        {
            float targetSpeed = m_inputReader.IsSprintHeld ? m_runSpeed : m_walkSpeed;
            Vector3 targetVelocity = DesiredDirection * targetSpeed;
            float changeRate = DesiredDirection.sqrMagnitude > 0.0f ? m_accelerationRate : m_decelerationRate;
            m_horizontalVelocity = Vector3.MoveTowards(m_horizontalVelocity, targetVelocity, changeRate * deltaTime);
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
