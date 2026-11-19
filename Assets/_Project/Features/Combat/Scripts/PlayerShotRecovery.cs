using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Combat
{
    // Holds the character still (movement and aim turning) while the shot animation plays out.
    public class PlayerShotRecovery : MonoBehaviour
    {
        [SerializeField] private PlayerWeapon m_weapon;
        [SerializeField] private MovementLock m_movementLock;
        [SerializeField] private float m_recoverySeconds = 0.8f;

        private float m_recoveryEndTime;
        private bool m_isRecovering;

        public bool IsRecovering => m_isRecovering;

        private void OnEnable()
        {
            m_weapon.OnShotFired += HandleShotFired;
        }

        private void OnDisable()
        {
            m_weapon.OnShotFired -= HandleShotFired;
            EndRecovery();
        }

        private void Update()
        {
            if (m_isRecovering && Time.time >= m_recoveryEndTime)
            {
                EndRecovery();
            }
        }

        private void HandleShotFired(ShotResult shot)
        {
            m_recoveryEndTime = Time.time + m_recoverySeconds;
            if (m_isRecovering)
            {
                return;
            }

            m_isRecovering = true;
            m_movementLock.AddLock();
        }

        private void EndRecovery()
        {
            if (!m_isRecovering)
            {
                return;
            }

            m_isRecovering = false;
            m_movementLock.RemoveLock();
        }
    }
}
