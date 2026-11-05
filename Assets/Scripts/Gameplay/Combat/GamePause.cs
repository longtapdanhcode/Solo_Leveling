// Implements: production/epics/micro-arena-survivor/story-004-upgrade-cards.md (card pick pauses the game),
// design/game-brief.md MVP 1 (hit-stop). Decision 2026-11-05: one explicit pause mechanism shared with hit-stop.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MicroArena.Gameplay.Combat
{
    /// <summary>
    /// The ONLY writer of <see cref="Time.timeScale"/>. Pausers (the card pick, later the pause menu) hold owner tokens;
    /// hit-stop is a separate flag. Time runs at 1 only when no owner is held and no hit-stop is active, so a hit-stop
    /// that ends during a card pick cannot resume the game.
    /// <code>
    /// timeScale = IsFrozen ? 0 : 1, re-applied after every call below.
    ///   Acquire(owner)     adds owner (no-op if held)
    ///   Release(owner)     removes owner (no-op if not held)
    ///   SetHitStop(active) sets the hit-stop flag
    ///   Reset()            clears owners and hit-stop
    /// </code>
    /// </summary>
    /// <remarks>Static by design (the timeScale it guards is global). Reset on SubsystemRegistration, so it is safe with
    /// domain reload off. The project never uses a time scale other than 0 or 1.</remarks>
    public static class GamePause
    {
        private static readonly HashSet<object> Owners = new HashSet<object>();
        private static bool _hitStop;

        /// <summary>True while any pause owner is held.</summary>
        public static bool IsPaused => Owners.Count > 0;

        /// <summary>True while paused or in a hit-stop (time scale is 0).</summary>
        public static bool IsFrozen => IsPaused || _hitStop;

        /// <summary>Pauses the game on behalf of <paramref name="owner"/>. Idempotent per owner.</summary>
        public static void Acquire(object owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }
            Owners.Add(owner);
            Apply();
        }

        /// <summary>Drops <paramref name="owner"/>'s pause. Idempotent; the game resumes when no owner and no hit-stop remain.</summary>
        public static void Release(object owner)
        {
            if (owner == null)
            {
                return;
            }
            Owners.Remove(owner);
            Apply();
        }

        /// <summary>Turns the hit-stop freeze on or off (called by <see cref="HitStop"/> only).</summary>
        public static void SetHitStop(bool active)
        {
            _hitStop = active;
            Apply();
        }

        /// <summary>Clears every owner and the hit-stop, and restores time scale 1.</summary>
        public static void Reset()
        {
            Owners.Clear();
            _hitStop = false;
            Apply();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad()
        {
            Reset();
        }

        private static void Apply()
        {
            Time.timeScale = IsFrozen ? 0f : 1f;
        }
    }
}
