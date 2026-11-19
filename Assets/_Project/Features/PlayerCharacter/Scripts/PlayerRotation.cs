using UnityEngine;

namespace FixedCameraPrototype.PlayerCharacter
{
    public class PlayerRotation : MonoBehaviour
    {
        [SerializeField] private PlayerMovement m_playerMovement;
        [SerializeField] private float m_turnSpeedDegreesPerSecond = 540.0f;
        [SerializeField] private float m_tankTurnSpeedDegreesPerSecond = 150.0f;

        private void Update()
        {
            // Tank turn takes priority so it is not cancelled by the faster camera-relative facing while WASD is held.
            if (HasTankTurnInput())
            {
                ApplyTankTurn(Time.deltaTime);
                return;
            }

            ApplyCameraRelativeTurn(Time.deltaTime);
        }

        private bool HasTankTurnInput()
        {
            return !Mathf.Approximately(m_playerMovement.TankTurnInput, 0.0f);
        }

        private void ApplyTankTurn(float deltaTime)
        {
            float turnDegrees = m_playerMovement.TankTurnInput * m_tankTurnSpeedDegreesPerSecond * deltaTime;
            transform.Rotate(Vector3.up, turnDegrees, Space.World);
        }

        private void ApplyCameraRelativeTurn(float deltaTime)
        {
            Vector3 desiredDirection = m_playerMovement.DesiredDirection;
            if (desiredDirection.sqrMagnitude <= 0.0f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                m_turnSpeedDegreesPerSecond * deltaTime);
        }
    }
}
