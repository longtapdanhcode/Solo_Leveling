// Implements: design/game-brief.md MVP 1, story-001. Shared damage pipeline reused by stories 002, 003, 006.

using UnityEngine;

namespace MicroArena.Gameplay.Combat
{
    /// <summary>One packet of damage and its side effects, passed to <see cref="IDamageable.TakeDamage"/>.</summary>
    public struct DamageInfo
    {
        /// <summary>Damage after crit/modifiers. 0 means "effects only" (e.g. the revive shock).</summary>
        public float Amount;

        /// <summary>True when the hit was a critical hit (for feedback).</summary>
        public bool IsCritical;

        /// <summary>World-space push direction on the ground plane (normalized, or zero).</summary>
        public Vector3 KnockbackDirection;

        /// <summary>Push distance in metres. 0 = none.</summary>
        public float KnockbackDistance;

        /// <summary>Stun seconds. 0 = none.</summary>
        public float StunDuration;
    }

    /// <summary>Anything that can receive damage, for Kaelen, enemies, bosses and test dummies alike.</summary>
    public interface IDamageable
    {
        /// <summary>Transform used for distance checks and identity.</summary>
        Transform Transform { get; }

        /// <summary>False once the object can no longer be targeted.</summary>
        bool IsAlive { get; }

        /// <summary>Applies a damage packet.</summary>
        /// <returns>True when the packet was accepted (not ignored by invulnerability or death).</returns>
        bool TakeDamage(in DamageInfo info);
    }
}
