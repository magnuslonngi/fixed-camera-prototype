using UnityEngine;

namespace FixedCameraPrototype.PlayerCharacter
{
    // Horizontal displacement comes from animation root motion; this class owns intent and gravity.
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        private const float k_maximumInputMagnitude = 1.0f;

        [Header("References")]
        [SerializeField] private PlayerInputReader m_inputReader;
        [SerializeField] private CameraRelativeInputBasis m_inputBasis;
        [SerializeField] private RootMotionRelay m_rootMotionRelay;

        [Header("Gravity (meters per second squared)")]
        [SerializeField] private float m_gravity = 20.0f;

        private CharacterController m_characterController;
        private float m_verticalVelocity;

        public Vector3 DesiredDirection { get; private set; }
        public bool IsRunning => m_inputReader.IsSprintHeld && DesiredDirection.sqrMagnitude > 0.0f;

        private void Awake()
        {
            m_characterController = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            m_rootMotionRelay.OnRootMotionMoved += ApplyRootMotion;
        }

        private void OnDisable()
        {
            m_rootMotionRelay.OnRootMotionMoved -= ApplyRootMotion;
        }

        private void Update()
        {
            UpdateDesiredDirection();
        }

        private void UpdateDesiredDirection()
        {
            Vector2 input = m_inputReader.MoveInput;
            bool isBelowDeadzone = input.magnitude < m_inputBasis.InputDeadzone;
            DesiredDirection = isBelowDeadzone
                ? Vector3.zero
                : Vector3.ClampMagnitude(m_inputBasis.ToWorldDirection(input), k_maximumInputMagnitude);
        }

        // Runs from OnAnimatorMove, after Update, once per animation step.
        private void ApplyRootMotion(Vector3 rootMotionDelta)
        {
            UpdateVerticalVelocity(Time.deltaTime);

            Vector3 horizontalDisplacement = Vector3.ProjectOnPlane(rootMotionDelta, Vector3.up);
            Vector3 verticalDisplacement = Vector3.up * (m_verticalVelocity * Time.deltaTime);
            m_characterController.Move(horizontalDisplacement + verticalDisplacement);
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
