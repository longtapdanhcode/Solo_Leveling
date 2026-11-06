// VERIFICATION SCAFFOLDING for story 003 (production/epics/micro-arena-survivor/story-003-arena-waves.md): shows the
// wave flow on screen for the evidence screenshot. Not the HUD. Story 007 (owner decision 2026-11-05): kept as a debug
// readout, shown only when Debug.isDebugBuild (Editor and development builds), in the bottom-left corner so it never
// covers the HUD; a release build shows only the real HUD.

using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Player;
using UnityEngine;

namespace MicroArena.Gameplay.Waves
{
    /// <summary>Plain IMGUI readout of the run: phase, wave, remaining kills or timer, enemies alive, Kaelen's HP.</summary>
    public sealed class WaveDebugOverlay : MonoBehaviour
    {
        private const float Width = 520f;
        private const float Height = 140f;
        private const float Margin = 10f;

        /// <summary>True when the overlay may appear: Editor and development builds only, never a release build.</summary>
        public static bool IsAllowed => Debug.isDebugBuild;

        [SerializeField] private WaveDirector _director;
        [Tooltip("Kaelen.")]
        [SerializeField] private Health _player;

        [Tooltip("Story 005 scaffolding: the rage line.")]
        [SerializeField] private SoulBurst _soulBurst;

        private string _lastEvent = "-";

        /// <summary>Injects dependencies when building the scene.</summary>
        public void Configure(WaveDirector director, Health player, SoulBurst soulBurst)
        {
            _director = director;
            _player = player;
            _soulBurst = soulBurst;
        }

        private void Awake()
        {
            enabled = IsAllowed; // disabled here, OnEnable never subscribes and OnGUI never draws in a release build
        }

        private void OnEnable()
        {
            _director.OnWaveStarted += HandleWaveStarted;
            _director.OnWaveEnded += HandleWaveEnded;
            _director.OnCardPickRequested += HandleCardPick;
            _director.OnRunReset += HandleRunReset;
        }

        private void OnDisable()
        {
            _director.OnWaveStarted -= HandleWaveStarted;
            _director.OnWaveEnded -= HandleWaveEnded;
            _director.OnCardPickRequested -= HandleCardPick;
            _director.OnRunReset -= HandleRunReset;
        }

        private void HandleWaveStarted(int wave) => _lastEvent = $"Wave {wave} started";
        private void HandleWaveEnded(int wave) => _lastEvent = $"Wave {wave} cleared";
        private void HandleCardPick(int wave) => _lastEvent = $"Card pick after wave {wave} (story 004)";
        private void HandleRunReset() => _lastEvent = "Run restarted";

        private void OnGUI()
        {
            if (!IsAllowed)
            {
                return;
            }
            GUILayout.BeginArea(new Rect(Margin, Screen.height - Height - Margin, Width, Height));
            GUILayout.Label($"Wave {_director.CurrentWave}/{_director.WaveCount}   phase: {_director.Run.Phase}   goal: {_director.CurrentGoal}");
            string timer = _director.SurviveTimeRemaining >= 0f ? $"   survive: {_director.SurviveTimeRemaining:0.0}s" : string.Empty;
            GUILayout.Label($"Remaining {_director.Remaining}/{_director.RemainingTarget}{timer}   enemies alive: {_director.AliveEnemyCount}");
            GUILayout.Label($"HP {_player.Current:0}/{_player.Max:0}   revive left: {_player.ReviveAvailable}   invulnerable: {_player.IsInvulnerable}{(_director.IsDebugInvulnerable ? " (debug)" : string.Empty)}");
            if (_soulBurst != null)
            {
                string burst = _soulBurst.LastBurstHits >= 0 ? $"hit {_soulBurst.LastBurstHits}" : "-";
                GUILayout.Label($"Rage {_soulBurst.Rage:0}/{_soulBurst.MaxRage:0}   last burst: {burst}");
            }
            GUILayout.Label($"Last event: {_lastEvent}");
            GUILayout.EndArea();
        }
    }
}
