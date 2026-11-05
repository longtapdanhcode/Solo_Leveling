// Implements: production/epics/micro-arena-survivor/story-004-upgrade-cards.md (a picked card applies at once),
// design/narrative/kich_ban_chuan.md section 6 (Huyết Nhũ: +max HP, heal).

using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;

namespace MicroArena.Gameplay.Cards
{
    /// <summary>Applies a picked card: stat bonuses go into the <see cref="ModifierStack"/>, immediate effects hit Kaelen's <see cref="Health"/>.</summary>
    public static class CardEffects
    {
        /// <summary>Applies <paramref name="card"/>. <paramref name="health"/> may be null (no immediate effect then).</summary>
        public static void Apply(CardConfig card, ModifierStack stack, Health health)
        {
            stack.Apply(card);
            if (card.Effect == CardEffectKind.HealAndMaxHealth && health != null)
            {
                // Raise the max first so the heal can fill the new headroom; Heal clamps to max.
                health.SetMaxHealthBonus(stack.MaxHealthBonus);
                health.Heal(card.HealAmount);
            }
        }
    }
}
