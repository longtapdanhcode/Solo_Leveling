// Implements: design/game-brief.md MVP 1 ("Giá trị ban đầu đã chốt"), design/narrative/kich_ban_chuan.md section 3,
// production/epics/micro-arena-survivor/story-001-kaelen-core-combat.md.
//
// Single source of every Kaelen tuning value. The serialized defaults below ARE the initial design values;
// designers tune them on the asset, never in code. All values are initial and expected to be rebalanced after playtests.

using System;
using UnityEngine;

namespace MicroArena.Gameplay.Config
{
    /// <summary>Damage and effects of one swing of the three-hit combo.</summary>
    [Serializable]
    public struct ComboStep
    {
        /// <summary>Base damage before critical multiplier.</summary>
        public float Damage;

        /// <summary>Distance in metres the target is pushed away. 0 = no knockback.</summary>
        public float KnockbackDistance;

        /// <summary>Seconds the target is stunned. 0 = no stun.</summary>
        public float StunDuration;
    }

    /// <summary>
    /// Tuning data for Kaelen (movement, health/revive, auto-attack combo, dash, hit-stop).
    /// Create instances with the asset menu or <see cref="ScriptableObject.CreateInstance{T}()"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Micro Arena Survivor/Player Config")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Run speed in metres per second.")]
        [SerializeField, Min(0f)] private float _moveSpeed = 7f;

        [Header("Health and revive")]
        [SerializeField, Min(1f)] private float _maxHealth = 100f;

        [Tooltip("Fraction of max HP restored by the once-per-run revive.")]
        [SerializeField, Range(0.01f, 1f)] private float _reviveHealthFraction = 0.5f;

        [Tooltip("Seconds of invulnerability after the revive.")]
        [SerializeField, Min(0f)] private float _reviveInvulnerability = 2f;

        // [PLACEHOLDER] Design only says "knock back nearby enemies"; radius not specified.
        [Tooltip("[PLACEHOLDER] Radius (m) of the revive shock that pushes enemies away.")]
        [SerializeField, Min(0f)] private float _reviveKnockbackRadius = 4f;

        // [PLACEHOLDER] Push distance of the revive shock is not specified.
        [Tooltip("[PLACEHOLDER] Distance (m) enemies are pushed by the revive shock.")]
        [SerializeField, Min(0f)] private float _reviveKnockbackDistance = 4f;

        [Header("Auto-attack")]
        [Tooltip("Enemies within this range (m) can be auto-targeted. Separate from slash radius on purpose.")]
        [SerializeField, Min(0f)] private float _detectionRange = 3f;

        // [PLACEHOLDER] Effective slash reach is not specified (a later card raises only this, not detection).
        [Tooltip("[PLACEHOLDER] Reach (m) of the slash from Kaelen. Defaults to the detection range.")]
        [SerializeField, Min(0f)] private float _slashRadius = 3f;

        // [PLACEHOLDER] Slash shape is not specified.
        [Tooltip("[PLACEHOLDER] Total arc (degrees) in front of the aim direction that a swing hits. 360 = full circle.")]
        [SerializeField, Range(1f, 360f)] private float _slashArcDegrees = 120f;

        [Tooltip("Seconds per swing.")]
        [SerializeField, Min(0.01f)] private float _swingDuration = 0.4f;

        [Tooltip("Seconds of rest after the last swing of the combo.")]
        [SerializeField, Min(0f)] private float _restAfterCombo = 0.3f;

        // [PLACEHOLDER] The moment inside a swing when damage lands is not specified.
        [Tooltip("[PLACEHOLDER] Point of the swing (0..1 of swing duration) at which damage is applied.")]
        [SerializeField, Range(0f, 1f)] private float _swingHitFraction = 0.5f;

        // [PLACEHOLDER] Step 1 knockback ("light", not specified) = 1 m. Steps 2 has none; step 3 is 4 m + 0.3 s stun.
        [Tooltip("Combo loop. Step 1 knockback 1 m is a [PLACEHOLDER] (design only says 'light').")]
        [SerializeField] private ComboStep[] _comboSteps =
        {
            new ComboStep { Damage = 20f, KnockbackDistance = 1f, StunDuration = 0f },
            new ComboStep { Damage = 25f, KnockbackDistance = 0f, StunDuration = 0f },
            new ComboStep { Damage = 40f, KnockbackDistance = 4f, StunDuration = 0.3f },
        };

