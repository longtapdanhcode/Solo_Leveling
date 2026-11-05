// Implements: production/epics/micro-arena-survivor/story-004-upgrade-cards.md (3 cards per pick, rarity odds
// Thường 60% / Hiếm 30% / Huyền Thoại 10%), design/narrative/kich_ban_chuan.md section 6.

using System.Collections.Generic;
using UnityEngine;

namespace MicroArena.Gameplay.Config
{
    /// <summary>The cards that can be offered and the odds of each rarity.</summary>
    [CreateAssetMenu(fileName = "CardPoolConfig", menuName = "Micro Arena Survivor/Card Pool Config")]
    public sealed class CardPoolConfig : ScriptableObject
    {
        [SerializeField] private List<CardConfig> _cards = new List<CardConfig>();
        [Tooltip("Distinct cards shown per pick.")]
        [SerializeField, Min(1)] private int _cardsPerPick = 3;

        [Header("Rarity weights (relative; renormalized over rarities that still have cards)")]
        [SerializeField, Min(0f)] private float _commonWeight = 60f;
        [SerializeField, Min(0f)] private float _rareWeight = 30f;
        [SerializeField, Min(0f)] private float _legendaryWeight = 10f;

        /// <summary>Every card in the pool.</summary>
        public IReadOnlyList<CardConfig> Cards => _cards;
        /// <summary>Cards shown per pick.</summary>
        public int CardsPerPick => _cardsPerPick;

        /// <summary>Relative weight of <paramref name="rarity"/>.</summary>
        public float WeightOf(CardRarity rarity)
        {
            switch (rarity)
            {
                case CardRarity.Common: return _commonWeight;
                case CardRarity.Rare: return _rareWeight;
                default: return _legendaryWeight;
            }
        }
    }
}
