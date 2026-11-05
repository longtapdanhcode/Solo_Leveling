// Implements: production/epics/micro-arena-survivor/story-004-upgrade-cards.md, design/narrative/kich_ban_chuan.md
// section 6 (upgrade cards). All 9 cards (milestones 1 and 2). Values live on the assets, never in code.

using UnityEngine;

namespace MicroArena.Gameplay.Config
{
    /// <summary>Rarity tier of an upgrade card.</summary>
    public enum CardRarity
    {
        /// <summary>Thường.</summary>
        Common,
        /// <summary>Hiếm.</summary>
        Rare,
        /// <summary>Huyền Thoại.</summary>
        Legendary,
    }

    /// <summary>What a card does when picked.</summary>
    /// <remarks>Append new values at the end so existing assets keep their meaning. Repeat picks add the per-card numbers;
    /// fixed counts and radii live on <see cref="CardEffectConfig"/>.</remarks>
    public enum CardEffectKind
    {
        /// <summary>+<see cref="CardConfig.Percent"/>% base damage.</summary>
        DamagePercent,
        /// <summary>+<see cref="CardConfig.Percent"/>% walking speed.</summary>
        MoveSpeedPercent,
        /// <summary>+<see cref="CardConfig.MaxHealthBonus"/> max HP, then heal <see cref="CardConfig.HealAmount"/>.</summary>
        HealAndMaxHealth,
        /// <summary>Vết Chém Hỏa Ngục: slash hits add a burn stack of <see cref="CardConfig.Amount"/> DMG/s.</summary>
        Burn,
        /// <summary>Lôi Chấn: combo step 3 chains <see cref="CardConfig.Amount"/> lightning damage.</summary>
        ChainLightning,
        /// <summary>Dịch Chuyển Phân Thân: a dash leaves a decoy for <see cref="CardConfig.Amount"/> seconds.</summary>
        Decoy,
        /// <summary>Vòng Xoáy Huyết Thần: +<see cref="CardConfig.Percent"/>% swing radius, heal <see cref="CardConfig.HealPercentPerKill"/>% max HP per kill.</summary>
        BloodVortex,
        /// <summary>Sao Băng Giáng Trần: a meteor of <see cref="CardConfig.Amount"/> damage lands where a dash started.</summary>
        Meteor,
        /// <summary>Cuồng Nộ Hư Vô: below the HP threshold, +<see cref="CardConfig.Percent"/>% damage and +<see cref="CardConfig.CritChanceBonus"/> crit chance.</summary>
        VoidRage,
    }

    /// <summary>One upgrade card: text shown on the pick screen plus its effect values.</summary>
    [CreateAssetMenu(fileName = "Card", menuName = "Micro Arena Survivor/Card Config")]
    public sealed class CardConfig : ScriptableObject
    {
        [Tooltip("Stable id used to count stacks. Must be unique in the pool.")]
        [SerializeField] private string _id = "card";
        [SerializeField] private string _displayName = "Thẻ";
        [SerializeField, TextArea] private string _description = "";
        [SerializeField] private CardRarity _rarity = CardRarity.Common;
        [SerializeField] private CardEffectKind _effect = CardEffectKind.DamagePercent;

        [Header("Values (only those used by the effect matter)")]
        [Tooltip("Percent bonus: DamagePercent / MoveSpeedPercent / BloodVortex radius / VoidRage damage (15 = +15%).")]
        [SerializeField] private float _percent;
        [Tooltip("HP healed by HealAndMaxHealth.")]
        [SerializeField, Min(0f)] private float _healAmount;
        [Tooltip("Max HP added by HealAndMaxHealth.")]
        [SerializeField, Min(0f)] private float _maxHealthBonus;
        [Tooltip("Burn DMG/s per stack, chain damage, decoy seconds or meteor damage, depending on the effect.")]
        [SerializeField, Min(0f)] private float _amount;
        [Tooltip("BloodVortex: percent of max HP healed per enemy killed (4 = 4%).")]
        [SerializeField, Min(0f)] private float _healPercentPerKill;
        [Tooltip("VoidRage: crit chance added on the 0..1 scale while below the HP threshold (0.5 = +50 points).")]
        [SerializeField, Min(0f)] private float _critChanceBonus;

        // [PLACEHOLDER] Repeats are not specified; decision 2026-11-05: allowed and additive, 0 = unlimited.
        [Tooltip("Times this card can be picked in one run. 0 = unlimited. A capped card is no longer offered.")]
        [SerializeField, Min(0)] private int _maxStacks;

        /// <summary>Unique id.</summary>
        public string Id => _id;
        /// <summary>Name shown on the card.</summary>
        public string DisplayName => _displayName;
        /// <summary>Description shown on the card.</summary>
        public string Description => _description;
        /// <summary>Rarity tier.</summary>
        public CardRarity Rarity => _rarity;
        /// <summary>Effect kind.</summary>
        public CardEffectKind Effect => _effect;
        /// <summary>Percent bonus (15 = +15%).</summary>
        public float Percent => _percent;
        /// <summary>HP healed.</summary>
        public float HealAmount => _healAmount;
        /// <summary>Max HP added.</summary>
        public float MaxHealthBonus => _maxHealthBonus;
        /// <summary>Effect amount (burn DMG/s per stack, chain damage, decoy seconds, meteor damage).</summary>
        public float Amount => _amount;
        /// <summary>Percent of max HP healed per kill.</summary>
        public float HealPercentPerKill => _healPercentPerKill;
        /// <summary>Conditional crit chance bonus, 0..1 scale.</summary>
        public float CritChanceBonus => _critChanceBonus;
        /// <summary>Stack cap per run; 0 = unlimited.</summary>
        public int MaxStacks => _maxStacks;
    }
}
