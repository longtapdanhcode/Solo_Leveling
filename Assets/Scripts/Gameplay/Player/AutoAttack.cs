// Implements: design/game-brief.md MVP 1 (auto-attack: nearest target, combo of 3, crit, hit-stop), story-001.

using System;
using System.Collections.Generic;
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

        private ComboStateMachine _combo;
        private DamageableQuery _query;
        private readonly List<IDamageable> _scratch = new List<IDamageable>();
        private Func<float> _roll = () => Random.value;
        private Vector3 _aimDirection = Vector3.forward;

        /// <summary>Raised for every target damaged by a swing: (target, damage packet).</summary>
        public event Action<IDamageable, DamageInfo> OnHitLanded;

        /// <summary>Current combo state, for feedback.</summary>
        public ComboState State => _combo.State;
        /// <summary>Zero-based combo step, for feedback.</summary>
        public int CurrentStep => _combo.CurrentStep;

        /// <summary>Injects dependencies when building the object at runtime. Call before the object is activated.</summary>
        public void Configure(PlayerConfig config, PlayerController player, Health health, HitStop hitStop, LayerMask enemyLayers)
        {
            _config = config;
            _player = player;
            _health = health;
            _hitStop = hitStop;
            _enemyLayers = enemyLayers;
        }

        /// <summary>Replaces the crit random source (0..1) so tests can be deterministic.</summary>
        public void SetRandomSource(Func<float> roll)
        {
            _roll = roll;
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
            _query = new DamageableQuery();
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
            _query.Gather(transform.position, _config.DetectionRange, _enemyLayers, transform, _scratch);
            IDamageable target = TargetSelector.FindNearest(transform.position, _scratch, _config.DetectionRange);

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
            bool critical = _roll() < _config.CritChance;
            float damage = data.Damage * (critical ? _config.CritMultiplier : 1f);
            float halfArc = _config.SlashArcDegrees * 0.5f;
            Vector3 origin = transform.position;

            _query.Gather(origin, _config.SlashRadius, _enemyLayers, transform, _scratch);
            bool landed = false;
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
                    OnHitLanded?.Invoke(candidate, info);
                }
            }

            if (landed && _hitStop != null)
            {
                _hitStop.Request(_config.HitStopDuration);
            }
        }
    }
}
