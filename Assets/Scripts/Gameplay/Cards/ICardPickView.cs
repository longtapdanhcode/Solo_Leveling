// Implements: production/epics/micro-arena-survivor/story-004-upgrade-cards.md (gameplay-to-UI contract for the card pick).

using System;
using System.Collections.Generic;
using MicroArena.Gameplay.Config;

namespace MicroArena.Gameplay.Cards
{
    /// <summary>A screen that offers cards and reports the player's choice. Gameplay depends on this, never on UI types.</summary>
    public interface ICardPickView
    {
        /// <summary>True while the cards are on screen.</summary>
        bool IsShown { get; }

        /// <summary>Shows <paramref name="cards"/> and calls <paramref name="onPicked"/> once with the chosen index.</summary>
        void Show(IReadOnlyList<CardConfig> cards, Action<int> onPicked);

        /// <summary>Hides the screen without a pick. Safe to call when already hidden.</summary>
        void Hide();
    }
}
