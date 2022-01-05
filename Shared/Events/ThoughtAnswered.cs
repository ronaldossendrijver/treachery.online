/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class ThoughtAnswered : GameEvent
    {
        public int _cardId;

        public ThoughtAnswered(Game game) : base(game)
        {
        }

        public ThoughtAnswered()
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
            if (!ValidCards(Game, Player).Any()) return "";
            if (!ValidCards(Game, Player).Contains(Card)) return "Select a valid card to show";

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (Card == null)
            {
                return new Message(Initiator, "{0} don't have a card to show.", Initiator);
            }
            else
            {
                return new Message(Initiator, "{0} show one of their cards.", Initiator);
            }
            
        }

        public static IEnumerable<TreacheryCard> ValidCards(Game g, Player p)
        {
            if (p.TreacheryCards.Contains(g.CurrentThought.Card))
            {
                return new TreacheryCard[] { g.CurrentThought.Card };
            }
            else
            {
                return p.TreacheryCards;
            }
        }
    }
}
