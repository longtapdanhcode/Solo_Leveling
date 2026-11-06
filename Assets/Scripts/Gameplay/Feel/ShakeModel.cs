// Implements: production/epics/micro-arena-survivor/story-007-hud-game-feel.md (light screen shake). Plain C#, no scene.

using UnityEngine;

namespace MicroArena.Gameplay.Feel
{
    /// <summary>
    /// Screen shake maths: a flat (XZ) offset whose strength fades linearly from the triggered amplitude to zero over the
    /// triggered duration. A new shake replaces the running one only when it is at least as strong as what is left of it,
    /// so a light shake never cuts a strong one short and shakes never stack into a large offset.
    /// <code>
    /// Transition table
    ///   Idle    --Trigger(a > 0, d > 0)---------------------------> Shaking (strength a, fades over d)
    ///   Shaking --Trigger(a >= current strength)-----------------> Shaking (restarted with a, d)
    ///   Shaking --Trigger(a &lt; current strength)------------------> Shaking (ignored)
    ///   Shaking --Tick, elapsed reaches duration-----------------> Idle    (offset zero)
    ///   any     --Reset----------------------------------------> Idle    (offset zero)
    /// </code>
    /// </summary>
    public sealed class ShakeModel
    {
        private const float TwoPi = Mathf.PI * 2f;
        // Second axis runs at a different rate and phase so the offset traces a jittery loop instead of a line.
        private const float SecondAxisRate = 1.37f;
        private const float SecondAxisPhase = 1.1f;

        private float _amplitude;
        private float _duration;
        private float _elapsed;
        private float _time;

        /// <summary>Offset to add to the camera's follow position this frame. Zero when idle.</summary>
        public Vector3 Offset { get; private set; }

        /// <summary>True while a shake is fading.</summary>
        public bool IsShaking => _duration > 0f && _elapsed < _duration;

        /// <summary>Strength left now: amplitude * (1 - elapsed / duration), 0 when idle.</summary>
        public float CurrentStrength => IsShaking ? _amplitude * (1f - _elapsed / _duration) : 0f;

        /// <summary>Starts a shake unless a stronger one is still running. Non-positive values are ignored.</summary>
        public void Trigger(float amplitude, float duration)
        {
            if (amplitude <= 0f || duration <= 0f || amplitude < CurrentStrength)
            {
                return;
            }
            _amplitude = amplitude;
            _duration = duration;
            _elapsed = 0f;
        }

        /// <summary>Advances the shake and recomputes <see cref="Offset"/>. Each axis is bounded by <see cref="CurrentStrength"/>.</summary>
        /// <param name="deltaTime">Seconds since the last tick (scaled time: frozen during pause and hit-stop).</param>
        /// <param name="frequency">Oscillations per second.</param>
        public void Tick(float deltaTime, float frequency)
        {
            if (!IsShaking)
            {
                Offset = Vector3.zero;
                return;
            }
            _elapsed += deltaTime;
            _time += deltaTime;
            float strength = CurrentStrength;
            float phase = _time * frequency * TwoPi;
            Offset = new Vector3(Mathf.Sin(phase), 0f, Mathf.Sin(phase * SecondAxisRate + SecondAxisPhase)) * strength;
        }

        /// <summary>Stops any shake at once; the offset is zero afterwards.</summary>
        public void Reset()
        {
            _amplitude = 0f;
            _duration = 0f;
            _elapsed = 0f;
            _time = 0f;
            Offset = Vector3.zero;
        }
    }
}
