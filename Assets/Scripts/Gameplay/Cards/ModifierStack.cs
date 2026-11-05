// Implements: production/epics/micro-arena-survivor/story-004-upgrade-cards.md ("Thẻ sửa chỉ số qua lớp modifier lên
// PlayerConfig", Vòng Xoáy rule: swing radius only, never the 3 m detection range, Cuồng Nộ below 30% HP),
// design/narrative/kich_ban_chuan.md section 6.

using System.Collections.Generic;
using MicroArena.Gameplay.Config;
using UnityEngine;

namespace MicroArena.Gameplay.Cards
{
    /// <summary>
    /// Plain-C# run modifiers layered on top of <see cref="PlayerConfig"/> (the asset is never changed at runtime).
    /// Every number is additive across picks; <see cref="Reset"/> returns everything to neutral at a new run.
    /// Cuồng Nộ is additive INSIDE the damage percent sum: multiplier = 1 + (card percents + rage percent while raging) / 100.
    /// </summary>
    public sealed class ModifierStack
    {
        private readonly Dictionary<string, int> _stacks = new Dictionary<string, int>();
        private float _damagePercent;
        private float _moveSpeedPercent;
        private float _swingRadiusPercent;
        private float _rageThreshold;

        /// <summary>Unconditional base damage multiplier: 1 + sum of damage percents / 100 (Cuồng Nộ excluded).</summary>
        public float DamageMultiplier => 1f + _damagePercent / 100f;
        /// <summary>Walking speed multiplier: 1 + sum of move speed percents / 100.</summary>
        public float MoveSpeedMultiplier => 1f + _moveSpeedPercent / 100f;
        /// <summary>Slash radius multiplier: 1 + sum of swing radius percents / 100 (Vòng Xoáy Huyết Thần).</summary>
        public float SwingRadiusMultiplier => 1f + _swingRadiusPercent / 100f;
        /// <summary>Flat max HP added.</summary>
        public float MaxHealthBonus { get; private set; }
        /// <summary>Unconditional additive bonus on the 0..1 crit chance.</summary>
        public float CritChanceBonus { get; private set; }
        /// <summary>Burn DMG/s per stack (Vết Chém Hỏa Ngục). 0 = no burn.</summary>
        public float BurnDamagePerSecond { get; private set; }
        /// <summary>Damage of each lightning jump (Lôi Chấn). 0 = no chain.</summary>
        public float ChainDamage { get; private set; }
        /// <summary>Decoy lifetime, s (Dịch Chuyển Phân Thân). 0 = no decoy.</summary>
        public float DecoySeconds { get; private set; }
        /// <summary>Percent of max HP healed per kill (Vòng Xoáy Huyết Thần).</summary>
        public float KillHealPercent { get; private set; }
        /// <summary>Meteor damage (Sao Băng Giáng Trần). 0 = no meteor.</summary>
        public float MeteorDamage { get; private set; }
        /// <summary>Damage percent added while raging (Cuồng Nộ Hư Vô).</summary>
        public float RageDamagePercent { get; private set; }
        /// <summary>Crit chance added while raging, 0..1 scale.</summary>
        public float RageCritChanceBonus { get; private set; }
        /// <summary>HP fraction below which Cuồng Nộ is active. Set from <see cref="CardEffectConfig.VoidRageThreshold"/>; not cleared by <see cref="Reset"/>.</summary>
        public float RageThreshold => _rageThreshold;

        /// <summary>Times the card with <paramref name="cardId"/> was picked this run.</summary>
        public int StackCount(string cardId)
        {
            return cardId != null && _stacks.TryGetValue(cardId, out int count) ? count : 0;
        }

        /// <summary>True when <paramref name="card"/> has a cap and has reached it.</summary>
        public bool IsCapped(CardConfig card)
        {
            return card.MaxStacks > 0 && StackCount(card.Id) >= card.MaxStacks;
        }

