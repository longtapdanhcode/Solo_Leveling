// Implements: design/narrative/kich_ban_chuan.md section 5.1, production/epics/micro-arena-survivor/story-002-enemies.md.

using MicroArena.Gameplay.Combat;

namespace MicroArena.Gameplay.Enemies
{
    /// <summary>
    /// Melee bug. Runs straight at Kaelen, stops for the windup, then bites if he is still in reach.
    /// <para>Transitions:
    /// Chasing -> WindingUp when within <c>BiteRange</c>;
    /// WindingUp -> Recovering when the windup ends (bite resolves here, hit or miss);
    /// WindingUp -> Chasing when stunned;
    /// Recovering -> Chasing when <c>BiteCooldown</c> ends.</para>
    /// </summary>
    public sealed class Crawler : EnemyBase
    {
        private enum State
        {
            Chasing,
            WindingUp,
            Recovering,
        }

        private State _state = State.Chasing;
        private float _timer;

        /// <inheritdoc />
        protected override void Act(float deltaTime)
        {
            switch (_state)
            {
                case State.Chasing:
                    if (DistanceToTarget <= Config.BiteRange)
                    {
                        _state = State.WindingUp;
                        _timer = Config.LungeWindup;
                    }
                    else
                    {
                        Move(DirectionToTarget, deltaTime);
                    }
                    break;

                case State.WindingUp:
                    _timer -= deltaTime;
                    if (_timer <= 0f)
                    {
                        if (DistanceToTarget <= Config.BiteRange)
                        {
                            Target.TakeDamage(new DamageInfo { Amount = Config.Damage });
                        }
                        _state = State.Recovering;
                        _timer = Config.BiteCooldown;
                    }
                    break;

                case State.Recovering:
                    _timer -= deltaTime;
                    if (_timer <= 0f)
                    {
                        _state = State.Chasing;
                    }
                    break;
            }
        }

        /// <inheritdoc />
        protected override void OnStunned()
        {
            if (_state == State.WindingUp)
            {
                _state = State.Chasing;
            }
        }
    }
}
