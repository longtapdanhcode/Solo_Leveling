// Implements: design/narrative/kich_ban_chuan.md section 5.2, production/epics/micro-arena-survivor/story-002-enemies.md.
// Note (story Implementation Notes): it stands outside Kaelen's 3 m auto-attack range on purpose; re-check in Wave 3-4 playtests.

using UnityEngine;

namespace MicroArena.Gameplay.Enemies
{
    /// <summary>
    /// Ranged caster. Approaches or backs off to hold <c>KeepDistance</c> from Kaelen and fires a straight
    /// <see cref="Projectile"/> every <c>FireInterval</c>, aimed where Kaelen is at the moment of firing.
    /// Stun pauses both movement and the fire timer.
    /// </summary>
    public sealed class VoidCaster : EnemyBase
    {
        // Visual only: size of the runtime-built bolt.
        [SerializeField, Min(0.01f)] private float _projectileScale = 0.4f;

        private float _fireTimer;

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            _fireTimer = Config.FireInterval;
        }

        /// <inheritdoc />
        protected override void Act(float deltaTime)
        {
            if (DistanceToTarget > Config.KeepDistance + Config.KeepDistanceTolerance)
            {
                Move(DirectionToTarget, deltaTime);
            }
            else if (DistanceToTarget < Config.KeepDistance - Config.KeepDistanceTolerance)
            {
                Move(-DirectionToTarget, deltaTime);
            }

            _fireTimer -= deltaTime;
            if (_fireTimer <= 0f)
            {
                Fire();
                _fireTimer = Config.FireInterval;
            }
        }

        private void Fire()
        {
            GameObject bolt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bolt.name = "VoidBolt";
            Destroy(bolt.GetComponent<Collider>()); // hit test is a distance check; keep it out of overlap queries
            bolt.transform.position = transform.position;
            bolt.transform.localScale = Vector3.one * _projectileScale;
            bolt.GetComponent<Renderer>().sharedMaterial = SharedMaterial; // shared with the caster: no per-bolt copy
            bolt.AddComponent<Projectile>().Launch(
                Target, DirectionToTarget, Config.ProjectileSpeed, Config.Damage, Config.ProjectileLifetime, Config.ProjectileHitRadius);
        }
    }
}
