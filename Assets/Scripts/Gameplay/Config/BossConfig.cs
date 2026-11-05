// Implements: design/narrative/kich_ban_chuan.md sections 4 and 5.4, design/game-brief.md MVP 6,
// production/epics/micro-arena-survivor/story-006-bosses.md.
//
// Single source of every boss tuning value (both bosses, one asset). The serialized defaults below ARE the confirmed
// initial values; fields marked [PLACEHOLDER] are not specified by the design and are expected to be rebalanced after
// playtests. Designers tune them on the asset, never in code.

using UnityEngine;

namespace MicroArena.Gameplay.Config
{
    /// <summary>Tuning for the Gargoyle Tyrant (Wave 5) and the Void Warden (Wave 10).</summary>
    [CreateAssetMenu(fileName = "BossConfig", menuName = "Micro Arena Survivor/Boss Config")]
    public sealed class BossConfig : ScriptableObject
    {
        [Header("Shared")]
        [Tooltip("Every dangerous attack shows its red area on the floor for at least this many seconds before it deals damage.")]
        [SerializeField, Min(0f)] private float _minTelegraphSeconds = 1f;
        [Tooltip("Seconds between the end of one attack and the start of the next.")]
        [SerializeField, Min(0f)] private float _attackCooldown = 3f;
        // [PLACEHOLDER] Chase stop distance is not specified.
        [Tooltip("[PLACEHOLDER] A boss stops chasing within this many metres of its target.")]
        [SerializeField, Min(0f)] private float _stopDistance = 1.5f;
        // Visual only: story 007 refines the look.
        [Tooltip("Visual only. Height (m) of the red area above the floor, so it does not flicker with the floor.")]
        [SerializeField, Min(0f)] private float _telegraphHeight = 0.03f;
        [Tooltip("Visual only. Alpha of a telegraph the moment it appears; rises to the end alpha as it resolves.")]
        [SerializeField, Range(0f, 1f)] private float _telegraphStartAlpha = 0.25f;
        [Tooltip("Visual only. Alpha of a telegraph the moment before it resolves.")]
        [SerializeField, Range(0f, 1f)] private float _telegraphEndAlpha = 0.7f;
        [Tooltip("Visual only. Edge segments of the fan / circle mesh.")]
        [SerializeField, Min(3)] private int _telegraphSegments = 48;

        [Header("Gargoyle Tyrant (Wave 5)")]
        [SerializeField, Min(1f)] private float _gargoyleMaxHealth = 600f;
        // [PLACEHOLDER] Speed is not specified ("heavy"); slower than Kaelen's 7 m/s.
        [Tooltip("[PLACEHOLDER] Run speed in metres per second.")]
        [SerializeField, Min(0f)] private float _gargoyleMoveSpeed = 3f;
        [Tooltip("Damage of the hammer slam, to Kaelen inside the red circle.")]
        [SerializeField, Min(0f)] private float _slamDamage = 25f;
        [Tooltip("Radius (m) of the red circle around the Gargoyle.")]
        [SerializeField, Min(0.1f)] private float _slamRadius = 4f;
        [Tooltip("Seconds the red circle shows before the slam lands. Never below Min Telegraph Seconds.")]
        [SerializeField, Min(0f)] private float _slamTelegraphSeconds = 1f;
        // [PLACEHOLDER] When the Gargoyle starts the slam is not specified.
        [Tooltip("[PLACEHOLDER] The slam starts when its target is within this many metres (and the cooldown is over).")]
        [SerializeField, Min(0f)] private float _slamTriggerRange = 3.5f;

        [Header("Void Warden (Wave 10)")]
        [SerializeField, Min(1f)] private float _wardenMaxHealth = 2000f;
        // [PLACEHOLDER] Speed is not specified.
        [Tooltip("[PLACEHOLDER] Run speed in metres per second.")]
        [SerializeField, Min(0f)] private float _wardenMoveSpeed = 3.5f;
        [Tooltip("Phase 2 starts once, when HP drops strictly below this fraction of max HP (0.5 = below 50%; exactly 50% is still phase 1).")]
        [SerializeField, Range(0.01f, 0.99f)] private float _phaseTwoThreshold = 0.5f;

