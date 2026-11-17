using System.Collections.Generic;
using UnityEngine;

namespace FixedCameraPrototype.FixedCamera
{
    // Single authority for which shot is live. Most recently entered zone wins, so overlapping
    // zone seams never flicker between shots.
    public class CameraZoneDirector : MonoBehaviour
    {
        [SerializeField] private CameraZone[] m_zones;

        private readonly List<CameraZone> m_occupiedZones = new List<CameraZone>();

        private void OnEnable()
        {
            foreach (CameraZone zone in m_zones)
            {
                zone.OnTargetEntered += HandleZoneEntered;
                zone.OnTargetExited += HandleZoneExited;
            }
        }

        private void OnDisable()
        {
            foreach (CameraZone zone in m_zones)
            {
                zone.OnTargetEntered -= HandleZoneEntered;
                zone.OnTargetExited -= HandleZoneExited;
            }
        }

        private void HandleZoneEntered(CameraZone zone)
        {
            m_occupiedZones.Remove(zone);
            m_occupiedZones.Add(zone);
            ActivateMostRecentZone();
        }

        private void HandleZoneExited(CameraZone zone)
        {
            m_occupiedZones.Remove(zone);
            ActivateMostRecentZone();
        }

        private void ActivateMostRecentZone()
        {
            if (m_occupiedZones.Count == 0)
            {
                return;
            }

            m_occupiedZones[m_occupiedZones.Count - 1].ShotCamera.Prioritize();
        }
    }
}
