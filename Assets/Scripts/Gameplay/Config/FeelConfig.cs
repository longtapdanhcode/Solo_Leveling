// Implements: design/game-brief.md MVP 7 (HUD + game feel), production/epics/micro-arena-survivor/story-007-hud-game-feel.md.
//
// Single source of every feel intensity and duration (screen shake, revive feedback, HUD warnings, end screen timing).
// The serialized defaults are [PLACEHOLDER] starting values: the design gives no numbers ("rung nhe"), so they are tuned
// on the asset (Assets/Data/FeelConfig.asset) after playtests, never in code. Hit-stop (0.06 s) stays in PlayerConfig (story 001).

using UnityEngine;

namespace MicroArena.Gameplay.Config
{
    /// <summary>Feel tuning: screen shake, revive feedback, HUD warning colours, end screen timing.</summary>
    [CreateAssetMenu(fileName = "FeelConfig", menuName = "Micro Arena Survivor/Feel Config")]
    public sealed class FeelConfig : ScriptableObject
    {
        [Header("Screen shake (camera only)")]
        [Tooltip("1-based combo step whose swing shakes the camera (3 = the third slash). The swing must hit something.")]
        [SerializeField, Min(1)] private int _comboShakeStep = 3;
        [Tooltip("[PLACEHOLDER] Peak camera offset (m) of the combo shake.")]
        [SerializeField, Min(0f)] private float _comboShakeAmplitude = 0.12f;
        [Tooltip("[PLACEHOLDER] Seconds the combo shake takes to fade to zero.")]
        [SerializeField, Min(0.01f)] private float _comboShakeDuration = 0.15f;
        [Tooltip("[PLACEHOLDER] Peak camera offset (m) when a Volatile Husk explodes.")]
        [SerializeField, Min(0f)] private float _explosionShakeAmplitude = 0.25f;
        [Tooltip("[PLACEHOLDER] Seconds the explosion shake takes to fade to zero.")]
        [SerializeField, Min(0.01f)] private float _explosionShakeDuration = 0.25f;
        [Tooltip("[PLACEHOLDER] Oscillations per second of the shake.")]
        [SerializeField, Min(0.1f)] private float _shakeFrequency = 30f;

        [Header("Revive feedback")]
        [Tooltip("[PLACEHOLDER] Colour of the full-screen flash when the revive triggers (alpha = peak opacity).")]
        [SerializeField] private Color _reviveFlashColor = new Color(1f, 0.85f, 0.4f, 0.55f);
        [Tooltip("[PLACEHOLDER] Seconds the revive flash takes to fade out (real time).")]
        [SerializeField, Min(0.01f)] private float _reviveFlashDuration = 0.6f;
        [Tooltip("[PLACEHOLDER] Seconds the revive message stays on screen (real time); it fades over the last third.")]
        [SerializeField, Min(0.01f)] private float _reviveMessageDuration = 1.6f;

        [Header("HUD")]
        [Tooltip("[PLACEHOLDER] HP fraction at or below which the HP bar switches to the low colour.")]
        [SerializeField, Range(0f, 1f)] private float _lowHealthFraction = 0.3f;
        [Tooltip("[PLACEHOLDER] Pulses per second of the rage bar while Soul Burst is ready.")]
        [SerializeField, Min(0f)] private float _rageReadyPulseSpeed = 3f;

        [Header("End screens")]
        [Tooltip("[PLACEHOLDER] Real-time seconds between the victory / defeat and its screen (the final blow stays visible).")]
        [SerializeField, Min(0f)] private float _endScreenDelay = 1f;
        [Tooltip("[PLACEHOLDER] Real-time seconds after the screen appears before replay input is accepted (no accidental skip while mashing).")]
        [SerializeField, Min(0f)] private float _replayInputDelay = 0.4f;

        /// <summary>1-based combo step that shakes the camera.</summary>
        public int ComboShakeStep => _comboShakeStep;
        /// <summary>Peak offset of the combo shake, metres.</summary>
        public float ComboShakeAmplitude => _comboShakeAmplitude;
        /// <summary>Fade time of the combo shake, seconds.</summary>
        public float ComboShakeDuration => _comboShakeDuration;
        /// <summary>Peak offset of the explosion shake, metres.</summary>
        public float ExplosionShakeAmplitude => _explosionShakeAmplitude;
        /// <summary>Fade time of the explosion shake, seconds.</summary>
        public float ExplosionShakeDuration => _explosionShakeDuration;
        /// <summary>Shake oscillations per second.</summary>
        public float ShakeFrequency => _shakeFrequency;
        /// <summary>Revive flash colour; alpha is the peak opacity.</summary>
        public Color ReviveFlashColor => _reviveFlashColor;
        /// <summary>Revive flash fade time, real seconds.</summary>
        public float ReviveFlashDuration => _reviveFlashDuration;
        /// <summary>Revive message time on screen, real seconds.</summary>
        public float ReviveMessageDuration => _reviveMessageDuration;
        /// <summary>HP fraction at or below which the HP bar shows the low colour.</summary>
        public float LowHealthFraction => _lowHealthFraction;
        /// <summary>Rage bar pulses per second while the burst is ready.</summary>
        public float RageReadyPulseSpeed => _rageReadyPulseSpeed;
        /// <summary>Real seconds from victory / defeat to its screen.</summary>
        public float EndScreenDelay => _endScreenDelay;
        /// <summary>Real seconds before an end screen accepts replay input.</summary>
        public float ReplayInputDelay => _replayInputDelay;
    }
}
