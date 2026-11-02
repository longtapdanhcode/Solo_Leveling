// Implements: design/game-brief.md MVP 1 (combo of 3 hits, 0.4 s per swing, 0.3 s rest after hit 3), story-001.

using System;
using MicroArena.Gameplay.Config;

namespace MicroArena.Gameplay.Player
{
    /// <summary>States of <see cref="ComboStateMachine"/>.</summary>
    public enum ComboState
    {
        /// <summary>Waiting for a target. <see cref="ComboStateMachine.CurrentStep"/> is the next step to play.</summary>
        Idle,
        /// <summary>Playing one swing (<see cref="ComboStateMachine.CurrentStep"/>).</summary>
        Swinging,
        /// <summary>Resting after the last swing of the combo.</summary>
        Resting,
    }

    /// <summary>
    /// Plain-C# combo timing. Pure logic: it never touches the scene.
    /// <code>
    /// Transition table
    ///   Idle     --Tick, target present------------------------------> Swinging (OnSwingStarted)
    ///   Swinging --swing time reached hit moment-------------------> Swinging (OnSwingHit, once per swing)
    ///   Swinging --swing time over, not last step, target present--> Swinging (next step, OnSwingStarted)
    ///   Swinging --swing time over, not last step, no target-------> Idle     (step reset to 1) [PLACEHOLDER rule]
    ///   Swinging --swing time over, last step----------------------> Resting  (step reset to 1)
    ///   Resting  --rest time over----------------------------------> Idle
    ///   any      --Reset---------------------------------------------> Idle   (step reset to 1)
    /// </code>
    /// </summary>
    public sealed class ComboStateMachine
    {
        private readonly PlayerConfig _config;
        private float _timer;
        private bool _hitFired;

        /// <summary>Raised when a swing begins. Argument: zero-based step index.</summary>
        public event Action<int> OnSwingStarted;
        /// <summary>Raised once per swing at the moment damage lands. Argument: zero-based step index.</summary>
        public event Action<int> OnSwingHit;

        /// <summary>Creates a machine reading timings from <paramref name="config"/>.</summary>
        public ComboStateMachine(PlayerConfig config)
        {
            _config = config;
        }

        /// <summary>Current state.</summary>
        public ComboState State { get; private set; } = ComboState.Idle;
        /// <summary>Zero-based index of the current (or next) combo step.</summary>
        public int CurrentStep { get; private set; }

        /// <summary>Advances the machine.</summary>
        /// <param name="deltaTime">Seconds since the last tick.</param>
        /// <param name="hasTarget">True when an enemy is in detection range.</param>
        public void Tick(float deltaTime, bool hasTarget)
        {
            switch (State)
            {
                case ComboState.Idle:
                    if (hasTarget)
                    {
                        BeginSwing();
                    }
                    break;

                case ComboState.Swinging:
                    _timer += deltaTime;
                    if (!_hitFired && _timer >= _config.SwingDuration * _config.SwingHitFraction)
                    {
                        _hitFired = true;
                        OnSwingHit?.Invoke(CurrentStep);
                    }
                    if (_timer >= _config.SwingDuration)
                    {
                        EndSwing(hasTarget);
                    }
                    break;

                case ComboState.Resting:
                    _timer += deltaTime;
                    if (_timer >= _config.RestAfterCombo)
                    {
                        State = ComboState.Idle;
                    }
                    break;
            }
        }

        /// <summary>Returns to Idle on step 1.</summary>
        public void Reset()
        {
            State = ComboState.Idle;
            CurrentStep = 0;
            _timer = 0f;
            _hitFired = false;
        }

        private void BeginSwing()
        {
            State = ComboState.Swinging;
            _timer = 0f;
            _hitFired = false;
            OnSwingStarted?.Invoke(CurrentStep);
        }

        private void EndSwing(bool hasTarget)
        {
            if (CurrentStep >= _config.ComboSteps.Length - 1)
            {
                CurrentStep = 0;
                State = ComboState.Resting;
                _timer = 0f;
            }
            else if (hasTarget)
            {
                CurrentStep++;
                BeginSwing();
            }
            else
            {
                CurrentStep = 0; // [PLACEHOLDER] combo returns to hit 1 when the target is lost
                State = ComboState.Idle;
            }
        }
    }
}
