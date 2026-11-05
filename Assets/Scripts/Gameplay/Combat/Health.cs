// Implements: design/game-brief.md MVP 1 + "Player goal & fail state", story-001 (HP, damage pipeline entry, revive shock).

using System;
using System.Collections.Generic;
using MicroArena.Gameplay.Config;
using UnityEngine;

namespace MicroArena.Gameplay.Combat
{
    /// <summary>
    /// Scene adapter for <see cref="HealthModel"/>. Kaelen's entry into the shared damage pipeline and the
    /// source of HP/revive/run-end events (the HUD subscribes; this class knows nothing about UI).
    /// </summary>
    public sealed class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private PlayerConfig _config;
        [Tooltip("Layers searched for enemies pushed away by the revive shock.")]
        [SerializeField] private LayerMask _enemyLayers = ~0;

        [Tooltip("Optional. A fresh instance with the default values is created when empty.")]
        [SerializeField] private CombatQueryConfig _queryConfig;

        private HealthModel _model;
        private DamageableQuery _query;
        private readonly List<IDamageable> _scratch = new List<IDamageable>();

        /// <summary>Raised when HP changes: (current, max).</summary>
        public event Action<float, float> OnHealthChanged;
        /// <summary>Raised when the revive triggers.</summary>
        public event Action OnRevived;
        /// <summary>Raised when HP reaches 0 with no revive left.</summary>
        public event Action OnRunEnded;

        /// <inheritdoc />
        public Transform Transform => transform;
        /// <inheritdoc />
        public bool IsAlive => _model.State == HealthState.Alive;
        /// <summary>Current HP.</summary>
        public float Current => _model.Current;
        /// <summary>Max HP.</summary>
        public float Max => _model.Max;
        /// <summary>True while damage is ignored (dash i-frames or revive).</summary>
        public bool IsInvulnerable => _model.IsInvulnerable;
        /// <summary>True while the revive is unused this run.</summary>
        public bool ReviveAvailable => _model.ReviveAvailable;

        /// <summary>Injects dependencies when building the object at runtime. Call before the object is activated.</summary>
        public void Configure(PlayerConfig config, LayerMask enemyLayers, CombatQueryConfig queryConfig = null)
        {
            _config = config;
            _enemyLayers = enemyLayers;
            if (queryConfig != null)
            {
                _queryConfig = queryConfig;
            }
        }

        private void Awake()
        {
            _model = new HealthModel(_config);
            if (_queryConfig == null)
            {
                _queryConfig = ScriptableObject.CreateInstance<CombatQueryConfig>();
            }
            _query = new DamageableQuery(_queryConfig.MaxQueryColliders);
            _model.OnHealthChanged += (current, max) => OnHealthChanged?.Invoke(current, max);
            _model.OnRevived += HandleRevived;
            _model.OnRunEnded += () => OnRunEnded?.Invoke();
        }

        private void Update()
        {
            _model.Tick(Time.deltaTime);
        }

        /// <inheritdoc />
        /// <remarks>Knockback and stun on Kaelen are not applied in story 001 (out of scope).</remarks>
        public bool TakeDamage(in DamageInfo info)
        {
            return _model.ApplyDamage(info.Amount);
        }

        /// <summary>Grants an invulnerability window (used by Dash).</summary>
        public void GrantInvulnerability(float seconds)
        {
            _model.GrantInvulnerability(seconds);
        }

        /// <summary>Sets the flat max HP bonus from cards (story 004). Does not heal.</summary>
        public void SetMaxHealthBonus(float bonus)
        {
            _model.SetMaxHealthBonus(bonus);
        }

        /// <summary>Heals up to max HP. Ignored when dead.</summary>
        /// <returns>True if any HP was restored.</returns>
        public bool Heal(float amount)
        {
            return _model.Heal(amount);
        }

        /// <summary>Drops the card max HP bonus, then restores HP and the revive for a new run (no stale bonus HP after a restart).</summary>
        public void ResetForNewRun()
        {
            _model.SetMaxHealthBonus(0f);
            _model.ResetForNewRun();
        }

        private void HandleRevived()
        {
            Vector3 origin = transform.position;
            // Damage can arrive at any point in the frame; make collider positions current before querying.
            Physics.SyncTransforms();
            _query.Gather(origin, _config.ReviveKnockbackRadius, _enemyLayers, transform, _scratch);
            foreach (IDamageable target in _scratch)
            {
                Vector3 away = target.Transform.position - origin;
                away.y = 0f;
                away = away.sqrMagnitude > 0f ? away.normalized : transform.forward;
                target.TakeDamage(new DamageInfo
                {
                    Amount = 0f,
                    KnockbackDirection = away,
                    KnockbackDistance = _config.ReviveKnockbackDistance,
                    StunDuration = 0f,
                });
            }
            OnRevived?.Invoke();
        }
    }
}
