// Implements: design/game-brief.md MVP 1 (auto-attack: nearest target, combo of 3, crit, hit-stop), story-001.

using System;
using System.Collections.Generic;
using MicroArena.Gameplay.Cards;
using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MicroArena.Gameplay.Player
{
    /// <summary>
    /// Scene adapter for <see cref="ComboStateMachine"/>. Each frame it picks the NEAREST living damageable inside
    /// the detection range, turns Kaelen toward it while swinging, and applies each swing's damage through
    /// <see cref="IDamageable"/> to everything inside the slash radius and arc. No target in range = no attack.
    /// </summary>
    public sealed class AutoAttack : MonoBehaviour
    {
        [SerializeField] private PlayerConfig _config;
        [SerializeField] private PlayerController _player;
        [SerializeField] private Health _health;
        [Tooltip("Optional. When set, a landed hit freezes the game briefly.")]
        [SerializeField] private HitStop _hitStop;
        [Tooltip("Layers that contain enemies. Kaelen's own object is always skipped.")]
        [SerializeField] private LayerMask _enemyLayers = ~0;
        [Tooltip("Optional. A fresh instance with the default values is created when empty.")]
        [SerializeField] private CombatQueryConfig _queryConfig;

        private ComboStateMachine _combo;
        private DamageableQuery _query;
        private readonly List<IDamageable> _scratch = new List<IDamageable>();
        private Func<float> _roll = () => Random.value;
        private Vector3 _aimDirection = Vector3.forward;
        private ModifierStack _modifiers;
        private readonly List<IDamageable> _struck = new List<IDamageable>();

        /// <summary>Raised for every target damaged by a swing: (target, damage packet).</summary>
        public event Action<IDamageable, DamageInfo> OnHitLanded;
        /// <summary>Raised after every swing's damage: (zero-based step, targets the swing damaged). The list is reused; read it during the call.</summary>
        public event Action<int, IReadOnlyList<IDamageable>> OnSwingResolved;

        /// <summary>Current combo state, for feedback.</summary>
        public ComboState State => _combo.State;
        /// <summary>Zero-based combo step, for feedback.</summary>
        public int CurrentStep => _combo.CurrentStep;

        /// <summary>Injects dependencies when building the object at runtime. Call before the object is activated.</summary>
        public void Configure(PlayerConfig config, PlayerController player, Health health, HitStop hitStop, LayerMask enemyLayers, CombatQueryConfig queryConfig = null)
        {
            _config = config;
            _player = player;
            _health = health;
            _hitStop = hitStop;
            _enemyLayers = enemyLayers;
            if (queryConfig != null)
            {
                _queryConfig = queryConfig;
            }
        }

        /// <summary>Replaces the crit random source (0..1) so tests can be deterministic.</summary>
        public void SetRandomSource(Func<float> roll)
        {
            _roll = roll;
        }

        /// <summary>
        /// Sets the run's card modifiers (story 004): damage multiplier and crit (incl. Cuồng Nộ below its HP threshold) and slash radius. Detection
        /// range is never scaled. Null = no modifiers (PlayerSandbox).
        /// </summary>
        public void SetModifiers(ModifierStack modifiers)
        {
            _modifiers = modifiers;
        }

        /// <summary>Returns the combo to step 1 and idle.</summary>
        public void ResetForNewRun()
        {
            _combo.Reset();
            _player.ClearAimOverride();
        }

        private void Awake()
        {
            _combo = new ComboStateMachine(_config);
            if (_queryConfig == null)
            {
                _queryConfig = ScriptableObject.CreateInstance<CombatQueryConfig>();
            }
            _query = new DamageableQuery(_queryConfig.MaxQueryColliders);
            _combo.OnSwingHit += ApplySwing;
        }

        private void Update()
        {
            if (!_health.IsAlive)
            {
                _combo.Reset();
                _player.ClearAimOverride();
                return;
            }

            // Colliders move via transform without Rigidbodies and auto-sync is off: sync once before querying.
            Physics.SyncTransforms();
            float detectionRange = _modifiers != null ? _modifiers.DetectionRange(_config.DetectionRange) : _config.DetectionRange;
            _query.Gather(transform.position, detectionRange, _enemyLayers, transform, _scratch);
            IDamageable target = TargetSelector.FindNearest(transform.position, _scratch, detectionRange);

            if (target != null)
            {
                Vector3 toTarget = target.Transform.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 0f)
                {
                    _aimDirection = toTarget.normalized;
                }
            }
            else
            {
                _aimDirection = _player.FacingDirection;
            }

            _combo.Tick(Time.deltaTime, target != null);

            if (_combo.State == ComboState.Swinging)
            {
                _player.SetAimOverride(_aimDirection);
            }
            else
            {
                _player.ClearAimOverride();
            }
        }

        private void ApplySwing(int step)
        {
            ComboStep data = _config.ComboSteps[step];
            float healthFraction = _health.Max > 0f ? _health.Current / _health.Max : 1f; // Cuồng Nộ condition
            float critChance = _modifiers != null ? _modifiers.CritChanceAt(_config.CritChance, healthFraction) : _config.CritChance;
            bool critical = _roll() < critChance;
            float baseDamage = data.Damage * (_modifiers != null ? _modifiers.DamageMultiplierAt(healthFraction) : 1f);
            float damage = baseDamage * (critical ? _config.CritMultiplier : 1f);
            float halfArc = _config.SlashArcDegrees * 0.5f;
            float slashRadius = _modifiers != null ? _modifiers.SwingRadius(_config.SlashRadius) : _config.SlashRadius;
            Vector3 origin = transform.position;

            _query.Gather(origin, slashRadius, _enemyLayers, transform, _scratch);
            bool landed = false;
            _struck.Clear();
            foreach (IDamageable candidate in _scratch)
            {
                Vector3 offset = candidate.Transform.position - origin;
                offset.y = 0f;
                if (offset.sqrMagnitude > 0f && Vector3.Angle(_aimDirection, offset) > halfArc)
                {
                    continue;
                }

                var info = new DamageInfo
                {
                    Amount = damage,
                    IsCritical = critical,
                    KnockbackDirection = offset.sqrMagnitude > 0f ? offset.normalized : _aimDirection,
                    KnockbackDistance = data.KnockbackDistance,
                    StunDuration = data.StunDuration,
                };
                if (candidate.TakeDamage(info))
                {
                    landed = true;
                    _struck.Add(candidate);
                    OnHitLanded?.Invoke(candidate, info);
                }
            }

            if (landed && _hitStop != null)
            {
                _hitStop.Request(_config.HitStopDuration);
            }
            OnSwingResolved?.Invoke(step, _struck);
        }
    }
}
