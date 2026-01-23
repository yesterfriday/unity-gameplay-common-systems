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

        public IReadOnlyList<CardDefinition> Peek(int n)
        {
            
            if (n <= 0 || _deck.Count <= 0)
            {
                return Array.Empty<CardDefinition>();
            }
            int count = Math.Min(n, _deck.Count);
            List<CardDefinition> result = new List<CardDefinition>(count);
            for (int i = 0; i < count; i++)
            {
                result.Add(_deck[i]);
            }
            return result;
        }

        public bool Shuffle(int seed)
        {
            if (_deck.Count < 2)
            {
                return false;
            }

            var rng = new Random(seed);   
            for (int i = _deck.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (_deck[i], _deck[j]) = (_deck[j], _deck[i]);
            }
            
            OnCardsChanged?.Invoke();
            return true;
        }

        public bool TryDraw(int requested, out int drawn, bool allowReshuffleDiscard = true)
        {
            drawn = 0;

            if (requested <= 0)
                return false;

            while (drawn < requested)
            {
                if (_deck.Count == 0)
                {
                    // 더 이상 뽑을 수 없으면 종료(부분 성공 가능)
                    if (!allowReshuffleDiscard || _discard.Count == 0)
                        break;

                    _deck.AddRange(_discard);
                    _discard.Clear();
                    Shuffle(Environment.TickCount);
                }

                var card = _deck[0];
                _deck.RemoveAt(0);
                _hand.Add(card);
                drawn++;
            }

            if (drawn <= 0)
                return false;

            OnCardsChanged?.Invoke();
            return true;
        }

    }

}
