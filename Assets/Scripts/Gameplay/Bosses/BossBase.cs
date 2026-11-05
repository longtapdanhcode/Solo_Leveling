// Implements: design/game-brief.md MVP 6, production/epics/micro-arena-survivor/story-006-bosses.md.
//
// Owner decisions (2026-11-05): bosses ignore stun and knockback from every source (damage still applies); the decoy
// (story 004) draws bosses too but never cancels a telegraph already placed; dying, being despawned or a run reset clears
// every telegraph and pending meteor at once; boss attacks hurt Kaelen only.

using System;
using System.Collections.Generic;
using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using MicroArena.Gameplay.Enemies;
using UnityEngine;

namespace MicroArena.Gameplay.Bosses
{
    /// <summary>
    /// Scene adapter shared by both bosses: stats from <see cref="BossConfig"/>, immunity to stun and knockback, a per-frame
    /// bridge to the plain-C# <see cref="BossBrain"/> (chase, telegraphs, pull) and the red floor drawings. Time comes from
    /// <c>Time.deltaTime</c> through <see cref="EnemyBase"/>, which is 0 during a pause or hit-stop, so telegraphs freeze with the game.
    /// </summary>
    public abstract class BossBase : EnemyBase
    {
        [SerializeField] private BossConfig _bossConfig;
        [Tooltip("Translucent red material of the floor telegraphs.")]
        [SerializeField] private Material _telegraphMaterial;

        private readonly List<TelegraphView> _views = new List<TelegraphView>();
        private BossBrain _brain;
        private Action<float> _hitKaelen; // cached: a method group passed to Tick would allocate every frame

        /// <summary>The boss tuning asset.</summary>
        public BossConfig Settings => _bossConfig;
        /// <summary>The attack logic (telegraphs, cooldown, phase).</summary>
        public BossBrain Brain => _brain;
        /// <inheritdoc />
        public override EnemyKind Kind => EnemyKind.Boss;

        /// <inheritdoc />
        protected override bool IgnoresCrowdControl => true;

        /// <summary>Injects dependencies when building the boss at runtime. Call before the object is activated.</summary>
        public void ConfigureBoss(BossConfig bossConfig, Health target, CombatQueryConfig queryConfig = null)
        {
            if (bossConfig != null)
            {
                _bossConfig = bossConfig; // a wave entry left empty keeps the prefab's own config
            }
            Configure(null, target, queryConfig); // bosses have no EnemyConfig: HP and speed come from BossConfig
        }

        /// <summary>Sets the material of the floor telegraphs (prefab building).</summary>
        public void SetTelegraphMaterial(Material material)
        {
            _telegraphMaterial = material;
        }

        /// <inheritdoc />
        public override void DespawnSilently()
        {
            _brain?.Stop();
            base.DespawnSilently();
        }

        /// <summary>Creates this boss's attack logic.</summary>
        protected abstract BossBrain CreateBrain();

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            _brain = CreateBrain();
            _brain.OnTelegraphPlaced += AddView;
            _brain.OnTelegraphRemoved += RemoveView;
            _hitKaelen = HitKaelen;
        }

        /// <inheritdoc />
        protected override void OnDying()
        {
            _brain.Stop(); // before OnDied: the director's Victory / wave-end handlers run with every telegraph gone
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            _brain?.Stop();
            base.OnDestroy();
        }

        /// <inheritdoc />
        protected override void Act(float deltaTime)
        {
            var frame = new BossFrame
            {
                Position = transform.position,
                DirectionToTarget = DirectionToTarget,
                DistanceToTarget = DistanceToTarget,
                PlayerPosition = Target.transform.position,
                PlayerAlive = Target.IsAlive,
                HealthFraction = CurrentHealth / StartingHealth,
            };
            _brain.Tick(deltaTime, frame, _hitKaelen);

            if (_brain.WantsToMove)
            {
                Move(DirectionToTarget, deltaTime);
            }
            if (_brain.PullStep != Vector3.zero)
            {
                // Added to Kaelen's own movement (a dash is not cancelled); WaveDirector keeps him inside the arena.
                Target.transform.position += _brain.PullStep;
            }
            foreach (TelegraphView view in _views)
            {
                view.Refresh();
            }
        }

        private void HitKaelen(float damage)
        {
            Target.TakeDamage(new DamageInfo { Amount = damage }); // Kaelen only: never other enemies, never the decoy
        }

        private void AddView(Telegraph telegraph)
        {
            _views.Add(new TelegraphView(telegraph, _bossConfig, _telegraphMaterial));
        }

        private void RemoveView(Telegraph telegraph)
        {
            for (int i = 0; i < _views.Count; i++)
            {
                if (_views[i].Telegraph == telegraph)
                {
                    _views[i].Dispose();
                    _views.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
