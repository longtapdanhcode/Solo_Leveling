// Implements: design/game-brief.md MVP 1 (Dash on Space, WASD direction or current facing, i-frames, no auto-aim), story-001.

using System;
using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MicroArena.Gameplay.Player
{
    /// <summary>
    /// Scene adapter for <see cref="DashModel"/>. Moves Kaelen along a fixed direction (WASD, else the WASD facing;
    /// never toward an enemy) and passes through enemies because it moves the transform and uses no collision.
    /// </summary>
    public sealed class Dash : MonoBehaviour
    {
        private const string ActionMapName = "Player";
        private const string DashActionName = "Dash";

        [SerializeField] private PlayerConfig _config;
        [SerializeField] private InputActionAsset _actions;
        [SerializeField] private PlayerController _player;
        [SerializeField] private Health _health;

        private DashModel _model;
        private InputAction _dashAction;
        private Vector3 _direction;
        private float _previousProgress;

        /// <summary>Raised when the charge count changes: (current, max).</summary>
        public event Action<int, int> OnChargesChanged;
        /// <summary>Raised when a dash starts: (Kaelen's position before moving). Story 004 decoy and meteor.</summary>
        public event Action<Vector3> OnDashStarted;

        /// <summary>Stored charges.</summary>
        public int Charges => _model.Charges;
        /// <summary>Maximum charges.</summary>
        public int MaxCharges => _model.MaxCharges;
        /// <summary>Progress (0..1) of the next charge.</summary>
        public float RechargeProgress => _model.RechargeProgress;
        /// <summary>True while travelling.</summary>
        public bool IsDashing => _model.State == DashState.Dashing;

        /// <summary>Injects dependencies when building the object at runtime. Call before the object is activated.</summary>
        public void Configure(PlayerConfig config, InputActionAsset actions, PlayerController player, Health health)
        {
            _config = config;
            _actions = actions;
            _player = player;
            _health = health;
        }

        /// <summary>Restores full charges and cancels any dash.</summary>
        public void ResetForNewRun()
        {
            _model.Reset();
            _player.SetMovementLocked(false);
        }

        private void Awake()
        {
            _model = new DashModel(_config);
            _model.OnChargesChanged += (current, max) => OnChargesChanged?.Invoke(current, max);
            _dashAction = _actions.FindActionMap(ActionMapName, true).FindAction(DashActionName, true);
        }

        private void OnEnable()
        {
            _dashAction.Enable();
        }

        private void OnDisable()
        {
            _dashAction.Disable();
            if (_model != null && _model.State == DashState.Dashing)
            {
                _model.Reset();
                _player.SetMovementLocked(false);
            }
        }

        private void Update()
        {
            // No dash while a pause owner (card pick) is held: it would place decoys or meteors behind the screen.
            if (_health.IsAlive && !GamePause.IsPaused && _dashAction.WasPressedThisFrame() && _model.TryStart())
            {
                _direction = _player.MoveDirection.sqrMagnitude > 0f ? _player.MoveDirection.normalized : _player.FacingDirection;
                _previousProgress = 0f;
                _health.GrantInvulnerability(_config.DashInvulnerability);
                _player.SetMovementLocked(true);
                OnDashStarted?.Invoke(transform.position);
            }

            bool wasDashing = IsDashing;
            _model.Tick(Time.deltaTime);

            if (wasDashing)
            {
                transform.position += _direction * (_config.DashDistance * (_model.Progress - _previousProgress));
                _previousProgress = _model.Progress;
                if (!IsDashing)
                {
                    _player.SetMovementLocked(false);
                }
            }
        }
    }
}
