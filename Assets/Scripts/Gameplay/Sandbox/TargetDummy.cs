// VERIFICATION SCAFFOLDING for story-001 (production/epics/micro-arena-survivor/story-001-kaelen-core-combat.md).
// Not a game feature: a stationary IDamageable that shows hits by flashing, logging and sliding on knockback.
// Real enemies arrive in story 002 and replace this.

using MicroArena.Gameplay.Combat;
using UnityEngine;

namespace MicroArena.Gameplay.Sandbox
{
    /// <summary>Stationary, unkillable damage receiver used to observe Kaelen's attacks.</summary>
    public sealed class TargetDummy : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _flashSeconds = 0.1f;
        [SerializeField] private float _slideSeconds = 0.15f;
        [SerializeField] private Color _flashColor = Color.red;

        private Renderer _renderer;
        private Color _baseColor;
        private float _flashRemaining;
        private float _slideRemaining;
        private Vector3 _slideVelocity;

        /// <summary>Total damage received, for the on-screen readout.</summary>
        public float TotalDamage { get; private set; }

        /// <inheritdoc />
        public Transform Transform => transform;
        /// <inheritdoc />
        public bool IsAlive => true;

        /// <inheritdoc />
        public bool TakeDamage(in DamageInfo info)
        {
            TotalDamage += info.Amount;
            if (info.Amount > 0f)
            {
                Debug.Log($"{name} took {info.Amount} (crit: {info.IsCritical}, knockback {info.KnockbackDistance} m, stun {info.StunDuration} s)");
                _flashRemaining = _flashSeconds;
                _renderer.material.color = _flashColor;
            }
            if (info.KnockbackDistance > 0f)
            {
                _slideRemaining = _slideSeconds;
                _slideVelocity = info.KnockbackDirection * (info.KnockbackDistance / _slideSeconds);
            }
            return true;
        }

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _baseColor = _renderer.material.color;
        }

        private void Update()
        {
            if (_flashRemaining > 0f)
            {
                _flashRemaining -= Time.deltaTime;
                if (_flashRemaining <= 0f)
                {
                    _renderer.material.color = _baseColor;
                }
            }
            if (_slideRemaining > 0f)
            {
                float step = Mathf.Min(Time.deltaTime, _slideRemaining);
                transform.position += _slideVelocity * step;
                _slideRemaining -= step;
            }
        }
    }
}
