// Implements: design/game-brief.md MVP 1 (Dash: 5 m, 2 stored charges, 2 s recharge each), story-001.
// The 0.18 s i-frames are granted through HealthModel.GrantInvulnerability by the Dash adapter.

using System;
using MicroArena.Gameplay.Config;

namespace MicroArena.Gameplay.Player
{
    /// <summary>States of <see cref="DashModel"/>.</summary>
    public enum DashState
    {
        /// <summary>Not dashing; a dash can start if a charge is available.</summary>
        Ready,
        /// <summary>Travelling; new dash requests are refused.</summary>
        Dashing,
    }

    /// <summary>
    /// Plain-C# dash charges, recharge timer and travel progress.
    /// <code>
    /// Transition table
    ///   Ready   --TryStart, charges > 0--> Dashing (charge spent, OnChargesChanged)
    ///   Ready   --TryStart, no charge----> Ready   (refused)
    ///   Dashing --TryStart----------------> Dashing (refused)
    ///   Dashing --Tick, duration reached--> Ready   (Progress = 1)
    ///   any     --Reset-------------------> Ready   (full charges)
    /// Recharge runs independently of state: one charge per recharge time while below max.
    /// </code>
    /// </summary>
    public sealed class DashModel
    {
        private readonly PlayerConfig _config;
        private float _rechargeTimer;
        private float _dashElapsed;

        /// <summary>Raised when the charge count changes: (current, max).</summary>
        public event Action<int, int> OnChargesChanged;

        /// <summary>Creates a model with full charges.</summary>
        public DashModel(PlayerConfig config)
        {
            _config = config;
            Charges = config.DashMaxCharges;
        }

        /// <summary>Stored charges.</summary>
        public int Charges { get; private set; }
        /// <summary>Maximum charges.</summary>
        public int MaxCharges => _config.DashMaxCharges;
        /// <summary>Current state.</summary>
        public DashState State { get; private set; } = DashState.Ready;
        /// <summary>Fraction (0..1) of the dash travelled so far.</summary>
        public float Progress { get; private set; }
        /// <summary>Fraction (0..1) of the next charge recovered; 0 when charges are full.</summary>
        public float RechargeProgress => Charges >= MaxCharges ? 0f : _rechargeTimer / _config.DashRechargeTime;

        /// <summary>Starts a dash if one is allowed.</summary>
        /// <returns>True when the dash started.</returns>
        public bool TryStart()
        {
            if (State != DashState.Ready || Charges <= 0)
            {
                return false;
            }
            Charges--;
            State = DashState.Dashing;
            _dashElapsed = 0f;
            Progress = 0f;
            OnChargesChanged?.Invoke(Charges, MaxCharges);
            return true;
        }

        /// <summary>Advances recharge and the running dash.</summary>
        public void Tick(float deltaTime)
        {
            if (Charges < MaxCharges)
            {
                _rechargeTimer += deltaTime;
                while (_rechargeTimer >= _config.DashRechargeTime && Charges < MaxCharges)
                {
                    _rechargeTimer -= _config.DashRechargeTime;
                    Charges++;
                    OnChargesChanged?.Invoke(Charges, MaxCharges);
                }
                if (Charges >= MaxCharges)
                {
                    _rechargeTimer = 0f;
                }
            }

            if (State == DashState.Dashing)
            {
                _dashElapsed += deltaTime;
                Progress = Math.Min(1f, _dashElapsed / _config.DashDuration);
                if (Progress >= 1f)
                {
                    State = DashState.Ready;
                }
            }
        }

        /// <summary>Restores full charges and cancels any dash.</summary>
        public void Reset()
        {
            Charges = MaxCharges;
            State = DashState.Ready;
            Progress = 0f;
            _rechargeTimer = 0f;
            _dashElapsed = 0f;
            OnChargesChanged?.Invoke(Charges, MaxCharges);
        }
    }
}