        [Header("Void Warden - triple slash (tam doan tram)")]
        [Tooltip("Damage of each slash, to Kaelen inside the red fan.")]
        [SerializeField, Min(0f)] private float _slashDamage = 30f;
        [Tooltip("Slashes in one attack.")]
        [SerializeField, Min(1)] private int _slashCount = 3;
        [Tooltip("Seconds each red fan shows before its slash lands. Never below Min Telegraph Seconds.")]
        [SerializeField, Min(0f)] private float _slashTelegraphSeconds = 1f;
        // [PLACEHOLDER] Fan size and the pause between slashes are not specified.
        [Tooltip("[PLACEHOLDER] Length (m) of the red fan.")]
        [SerializeField, Min(0.1f)] private float _slashRange = 8f;
        [Tooltip("[PLACEHOLDER] Opening angle (degrees) of the red fan.")]
        [SerializeField, Range(1f, 359f)] private float _slashAngleDegrees = 90f;
        [Tooltip("[PLACEHOLDER] Seconds between one slash landing and the next fan appearing.")]
        [SerializeField, Min(0f)] private float _slashGapSeconds = 0.4f;

        [Header("Void Warden - pull and ground burst (hut vao tam, dam kiem no dat)")]
        // [PLACEHOLDER] Pull values are not specified. 3 m/s is below walking (7 m/s) and far below the dash (5 m in 0.2 s).
        [Tooltip("[PLACEHOLDER] Kaelen within this many metres of the Warden is pulled toward it (and the pull only starts when the target is this close).")]
        [SerializeField, Min(0.1f)] private float _pullRadius = 14f;
        [Tooltip("[PLACEHOLDER] Seconds the pull area is shown before the pull starts. Never below Min Telegraph Seconds.")]
        [SerializeField, Min(0f)] private float _pullWindupSeconds = 1.2f;
        [Tooltip("[PLACEHOLDER] Pull speed in metres per second, added to Kaelen's own movement. Keep well below the dash speed so a dash escapes it.")]
        [SerializeField, Min(0f)] private float _pullSpeed = 3f;
        [Tooltip("[PLACEHOLDER] Seconds the pull lasts.")]
        [SerializeField, Min(0f)] private float _pullDurationSeconds = 2f;
        [Tooltip("Damage of the ground burst, to Kaelen inside the red circle at the centre.")]
        [SerializeField, Min(0f)] private float _burstDamage = 40f;
        [Tooltip("Radius (m) of the red circle at the centre.")]
        [SerializeField, Min(0.1f)] private float _burstRadius = 4f;
        [Tooltip("Seconds the red circle shows before the burst. Never below Min Telegraph Seconds.")]
        [SerializeField, Min(0f)] private float _burstTelegraphSeconds = 1f;

        [Header("Void Warden - meteor rain (phase 2)")]
        [Tooltip("Damage of each meteor, to Kaelen inside its red circle.")]
        [SerializeField, Min(0f)] private float _meteorDamage = 20f;
        [Tooltip("Meteors per rain.")]
        [SerializeField, Min(1)] private int _meteorCount = 5;
        [Tooltip("Seconds between two rains (the first comes this long after phase 2 starts).")]
        [SerializeField, Min(0.1f)] private float _meteorIntervalSeconds = 8f;
        [Tooltip("Radius (m) of each meteor's red circle.")]
        [SerializeField, Min(0.1f)] private float _meteorRadius = 2f;
        // [PLACEHOLDER] Meteor warning time and scatter area are not specified.
        [Tooltip("[PLACEHOLDER] Seconds each red circle shows before the meteor lands. Never below Min Telegraph Seconds.")]
        [SerializeField, Min(0f)] private float _meteorTelegraphSeconds = 1.5f;
        [Tooltip("[PLACEHOLDER] Meteor points are picked uniformly in a square of this half side (m) around the arena centre.")]
        [SerializeField, Min(0.1f)] private float _meteorAreaHalfExtent = 16f;

