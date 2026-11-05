// Implements: design/game-brief.md MVP feature 5 (Soul Burst: rage bar + Q shockwave), production/epics/micro-arena-survivor/story-005-soul-burst.md.
//
// Single source of every Soul Burst tuning value. The serialized defaults below ARE the confirmed initial values;
// designers tune them on the asset, never in code.

using UnityEngine;

namespace MicroArena.Gameplay.Config
{
    /// <summary>Tuning for Kaelen's Soul Burst. Create with the asset menu or <see cref="ScriptableObject.CreateInstance{T}()"/>.</summary>
    [CreateAssetMenu(fileName = "SoulBurstConfig", menuName = "Micro Arena Survivor/Soul Burst Config")]
    public sealed class SoulBurstConfig : ScriptableObject
    {
        [Header("Rage")]
        [Tooltip("Rage gained for every enemy death (any cause), on the 0..MaxRage scale. 5 = 5% of the bar.")]
        [SerializeField, Min(0f)] private float _ragePerKill = 5f;

        [Tooltip("Rage needed to fire the burst; the bar never exceeds it.")]
        [SerializeField, Min(1f)] private float _maxRage = 100f;

        [Header("Shockwave")]
        [Tooltip("Flat damage dealt to each enemy hit. No card modifiers, no critical hit.")]
        [SerializeField, Min(0f)] private float _damage = 120f;

        [Tooltip("Radius in metres around Kaelen.")]
        [SerializeField, Min(0.1f)] private float _shockwaveRadius = 6f;

        [Tooltip("Distance in metres enemies are pushed away from Kaelen. Bosses are never pushed.")]
        [SerializeField, Min(0f)] private float _knockbackDistance = 4f;

        [Header("Ring visual (story 007 replaces this)")]
        [Tooltip("Seconds the expanding ring takes to reach the full radius and fade out.")]
        [SerializeField, Min(0.05f)] private float _ringDuration = 0.5f;

        [Tooltip("Vertical offset from Kaelen's pivot to the floor where the ring is drawn (negative = down).")]
        [SerializeField] private float _ringHeightOffset = -0.95f;

        [Tooltip("Thickness of the flat ring disc in metres.")]
        [SerializeField, Min(0.01f)] private float _ringThickness = 0.05f;

        /// <summary>Rage gained per enemy death.</summary>
        public float RagePerKill => _ragePerKill;
        /// <summary>Rage needed to fire (and the bar's cap).</summary>
        public float MaxRage => _maxRage;
        /// <summary>Flat damage per enemy hit.</summary>
        public float Damage => _damage;
        /// <summary>Shockwave radius in metres.</summary>
        public float ShockwaveRadius => _shockwaveRadius;
        /// <summary>Knockback distance in metres for non-boss enemies.</summary>
        public float KnockbackDistance => _knockbackDistance;
        /// <summary>Seconds the ring takes to expand and fade.</summary>
        public float RingDuration => _ringDuration;
        /// <summary>Vertical offset from Kaelen's pivot to the ring.</summary>
        public float RingHeightOffset => _ringHeightOffset;
        /// <summary>Ring disc thickness in metres.</summary>
        public float RingThickness => _ringThickness;
    }
}
