using System;
using UnityEngine;

namespace FixedCameraPrototype.PlayerCharacter
{
    // OnAnimatorMove only fires on the Animator's own GameObject (the model child), so this relays
    // the root motion delta up to the CharacterController on the player root. Defining
    // OnAnimatorMove also stops Unity from moving the model child by itself.
    [RequireComponent(typeof(Animator))]
    public class RootMotionRelay : MonoBehaviour
    {
        private Animator m_animator;

        public event Action<Vector3> OnRootMotionMoved;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
        }

        private void OnAnimatorMove()
        {
            OnRootMotionMoved?.Invoke(m_animator.deltaPosition);
        }
    }
}
