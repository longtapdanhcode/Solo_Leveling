// Implements: production/epics/micro-arena-survivor/story-004-upgrade-cards.md (playtest helper, decision 2026-11-05):
// the -arenaForceCards id,id,id launch argument (development builds only) puts chosen cards first at every card pick.

using System;
using System.Collections.Generic;
using MicroArena.Gameplay.Config;

namespace MicroArena.Gameplay.Cards
{
    /// <summary>Plain-C# parsing and selection for forced cards. Holds no state.</summary>
    public static class ForcedCards
    {
        /// <summary>
        /// Returns the comma-separated ids following <paramref name="argumentName"/> in <paramref name="args"/> (trimmed,
        /// empty entries dropped). Empty when the argument is absent or has no value (a following "-option" is not a value).
        /// </summary>
        public static List<string> ParseIds(string[] args, string argumentName)
        {
            var ids = new List<string>();
            if (args == null)
            {
                return ids;
            }
            int index = Array.IndexOf(args, argumentName);
            if (index < 0 || index + 1 >= args.Length || args[index + 1].StartsWith("-", StringComparison.Ordinal))
            {
                return ids;
            }
            foreach (string part in args[index + 1].Split(','))
            {
                string id = part.Trim();
                if (id.Length > 0)
                {
                    ids.Add(id);
                }
            }
            return ids;
        }

        /// <summary>Maps <paramref name="ids"/> to pool cards in order, dropping repeats; unknown ids are reported to <paramref name="warn"/> and skipped.</summary>
        public static List<CardConfig> Resolve(CardPoolConfig pool, IReadOnlyList<string> ids, Action<string> warn)
        {
            var cards = new List<CardConfig>();
            foreach (string id in ids)
            {
                CardConfig match = null;
                foreach (CardConfig card in pool.Cards)
                {
                    if (card != null && card.Id == id)
                    {
                        match = card;
                        break;
                    }
                }
                if (match == null)
                {
                    warn?.Invoke($"-arenaForceCards: unknown card id '{id}' skipped.");
                }
                else if (!cards.Contains(match))
                {
                    cards.Add(match);
                }
            }
            return cards;
        }

        /// <summary>The forced cards usable at this pick: not capped by <paramref name="stack"/>, at most <paramref name="max"/>, in order.</summary>
        public static List<CardConfig> Eligible(IReadOnlyList<CardConfig> forced, ModifierStack stack, int max)
        {
            var result = new List<CardConfig>();
            foreach (CardConfig card in forced)
            {
                if (result.Count >= max)
                {
                    break;
                }
                if (stack == null || !stack.IsCapped(card))
                {
                    result.Add(card);
                }
            }
            return result;
        }
    }
}
