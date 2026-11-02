// Implements: design/narrative/kich_ban_chuan.md section 5.2 (Void Caster bolt), story-002.

using MicroArena.Gameplay.Combat;
using UnityEngine;

namespace MicroArena.Gameplay.Enemies
{
    /// <summary>
    /// Straight-flying bolt. Its direction is fixed at launch, so Kaelen dodges by moving or dashing.
    /// Hits only Kaelen, through <see cref="Health.TakeDamage"/> (dash invulnerability is respected there),
    /// and disappears on contact or when its lifetime runs out.
    /// </summary>
    public sealed class Projectile : MonoBehaviour
    {
        private Health _target;
        private Vector3 _velocity;
        private float _damage;
        private float _lifeRemaining;
        private float _hitRadius;

        /// <summary>Launches the bolt. <paramref name="direction"/> must be a flat unit vector.</summary>
        public void Launch(Health target, Vector3 direction, float speed, float damage, float lifetime, float hitRadius)
        {
            _target = target;
            _velocity = direction * speed;
            _damage = damage;
            _lifeRemaining = lifetime;
            _hitRadius = hitRadius;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            transform.position += _velocity * deltaTime;
            _lifeRemaining -= deltaTime;

            Vector3 offset = _target.transform.position - transform.position;
            offset.y = 0f;
            if (_target.IsAlive && offset.sqrMagnitude <= _hitRadius * _hitRadius)
            {
                _target.TakeDamage(new DamageInfo { Amount = _damage });
                Destroy(gameObject);
            }
            else if (_lifeRemaining <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
