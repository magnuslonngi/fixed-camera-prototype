using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.PlayerCharacter
{
    public class PlayerPursuitTarget : MonoBehaviour, IPursuitTarget
    {
        [SerializeField] private Health m_health;
        [SerializeField] private PlayerHitInvulnerability m_damageGate;
        [SerializeField] private Transform m_bodyRoot;
        [Tooltip("Point enemies must see to detect the player, e.g. the chest bone.")]
        [SerializeField] private Transform m_lineOfSightAnchor;

        public bool IsTargetable => !m_health.IsDead;
        public Vector3 Position => m_bodyRoot.position;
        public Vector3 LineOfSightPoint => m_lineOfSightAnchor.position;
        public IDamageable Damageable => m_damageGate;

        private void OnEnable()
        {
            PursuitTargetRegistry.Register(this);
        }

        private void OnDisable()
        {
            PursuitTargetRegistry.Unregister(this);
        }
    }
}
