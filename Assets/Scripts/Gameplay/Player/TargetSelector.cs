// Implements: design/game-brief.md MVP 1 (auto-attack targets the NEAREST enemy within detection range), story-001.

using System.Collections.Generic;
using MicroArena.Gameplay.Combat;
using UnityEngine;

namespace MicroArena.Gameplay.Player
{
    /// <summary>Pure nearest-target selection on the ground plane (XZ). Holds no state.</summary>
    public static class TargetSelector
    {
        /// <summary>Returns the living candidate closest to <paramref name="origin"/> within <paramref name="range"/>, or null.</summary>
        public static IDamageable FindNearest(Vector3 origin, IReadOnlyList<IDamageable> candidates, float range)
        {
            IDamageable best = null;
            float bestSqr = range * range;
            for (int i = 0; i < candidates.Count; i++)
            {
                IDamageable candidate = candidates[i];
                if (!candidate.IsAlive)
                {
                    continue;
                }
                Vector3 offset = candidate.Transform.position - origin;
                offset.y = 0f;
                float sqr = offset.sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    best = candidate;
                }
            }
            return best;
        }
    }
}
