// Implements: production/epics/micro-arena-survivor/story-004-upgrade-cards.md (Rare/Legendary cards, milestone 2),
// design/narrative/kich_ban_chuan.md sections 6.2 and 6.3. Fixed counts, radii and timings that do NOT grow when a card
// is picked again (decision 2026-11-05); the per-pick numbers live on each CardConfig.

using UnityEngine;

namespace MicroArena.Gameplay.Config
{
    /// <summary>Shared tuning for the Rare and Legendary card effects, plus their placeholder visuals.</summary>
    [CreateAssetMenu(fileName = "CardEffectConfig", menuName = "Micro Arena Survivor/Card Effect Config")]
    public sealed class CardEffectConfig : ScriptableObject
    {
        [Header("Vết Chém Hỏa Ngục")]
        [Tooltip("Burn stacks one enemy can carry.")]
        [SerializeField, Min(1)] private int _maxBurnStacks = 3;
        [Tooltip("Seconds a burn lasts; every new slash hit restarts it.")]
        [SerializeField, Min(0.01f)] private float _burnDuration = 3f;

        [Header("Lôi Chấn")]
        [Tooltip("1-based combo step that releases the lightning.")]
        [SerializeField, Min(1)] private int _chainComboStep = 3;
        [Tooltip("Other enemies the lightning jumps to.")]
        [SerializeField, Min(1)] private int _chainTargets = 3;
        // [PLACEHOLDER] Chain reach is not specified in the script; accepted 2026-11-05.
        [Tooltip("[PLACEHOLDER] Reach (m) of the lightning from the struck enemy.")]
        [SerializeField, Min(0f)] private float _chainRange = 4f;

        [Header("Sao Băng Giáng Trần")]
        [Tooltip("Seconds after the dash starts before the meteor lands (scaled time).")]
        [SerializeField, Min(0f)] private float _meteorDelay = 0.4f;
        // [PLACEHOLDER] Blast radius is not specified in the script; accepted 2026-11-05.
        [Tooltip("[PLACEHOLDER] Blast radius (m).")]
        [SerializeField, Min(0f)] private float _meteorRadius = 3f;
        [Tooltip("Stun seconds on every enemy hit by the meteor.")]
        [SerializeField, Min(0f)] private float _meteorStun = 1f;

        [Header("Cuồng Nộ Hư Vô")]
        [Tooltip("Active while HP / max HP is strictly below this fraction.")]
        [SerializeField, Range(0f, 1f)] private float _voidRageThreshold = 0.3f;

        [Header("Placeholder visuals (polish in story 007)")]
        [SerializeField] private Material _decoyMaterial;
        [SerializeField] private Material _meteorMaterial;
        [Tooltip("Seconds the meteor marker stays after impact.")]
        [SerializeField, Min(0f)] private float _meteorMarkerLinger = 0.3f;
        [Tooltip("Height (m) of the flat meteor marker disc.")]
        [SerializeField, Min(0.001f)] private float _meteorMarkerThickness = 0.05f;

        /// <summary>Max burn stacks per enemy.</summary>
        public int MaxBurnStacks => _maxBurnStacks;
        /// <summary>Burn duration, s.</summary>
        public float BurnDuration => _burnDuration;
        /// <summary>1-based combo step that chains.</summary>
        public int ChainComboStep => _chainComboStep;
        /// <summary>Chain target count.</summary>
        public int ChainTargets => _chainTargets;
        /// <summary>Chain reach, m.</summary>
        public float ChainRange => _chainRange;
        /// <summary>Meteor delay, s.</summary>
        public float MeteorDelay => _meteorDelay;
        /// <summary>Meteor radius, m.</summary>
        public float MeteorRadius => _meteorRadius;
        /// <summary>Meteor stun, s.</summary>
        public float MeteorStun => _meteorStun;
        /// <summary>Cuồng Nộ HP fraction threshold.</summary>
        public float VoidRageThreshold => _voidRageThreshold;
        /// <summary>Decoy material (may be null).</summary>
        public Material DecoyMaterial => _decoyMaterial;
        /// <summary>Meteor marker material (may be null).</summary>
        public Material MeteorMaterial => _meteorMaterial;
        /// <summary>Marker linger after impact, s.</summary>
        public float MeteorMarkerLinger => _meteorMarkerLinger;
        /// <summary>Marker disc thickness, m.</summary>
        public float MeteorMarkerThickness => _meteorMarkerThickness;
    }
}