        /// <summary>Adds <paramref name="card"/>'s numbers and counts one stack. Immediate effects (healing) are applied by <see cref="CardEffects"/>.</summary>
        public void Apply(CardConfig card)
        {
            switch (card.Effect)
            {
                case CardEffectKind.DamagePercent:
                    _damagePercent += card.Percent;
                    break;
                case CardEffectKind.MoveSpeedPercent:
                    _moveSpeedPercent += card.Percent;
                    break;
                case CardEffectKind.HealAndMaxHealth:
                    MaxHealthBonus += card.MaxHealthBonus;
                    break;
                case CardEffectKind.Burn:
                    BurnDamagePerSecond += card.Amount;
                    break;
                case CardEffectKind.ChainLightning:
                    ChainDamage += card.Amount;
                    break;
                case CardEffectKind.Decoy:
                    DecoySeconds += card.Amount;
                    break;
                case CardEffectKind.BloodVortex:
                    _swingRadiusPercent += card.Percent;
                    KillHealPercent += card.HealPercentPerKill;
                    break;
                case CardEffectKind.Meteor:
                    MeteorDamage += card.Amount;
                    break;
                case CardEffectKind.VoidRage:
                    RageDamagePercent += card.Percent;
                    RageCritChanceBonus += card.CritChanceBonus;
                    break;
            }
            _stacks[card.Id] = StackCount(card.Id) + 1;
        }

        /// <summary>Sets the Cuồng Nộ HP fraction threshold (from config).</summary>
        public void SetRageThreshold(float threshold)
        {
            _rageThreshold = threshold;
        }

        /// <summary>Adds a swing radius percent (30 = +30%).</summary>
        public void AddSwingRadiusPercent(float percent)
        {
            _swingRadiusPercent += percent;
        }

        /// <summary>Adds unconditional crit chance (0.50 = +50 percentage points).</summary>
        public void AddCritChanceBonus(float bonus)
        {
            CritChanceBonus += bonus;
        }

        /// <summary>True while Cuồng Nộ is owned and HP / max is strictly below <see cref="RageThreshold"/>.</summary>
        public bool IsRaging(float healthFraction)
        {
            return (RageDamagePercent > 0f || RageCritChanceBonus > 0f) && healthFraction < _rageThreshold;
        }

        /// <summary>Slash damage multiplier at <paramref name="healthFraction"/> (Cuồng Nộ added to the percent sum while raging).</summary>
        public float DamageMultiplierAt(float healthFraction)
        {
            float percent = _damagePercent + (IsRaging(healthFraction) ? RageDamagePercent : 0f);
            return 1f + percent / 100f;
        }

        /// <summary>Crit chance at <paramref name="healthFraction"/>, clamped to 0..1.</summary>
        public float CritChanceAt(float baseChance, float healthFraction)
        {
            float rage = IsRaging(healthFraction) ? RageCritChanceBonus : 0f;
            return Mathf.Clamp01(baseChance + CritChanceBonus + rage);
        }

        /// <summary>Slash reach after modifiers.</summary>
        public float SwingRadius(float baseRadius)
        {
            return baseRadius * SwingRadiusMultiplier;
        }

        /// <summary>Auto-target detection range. Deliberately NEVER scaled by any card (Vòng Xoáy rule).</summary>
        public float DetectionRange(float baseRange)
        {
            return baseRange;
        }

        /// <summary>Unconditional crit chance after modifiers, clamped to 0..1.</summary>
        public float CritChance(float baseChance)
        {
            return Mathf.Clamp01(baseChance + CritChanceBonus);
        }

        /// <summary>HP healed per kill for a max HP of <paramref name="maxHealth"/>.</summary>
        public float KillHeal(float maxHealth)
        {
            return maxHealth * KillHealPercent / 100f;
        }

        /// <summary>Clears every bonus and stack count (the rage threshold is config and stays).</summary>
        public void Reset()
        {
            _stacks.Clear();
            _damagePercent = 0f;
            _moveSpeedPercent = 0f;
            _swingRadiusPercent = 0f;
            MaxHealthBonus = 0f;
            CritChanceBonus = 0f;
            BurnDamagePerSecond = 0f;
            ChainDamage = 0f;
            DecoySeconds = 0f;
            KillHealPercent = 0f;
            MeteorDamage = 0f;
            RageDamagePercent = 0f;
            RageCritChanceBonus = 0f;
        }
    }
}
