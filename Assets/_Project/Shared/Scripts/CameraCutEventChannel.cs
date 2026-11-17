using System;
using UnityEngine;

namespace FixedCameraPrototype.Shared
{
    // Decouples camera features from player features: neither references the other, both reference this channel.
    [CreateAssetMenu(menuName = "FixedCameraPrototype/Camera Cut Event Channel", fileName = "CameraCutEventChannel")]
    public class CameraCutEventChannel : ScriptableObject
    {
        public event Action OnCameraCut;

        public void RaiseCameraCut()
        {
            OnCameraCut?.Invoke();
        }
    }
}
