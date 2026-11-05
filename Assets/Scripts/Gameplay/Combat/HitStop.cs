// Implements: design/game-brief.md MVP 1 / Build order step 1 (hit-stop), story-001.

using UnityEngine;

namespace MicroArena.Gameplay.Combat
{
    /// <summary>
    /// Plain-C# hit-stop countdown. Requests never stack: a new request only extends the freeze up to its own
    /// length, so a burst of hits cannot build a permanent freeze.
    /// <code>
    /// Transition table
    ///   Inactive --Request(d>0)--------------> Active (remaining = d)
    ///   Active   --Request(d)----------------> Active (remaining = max(remaining, d))
    ///   Active   --Tick, remaining reaches 0--> Inactive
    ///   Active   --Cancel--------------------> Inactive
    /// </code>
    /// </summary>
    public sealed class HitStopTimer
    {
        private float _remaining;

        /// <summary>True while a freeze is running.</summary>
        public bool IsActive => _remaining > 0f;

        /// <summary>Starts or extends (never adds to) a freeze.</summary>
        public void Request(float duration)
        {
            if (duration > _remaining)
            {
                _remaining = duration;
            }
        }

        /// <summary>Advances the freeze using unscaled time.</summary>
        /// <returns>True while the freeze is still running afterwards.</returns>
        public bool Tick(float unscaledDeltaTime)
        {
            if (_remaining > 0f)
            {
                _remaining = Mathf.Max(0f, _remaining - unscaledDeltaTime);
            }
            return _remaining > 0f;
        }

        /// <summary>Ends the freeze immediately.</summary>
        public void Cancel()
        {
            _remaining = 0f;
        }
    }

    /// <summary>
    /// Freezes the game for a short time through <see cref="GamePause.SetHitStop"/> and always ends the freeze. The
    /// countdown runs on unscaled time, so the freeze cannot hang. Scaled-time timers (dash recharge, i-frames,
    /// revive invulnerability) simply pause during the freeze and resume afterwards; none depends on this class.
    /// </summary>
    /// <remarks>Never writes <see cref="Time.timeScale"/> itself: <see cref="GamePause"/> does, so a hit-stop that ends
    /// while a pause owner (card pick, later the pause menu) is held leaves the game paused.</remarks>
    public sealed class HitStop : MonoBehaviour
    {
        private readonly HitStopTimer _timer = new HitStopTimer();
        private bool _frozen;
        private int _requestFrame = -1;

        /// <summary>Requests a freeze of <paramref name="duration"/> seconds (non-stacking).</summary>
        public void Request(float duration)
        {
            if (duration <= 0f)
            {
                return;
            }
            _timer.Request(duration);
            _requestFrame = Time.frameCount;
            if (!_frozen)
            {
                _frozen = true;
                GamePause.SetHitStop(true);
            }
        }

        /// <summary>Ends any running freeze (time resumes unless something else holds a pause).</summary>
        public void Cancel()
        {
            _timer.Cancel();
            Restore();
        }

        private void Update()
        {
            if (!_frozen || Time.frameCount == _requestFrame)
            {
                return;
            }
            if (!_timer.Tick(Time.unscaledDeltaTime))
            {
                Restore();
            }
        }

        private void OnDisable()
        {
            Cancel();
        }

        private void Restore()
        {
            if (_frozen)
            {
                _frozen = false;
                GamePause.SetHitStop(false);
            }
        }
    }
}
