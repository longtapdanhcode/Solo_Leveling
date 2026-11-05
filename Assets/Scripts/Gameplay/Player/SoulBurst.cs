// Implements: design/game-brief.md MVP feature 5 (Soul Burst), production/epics/micro-arena-survivor/story-005-soul-burst.md.
//
// Owner decisions (2026-11-05): +5 rage per EnemyDied (a Volatile Husk self-explosion counts), cap 100, kept across waves,
// reset on run reset. Q at 100 stomps: flat 120 DMG (no card modifiers, no crit, no on-hit procs) to every enemy within
// 6 m through IDamageable, knockback 4 m outward; bosses take the damage but are not pushed. Q does nothing while paused
// (card pick / pause), outside the Playing phase, or when Kaelen is dead; hit-stop alone does not block it.

using System;
using System.Collections.Generic;
using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using MicroArena.Gameplay.Enemies;
using MicroArena.Gameplay.Waves;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MicroArena.Gameplay.Player
{
    /// <summary>
    /// Scene adapter for <see cref="SoulBurstModel"/>: collects rage from the wave director's kills, reads the
    /// <c>SoulBurst</c> input action (Q), applies the shockwave through the shared <see cref="IDamageable"/> path and
    /// draws a flat expanding ring (verification visual; the real effect and bar are story 007).
    /// </summary>
    public sealed class SoulBurst : MonoBehaviour
    {
        /// <summary>
        /// Development builds only: launch option <c>-arenaAutoSoulBurst &lt;seconds&gt;</c>. Every N seconds of Playing
        /// time the rage is filled and the burst fires through <see cref="TryActivate"/> (so the same gating applies).
        /// Exists so a build screenshot can capture the shockwave.
        /// </summary>
        public const string AutoBurstArgument = "-arenaAutoSoulBurst";

        private const string ActionMapName = "Player";
        private const string ActionName = "SoulBurst";
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private SoulBurstConfig _config;
        [SerializeField] private InputActionAsset _actions;
        [Tooltip("Kaelen. Never damaged by his own burst.")]
        [SerializeField] private Health _health;
        [SerializeField] private PlayerController _player;
        [SerializeField] private RunState _runState;
        [Tooltip("Source of kills and run resets, and of the boss check. Optional in tests.")]
        [SerializeField] private WaveDirector _director;
        [Tooltip("Layers that contain enemies (the same mask AutoAttack uses).")]
        [SerializeField] private LayerMask _enemyLayers = ~0;
        [Tooltip("Optional. A fresh instance with the default values is created when empty.")]
        [SerializeField] private CombatQueryConfig _queryConfig;
        [Tooltip("Translucent material of the expanding ring.")]
        [SerializeField] private Material _ringMaterial;

        private SoulBurstModel _model;
        private DamageableQuery _query;
        private readonly List<IDamageable> _scratch = new List<IDamageable>();
        private Func<IDamageable, bool> _isBoss;
        private InputAction _action;
        private MaterialPropertyBlock _block;
        private Transform _ring;
        private Renderer _ringRenderer;
        private float _ringAge = -1f;
        private float _ringAlpha;
        private Vector3 _ringCentre;
        private Color _ringColor;
        private float _autoInterval;
        private float _autoTimer;

        /// <summary>Raised when the rage changes: (current, max). The HUD of story 007 listens here.</summary>
        public event Action<float, float> OnRageChanged;
        /// <summary>Raised after a burst: (enemies damaged).</summary>
        public event Action<int> OnBurst;

        /// <summary>Current rage, 0..<see cref="MaxRage"/>.</summary>
        public float Rage => _model.Rage;
        /// <summary>Rage needed to fire.</summary>
        public float MaxRage => _model.MaxRage;
        /// <summary>Enemies damaged by the last burst, or -1 before the first one (debug overlay).</summary>
        public int LastBurstHits { get; private set; } = -1;

        /// <summary>Injects dependencies when building the scene. Call before the object is activated.</summary>
        public void Configure(SoulBurstConfig config, InputActionAsset actions, Health health, PlayerController player,
            RunState runState, WaveDirector director, LayerMask enemyLayers, CombatQueryConfig queryConfig, Material ringMaterial)
        {
            _config = config;
            _actions = actions;
            _health = health;
            _player = player;
            _runState = runState;
            _director = director;
            _enemyLayers = enemyLayers;
            _queryConfig = queryConfig;
            _ringMaterial = ringMaterial;
        }

        /// <summary>
        /// Replaces the boss check (default: <see cref="WaveDirector.IsBoss"/> of the injected director). A delegate cannot
        /// be serialized into the scene, so the default is resolved at runtime rather than in <see cref="Configure"/>.
        /// </summary>
        public void SetBossPredicate(Func<IDamageable, bool> isBoss)
        {
            _isBoss = isBoss;
        }

        /// <summary>Counts one enemy death (subscribed to <see cref="WaveDirector.OnEnemyKilled"/>). Every kind counts, bosses and Husk explosions included.</summary>
        public void RegisterKill(EnemyDied died)
        {
            _model.AddKill();
        }

        /// <summary>Empties the bar and hides the ring (subscribed to <see cref="WaveDirector.OnRunReset"/>).</summary>
        public void ResetForNewRun()
        {
            _model.Reset();
            _ringAge = -1f;
            if (_ring != null)
            {
                _ring.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// The only activation path (Q and the debug auto-burst). Does nothing and keeps the rage unless the bar is full,
        /// no pause owner is held, the run phase is Playing and Kaelen is alive. Hit-stop does not block it.
        /// </summary>
        /// <returns>True when the burst fired.</returns>
        public bool TryActivate()
        {
            if (!_model.CanActivate(GamePause.IsPaused, _runState.Phase == RunPhase.Playing, _health.IsAlive) || !_model.TryConsume())
            {
                return false;
            }

            if (_isBoss == null && _director != null)
            {
                _isBoss = _director.IsBoss;
            }

            Vector3 origin = transform.position;
            // Colliders move via transform without Rigidbodies and auto-sync is off: sync once before querying.
            Physics.SyncTransforms();
            _query.Gather(origin, _config.ShockwaveRadius, _enemyLayers, _health.Transform, _scratch);
            int hits = 0;
            foreach (IDamageable target in _scratch)
            {
                Vector3 outward = target.Transform.position - origin;
                outward.y = 0f;
                var info = new DamageInfo
                {
                    Amount = _config.Damage,
                    IsCritical = false,
                    KnockbackDirection = outward.sqrMagnitude > 0f ? outward.normalized : _player.FacingDirection,
                    KnockbackDistance = _isBoss != null && _isBoss(target) ? 0f : _config.KnockbackDistance,
                    StunDuration = 0f,
                };
                if (target.TakeDamage(info))
                {
                    hits++;
                }
            }

            LastBurstHits = hits;
            StartRing();
            OnBurst?.Invoke(hits);
            return true;
        }

        private void Awake()
        {
            _model = new SoulBurstModel(_config.RagePerKill, _config.MaxRage);
            _model.OnRageChanged += (current, max) => OnRageChanged?.Invoke(current, max);
            if (_queryConfig == null)
            {
                _queryConfig = ScriptableObject.CreateInstance<CombatQueryConfig>();
            }
            _query = new DamageableQuery(_queryConfig.MaxQueryColliders);
            _action = _actions.FindActionMap(ActionMapName, true).FindAction(ActionName, true);

            // Debug switches never reach a release player: Debug.isDebugBuild is true only in the Editor and development builds.
            if (Debug.isDebugBuild)
            {
                string[] args = Environment.GetCommandLineArgs();
                int index = Array.IndexOf(args, AutoBurstArgument);
                if (index >= 0 && index + 1 < args.Length
                    && float.TryParse(args[index + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float seconds))
                {
                    _autoInterval = Mathf.Max(0f, seconds);
                }
            }
        }

        private void OnEnable()
        {
            _action.Enable();
            if (_director != null)
            {
                _director.OnEnemyKilled += RegisterKill;
                _director.OnRunReset += ResetForNewRun;
            }
        }

        private void OnDisable()
        {
            _action.Disable();
            if (_director != null)
            {
                _director.OnEnemyKilled -= RegisterKill;
                _director.OnRunReset -= ResetForNewRun;
            }
        }

        private void OnDestroy()
        {
            if (_ring != null)
            {
                Destroy(_ring.gameObject);
            }
        }

        private void Update()
        {
            if (_action.WasPressedThisFrame())
            {
                TryActivate();
            }

            if (_autoInterval > 0f && !GamePause.IsPaused && _runState.Phase == RunPhase.Playing && _health.IsAlive)
            {
                _autoTimer += Time.deltaTime;
                if (_autoTimer >= _autoInterval)
                {
                    _autoTimer = 0f;
                    _model.Fill();
                    TryActivate();
                }
            }

            TickRing(Time.deltaTime);
        }

        private void StartRing()
        {
            if (_ring == null)
            {
                GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                disc.name = "SoulBurstRing";
                disc.GetComponent<Collider>().enabled = false; // visual only: never part of an overlap query
                _ringRenderer = disc.GetComponent<Renderer>();
                _ringRenderer.sharedMaterial = _ringMaterial;
                _ringRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _ring = disc.transform;
                _block = new MaterialPropertyBlock();
                _ringColor = _ringMaterial.color;
            }
            _ringAge = 0f;
            _ringAlpha = _ringColor.a;
            _ringCentre = transform.position + new Vector3(0f, _config.RingHeightOffset, 0f); // world-space marker of the burst area
            _ring.gameObject.SetActive(true);
            TickRing(0f);
        }

        private void TickRing(float deltaTime)
        {
            if (_ringAge < 0f)
            {
                return;
            }
            _ringAge += deltaTime;
            float t = Mathf.Clamp01(_ringAge / _config.RingDuration);
            float diameter = 2f * _config.ShockwaveRadius * t;
            _ring.position = _ringCentre;
            _ring.localScale = new Vector3(diameter, _config.RingThickness * 0.5f, diameter); // cylinder is 2 units tall
            Color color = _ringColor;
            color.a = _ringAlpha * (1f - t);
            _block.SetColor(BaseColorId, color);
            _ringRenderer.SetPropertyBlock(_block);
            if (t >= 1f)
            {
                _ringAge = -1f;
                _ring.gameObject.SetActive(false);
            }
        }
    }
}
