// Implements: design/narrative/kich_ban_chuan.md section 5.3, production/epics/micro-arena-survivor/story-002-enemies.md.

using System.Collections.Generic;
using MicroArena.Gameplay.Combat;
using UnityEngine;

namespace MicroArena.Gameplay.Enemies
{
    /// <summary>
    /// Suicide runner. Rushes Kaelen; within <c>FuseTriggerDistance</c> it stops and blinks red for
    /// <c>FuseDuration</c>, then explodes for <c>Damage</c> to every non-enemy damageable in <c>ExplosionRadius</c>.
    /// <para>Transitions: Rushing -> Fusing within the trigger distance; Fusing -> explode (dies) when the fuse ends.
    /// Stun pauses the fuse without cancelling it. Killed while fusing = no explosion (story default, [PLACEHOLDER]).</para>
    /// </summary>
    public sealed class VolatileHusk : EnemyBase
    {
        // Visual only: warning colour of the blink.
        [SerializeField] private Color _flashColor = Color.red;

        private readonly List<IDamageable> _scratch = new List<IDamageable>();
        private DamageableQuery _query;
        private bool _fusing;
        private float _fuseElapsed;

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            _query = new DamageableQuery(QueryConfig.MaxQueryColliders);
        }

        /// <inheritdoc />
        protected override void Act(float deltaTime)
        {
            if (!_fusing)
            {
                if (DistanceToTarget <= Config.FuseTriggerDistance)
                {
                    _fusing = true;
                }
                else
                {
                    Move(DirectionToTarget, deltaTime);
                }
                return;
            }

            _fuseElapsed += deltaTime;
            bool lit = Mathf.FloorToInt(_fuseElapsed / Config.BlinkInterval) % 2 == 0;
            SetColor(lit ? _flashColor : BaseColor);
            if (_fuseElapsed >= Config.FuseDuration)
            {
                Explode();
            }
        }

        private void Explode()
        {
            Physics.SyncTransforms();
            _query.Gather(transform.position, Config.ExplosionRadius, Physics.AllLayers, transform, _scratch);
            foreach (IDamageable target in _scratch)
            {
                if (target is EnemyBase)
                {
                    continue; // no friendly fire: the explosion is aimed at Kaelen
                }
                target.TakeDamage(new DamageInfo { Amount = Config.Damage });
            }
            // Self-explosion is a death and raises EnemyDied like any other; it counts as a wave kill (decided 2026-11-02).
            Die();
        }
    }
}
