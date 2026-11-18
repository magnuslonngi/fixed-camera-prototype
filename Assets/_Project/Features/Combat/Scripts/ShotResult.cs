using UnityEngine;

namespace FixedCameraPrototype.Combat
{
    public readonly struct ShotResult
    {
        public ShotResult(Vector3 origin, Vector3 rawDirection, Vector3 finalDirection, Vector3 endPoint, bool didHit)
        {
            Origin = origin;
            RawDirection = rawDirection;
            FinalDirection = finalDirection;
            EndPoint = endPoint;
            DidHit = didHit;
        }

        public Vector3 Origin { get; }
        public Vector3 RawDirection { get; }
        public Vector3 FinalDirection { get; }
        public Vector3 EndPoint { get; }
        public bool DidHit { get; }
    }
}
