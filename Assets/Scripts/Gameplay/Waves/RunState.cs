// Implements: design/game-brief.md MVP 2 + "Player goal & fail state", design/narrative/kich_ban_chuan.md section 2,
// production/epics/micro-arena-survivor/story-003-arena-waves.md (run state machine: playing, card pick, victory, defeat).

using System;
using UnityEngine;

namespace MicroArena.Gameplay.Waves
{
    /// <summary>Phases of a run.</summary>
    public enum RunPhase
    {
        /// <summary>A wave is being fought.</summary>
        Playing,
        /// <summary>Between waves 1-9 and the next wave: the card pick of story 004.</summary>
        CardPick,
        /// <summary>Wave 10 cleared.</summary>
        Victory,
        /// <summary>Kaelen died with the revive spent.</summary>
        Defeat,
    }

    /// <summary>
    /// Run state machine and current wave number. Holds no static state, so it is safe with domain reload off;
    /// every run starts through <see cref="StartRun"/>. Driven by <see cref="WaveDirector"/>.
    /// <code>
    /// Transition table
    ///   (any)    --StartRun-------------------------------> Playing  (wave = start wave, 1 unless a debug build asks)
    ///   Playing  --CompleteWave, wave &lt; last--------------> CardPick
    ///   Playing  --CompleteWave, wave == last-------------> Victory
    ///   Playing  --Fail-----------------------------------> Defeat
    ///   CardPick --ContinueToNextWave---------------------> Playing  (wave + 1)
    ///   CardPick --Fail (a stray bolt kills Kaelen)--------> Defeat
    ///   Any other trigger is ignored and returns false.
    /// </code>
    /// </summary>
    public sealed class RunState : MonoBehaviour
    {
        /// <summary>Raised on every transition: (previous, current).</summary>
        public event Action<RunPhase, RunPhase> OnPhaseChanged;

        /// <summary>Current phase.</summary>
        public RunPhase Phase { get; private set; } = RunPhase.Playing;

        /// <summary>Current wave, 1-based.</summary>
        public int WaveNumber { get; private set; } = 1;

        /// <summary>Starts a new run, from any phase.</summary>
        /// <param name="startWave">1-based first wave. 1 in a normal run; higher only for the debug start-wave argument.</param>
        public void StartRun(int startWave = 1)
        {
            WaveNumber = startWave;
            SetPhase(RunPhase.Playing);
        }

        /// <summary>Ends the current wave: card pick, or victory after the last wave.</summary>
        /// <returns>False when not playing.</returns>
        public bool CompleteWave(int waveCount)
        {
            if (Phase != RunPhase.Playing)
            {
                return false;
            }
            SetPhase(WaveNumber >= waveCount ? RunPhase.Victory : RunPhase.CardPick);
            return true;
        }

        /// <summary>Leaves the card pick and moves to the next wave.</summary>
        /// <returns>False when not in the card pick.</returns>
        public bool ContinueToNextWave()
        {
            if (Phase != RunPhase.CardPick)
            {
                return false;
            }
            WaveNumber++;
            SetPhase(RunPhase.Playing);
            return true;
        }

        /// <summary>Ends the run in defeat.</summary>
        /// <returns>False when already won or lost.</returns>
        public bool Fail()
        {
            if (Phase != RunPhase.Playing && Phase != RunPhase.CardPick)
            {
                return false;
            }
            SetPhase(RunPhase.Defeat);
            return true;
        }

        private void SetPhase(RunPhase next)
        {
            RunPhase previous = Phase;
            Phase = next;
            OnPhaseChanged?.Invoke(previous, next);
        }
    }
}
