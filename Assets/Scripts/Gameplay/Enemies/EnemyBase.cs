// Implements: design/narrative/kich_ban_chuan.md section 5, design/game-brief.md MVP 3,
// production/epics/micro-arena-survivor/story-002-enemies.md (shared HP, knockback, stun, death event).

using System;
using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using UnityEngine;

namespace MicroArena.Gameplay.Enemies
{
    /// <summary>Payload of <see cref="EnemyBase.OnDied"/>, consumed by waves, rage and cards.</summary>
    public struct EnemyDied
    {
        /// <summary>Kind of the enemy that died.</summary>
        public EnemyKind Kind;

        /// <summary>World position at the moment of death.</summary>
        public Vector3 Position;
    }

    /// <summary>
    /// Shared enemy body: HP, entry into the damage pipeline, knockback slide, stun and death.
    /// Subclasses only implement <see cref="Act"/>, which is skipped while stunned or once Kaelen is down.
    /// <para>Moves by transform on a kinematic <see cref="Rigidbody"/> so PhysX treats it as a moving body rather
    /// than rebuilding a static collider every frame. Auto Sync Transforms is off, so every overlap query that
    /// can see enemies must call <see cref="Physics.SyncTransforms"/> first (AutoAttack, Health, VolatileHusk do).</para>
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        [SerializeField] private EnemyConfig _config;
        [Tooltip("Kaelen. Enemies hurt him only through Health.TakeDamage.")]
        [SerializeField] private Health _target;
        [Tooltip("Optional. Used by enemies that query an area. A fresh default instance is created when empty.")]
        [SerializeField] private CombatQueryConfig _queryConfig;
        // Visual only: how long the knockback push is spread over (same as the story-001 dummy).
        [SerializeField, Min(0.01f)] private float _knockbackSlideSeconds = 0.15f;

        private Renderer _renderer;
        private Material _tintMaterial; // per-enemy copy, created on the first SetColor and destroyed with the enemy
        private float _health;
        private float _stunRemaining;
        private float _slideRemaining;
        private Vector3 _slideVelocity;

        /// <summary>Raised once when the enemy dies, just before its object is destroyed.</summary>
        public event Action<EnemyDied> OnDied;

        /// <inheritdoc />
        public Transform Transform => transform;
        /// <inheritdoc />
        public bool IsAlive { get; private set; }
        /// <summary>Current HP.</summary>
        public float CurrentHealth => _health;
        /// <summary>Kind from the config.</summary>
        public EnemyKind Kind => _config.Kind;
        /// <summary>True while stunned (no acting).</summary>
        public bool IsStunned => _stunRemaining > 0f;

        /// <summary>Tuning values of this enemy.</summary>
        protected EnemyConfig Config => _config;
        /// <summary>Kaelen.</summary>
        protected Health Target => _target;
        /// <summary>Area-query sizing (never null after Awake).</summary>
        protected CombatQueryConfig QueryConfig => _queryConfig;
        /// <summary>Flat (XZ) distance to Kaelen, updated each frame before <see cref="Act"/>.</summary>
        protected float DistanceToTarget { get; private set; }
        /// <summary>Flat unit direction to Kaelen, updated each frame before <see cref="Act"/>.</summary>
        protected Vector3 DirectionToTarget { get; private set; } = Vector3.forward;
        /// <summary>Colour of the body at spawn.</summary>
        protected Color BaseColor { get; private set; }
        /// <summary>The material assigned at spawn. Shared, owned by whoever spawned the enemy; never destroy it.</summary>
        protected Material SharedMaterial { get; private set; }

        /// <summary>Injects dependencies when building the object at runtime. Call before the object is activated.</summary>
        public void Configure(EnemyConfig config, Health target, CombatQueryConfig queryConfig = null)
        {
            _config = config;
            _target = target;
            if (queryConfig != null)
            {
                _queryConfig = queryConfig;
            }
        }

        /// <summary>Initialises HP, the kinematic body and caches the renderer. Overrides must call the base.</summary>
        protected virtual void Awake()
        {
            if (_queryConfig == null)
            {
                _queryConfig = ScriptableObject.CreateInstance<CombatQueryConfig>();
            }
            Rigidbody body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            _renderer = GetComponent<Renderer>();
            SharedMaterial = _renderer.sharedMaterial;
            BaseColor = SharedMaterial.color;
            _health = _config.MaxHealth;
            IsAlive = true;
        }

        /// <summary>Destroys the tint copy, if any. Overrides must call the base.</summary>
        protected virtual void OnDestroy()
        {
            if (_tintMaterial != null)
            {
                Destroy(_tintMaterial);
            }
        }

        /// <inheritdoc />
        public bool TakeDamage(in DamageInfo info)
        {
            if (!IsAlive)
            {
                return false;
            }
            if (info.KnockbackDistance > 0f)
            {
                _slideRemaining = _knockbackSlideSeconds;
                _slideVelocity = info.KnockbackDirection * (info.KnockbackDistance / _knockbackSlideSeconds);
            }
            if (info.StunDuration > 0f)
            {
                _stunRemaining = Mathf.Max(_stunRemaining, info.StunDuration);
                OnStunned();
            }
            _health -= info.Amount;
            if (_health <= 0f)
            {
                Die();
            }
            return true;
        }

        /// <summary>
        /// Removes the enemy without a death: <see cref="OnDied"/> is NOT raised, so it counts as no kill.
        /// Used by the wave director to clear leftovers when a wave ends by timer or boss kill, and on restart.
        /// </summary>
        public void DespawnSilently()
        {
            IsAlive = false;
            Destroy(gameObject);
        }

        /// <summary>Per-frame behaviour. Not called while stunned, dead, or while Kaelen is down.</summary>
        protected abstract void Act(float deltaTime);

        /// <summary>Called when a stun lands, so subclasses can cancel a windup.</summary>
        protected virtual void OnStunned()
        {
        }

        /// <summary>Moves on the ground plane at <see cref="EnemyConfig.MoveSpeed"/>.</summary>
        protected void Move(Vector3 direction, float deltaTime)
        {
            transform.position += direction * (_config.MoveSpeed * deltaTime);
        }

        /// <summary>Tints this enemy only. The first call gives it its own material copy.</summary>
        protected void SetColor(Color color)
        {
            if (_tintMaterial == null)
            {
                _tintMaterial = new Material(SharedMaterial);
                _renderer.sharedMaterial = _tintMaterial;
            }
            _tintMaterial.color = color;
        }

        /// <summary>Raises <see cref="OnDied"/> and destroys the object. Safe to call once only.</summary>
        protected void Die()
        {
            IsAlive = false;
            OnDied?.Invoke(new EnemyDied { Kind = _config.Kind, Position = transform.position });
            Destroy(gameObject);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (_slideRemaining > 0f)
            {
                float step = Mathf.Min(deltaTime, _slideRemaining);
                transform.position += _slideVelocity * step;
                _slideRemaining -= step;
            }
            if (_stunRemaining > 0f)
            {
                _stunRemaining -= deltaTime;
                return;
            }
            if (!IsAlive || !_target.IsAlive)
            {
                return;
            }

            Vector3 toTarget = _target.transform.position - transform.position;
            toTarget.y = 0f;
            DistanceToTarget = toTarget.magnitude;
            if (DistanceToTarget > 0f)
            {
                DirectionToTarget = toTarget / DistanceToTarget;
            }
            Act(deltaTime);
        }
    }
}
