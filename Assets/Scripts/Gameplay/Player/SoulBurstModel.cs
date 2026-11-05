// Implements: design/game-brief.md MVP feature 5, story-005. Plain C# rage bar; no scene objects, unit-testable.

using System;

namespace MicroArena.Gameplay.Player
{
    /// <summary>
    /// The Soul Burst rage bar. Gains <c>ragePerKill</c> per kill up to <c>maxRage</c>; the burst is allowed only at full
    /// rage and only while the game is not paused, a wave is being fought and Kaelen is alive.
    /// </summary>
    public sealed class SoulBurstModel
    {
        private readonly float _ragePerKill;

        /// <summary>Raised when the rage value changes: (current, max).</summary>
        public event Action<float, float> OnRageChanged;

        /// <summary>Current rage, 0..<see cref="MaxRage"/>.</summary>
        public float Rage { get; private set; }
        /// <summary>Rage needed to fire.</summary>
        public float MaxRage { get; }

        /// <param name="ragePerKill">Rage per kill, see <see cref="Config.SoulBurstConfig.RagePerKill"/>.</param>
        /// <param name="maxRage">Cap and activation threshold, see <see cref="Config.SoulBurstConfig.MaxRage"/>.</param>
        public SoulBurstModel(float ragePerKill, float maxRage)
        {
            _ragePerKill = ragePerKill;
            MaxRage = maxRage;
        }

        /// <summary>Adds one kill's rage, capped at <see cref="MaxRage"/>.</summary>
        public void AddKill()
        {
            SetRage(Math.Min(MaxRage, Rage + _ragePerKill));
        }

        /// <summary>Empties the bar (new run).</summary>
        public void Reset()
        {
            SetRage(0f);
        }

        /// <summary>Fills the bar. Debug launch option only (<c>-arenaAutoSoulBurst</c>).</summary>
        public void Fill()
        {
            SetRage(MaxRage);
        }

        /// <summary>True when the bar is full and nothing blocks the burst. Hit-stop alone is not a block, so it is not a parameter.</summary>
        /// <param name="isPaused">A pause owner (card pick, pause menu) is held.</param>
        /// <param name="isPlaying">The run phase is Playing.</param>
        /// <param name="isKaelenAlive">Kaelen is alive.</param>
        public bool CanActivate(bool isPaused, bool isPlaying, bool isKaelenAlive)
        {
            return Rage >= MaxRage && !isPaused && isPlaying && isKaelenAlive;
        }

        /// <summary>Empties the bar when it is full.</summary>
        /// <returns>False (rage unchanged) when the bar is not full.</returns>
        public bool TryConsume()
        {
            if (Rage < MaxRage)
            {
                return false;
            }
            SetRage(0f);
            return true;
        }

        private void SetRage(float value)
        {
            if (value == Rage)
            {
                return;
            }
            Rage = value;
            OnRageChanged?.Invoke(Rage, MaxRage);
        }
    }
}
