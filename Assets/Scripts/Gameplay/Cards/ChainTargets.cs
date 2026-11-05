// Implements: production/epics/micro-arena-survivor/story-004-upgrade-cards.md (Lôi Chấn: lightning jumps to the 3 nearest
// other enemies within 4 m of the struck enemy), design/narrative/kich_ban_chuan.md section 6.2.

using System.Collections.Generic;
using MicroArena.Gameplay.Combat;
using UnityEngine;

namespace MicroArena.Gameplay.Cards
{
    /// <summary>Pure chain-target selection on the ground plane (XZ). Holds no state.</summary>
    public static class ChainTargets
    {
        /// <summary>
        /// Fills <paramref name="results"/> with up to <paramref name="count"/> living candidates nearest to
        /// <paramref name="sourcePosition"/> within <paramref name="range"/>, nearest first, never <paramref name="source"/>.
        /// </summary>
        public static void Select(IDamageable source, Vector3 sourcePosition, IReadOnlyList<IDamageable> candidates, float range,
            int count, List<IDamageable> results)
        {
            results.Clear();
            if (count <= 0)
            {
                return;
            }
            float maxSqr = range * range;
            for (int c = 0; c < candidates.Count; c++)
            {
                IDamageable candidate = candidates[c];
                if (candidate == null || ReferenceEquals(candidate, source) || !candidate.IsAlive || results.Contains(candidate))
                {
                    continue;
                }
                float sqr = FlatSqr(candidate.Transform.position, sourcePosition);
                if (sqr > maxSqr)
                {
                    continue;
                }
                int index = results.Count;
                while (index > 0 && FlatSqr(results[index - 1].Transform.position, sourcePosition) > sqr)
                {
                    index--;
                }
                if (index < count)
                {
                    results.Insert(index, candidate);
                    if (results.Count > count)
                    {
                        results.RemoveAt(count);
                    }
                }
            }
        }

        private static float FlatSqr(Vector3 a, Vector3 b)
        {
            Vector3 offset = a - b;
            offset.y = 0f;
            return offset.sqrMagnitude;
        }
    }
}
