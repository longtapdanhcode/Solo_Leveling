// Implements: design/narrative/kich_ban_chuan.md section 4 (Wave 5), production/epics/micro-arena-survivor/story-006-bosses.md.

using MicroArena.Gameplay.Config;
using UnityEngine;

namespace MicroArena.Gameplay.Bosses
{
    /// <summary>
    /// Gargoyle Tyrant: chases, and when its target is within <c>SlamTriggerRange</c> with the cooldown over it stands still and
    /// shows a red circle (<c>SlamRadius</c>) around itself; when the circle resolves, Kaelen inside takes <c>SlamDamage</c>.
    /// <code>
    /// Transition table
    ///   Chasing  --cooldown over and target within SlamTriggerRange--> Slamming (circle placed, boss stops)
    ///   Slamming --circle resolved (hit or miss)----------------------> Chasing  (cooldown = AttackCooldown)
    /// </code>
    /// </summary>
    public sealed class GargoyleBrain : BossBrain
    {
        private enum State
        {
            Chasing,
            Slamming,
        }

        private State _state = State.Chasing;
        private Telegraph _current;

        /// <summary>Creates the brain.</summary>
        public GargoyleBrain(BossConfig config) : base(config)
        {
        }

        /// <inheritdoc />
        public override bool IsAttacking => _state == State.Slamming;

        /// <inheritdoc />
        protected override void Advance(float deltaTime, in BossFrame frame)
        {
            if (_state == State.Slamming)
            {
                WantsToMove = false;
                if (_current.IsResolved)
                {
                    _state = State.Chasing;
                    CooldownRemaining = Config.AttackCooldown;
                }
                return;
            }

            CooldownRemaining = Mathf.Max(0f, CooldownRemaining - deltaTime);
            WantsToMove = frame.DistanceToTarget > Config.StopDistance;
            if (CooldownRemaining <= 0f && frame.DistanceToTarget <= Config.SlamTriggerRange)
            {
                _current = Place(TelegraphShape.Circle(frame.Position, Config.SlamRadius), Config.SlamDamage, Config.SlamTelegraphSeconds);
                _state = State.Slamming;
                WantsToMove = false;
            }
        }

        /// <inheritdoc />
        protected override void ResetSequence()
        {
            _state = State.Chasing;
            _current = null;
        }
    }
}