        /// <summary>Shortest red-area warning of any attack, seconds.</summary>
        public float MinTelegraphSeconds => _minTelegraphSeconds;
        /// <summary>Seconds between attacks.</summary>
        public float AttackCooldown => _attackCooldown;
        /// <summary>Chase stop distance, metres.</summary>
        public float StopDistance => _stopDistance;
        /// <summary>Visual: height of the red area above the floor.</summary>
        public float TelegraphHeight => _telegraphHeight;
        /// <summary>Visual: alpha when a telegraph appears.</summary>
        public float TelegraphStartAlpha => _telegraphStartAlpha;
        /// <summary>Visual: alpha just before it resolves.</summary>
        public float TelegraphEndAlpha => _telegraphEndAlpha;
        /// <summary>Visual: edge segments of the telegraph mesh.</summary>
        public int TelegraphSegments => _telegraphSegments;

        /// <summary>Gargoyle HP.</summary>
        public float GargoyleMaxHealth => _gargoyleMaxHealth;
        /// <summary>Gargoyle run speed.</summary>
        public float GargoyleMoveSpeed => _gargoyleMoveSpeed;
        /// <summary>Hammer slam damage.</summary>
        public float SlamDamage => _slamDamage;
        /// <summary>Hammer slam radius, metres.</summary>
        public float SlamRadius => _slamRadius;
        /// <summary>Hammer slam warning, seconds (raw; use <see cref="EffectiveTelegraph"/> for the enforced value).</summary>
        public float SlamTelegraphSeconds => _slamTelegraphSeconds;
        /// <summary>Distance at which the slam starts.</summary>
        public float SlamTriggerRange => _slamTriggerRange;

        /// <summary>Void Warden HP.</summary>
        public float WardenMaxHealth => _wardenMaxHealth;
        /// <summary>Void Warden run speed.</summary>
        public float WardenMoveSpeed => _wardenMoveSpeed;
        /// <summary>HP fraction below which phase 2 starts.</summary>
        public float PhaseTwoThreshold => _phaseTwoThreshold;
        /// <summary>Damage per slash.</summary>
        public float SlashDamage => _slashDamage;
        /// <summary>Slashes per attack.</summary>
        public int SlashCount => _slashCount;
        /// <summary>Slash warning, seconds (raw).</summary>
        public float SlashTelegraphSeconds => _slashTelegraphSeconds;
        /// <summary>Fan length, metres.</summary>
        public float SlashRange => _slashRange;
        /// <summary>Fan opening angle, degrees.</summary>
        public float SlashAngleDegrees => _slashAngleDegrees;
        /// <summary>Pause between slashes, seconds.</summary>
        public float SlashGapSeconds => _slashGapSeconds;
        /// <summary>Pull reach, metres.</summary>
        public float PullRadius => _pullRadius;
        /// <summary>Pull wind-up, seconds (raw).</summary>
        public float PullWindupSeconds => _pullWindupSeconds;
        /// <summary>Pull speed, metres per second.</summary>
        public float PullSpeed => _pullSpeed;
        /// <summary>Pull duration, seconds.</summary>
        public float PullDurationSeconds => _pullDurationSeconds;
        /// <summary>Ground burst damage.</summary>
        public float BurstDamage => _burstDamage;
        /// <summary>Ground burst radius, metres.</summary>
        public float BurstRadius => _burstRadius;
        /// <summary>Ground burst warning, seconds (raw).</summary>
        public float BurstTelegraphSeconds => _burstTelegraphSeconds;
        /// <summary>Damage per meteor.</summary>
        public float MeteorDamage => _meteorDamage;
        /// <summary>Meteors per rain.</summary>
        public int MeteorCount => _meteorCount;
        /// <summary>Seconds between rains.</summary>
        public float MeteorIntervalSeconds => _meteorIntervalSeconds;
        /// <summary>Radius of each meteor circle, metres.</summary>
        public float MeteorRadius => _meteorRadius;
        /// <summary>Meteor warning, seconds (raw).</summary>
        public float MeteorTelegraphSeconds => _meteorTelegraphSeconds;
        /// <summary>Half side of the meteor scatter square, metres.</summary>
        public float MeteorAreaHalfExtent => _meteorAreaHalfExtent;

        /// <summary>The warning time actually used for an attack: <paramref name="seconds"/>, but never below <see cref="MinTelegraphSeconds"/>.</summary>
        public float EffectiveTelegraph(float seconds)
        {
            return Mathf.Max(seconds, _minTelegraphSeconds);
        }
    }
}
