// Implements: design/game-brief.md MVP 1 + "Player goal & fail state", story-001 (HP, revive, run end).

using System;
using MicroArena.Gameplay.Config;

namespace MicroArena.Gameplay.Combat
{
    /// <summary>States of <see cref="HealthModel"/>.</summary>
    public enum HealthState
    {
        /// <summary>Taking damage normally (possibly invulnerable for a while).</summary>
        Alive,
        /// <summary>HP hit 0 with no revive left. The run is over until <see cref="HealthModel.ResetForNewRun"/>.</summary>
        Dead,
    }

    /// <summary>
    /// Plain-C# health, invulnerability timer and once-per-run revive.
    /// <code>
    /// Transition table
    ///   Alive --damage, HP stays > 0--------------------------> Alive
    ///   Alive --damage, HP reaches 0, revive available--------> Alive  (HP = fraction * max, invulnerable, OnRevived)
    ///   Alive --damage, HP reaches 0, revive already used------> Dead   (OnRunEnded)
    ///   Dead  --ResetForNewRun---------------------------------> Alive  (full HP, revive restored)
    ///   Alive --ResetForNewRun---------------------------------> Alive
    /// Damage is ignored while invulnerable, while Dead, or when amount is 0 or less.
    /// </code>
    /// </summary>
    public sealed class HealthModel
    {
        private readonly PlayerConfig _config;
        private float _invulnerableRemaining;
        private float _maxBonus;

        /// <summary>Raised when HP changes: (current, max).</summary>
        public event Action<float, float> OnHealthChanged;
        /// <summary>Raised when the one-per-run revive triggers.</summary>
        public event Action OnRevived;
        /// <summary>Raised when HP hits 0 and no revive remains.</summary>
        public event Action OnRunEnded;

        /// <summary>Creates a full-health model reading tuning from <paramref name="config"/>.</summary>
        public HealthModel(PlayerConfig config)
        {
            _config = config;
            Current = config.MaxHealth;
            ReviveAvailable = true;
        }

        /// <summary>Current HP.</summary>
        public float Current { get; private set; }
        /// <summary>Maximum HP: config max plus the card bonus (story 004).</summary>
        public float Max => _config.MaxHealth + _maxBonus;
        /// <summary>Flat max HP added by cards.</summary>
        public float MaxBonus => _maxBonus;
        /// <summary>Current state.</summary>
        public HealthState State { get; private set; } = HealthState.Alive;
        /// <summary>True while the revive has not been used this run.</summary>
        public bool ReviveAvailable { get; private set; }
        /// <summary>True while damage is ignored.</summary>
        public bool IsInvulnerable => _invulnerableRemaining > 0f;

        /// <summary>Applies damage if allowed.</summary>
        /// <returns>True if the damage was applied.</returns>
        public bool ApplyDamage(float amount)
        {
            if (State == HealthState.Dead || IsInvulnerable || amount <= 0f)
            {
                return false;
            }

            Current = Math.Max(0f, Current - amount);
            if (Current > 0f)
            {
                OnHealthChanged?.Invoke(Current, Max);
                return true;
            }

            if (ReviveAvailable)
            {
                ReviveAvailable = false;
                Current = Max * _config.ReviveHealthFraction;
                GrantInvulnerability(_config.ReviveInvulnerability);
                OnHealthChanged?.Invoke(Current, Max);
                OnRevived?.Invoke();
            }
            else
            {
                State = HealthState.Dead;
                OnHealthChanged?.Invoke(Current, Max);
                OnRunEnded?.Invoke();
            }
            return true;
        }

        /// <summary>Makes Kaelen invulnerable for at least <paramref name="seconds"/> (never shortens an existing window).</summary>
        public void GrantInvulnerability(float seconds)
        {
            _invulnerableRemaining = Math.Max(_invulnerableRemaining, seconds);
        }

        /// <summary>Advances the invulnerability timer.</summary>
        public void Tick(float deltaTime)
        {
            if (_invulnerableRemaining > 0f)
            {
                _invulnerableRemaining = Math.Max(0f, _invulnerableRemaining - deltaTime);
            }
        }

        /// <summary>
        /// Sets the flat max HP bonus (negative values count as 0). Current HP is not raised; it is clamped down if the
        /// new max is lower. Raises <see cref="OnHealthChanged"/>.
        /// </summary>
        public void SetMaxHealthBonus(float bonus)
        {
            _maxBonus = Math.Max(0f, bonus);
            Current = Math.Min(Current, Max);
            OnHealthChanged?.Invoke(Current, Max);
        }

        /// <summary>Heals up to <see cref="Max"/>. Ignored while Dead or for amounts of 0 or less.</summary>
        /// <returns>True if any HP was restored.</returns>
        public bool Heal(float amount)
        {
            if (State == HealthState.Dead || amount <= 0f)
            {
                return false;
            }
            float before = Current;
            Current = Math.Min(Max, Current + amount);
            if (Current <= before)
            {
                return false;
            }
            OnHealthChanged?.Invoke(Current, Max);
            return true;
        }

        /// <summary>Restores full HP (to the current <see cref="Max"/>, including any bonus still set), the revive and clears invulnerability for a new run.</summary>
        public void ResetForNewRun()
        {
            State = HealthState.Alive;
            Current = Max;
            ReviveAvailable = true;
            _invulnerableRemaining = 0f;
            OnHealthChanged?.Invoke(Current, Max);
        }
    }
}
