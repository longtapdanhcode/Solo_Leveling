// Implements: production/epics/micro-arena-survivor/story-004-upgrade-cards.md (3 distinct random cards, rarity odds
// 60/30/10 from CardPoolConfig, capped cards excluded).

using System;
using System.Collections.Generic;
using MicroArena.Gameplay.Config;

namespace MicroArena.Gameplay.Cards
{
    /// <summary>
    /// Plain-C# card draw. Each slot first picks a rarity by weight among the rarities that still have an eligible,
    /// not-yet-drawn card (weights renormalized), then a uniform card of that rarity. Cards in one draw never repeat.
    /// </summary>
    public static class CardPool
    {
        private static readonly CardRarity[] Rarities = { CardRarity.Common, CardRarity.Rare, CardRarity.Legendary };

        /// <summary>Draws up to <see cref="CardPoolConfig.CardsPerPick"/> distinct cards (fewer if fewer are eligible).</summary>
        /// <param name="pool">Cards and weights.</param>
        /// <param name="stack">Current run stacks; capped cards are skipped. May be null.</param>
        /// <param name="roll">Random source returning 0..1 (exclusive of 1 preferred).</param>
        public static List<CardConfig> Draw(CardPoolConfig pool, ModifierStack stack, Func<float> roll)
        {
            return Draw(pool, stack, roll, null);
        }

        /// <summary>
        /// Like <see cref="Draw(CardPoolConfig, ModifierStack, Func{float})"/>, but <paramref name="preselected"/> cards come
        /// first (in order, repeats dropped, at most CardsPerPick) and the normal draw fills the remaining slots.
        /// The caller filters preselected cards for caps (see <see cref="ForcedCards.Eligible"/>).
        /// </summary>
        public static List<CardConfig> Draw(CardPoolConfig pool, ModifierStack stack, Func<float> roll, IReadOnlyList<CardConfig> preselected)
        {
            var drawn = new List<CardConfig>(pool.CardsPerPick);
            if (preselected != null)
            {
                foreach (CardConfig card in preselected)
                {
                    if (drawn.Count < pool.CardsPerPick && card != null && !drawn.Contains(card))
                    {
                        drawn.Add(card);
                    }
                }
            }

            var eligible = new List<CardConfig>();
            foreach (CardConfig card in pool.Cards)
            {
                if (card != null && !eligible.Contains(card) && !drawn.Contains(card) && (stack == null || !stack.IsCapped(card)))
                {
                    eligible.Add(card);
                }
            }

            var ofRarity = new List<CardConfig>();
            while (drawn.Count < pool.CardsPerPick && eligible.Count > 0)
            {
                float total = 0f;
                bool anyRarity = false;
                foreach (CardRarity rarity in Rarities)
                {
                    if (HasRarity(eligible, rarity))
                    {
                        anyRarity = true;
                        total += pool.WeightOf(rarity);
                    }
                }
                if (!anyRarity)
                {
                    break;
                }

                CardRarity chosen = PickRarity(pool, eligible, total, roll());
                ofRarity.Clear();
                foreach (CardConfig card in eligible)
                {
                    if (card.Rarity == chosen)
                    {
                        ofRarity.Add(card);
                    }
                }
                CardConfig pick = ofRarity[Index(roll(), ofRarity.Count)];
                drawn.Add(pick);
                eligible.Remove(pick);
            }
            return drawn;
        }

        private static CardRarity PickRarity(CardPoolConfig pool, List<CardConfig> eligible, float total, float value)
        {
            CardRarity last = CardRarity.Common;
            if (total <= 0f)
            {
                // Every remaining rarity has weight 0: fall back to the first rarity that still has cards.
                foreach (CardRarity rarity in Rarities)
                {
                    if (HasRarity(eligible, rarity))
                    {
                        return rarity;
                    }
                }
                return last;
            }

            float target = Clamp01(value) * total;
            foreach (CardRarity rarity in Rarities)
            {
                if (!HasRarity(eligible, rarity))
                {
                    continue;
                }
                last = rarity;
                float weight = pool.WeightOf(rarity);
                if (weight > 0f && target < weight)
                {
                    return rarity;
                }
                target -= weight;
            }
            return last;
        }

        private static bool HasRarity(List<CardConfig> cards, CardRarity rarity)
        {
            foreach (CardConfig card in cards)
            {
                if (card.Rarity == rarity)
                {
                    return true;
                }
            }
            return false;
        }

        private static int Index(float value, int count)
        {
            int index = (int)(Clamp01(value) * count);
            return index >= count ? count - 1 : index;
        }

        private static float Clamp01(float value)
        {
            return value < 0f ? 0f : value > 1f ? 1f : value;
        }
    }
}
