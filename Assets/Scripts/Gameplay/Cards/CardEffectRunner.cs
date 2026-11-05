// Implements: production/epics/micro-arena-survivor/story-004-upgrade-cards.md (milestone 2: Rare and Legendary cards),
// design/narrative/kich_ban_chuan.md sections 6.2 and 6.3. Decisions 2026-11-05: card damage (burn, chain, meteor) is flat,
// with no damage multiplier, crit or hit-stop; a meteor pending at wave end still lands; everything clears on run reset.

using System.Collections.Generic;
using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using MicroArena.Gameplay.Enemies;
using MicroArena.Gameplay.Player;
using MicroArena.Gameplay.Waves;
using UnityEngine;

namespace MicroArena.Gameplay.Cards
{
    /// <summary>
    /// Runs the event-driven card effects on scaled time (they pause with the card pick and hit-stop):
    /// burn stacks on slash hits (Vết Chém Hỏa Ngục), chain lightning on combo step 3 (Lôi Chấn), the dash decoy that
    /// enemies chase (Dịch Chuyển Phân Thân, via <see cref="IEnemyTargetOverride"/>), heal per kill (Vòng Xoáy Huyết
    /// Thần) and the delayed meteor at the dash start (Sao Băng Giáng Trần). Every effect is off while its
    /// <see cref="ModifierStack"/> number is 0.
    /// </summary>
    public sealed class CardEffectRunner : MonoBehaviour, IEnemyTargetOverride
    {
        private struct PendingMeteor
        {
            public Vector3 Position;
            public float Remaining;
            public bool Landed;
            public GameObject Marker;
        }

        [SerializeField] private CardEffectConfig _config;
        [SerializeField] private WaveDirector _director;
        [SerializeField] private AutoAttack _autoAttack;
        [SerializeField] private Dash _dash;
        [SerializeField] private Health _health;
        [Tooltip("Layers searched by the chain lightning and the meteor. Kaelen is always skipped.")]
        [SerializeField] private LayerMask _enemyLayers = ~0;
        [Tooltip("Optional. A fresh instance with the default values is created when empty.")]
        [SerializeField] private CombatQueryConfig _queryConfig;

        private ModifierStack _stack;
        private DamageableQuery _query;
        private readonly List<IDamageable> _scratch = new List<IDamageable>();
        private readonly List<IDamageable> _chain = new List<IDamageable>();
        private readonly Dictionary<IDamageable, BurnStatus> _burns = new Dictionary<IDamageable, BurnStatus>();
        private readonly List<IDamageable> _burnKeys = new List<IDamageable>();
        private readonly List<PendingMeteor> _meteors = new List<PendingMeteor>();
        private GameObject _decoy;
        private float _decoyRemaining;

        /// <summary>Injects dependencies when building the scene. Call before the object is activated.</summary>
        public void Configure(CardEffectConfig config, WaveDirector director, AutoAttack autoAttack, Dash dash, Health health,
            LayerMask enemyLayers, CombatQueryConfig queryConfig)
        {
            _config = config;
            _director = director;
            _autoAttack = autoAttack;
            _dash = dash;
            _health = health;
            _enemyLayers = enemyLayers;
            _queryConfig = queryConfig;
        }

        /// <summary>Sets the run's modifiers (owned by <see cref="CardController"/>) and gives them the Cuồng Nộ threshold.</summary>
        public void SetModifiers(ModifierStack stack)
        {
            _stack = stack;
            _stack?.SetRageThreshold(_config.VoidRageThreshold);
        }

        /// <summary>Clears burns and destroys the decoy and every pending meteor (new run).</summary>
        public void ResetForNewRun()
        {
            _burns.Clear();
            DestroyDecoy();
            foreach (PendingMeteor meteor in _meteors)
            {
                if (meteor.Marker != null)
                {
                    Destroy(meteor.Marker);
                }
            }
            _meteors.Clear();
        }

