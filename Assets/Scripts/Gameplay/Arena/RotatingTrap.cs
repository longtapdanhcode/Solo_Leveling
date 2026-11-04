// Implements: design/narrative/kich_ban_chuan.md section 4 (Bang 1, Wave 8-9 rotating trap), design/game-brief.md MVP 2,
// production/epics/micro-arena-survivor/story-003-arena-waves.md (1 revolution per 6 s, 15 DMG on contact).

using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using UnityEngine;

namespace MicroArena.Gameplay.Arena
{
    /// <summary>
    /// Blade bar centred on the arena floor that sweeps around the Y axis. Damages Kaelen only (enemies are not
    /// hit, decided 2026-11-04) through <see cref="Health.TakeDamage"/>, so dash and revive invulnerability apply.
    /// After a landed hit it waits <c>TrapContactCooldown</c> before it can hit again. Contact is a flat box test in
    /// the trap's local space (no colliders), because Auto Sync Transforms is off and Kaelen moves by transform.
    /// The blade visual is a child; the root must keep scale 1.
    /// </summary>
    public sealed class RotatingTrap : MonoBehaviour
    {
        [SerializeField] private WaveConfig _config;
        [Tooltip("Kaelen.")]
        [SerializeField] private Health _target;

        private float _cooldownRemaining;

        /// <summary>Injects dependencies when building the object. Call before the object is activated.</summary>
        public void Configure(WaveConfig config, Health target)
        {
            _config = config;
            _target = target;
        }

        /// <summary>Turns the trap on or off (it is hidden and harmless while off) and clears the re-hit cooldown.</summary>
        public void SetArmed(bool armed)
        {
            _cooldownRemaining = 0f;
            gameObject.SetActive(armed);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            transform.Rotate(0f, 360f / _config.TrapSecondsPerRevolution * deltaTime, 0f, Space.World);

            if (_cooldownRemaining > 0f)
            {
                _cooldownRemaining -= deltaTime;
                return;
            }
            if (_target.IsAlive && IsTouching(_target.transform.position)
                && _target.TakeDamage(new DamageInfo { Amount = _config.TrapDamage }))
            {
                _cooldownRemaining = _config.TrapContactCooldown;
            }
        }

        private bool IsTouching(Vector3 worldPosition)
        {
            Vector3 local = Quaternion.Inverse(transform.rotation) * (worldPosition - transform.position);
            float halfWidth = _config.TrapWidth * 0.5f + _config.TrapHitPadding;
            float halfLength = _config.TrapLength * 0.5f + _config.TrapHitPadding;
            return Mathf.Abs(local.x) <= halfWidth && Mathf.Abs(local.z) <= halfLength;
        }
    }
}
