using System;
using System.Collections.Generic;

namespace Yesterfriday.GameplayCommonSystems.Cards
{
    public sealed class CardsModel
    {
        private readonly List<CardDefinition> _deck = new List<CardDefinition>();
        private readonly List<CardDefinition> _hand = new List<CardDefinition>();
        private readonly List<CardDefinition> _discard = new List<CardDefinition>();

        public event Action OnCardsChanged;

        public CardsModel(IReadOnlyList<CardDefinition> decklist)
        {
            Reset(decklist);
        }

        private bool Reset(IReadOnlyList<CardDefinition> decklist)
        {
            if (decklist == null)
            {
                return false;
            }

            for (int i = 0; i < decklist.Count; i++)
            {
                if(decklist[i] == null)
                {
                    return false;
                }
            }
 
            _deck.Clear();
            _deck.AddRange(decklist);
            
            _hand.Clear();
            _discard.Clear();

            OnCardsChanged?.Invoke();
            return true;
        }
        public int GetCount(CardsZone zone)
        {
            switch (zone)
            {
                case CardsZone.Deck:
                    return _deck.Count;
                case CardsZone.Hand:
                    return _hand.Count;
                case CardsZone.Discard:
                    return _discard.Count;
                default:
                    throw new ArgumentOutOfRangeException(nameof(zone), zone, null);
            }
        }
    }

}
