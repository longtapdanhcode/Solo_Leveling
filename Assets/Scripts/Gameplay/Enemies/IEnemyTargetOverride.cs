// Implements: production/epics/micro-arena-survivor/story-004-upgrade-cards.md (Dịch Chuyển Phân Thân decoy draws enemies).

using UnityEngine;

namespace MicroArena.Gameplay.Enemies
{
    /// <summary>Optional point enemies chase instead of Kaelen (the decoy). Enemies still damage only Kaelen.</summary>
    public interface IEnemyTargetOverride
    {
        /// <summary>True while an override point exists; <paramref name="position"/> is then the point to chase.</summary>
        bool TryGetTargetPosition(out Vector3 position);
    }
}
