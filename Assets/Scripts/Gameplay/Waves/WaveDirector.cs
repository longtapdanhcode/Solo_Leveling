// Implements: design/narrative/kich_ban_chuan.md sections 2 and 4 (Bang 1), design/game-brief.md MVP 2 +
// "Player goal & fail state", production/epics/micro-arena-survivor/story-003-arena-waves.md.
//
// Decisions recorded in the story thread (2026-11-04): leftovers are despawned silently (no kill, no EnemyDied)
// when a wave ends by timer (Wave 6-7); the trap hits Kaelen only; live bolts are
// destroyed on restart.
// Story 006 (2026-11-05): Wave 5/10 spawn the real bosses. A Boss wave ends when the boss is dead AND no other enemy is
// alive (Wave 5: the 4 escorts; Wave 10 has none, so the Void Warden's death ends the run in Victory). Once the boss is
// dead no more escorts spawn.
// Story 007 (2026-11-05): the defeat no longer restarts by itself (the defeat screen offers replay and calls RestartRun);
// a Boss wave's remaining count is the boss plus every escort still to kill; boss and husk-explosion events for the HUD
// and the screen shake.

using System;
using System.Collections.Generic;
using MicroArena.Gameplay.Arena;
using MicroArena.Gameplay.Bosses;
using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using MicroArena.Gameplay.Enemies;
using MicroArena.Gameplay.Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MicroArena.Gameplay.Waves
{
    /// <summary>
    /// Runs the 10 waves of <see cref="WaveConfig"/>: spawns enemies along the walls, counts kills from
    /// <see cref="EnemyBase.OnDied"/> (a Volatile Husk's self-explosion counts), ends each wave on its goal, arms
    /// the rotating trap from its start wave, keeps bodies inside the arena, and drives <see cref="RunState"/>.
    /// Exposes C# events for the HUD (story 007) and the card pick hand-off (story 004); knows nothing about UI.
    /// <para>No static state: everything resets in <see cref="RestartRun"/>, which also runs on Start.</para>
    /// </summary>
    public sealed class WaveDirector : MonoBehaviour
    {
        /// <summary>Command-line switch that turns on the debug invulnerability. Development builds only.</summary>
        public const string InvulnerableArgument = "-arenaInvulnerable";
        /// <summary>Command-line option <c>-arenaStartWave &lt;n&gt;</c>: every run starts at wave n. Development builds only.</summary>
        public const string StartWaveArgument = "-arenaStartWave";
        /// <summary>
        /// Command-line option <c>-arenaForceEnd victory|defeat</c>: the first run ends at once in that phase (victory: Wave 10
        /// is started and completed; defeat: the run fails), so the end screen can be captured. Development builds only.
        /// </summary>
        public const string ForceEndArgument = "-arenaForceEnd";

        // Debug only: window re-granted every frame while the debug invulnerability is on.
        private const float DebugInvulnerabilityWindow = 1f;

        private enum PendingAction
        {
            None,
            ContinueFromCardPick,
        }

        [SerializeField] private WaveConfig _config;
        [SerializeField] private RunState _runState;
        [Tooltip("Kaelen.")]
        [SerializeField] private Health _player;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private RotatingTrap _trap;
        [Tooltip("Optional. Passed to spawned enemies; when empty they keep their prefab's value.")]
        [SerializeField] private CombatQueryConfig _queryConfig;

        [Header("Hand-offs")]
        [Tooltip("Leave the card pick by itself after the delay. Turn off once story 004 calls ContinueFromCardPick.")]
        [SerializeField] private bool _autoContinueCardPick = true;
        // [PLACEHOLDER] Pause between waves while no card pick exists.
        [Tooltip("[PLACEHOLDER] Seconds before the automatic continue.")]
        [SerializeField, Min(0f)] private float _autoContinueDelay = 1.5f;

        [Header("Debug")]
        [Tooltip("Keeps Kaelen invulnerable (evidence capture). Also enabled by the -arenaInvulnerable launch argument. Editor and development builds only. Keep off.")]
        [SerializeField] private bool _debugInvulnerable;

        private readonly List<EnemyBase> _alive = new List<EnemyBase>();
        private readonly List<Projectile> _projectiles = new List<Projectile>();
        private Transform _staging;
        private Vector3 _playerSpawn;
        private bool _invulnerable;
        private int _startWave = 1;
        private RunPhase _forcedEnd = RunPhase.Playing; // Playing = none; debug only

        private WaveDefinition _wave;
        private bool _waveActive;
        private int _kills;
        private int _poolSpawned;
        private EnemyBase _boss;
        private bool _bossDefeated;
        private float _spawnTimer;
        private float _surviveRemaining = -1f;
        private PendingAction _pending;
        private float _pendingTimer;
        private IEnemyTargetOverride _targetOverride;

        /// <summary>Raised when a wave starts: (1-based wave number).</summary>
        public event Action<int> OnWaveStarted;
        /// <summary>Raised when a wave's goal is met: (1-based wave number). Not raised on defeat.</summary>
        public event Action<int> OnWaveEnded;
        /// <summary>Raised when the wave progress changes: (remaining, target). See <see cref="Remaining"/>.</summary>
        public event Action<int, int> OnRemainingChanged;
        /// <summary>Raised after waves 1-9: (cleared wave). Story 004 shows the cards, then calls <see cref="ContinueFromCardPick"/>.</summary>
        public event Action<int> OnCardPickRequested;
        /// <summary>Raised for every enemy death (any cause, any phase), before the wave counts it. Story 004 heals on kill here.</summary>
        public event Action<EnemyDied> OnEnemyKilled;
        /// <summary>Raised when a run restarts, after Kaelen's HP, revive, dash and combo are reset. Story 004 clears cards here.</summary>
        public event Action OnRunReset;
        /// <summary>Raised when a wave's boss spawns: (display name, current HP, max HP). Story 007 shows the boss bar.</summary>
        public event Action<string, float, float> OnBossSpawned;
        /// <summary>Raised when the boss takes a hit: (current HP, max HP).</summary>
        public event Action<float, float> OnBossHealthChanged;
        /// <summary>Raised once when the boss leaves: killed, or removed by a run reset / wave end. Story 007 hides the boss bar.</summary>
        public event Action OnBossCleared;
        /// <summary>Raised when a Volatile Husk explodes (its fuse ended): (position). Story 007 screen shake.</summary>
        public event Action<Vector3> OnEnemyExploded;

        /// <summary>Run state machine (phase and wave number).</summary>
        public RunState Run => _runState;
        /// <summary>Current wave, 1-based.</summary>
        public int CurrentWave => _runState.WaveNumber;
        /// <summary>Waves in a run.</summary>
        public int WaveCount => _config.WaveCount;
        /// <summary>Goal of the current wave.</summary>
        public WaveGoal CurrentGoal => _wave.Goal;
        /// <summary>
        /// Enemies still to kill in the current wave. Kill / SurviveOrKill: kills left. Boss: the boss while alive, plus every
        /// escort alive, plus (while the boss lives) the escorts not spawned yet; once the boss is dead no more escorts spawn,
        /// so only the living ones count. Reaches 0 exactly when a Boss wave ends.
        /// </summary>
        public int Remaining => _wave.Goal == WaveGoal.Boss ? BossWaveRemaining() : Mathf.Max(0, _wave.KillTarget - _kills);
        /// <summary>Total of <see cref="Remaining"/> at wave start: the kill target, or the boss plus all its escorts.</summary>
        public int RemainingTarget => _wave.Goal == WaveGoal.Boss ? 1 + EscortTotal : _wave.KillTarget;
        /// <summary>Seconds left to survive in a SurviveOrKill wave; -1 in other waves.</summary>
        public float SurviveTimeRemaining => _surviveRemaining;
        /// <summary>Enemies alive now.</summary>
        public int AliveEnemyCount => _alive.Count;
        /// <summary>True while the debug invulnerability is on.</summary>
        public bool IsDebugInvulnerable => _invulnerable;

        /// <summary>True when <paramref name="target"/> is this wave's boss (Soul Burst does not knock bosses back).</summary>
        public bool IsBoss(IDamageable target)
        {
            return _boss != null && ReferenceEquals(target, _boss);
        }

        /// <summary>Injects dependencies when building the scene. Call before the object is activated.</summary>
        public void Configure(WaveConfig config, RunState runState, Health player, PlayerController playerController,
            RotatingTrap trap, CombatQueryConfig queryConfig)
        {
            _config = config;
            _runState = runState;
            _player = player;
            _playerController = playerController;
            _trap = trap;
            _queryConfig = queryConfig;
        }

        /// <summary>Turns the automatic card pick continue on or off. The story-004 Arena scene sets it off (CardController continues).</summary>
        public void SetAutoContinueCardPick(bool enabled)
        {
            _autoContinueCardPick = enabled;
        }

        /// <summary>Sets the decoy provider passed to every enemy spawned from now on (and to the living ones). Null = none.</summary>
        public void SetEnemyTargetOverride(IEnemyTargetOverride targetOverride)
        {
            _targetOverride = targetOverride;
            foreach (EnemyBase enemy in _alive)
            {
                enemy.SetTargetOverride(targetOverride);
            }
        }

        /// <summary>Leaves the card pick and starts the next wave. Ignored outside the card pick.</summary>
        public void ContinueFromCardPick()
        {
            if (!_runState.ContinueToNextWave())
            {
                return;
            }
            _pending = PendingAction.None;
            StartWave();
        }

        /// <summary>
        /// Starts a new run at Wave 1 (or the <see cref="StartWaveArgument"/> wave in a development build): clears enemies and live bolts, resets Kaelen (HP, revive, dash, combo) and puts
        /// him back at his scene position (arena centre), then raises <see cref="OnRunReset"/>. Usable from any phase.
        /// </summary>
        public void RestartRun()
        {
            _pending = PendingAction.None;
            _waveActive = false;
            DespawnAllEnemies();
            foreach (Projectile projectile in _projectiles)
            {
                if (projectile != null)
                {
                    Destroy(projectile.gameObject);
                }
            }
            _projectiles.Clear();

            _playerController.ResetForNewRun();
            _player.transform.position = _playerSpawn;
            OnRunReset?.Invoke();
            _runState.StartRun(_startWave);
            StartWave();
        }

        private int EscortTotal => _wave.SpawnPool.Length > 0 ? _wave.EscortCount : 0;

        private int BossWaveRemaining()
        {
            bool bossAlive = _boss != null && !_bossDefeated;
            int escortsAlive = 0;
            foreach (EnemyBase enemy in _alive)
            {
                if (enemy != _boss)
                {
                    escortsAlive++;
                }
            }
            int unspawned = bossAlive ? Mathf.Max(0, EscortTotal - _poolSpawned) : 0;
            return (bossAlive ? 1 : 0) + escortsAlive + unspawned;
        }

        private void Awake()
        {
            var staging = new GameObject("SpawnStaging");
            staging.SetActive(false); // enemies are configured here before their Awake runs
            _staging = staging.transform;
            _staging.SetParent(transform, false);
            _playerSpawn = _player.transform.position;
            // Debug switches never reach a release player: Debug.isDebugBuild is true only in the Editor and development builds.
            if (Debug.isDebugBuild)
            {
                string[] args = Environment.GetCommandLineArgs();
                _invulnerable = _debugInvulnerable || Array.IndexOf(args, InvulnerableArgument) >= 0;
                int index = Array.IndexOf(args, StartWaveArgument);
                if (index >= 0 && index + 1 < args.Length && int.TryParse(args[index + 1], out int wave))
                {
                    _startWave = Mathf.Clamp(wave, 1, _config.WaveCount);
                }
                index = Array.IndexOf(args, ForceEndArgument);
                if (index >= 0 && index + 1 < args.Length)
                {
                    string end = args[index + 1].ToLowerInvariant();
                    _forcedEnd = end == "victory" ? RunPhase.Victory : end == "defeat" ? RunPhase.Defeat : RunPhase.Playing;
                }
            }
        }

        private void OnEnable()
        {
            _player.OnRunEnded += HandlePlayerRunEnded;
        }

        private void OnDisable()
        {
            _player.OnRunEnded -= HandlePlayerRunEnded;
        }

        private void Start()
        {
            RestartRun();
            if (_forcedEnd == RunPhase.Victory)
            {
                _runState.StartRun(_config.WaveCount);
                DespawnAllEnemies();
                StartWave();
                CompleteWave();
            }
            else if (_forcedEnd == RunPhase.Defeat)
            {
                HandlePlayerRunEnded();
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (_invulnerable && _player.IsAlive)
            {
                _player.GrantInvulnerability(DebugInvulnerabilityWindow);
            }

            if (_pending != PendingAction.None)
            {
                _pendingTimer -= deltaTime;
                if (_pendingTimer <= 0f)
                {
                    PendingAction action = _pending;
                    _pending = PendingAction.None;
                    if (action == PendingAction.ContinueFromCardPick)
                    {
                        ContinueFromCardPick();
                    }
                }
            }

            if (!_waveActive)
            {
                return;
            }

            if (_surviveRemaining >= 0f)
            {
                _surviveRemaining = Mathf.Max(0f, _surviveRemaining - deltaTime);
                if (_surviveRemaining <= 0f)
                {
                    CompleteWave();
                    return;
                }
            }

            int limit = _wave.PoolSpawnLimit;
            bool poolHasMore = _wave.SpawnPool.Length > 0 && (limit < 0 || _poolSpawned < limit) && !_bossDefeated;
            if (poolHasMore && _alive.Count < _config.MaxAlive)
            {
                _spawnTimer -= deltaTime;
                if (_spawnTimer <= 0f)
                {
                    _spawnTimer = _config.SpawnInterval;
                    WaveSpawnEntry entry = PickFromPool(_wave.SpawnPool);
                    Spawn(entry.Prefab, entry.Config);
                    _poolSpawned++;
                }
            }
        }

        // Walls are visual only for movement (everything moves by transform), so bodies are kept inside here.
        private void LateUpdate()
        {
            float extent = _config.PlayableHalfExtent;
            KeepInside(_player.transform, extent);
            foreach (EnemyBase enemy in _alive)
            {
                KeepInside(enemy.transform, extent);
            }
        }

        private void StartWave()
        {
            int waveNumber = _runState.WaveNumber;
            _wave = _config.GetWave(waveNumber);
            _kills = 0;
            _poolSpawned = 0;
            _boss = null;
            _bossDefeated = false;
            _spawnTimer = _config.SpawnInterval;
            _surviveRemaining = _wave.Goal == WaveGoal.SurviveOrKill ? _wave.SurviveSeconds : -1f;
            _trap.SetArmed(waveNumber >= _config.TrapStartWave);
            _waveActive = true;
            if (_wave.Goal == WaveGoal.Boss)
            {
                _boss = Spawn(_wave.BossPrefab, null, _wave.BossConfig);
                _boss.OnHealthChanged += RelayBossHealth;
                OnBossSpawned?.Invoke(((BossBase)_boss).DisplayName, _boss.CurrentHealth, _boss.MaxHealth);
            }
            OnWaveStarted?.Invoke(waveNumber);
            OnRemainingChanged?.Invoke(Remaining, RemainingTarget);
        }

        private void CompleteWave()
        {
            _waveActive = false;
            DespawnAllEnemies();
            int waveNumber = _runState.WaveNumber;
            OnWaveEnded?.Invoke(waveNumber);
            _runState.CompleteWave(_config.WaveCount);
            if (_runState.Phase == RunPhase.Victory)
            {
                _trap.SetArmed(false);
            }
            else if (_runState.Phase == RunPhase.CardPick)
            {
                OnCardPickRequested?.Invoke(waveNumber);
                if (_autoContinueCardPick && _runState.Phase == RunPhase.CardPick)
                {
                    Schedule(PendingAction.ContinueFromCardPick, _autoContinueDelay);
                }
            }
        }

        private void HandlePlayerRunEnded()
        {
            if (!_runState.Fail())
            {
                return;
            }
            _waveActive = false; // the defeat screen (story 007) offers the replay, which calls RestartRun
        }

        private void HandleEnemyDied(EnemyBase enemy, EnemyDied died)
        {
            _alive.Remove(enemy);
            OnEnemyKilled?.Invoke(died);
            if (enemy == _boss && !_bossDefeated)
            {
                _bossDefeated = true;
                OnBossCleared?.Invoke();
            }
            if (!_waveActive)
            {
                return;
            }
            _kills++;
            OnRemainingChanged?.Invoke(Remaining, RemainingTarget);
            bool goalMet = _wave.Goal == WaveGoal.Boss ? _bossDefeated && _alive.Count == 0 : _kills >= _wave.KillTarget;
            if (goalMet)
            {
                CompleteWave();
            }
        }

        private EnemyBase Spawn(EnemyBase prefab, EnemyConfig config, BossConfig bossConfig = null)
        {
            Vector3 position = PickSpawnPoint();
            position.y = prefab.transform.position.y; // prefabs are saved resting on the floor
            EnemyBase enemy = Instantiate(prefab, position, Quaternion.identity, _staging);
            if (enemy is BossBase boss)
            {
                boss.ConfigureBoss(bossConfig, _player, _queryConfig);
            }
            else
            {
                enemy.Configure(config, _player, _queryConfig);
            }
            enemy.SetTargetOverride(_targetOverride);
            enemy.OnDied += died => HandleEnemyDied(enemy, died);
            if (enemy is VoidCaster caster)
            {
                caster.OnFired += TrackProjectile;
            }
            else if (enemy is VolatileHusk husk)
            {
                husk.OnExploded += position => OnEnemyExploded?.Invoke(position);
            }
            _alive.Add(enemy);
            enemy.transform.SetParent(null, true); // leaves the inactive staging: Awake runs now, configured
            return enemy;
        }

        private Vector3 PickSpawnPoint()
        {
            float half = _config.ArenaSize * 0.5f - _config.SpawnInset;
            float along = Random.Range(-half, half);
            Vector3 point = Random.Range(0, 4) switch
            {
                0 => new Vector3(along, 0f, half),
                1 => new Vector3(along, 0f, -half),
                2 => new Vector3(half, 0f, along),
                _ => new Vector3(-half, 0f, along),
            };
            Vector3 player = _player.transform.position;
            player.y = 0f;
            float minDistance = _config.MinSpawnDistanceFromPlayer;
            if ((point - player).sqrMagnitude < minDistance * minDistance)
            {
                point = -point; // the far side of the square is never this close at the same time
            }
            return point;
        }

        private static WaveSpawnEntry PickFromPool(WaveSpawnEntry[] pool)
        {
            int total = 0;
            foreach (WaveSpawnEntry entry in pool)
            {
                total += entry.Weight;
            }
            int roll = Random.Range(0, total);
            foreach (WaveSpawnEntry entry in pool)
            {
                roll -= entry.Weight;
                if (roll < 0)
                {
                    return entry;
                }
            }
            return pool[pool.Length - 1];
        }

        private void TrackProjectile(Projectile projectile)
        {
            _projectiles.RemoveAll(p => p == null); // bolts destroy themselves on hit or timeout
            _projectiles.Add(projectile);
        }

        private void RelayBossHealth(float current, float max)
        {
            OnBossHealthChanged?.Invoke(current, max);
        }

        private void DespawnAllEnemies()
        {
            if (_boss != null && !_bossDefeated)
            {
                OnBossCleared?.Invoke();
            }
            _boss = null;
            foreach (EnemyBase enemy in _alive)
            {
                enemy.DespawnSilently();
            }
            _alive.Clear();
        }

        private void Schedule(PendingAction action, float delay)
        {
            _pending = action;
            _pendingTimer = delay;
        }

        private static void KeepInside(Transform body, float extent)
        {
            Vector3 position = body.position;
            float x = Mathf.Clamp(position.x, -extent, extent);
            float z = Mathf.Clamp(position.z, -extent, extent);
            if (x != position.x || z != position.z)
            {
                body.position = new Vector3(x, position.y, z);
            }
        }
    }
}
