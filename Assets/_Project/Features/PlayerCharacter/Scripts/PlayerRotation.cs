using UnityEngine;

namespace FixedCameraPrototype.PlayerCharacter
{
    public class PlayerRotation : MonoBehaviour
    {
        [SerializeField] private PlayerMovement m_playerMovement;
        [SerializeField] private float m_turnSpeedDegreesPerSecond = 540.0f;

        private void Update()
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
                m_turnSpeedDegreesPerSecond * Time.deltaTime);
        }
    }
}
