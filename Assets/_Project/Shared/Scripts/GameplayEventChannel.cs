using System;
using UnityEngine;

namespace FixedCameraPrototype.Shared
{
    // Lets one feature announce a level moment (a pickup, an enemy alert) that another feature reacts to,
    // without either referencing the other. Each moment is its own channel asset.
    [CreateAssetMenu(menuName = "FixedCameraPrototype/Gameplay Event Channel", fileName = "GameplayEventChannel")]
    public class GameplayEventChannel : ScriptableObject
    {
        public event Action OnRaised;

        public void Raise()
        {
            OnRaised?.Invoke();
        }
    }
}