        /// <inheritdoc />
        public bool TryGetTargetPosition(out Vector3 position)
        {
            if (_decoy != null && _decoyRemaining > 0f)
            {
                position = _decoy.transform.position;
                return true;
            }
            position = default;
            return false;
        }

        private void Awake()
        {
            if (_queryConfig == null)
            {
                _queryConfig = ScriptableObject.CreateInstance<CombatQueryConfig>();
            }
            _query = new DamageableQuery(_queryConfig.MaxQueryColliders);
            _director.SetEnemyTargetOverride(this);
        }

        private void OnEnable()
        {
            _autoAttack.OnSwingResolved += HandleSwingResolved;
            _dash.OnDashStarted += HandleDashStarted;
            _director.OnEnemyKilled += HandleEnemyKilled;
        }

        private void OnDisable()
        {
            _autoAttack.OnSwingResolved -= HandleSwingResolved;
            _dash.OnDashStarted -= HandleDashStarted;
            _director.OnEnemyKilled -= HandleEnemyKilled;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
            {
                return; // paused or hit-stop
            }
            TickDecoy(deltaTime);
            TickMeteors(deltaTime);
            TickBurns(deltaTime);
        }

        private void HandleSwingResolved(int step, IReadOnlyList<IDamageable> struck)
        {
            if (_stack == null || struck.Count == 0)
            {
                return;
            }
            if (_stack.BurnDamagePerSecond > 0f)
            {
                foreach (IDamageable target in struck)
                {
                    if (!target.IsAlive)
                    {
                        continue;
                    }
                    if (!_burns.TryGetValue(target, out BurnStatus burn))
                    {
                        burn = new BurnStatus();
                        _burns.Add(target, burn);
                    }
                    burn.AddStack(_config.MaxBurnStacks, _config.BurnDuration);
                }
            }
            if (_stack.ChainDamage > 0f && step == _config.ChainComboStep - 1)
            {
                ReleaseChain(struck);
            }
        }

        // Chains from ONE source: the struck enemy nearest Kaelen (each jump hits a different enemy, no double hits).
        private void ReleaseChain(IReadOnlyList<IDamageable> struck)
        {
            Vector3 kaelen = _health.transform.position;
            IDamageable source = null;
            float best = float.MaxValue;
            foreach (IDamageable target in struck)
            {
                Vector3 offset = target.Transform.position - kaelen;
                offset.y = 0f;
                if (offset.sqrMagnitude < best)
                {
                    best = offset.sqrMagnitude;
                    source = target;
                }
            }
            if (source == null)
            {
                return;
            }
            Vector3 origin = source.Transform.position; // still valid this frame even if the slash killed it
            Physics.SyncTransforms();
            _query.Gather(origin, _config.ChainRange, _enemyLayers, _health.transform, _scratch);
            ChainTargets.Select(source, origin, _scratch, _config.ChainRange, _config.ChainTargets, _chain);
            float damage = _stack.ChainDamage;
            foreach (IDamageable target in _chain)
            {
                target.TakeDamage(new DamageInfo { Amount = damage });
            }
        }

        private void HandleDashStarted(Vector3 start)
        {
            if (_stack == null)
            {
                return;
            }
            if (_stack.DecoySeconds > 0f)
            {
                PlaceDecoy(start, _stack.DecoySeconds);
            }
            if (_stack.MeteorDamage > 0f)
            {
                Vector3 ground = new Vector3(start.x, 0f, start.z);
                _meteors.Add(new PendingMeteor
                {
                    Position = ground,
                    Remaining = _config.MeteorDelay,
                    Marker = CreateMeteorMarker(ground),
                });
            }
        }

        private void HandleEnemyKilled(EnemyDied died)
        {
            if (_stack != null && _stack.KillHealPercent > 0f)
            {
                _health.Heal(_stack.KillHeal(_health.Max)); // ignored while Kaelen is dead
            }
        }

