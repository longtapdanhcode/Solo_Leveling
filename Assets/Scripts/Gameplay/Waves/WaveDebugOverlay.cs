// VERIFICATION SCAFFOLDING for story 003 (production/epics/micro-arena-survivor/story-003-arena-waves.md): shows the
// wave flow on screen for the evidence screenshot. Not the HUD; replace or delete when story 007 adds the real HUD.

using MicroArena.Gameplay.Combat;
using UnityEngine;

namespace MicroArena.Gameplay.Waves
{
    /// <summary>Plain IMGUI readout of the run: phase, wave, remaining kills or timer, enemies alive, Kaelen's HP.</summary>
    public sealed class WaveDebugOverlay : MonoBehaviour
    {
        [SerializeField] private WaveDirector _director;
        [Tooltip("Kaelen.")]
        [SerializeField] private Health _player;

        private string _lastEvent = "-";

        /// <summary>Injects dependencies when building the scene.</summary>
        public void Configure(WaveDirector director, Health player)
        {
            _director = director;
            _player = player;
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
            GUILayout.BeginArea(new Rect(10f, 10f, 520f, 200f));
            GUILayout.Label($"Wave {_director.CurrentWave}/{_director.WaveCount}   phase: {_director.Run.Phase}   goal: {_director.CurrentGoal}");
            string timer = _director.SurviveTimeRemaining >= 0f ? $"   survive: {_director.SurviveTimeRemaining:0.0}s" : string.Empty;
            GUILayout.Label($"Remaining {_director.Remaining}/{_director.RemainingTarget}{timer}   enemies alive: {_director.AliveEnemyCount}");
            GUILayout.Label($"HP {_player.Current:0}/{_player.Max:0}   revive left: {_player.ReviveAvailable}   invulnerable: {_player.IsInvulnerable}{(_director.IsDebugInvulnerable ? " (debug)" : string.Empty)}");
            GUILayout.Label($"Last event: {_lastEvent}");
            GUILayout.EndArea();
        }
    }
}
