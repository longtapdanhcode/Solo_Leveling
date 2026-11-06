// Implements: design/game-brief.md MVP 2 (top-down 3D arena), story-003. Same framing as the story-001 sandbox.
// Story 007: screen shake is an offset added here, to the camera only.

using UnityEngine;

namespace MicroArena.Gameplay.Arena
{
    /// <summary>High-angle camera that keeps a fixed offset from Kaelen and looks at him, plus an optional shake offset.</summary>
    public sealed class FollowCamera : MonoBehaviour
    {
        [Tooltip("Kaelen.")]
        [SerializeField] private Transform _target;
        [Tooltip("Camera position relative to the target.")]
        [SerializeField] private Vector3 _offset = new Vector3(0f, 14f, -5f);

        private Vector3 _shakeOffset;

        /// <summary>The shake offset currently applied (zero when not shaking).</summary>
        public Vector3 ShakeOffset => _shakeOffset;

        /// <summary>Injects the followed transform when building the scene.</summary>
        public void Configure(Transform target)
        {
            _target = target;
        }

        /// <summary>
        /// Sets the screen shake offset (story 007). It moves the camera only: the look-at point moves with it, so the view
        /// translates without tilting, and the target is never touched.
        /// </summary>
        public void SetShakeOffset(Vector3 offset)
        {
            _shakeOffset = offset;
        }

        private void LateUpdate()
        {
            Vector3 focus = _target.position + _shakeOffset;
            transform.position = focus + _offset;
            transform.LookAt(focus);
        }
    }
}
