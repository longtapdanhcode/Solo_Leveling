// Implements: design/game-brief.md MVP 7, production/epics/micro-arena-survivor/story-007-hud-game-feel.md
// ("screen shake nhe khi tung nhat chem thu 3 va khi quai no"; the shake must never move the player or gameplay).

using System.Collections.Generic;
using MicroArena.Gameplay.Arena;
using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using MicroArena.Gameplay.Player;
using MicroArena.Gameplay.Waves;
using UnityEngine;

namespace MicroArena.Gameplay.Feel
{
    /// <summary>
    /// Camera-only screen shake. Listens to the combo (<see cref="AutoAttack.OnSwingResolved"/>, the configured step that
    /// hit something) and to Volatile Husk explosions (<see cref="WaveDirector.OnEnemyExploded"/>), runs a
    /// <see cref="ShakeModel"/> and hands its offset to <see cref="FollowCamera.SetShakeOffset"/>, which adds it on top of
    /// the follow position. Nothing else is moved. While a pause owner is held (card pick, end screens) and on a run reset
    /// the shake is stopped and the offset is zero. Every intensity and duration comes from <see cref="FeelConfig"/>.
    /// </summary>
    public sealed class ScreenShake : MonoBehaviour
    {
        [SerializeField] private FeelConfig _config;
        [SerializeField] private FollowCamera _camera;
        [SerializeField] private AutoAttack _autoAttack;
        [SerializeField] private WaveDirector _director;

        private readonly ShakeModel _model = new ShakeModel();

        /// <summary>The current camera offset (zero when idle).</summary>
        public Vector3 Offset => _model.Offset;

        /// <summary>Injects dependencies when building the scene. Call before the object is activated.</summary>
        public void Configure(FeelConfig config, FollowCamera followCamera, AutoAttack autoAttack, WaveDirector director)
        {
            _config = config;
            _camera = followCamera;
            _autoAttack = autoAttack;
            _director = director;
        }

        private void OnEnable()
        {
            _autoAttack.OnSwingResolved += HandleSwingResolved;
            _director.OnEnemyExploded += HandleEnemyExploded;
            _director.OnRunReset += StopShake;
        }

        private void OnDisable()
        {
            _autoAttack.OnSwingResolved -= HandleSwingResolved;
            _director.OnEnemyExploded -= HandleEnemyExploded;
            _director.OnRunReset -= StopShake;
            StopShake();
        }

        private void Update()
        {
            if (GamePause.IsPaused)
            {
                StopShake();
                return;
            }
            _model.Tick(Time.deltaTime, _config.ShakeFrequency);
            _camera.SetShakeOffset(_model.Offset);
        }

        private void HandleSwingResolved(int step, IReadOnlyList<IDamageable> struck)
        {
            if (step + 1 == _config.ComboShakeStep && struck.Count > 0 && !GamePause.IsPaused)
            {
                _model.Trigger(_config.ComboShakeAmplitude, _config.ComboShakeDuration);
            }
        }

        private void HandleEnemyExploded(Vector3 position)
        {
            if (!GamePause.IsPaused)
            {
                _model.Trigger(_config.ExplosionShakeAmplitude, _config.ExplosionShakeDuration);
            }
        }

        private void StopShake()
        {
            _model.Reset();
            _camera.SetShakeOffset(Vector3.zero);
        }
    }
}
