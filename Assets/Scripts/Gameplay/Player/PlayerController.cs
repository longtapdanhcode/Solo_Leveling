// Implements: design/game-brief.md MVP 1 (WASD movement + facing, no mouse aiming), story-001.

using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MicroArena.Gameplay.Player
{
    /// <summary>
    /// Kaelen's movement and facing on the XZ ground plane. WASD moves and sets facing. While swinging,
    /// <see cref="AutoAttack"/> overrides the visual facing toward its target; afterwards facing returns to the
    /// WASD facing. Also the root of the "new run" reset.
    /// </summary>
    public sealed class PlayerController : MonoBehaviour
    {
        private const string ActionMapName = "Player";
        private const string MoveActionName = "Move";

        [SerializeField] private PlayerConfig _config;
        [SerializeField] private InputActionAsset _actions;
        [SerializeField] private Health _health;
        [SerializeField] private Dash _dash;
        [SerializeField] private AutoAttack _autoAttack;

        private InputAction _move;
        private Vector3 _facing = Vector3.forward;
        private bool _hasAimOverride;
        private Vector3 _aimOverride;
        private bool _movementLocked;

        /// <summary>Direction faced from the last WASD input (ground plane, normalized). Dash uses this when standing still.</summary>
        public Vector3 FacingDirection => _facing;
        /// <summary>Current WASD direction (zero when no key is held), normalized to at most length 1.</summary>
        public Vector3 MoveDirection { get; private set; }

        /// <summary>Injects dependencies when building the object at runtime. Call before the object is activated.</summary>
        public void Configure(PlayerConfig config, InputActionAsset actions, Health health, Dash dash, AutoAttack autoAttack)
        {
            _config = config;
            _actions = actions;
            _health = health;
            _dash = dash;
            _autoAttack = autoAttack;
        }

        /// <summary>Makes the visual face <paramref name="direction"/> until <see cref="ClearAimOverride"/> (used while swinging).</summary>
        public void SetAimOverride(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude > 0f)
            {
                _hasAimOverride = true;
                _aimOverride = direction.normalized;
            }
        }

        /// <summary>Returns the visual facing to the WASD facing.</summary>
        public void ClearAimOverride()
        {
            _hasAimOverride = false;
        }

        /// <summary>Blocks or allows WASD movement (the dash locks it while travelling).</summary>
        public void SetMovementLocked(bool locked)
        {
            _movementLocked = locked;
        }

        /// <summary>Restores HP, revive, dash charges and the combo for a new run.</summary>
        public void ResetForNewRun()
        {
            _health.ResetForNewRun();
            _dash.ResetForNewRun();
            _autoAttack.ResetForNewRun();
        }

        private void Awake()
        {
            _move = _actions.FindActionMap(ActionMapName, true).FindAction(MoveActionName, true);
        }

        private void OnEnable()
        {
            _move.Enable();
        }

        private void OnDisable()
        {
            _move.Disable();
        }

        private void Update()
        {
            Vector2 input = Vector2.ClampMagnitude(_move.ReadValue<Vector2>(), 1f);
            MoveDirection = new Vector3(input.x, 0f, input.y);

            if (!_health.IsAlive)
            {
                MoveDirection = Vector3.zero;
                return;
            }

            if (MoveDirection.sqrMagnitude > 0f)
            {
                _facing = MoveDirection.normalized;
                if (!_movementLocked)
                {
                    transform.position += MoveDirection * (_config.MoveSpeed * Time.deltaTime);
                }
            }
        }

        private void LateUpdate()
        {
            Vector3 look = _hasAimOverride ? _aimOverride : _facing;
            transform.rotation = Quaternion.LookRotation(look, Vector3.up);
        }
    }
}
