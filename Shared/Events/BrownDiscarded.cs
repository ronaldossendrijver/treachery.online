/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class BrownDiscarded : GameEvent
    {
        public int _cardId;
        public FactionAdvantage Prevented;

        public BrownDiscarded(Game game) : base(game)
        {
        }

        public BrownDiscarded()
        {
        }

        [JsonIgnore]
        public TreacheryCard Card
        {
            get
            {
                return TreacheryCardManager.Get(_cardId);
            }
            set
            {
                _cardId = TreacheryCardManager.GetId(value);
            }
        }

        public override string Validate()
        {
            if (!ValidCards(Game, Player).Contains(Card)) return "Invalid card";

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (Card.Type == TreacheryCardType.Useless)
            {
                return new Message(Initiator, "{0} receive 2 by discarding a {1} card.", Initiator, TreacheryCardType.Useless);
            }
            else
            {
                return new Message(Initiator, "{0} receive 3 by discarding a duplicate {1}.", Initiator, Card);
            }
        }

        public static IEnumerable<TreacheryCard> ValidCards(Game g, Player p)
        {
            return p.TreacheryCards.Where(c => c.Type == TreacheryCardType.Useless || p.TreacheryCards.Count(toCount => toCount.Type == c.Type) > 1);
        }
    }
}
