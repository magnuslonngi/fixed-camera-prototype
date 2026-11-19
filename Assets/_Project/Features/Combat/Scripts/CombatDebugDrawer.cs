using FixedCameraPrototype.Shared;
using UnityEngine;

namespace FixedCameraPrototype.Combat
{
    // Temporary stand-in for weapon effects until animation and VFX exist.
    public class CombatDebugDrawer : MonoBehaviour
    {
        private const int k_arcSegmentCount = 12;
        private const float k_bendArcRadiusMeters = 2.0f;
        private const float k_minimumVisibleBendDegrees = 0.01f;
        private const float k_previewFloorOffsetMeters = 0.05f;
        private const float k_previewMarkerRadiusMeters = 0.3f;
        private const float k_previewBodyRayMeters = 1.5f;

        private static readonly Color s_coneColor = Color.yellow;
        private static readonly Color s_rawAimColor = Color.white;
        private static readonly Color s_assistColor = Color.cyan;
        private static readonly Color s_shotHitColor = Color.red;
        private static readonly Color s_shotMissColor = Color.grey;
        private static readonly Color s_snapConeColor = Color.green;
        private static readonly Color s_snapTargetColor = Color.magenta;
        private static readonly Color s_snapBodyColor = Color.blue;
        private static readonly Color s_snapLineOfFireColor = new Color(1.0f, 0.5f, 0.0f);
        private static readonly Color s_snapUnsolvedColor = Color.red;

        [SerializeField] private PlayerAim m_playerAim;
        [SerializeField] private AimAssist m_aimAssist;
        [SerializeField] private PlayerWeapon m_playerWeapon;
        [SerializeField] private float m_shotLineSeconds = 1.5f;
        [SerializeField] private bool m_isSnapPreviewEnabled = true;

        private ShotResult m_lastShotResult;
        private float m_lastShotExpiryTime;

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
            else if (m_isSnapPreviewEnabled)
            {
                DrawSnapPreview();
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Play Mode draws only the cone for the current aim state in Update.
            if (m_aimAssist == null || Application.isPlaying)
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
            m_lastShotResult = shotResult;
            m_lastShotExpiryTime = Time.time + m_shotLineSeconds;
        }

        // NOTE(Design): Redrawn per frame instead of using a Debug.DrawLine duration, which cannot be
        // cancelled and would leave the trace visible after aiming ends.
        private void DrawLastShot()
        {
            if (Time.time >= m_lastShotExpiryTime)
            {
                return;
            }

            Color shotColor = m_lastShotResult.DidHit ? s_shotHitColor : s_shotMissColor;
            Debug.DrawLine(m_lastShotResult.Origin, m_lastShotResult.EndPoint, shotColor);
        }

        private void DrawSnapPreview()
        {
            Vector3 floorOrigin = transform.position + (Vector3.up * k_previewFloorOffsetMeters);
            DrawAcquisitionCone(floorOrigin);

            if (!m_playerAim.TryFindSnapTarget(out IAimAssistTarget target))
            {
                return;
            }

            Vector3 floorTarget = new Vector3(target.Position.x, floorOrigin.y, target.Position.z);
            if (!m_playerAim.TryCalculateSnapRotation(target.Position, out Quaternion snapRotation))
            {
                DrawPreviewLine(floorOrigin, floorTarget, s_snapUnsolvedColor);
                DrawFloorMarker(floorTarget, s_snapUnsolvedColor);
                return;
            }

            DrawPreviewLine(floorOrigin, floorTarget, s_snapTargetColor);
            DrawFloorMarker(floorTarget, s_snapTargetColor);
            DrawPredictedSnapPose(floorOrigin, floorTarget, snapRotation);
        }

        private void DrawAcquisitionCone(Vector3 floorOrigin)
        {
            float halfAngleDegrees = m_aimAssist.AcquisitionHalfAngleDegrees;
            float rangeMeters = m_aimAssist.RangeMeters;
            Vector3 previousPoint = floorOrigin + (Quaternion.AngleAxis(-halfAngleDegrees, Vector3.up) * transform.forward * rangeMeters);
            DrawPreviewLine(floorOrigin, previousPoint, s_snapConeColor);

            for (int i = 1; i <= k_arcSegmentCount; i++)
            {
                float arcProgress = (float)i / k_arcSegmentCount;
                float arcAngleDegrees = Mathf.Lerp(-halfAngleDegrees, halfAngleDegrees, arcProgress);
                Vector3 arcPoint = floorOrigin + (Quaternion.AngleAxis(arcAngleDegrees, Vector3.up) * transform.forward * rangeMeters);
                DrawPreviewLine(previousPoint, arcPoint, s_snapConeColor);
                previousPoint = arcPoint;
            }

            DrawPreviewLine(floorOrigin, previousPoint, s_snapConeColor);
        }

        private void DrawPredictedSnapPose(Vector3 floorOrigin, Vector3 floorTarget, Quaternion snapRotation)
        {
            Quaternion rotationToSnap = snapRotation * Quaternion.Inverse(transform.rotation);
            Vector3 muzzleOffset = m_playerWeapon.MuzzlePosition - transform.position;
            Vector3 predictedMuzzle = transform.position + (rotationToSnap * muzzleOffset);
            Vector3 predictedFloorMuzzle = new Vector3(predictedMuzzle.x, floorOrigin.y, predictedMuzzle.z);

            DrawPreviewLine(floorOrigin, floorOrigin + ((snapRotation * Vector3.forward) * k_previewBodyRayMeters), s_snapBodyColor);
            DrawPreviewLine(predictedFloorMuzzle, floorTarget, s_snapLineOfFireColor);
        }

        private static void DrawFloorMarker(Vector3 center, Color color)
        {
            DrawPreviewLine(center + (Vector3.left * k_previewMarkerRadiusMeters), center + (Vector3.right * k_previewMarkerRadiusMeters), color);
            DrawPreviewLine(center + (Vector3.back * k_previewMarkerRadiusMeters), center + (Vector3.forward * k_previewMarkerRadiusMeters), color);
        }

        // NOTE(Design): Depth testing is off because these lines lie almost on the floor and would fade into it.
        private static void DrawPreviewLine(Vector3 start, Vector3 end, Color color)
        {
            Debug.DrawLine(start, end, color, 0.0f, false);
        }

        private void DrawLiveAim()
        {
            DrawLastShot();

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
            Vector3 flatAimDirection = Vector3.ProjectOnPlane(m_playerAim.AimDirection, Vector3.up).normalized;
            Vector3 leftEdge = Quaternion.AngleAxis(-halfAngleDegrees, Vector3.up) * flatAimDirection;
            Vector3 rightEdge = Quaternion.AngleAxis(halfAngleDegrees, Vector3.up) * flatAimDirection;
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
