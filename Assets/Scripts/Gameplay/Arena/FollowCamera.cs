// Implements: design/game-brief.md MVP 2 (top-down 3D arena), story-003. Same framing as the story-001 sandbox.

using UnityEngine;

namespace MicroArena.Gameplay.Arena
{
    /// <summary>High-angle camera that keeps a fixed offset from Kaelen and looks at him.</summary>
    public sealed class FollowCamera : MonoBehaviour
    {
        [Tooltip("Kaelen.")]
        [SerializeField] private Transform _target;
        [Tooltip("Camera position relative to the target.")]
        [SerializeField] private Vector3 _offset = new Vector3(0f, 14f, -5f);

        /// <summary>Injects the followed transform when building the scene.</summary>
        public void Configure(Transform target)
        {
            _target = target;
        }

        private void LateUpdate()
        {
            transform.position = _target.position + _offset;
            transform.LookAt(_target.position);
        }
    }
}
