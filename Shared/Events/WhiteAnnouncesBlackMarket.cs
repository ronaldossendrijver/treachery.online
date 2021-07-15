/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class WhiteAnnouncesBlackMarket : GameEvent
    {
        public int _cardId;
        public bool Passed;
        public AuctionType AuctionType;
        public int Direction;

        public WhiteAnnouncesBlackMarket(Game game) : base(game)
        {
        }

        public WhiteAnnouncesBlackMarket()
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
            if (!ValidCards(Player).Contains(Card)) return "Invalid card";

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (!Passed)
            {
                return new Message(Initiator, "{0} put a card on the black market using {1}", Initiator, AuctionType);
            }
            else
            {
                return new Message(Initiator, "{0} don't put a card on the black market", Initiator);
            }
        }

        public static IEnumerable<TreacheryCard> ValidCards(Player p)
        {
            return p.TreacheryCards;
        }
    }
}
