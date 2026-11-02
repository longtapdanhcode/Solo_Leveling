// Implements: design/game-brief.md MVP 1, story-001. Shared "who is near me" query for detection, slash and revive shock.

using System.Collections.Generic;
using UnityEngine;

namespace MicroArena.Gameplay.Combat
{
    /// <summary>Finds <see cref="IDamageable"/>s inside a sphere. Instance-owned buffer, no static state.</summary>
    public sealed class DamageableQuery
    {
        private readonly Collider[] _buffer;

        /// <param name="maxColliders">Max colliders inspected per query, see <see cref="Config.CombatQueryConfig.MaxQueryColliders"/>.</param>
        public DamageableQuery(int maxColliders)
        {
            _buffer = new Collider[maxColliders];
        }

        /// <summary>Fills <paramref name="results"/> with distinct living damageables within <paramref name="radius"/> of <paramref name="center"/>.</summary>
        /// <param name="exclude">Transform of a damageable to skip (the caller itself); may be null.</param>
        /// <returns>Number of results.</returns>
        public int Gather(Vector3 center, float radius, LayerMask layers, Transform exclude, List<IDamageable> results)
        {
            results.Clear();
            int count = Physics.OverlapSphereNonAlloc(center, radius, _buffer, layers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                IDamageable damageable = _buffer[i].GetComponentInParent<IDamageable>();
                if (damageable == null || !damageable.IsAlive || damageable.Transform == exclude || results.Contains(damageable))
                {
                    continue;
                }
                results.Add(damageable);
            }
            return results.Count;
        }
    }
}
