using UnityEngine;

namespace FixedCameraPrototype.Combat
{
    // Temporary stand-in for weapon effects until animation and VFX exist.
    public class CombatDebugDrawer : MonoBehaviour
    {
        private const int k_arcSegmentCount = 12;
        private const float k_bendArcRadiusMeters = 2.0f;
        private const float k_minimumVisibleBendDegrees = 0.01f;

        private static readonly Color s_coneColor = Color.yellow;
        private static readonly Color s_rawAimColor = Color.white;
        private static readonly Color s_assistColor = Color.cyan;
        private static readonly Color s_shotHitColor = Color.red;
        private static readonly Color s_shotMissColor = Color.grey;

        [SerializeField] private PlayerAim m_playerAim;
        [SerializeField] private AimAssist m_aimAssist;
        [SerializeField] private PlayerWeapon m_playerWeapon;
        [SerializeField] private float m_shotLineSeconds = 1.5f;

        private void OnEnable()
        {
            m_playerWeapon.OnShotFired += HandleShotFired;
        }

        private void OnDisable()
        {
            m_playerWeapon.OnShotFired -= HandleShotFired;
        }

        private void Update()
        {
            if (m_playerAim.IsAiming)
            {
                DrawLiveAim();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (m_aimAssist == null)
            {
                return;
            }

            Vector3 origin = transform.position;
            Gizmos.color = s_coneColor;
            DrawGizmoConeEdges(origin, m_aimAssist.ShotAssistHalfAngleDegrees);
            Gizmos.color = s_assistColor;
            DrawGizmoConeEdges(origin, m_aimAssist.AcquisitionHalfAngleDegrees);
        }

        private void HandleShotFired(ShotResult shotResult)
        {
            Color shotColor = shotResult.DidHit ? s_shotHitColor : s_shotMissColor;
            Debug.DrawLine(shotResult.Origin, shotResult.EndPoint, shotColor, m_shotLineSeconds);
        }

        private void DrawLiveAim()
        {
            Vector3 origin = m_playerWeapon.MuzzlePosition;
            Vector3 rawDirection = m_playerAim.AimDirection;
            float rangeMeters = m_playerWeapon.RangeMeters;

            DrawConeEdges(origin, m_aimAssist.ShotAssistHalfAngleDegrees);
            Debug.DrawRay(origin, rawDirection * rangeMeters, s_rawAimColor);

            Vector3 assistedDirection = m_aimAssist.ResolveShotDirection(origin, rawDirection, m_playerAim.CurrentTier.ToHitZone());
            if (Vector3.Angle(rawDirection, assistedDirection) > k_minimumVisibleBendDegrees)
            {
                Debug.DrawRay(origin, assistedDirection * rangeMeters, s_assistColor);
                DrawBendArc(origin, rawDirection, assistedDirection);
            }
        }

        private void DrawConeEdges(Vector3 origin, float halfAngleDegrees)
        {
            Vector3 leftEdge = Quaternion.AngleAxis(-halfAngleDegrees, Vector3.up) * transform.forward;
            Vector3 rightEdge = Quaternion.AngleAxis(halfAngleDegrees, Vector3.up) * transform.forward;
            Debug.DrawRay(origin, leftEdge * m_aimAssist.RangeMeters, s_coneColor);
            Debug.DrawRay(origin, rightEdge * m_aimAssist.RangeMeters, s_coneColor);
        }

        private void DrawBendArc(Vector3 origin, Vector3 rawDirection, Vector3 assistedDirection)
        {
            Vector3 previousPoint = origin + (rawDirection * k_bendArcRadiusMeters);
            for (int i = 1; i <= k_arcSegmentCount; i++)
            {
                float arcProgress = (float)i / k_arcSegmentCount;
                Vector3 arcDirection = Vector3.Slerp(rawDirection, assistedDirection, arcProgress);
                Vector3 arcPoint = origin + (arcDirection * k_bendArcRadiusMeters);
                Debug.DrawLine(previousPoint, arcPoint, s_assistColor);
                previousPoint = arcPoint;
            }
        }

        private void DrawGizmoConeEdges(Vector3 origin, float halfAngleDegrees)
        {
            Vector3 leftEdge = Quaternion.AngleAxis(-halfAngleDegrees, Vector3.up) * transform.forward;
            Vector3 rightEdge = Quaternion.AngleAxis(halfAngleDegrees, Vector3.up) * transform.forward;
            Gizmos.DrawRay(origin, leftEdge * m_aimAssist.RangeMeters);
            Gizmos.DrawRay(origin, rightEdge * m_aimAssist.RangeMeters);
        }
    }
}
