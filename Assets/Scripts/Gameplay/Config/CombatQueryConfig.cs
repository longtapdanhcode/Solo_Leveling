// Implements: design/game-brief.md MVP 1 and 3, stories 001-002. Sizing of the shared overlap query used by auto-attack,
// revive shock and enemy explosions.

using UnityEngine;

namespace MicroArena.Gameplay.Config
{
    /// <summary>Tuning for <see cref="Combat.DamageableQuery"/>. Create with the asset menu or <see cref="ScriptableObject.CreateInstance{T}()"/>.</summary>
    [CreateAssetMenu(fileName = "CombatQueryConfig", menuName = "Micro Arena Survivor/Combat Query Config")]
    public sealed class CombatQueryConfig : ScriptableObject
    {
        // [PLACEHOLDER] The brief sets no maximum of concurrent enemies (story 003 lists it as a placeholder).
        // The largest wave is 50 kills, so 128 colliders covers a full wave plus Kaelen, projectiles and boss parts
        // inside one query radius. Raise it if a query ever reports exactly this many hits.
        [Tooltip("[PLACEHOLDER] Max colliders a single overlap query can return; extra colliders are silently ignored.")]
        [SerializeField, Min(1)] private int _maxQueryColliders = 128;

        /// <summary>Max colliders a single overlap query can return.</summary>
        public int MaxQueryColliders => _maxQueryColliders;
    }
}