        // [PLACEHOLDER] Combo returns to step 1 when the target is lost: implemented in ComboStateMachine (no config).

        [Tooltip("Critical hit chance, 0..1.")]
        [SerializeField, Range(0f, 1f)] private float _critChance = 0.05f;

        [Tooltip("Damage multiplier on a critical hit.")]
        [SerializeField, Min(1f)] private float _critMultiplier = 1.5f;

        [Header("Dash")]
        [Tooltip("Distance of one dash in metres.")]
        [SerializeField, Min(0f)] private float _dashDistance = 5f;

        // [PLACEHOLDER] Dash travel time is not specified (only the 0.18 s i-frames).
        [Tooltip("[PLACEHOLDER] Seconds a dash takes to cover its distance.")]
        [SerializeField, Min(0.01f)] private float _dashDuration = 0.2f;

        [SerializeField, Min(1)] private int _dashMaxCharges = 2;

        [Tooltip("Seconds to regain one dash charge.")]
        [SerializeField, Min(0.01f)] private float _dashRechargeTime = 2f;

        [Tooltip("Invulnerability seconds at the start of a dash.")]
        [SerializeField, Min(0f)] private float _dashInvulnerability = 0.18f;

        [Header("Game feel")]
        [Tooltip("Seconds the game freezes when a slash lands.")]
        [SerializeField, Min(0f)] private float _hitStopDuration = 0.06f;

        /// <summary>Run speed, m/s.</summary>
        public float MoveSpeed => _moveSpeed;
        /// <summary>Maximum HP.</summary>
        public float MaxHealth => _maxHealth;
        /// <summary>Fraction of max HP restored on revive.</summary>
        public float ReviveHealthFraction => _reviveHealthFraction;
        /// <summary>Invulnerability seconds after revive.</summary>
        public float ReviveInvulnerability => _reviveInvulnerability;
        /// <summary>Radius of the revive shock.</summary>
        public float ReviveKnockbackRadius => _reviveKnockbackRadius;
        /// <summary>Push distance of the revive shock.</summary>
        public float ReviveKnockbackDistance => _reviveKnockbackDistance;
        /// <summary>Auto-target detection range, m.</summary>
        public float DetectionRange => _detectionRange;
        /// <summary>Slash reach, m.</summary>
        public float SlashRadius => _slashRadius;
        /// <summary>Total slash arc, degrees.</summary>
        public float SlashArcDegrees => _slashArcDegrees;
        /// <summary>Seconds per swing.</summary>
        public float SwingDuration => _swingDuration;
        /// <summary>Rest seconds after the last combo swing.</summary>
        public float RestAfterCombo => _restAfterCombo;
        /// <summary>Fraction of the swing at which damage lands.</summary>
        public float SwingHitFraction => _swingHitFraction;
        /// <summary>Combo steps in order; the combo loops over them.</summary>
        public ComboStep[] ComboSteps => _comboSteps;
        /// <summary>Critical hit chance, 0..1.</summary>
        public float CritChance => _critChance;
        /// <summary>Critical damage multiplier.</summary>
        public float CritMultiplier => _critMultiplier;
        /// <summary>Dash distance, m.</summary>
        public float DashDistance => _dashDistance;
        /// <summary>Dash travel time, s.</summary>
        public float DashDuration => _dashDuration;
        /// <summary>Stored dash charges.</summary>
        public int DashMaxCharges => _dashMaxCharges;
        /// <summary>Seconds to regain one charge.</summary>
        public float DashRechargeTime => _dashRechargeTime;
        /// <summary>Dash i-frame seconds.</summary>
        public float DashInvulnerability => _dashInvulnerability;
        /// <summary>Hit-stop seconds.</summary>
        public float HitStopDuration => _hitStopDuration;
    }
}