        private void PlaceDecoy(Vector3 position, float seconds)
        {
            if (_decoy == null)
            {
                _decoy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                _decoy.name = "DashDecoy";
                Destroy(_decoy.GetComponent<Collider>()); // never targeted, never damaged
                if (_config.DecoyMaterial != null)
                {
                    _decoy.GetComponent<Renderer>().sharedMaterial = _config.DecoyMaterial;
                }
            }
            _decoy.transform.position = position;
            _decoyRemaining = seconds;
        }

        private void TickDecoy(float deltaTime)
        {
            if (_decoy == null)
            {
                return;
            }
            _decoyRemaining -= deltaTime;
            if (_decoyRemaining <= 0f)
            {
                DestroyDecoy();
            }
        }

        private void DestroyDecoy()
        {
            if (_decoy != null)
            {
                Destroy(_decoy);
            }
            _decoy = null;
            _decoyRemaining = 0f;
        }

        private GameObject CreateMeteorMarker(Vector3 ground)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "MeteorMarker";
            Destroy(marker.GetComponent<Collider>());
            float diameter = _config.MeteorRadius * 2f;
            float thickness = _config.MeteorMarkerThickness;
            marker.transform.position = ground + Vector3.up * (thickness * 0.5f);
            marker.transform.localScale = new Vector3(diameter, thickness * 0.5f, diameter); // cylinder primitive is 2 units tall
            if (_config.MeteorMaterial != null)
            {
                marker.GetComponent<Renderer>().sharedMaterial = _config.MeteorMaterial;
            }
            return marker;
        }

        private void TickMeteors(float deltaTime)
        {
            for (int i = _meteors.Count - 1; i >= 0; i--)
            {
                PendingMeteor meteor = _meteors[i];
                meteor.Remaining -= deltaTime;
                if (!meteor.Landed && meteor.Remaining <= 0f)
                {
                    meteor.Landed = true;
                    meteor.Remaining = _config.MeteorMarkerLinger;
                    _meteors[i] = meteor;
                    LandMeteor(meteor.Position); // may restart the run, which clears _meteors
                    if (i >= _meteors.Count)
                    {
                        continue;
                    }
                    meteor = _meteors[i];
                }
                if (meteor.Landed && meteor.Remaining <= 0f)
                {
                    if (meteor.Marker != null)
                    {
                        Destroy(meteor.Marker);
                    }
                    _meteors.RemoveAt(i);
                }
                else
                {
                    _meteors[i] = meteor;
                }
            }
        }

        private void LandMeteor(Vector3 position)
        {
            if (_stack == null)
            {
                return;
            }
            Physics.SyncTransforms();
            _query.Gather(position, _config.MeteorRadius, _enemyLayers, _health.transform, _scratch);
            _chain.Clear();
            _chain.AddRange(_scratch); // copy: a kill can complete the wave and despawn the rest mid-loop
            float damage = _stack.MeteorDamage;
            foreach (IDamageable target in _chain)
            {
                target.TakeDamage(new DamageInfo { Amount = damage, StunDuration = _config.MeteorStun });
            }
        }

        private void TickBurns(float deltaTime)
        {
            if (_burns.Count == 0)
            {
                return;
            }
            float dps = _stack != null ? _stack.BurnDamagePerSecond : 0f;
            _burnKeys.Clear();
            _burnKeys.AddRange(_burns.Keys);
            foreach (IDamageable target in _burnKeys)
            {
                if (!_burns.TryGetValue(target, out BurnStatus burn))
                {
                    continue; // cleared by a run reset during this loop
                }
                if (!target.IsAlive || !burn.IsActive)
                {
                    _burns.Remove(target);
                    continue;
                }
                float damage = burn.Tick(deltaTime, dps);
                if (damage > 0f)
                {
                    target.TakeDamage(new DamageInfo { Amount = damage }); // no knockback, stun or hit-stop
                }
                if (!burn.IsActive || !target.IsAlive)
                {
                    _burns.Remove(target);
                }
            }
        }
    }
}
