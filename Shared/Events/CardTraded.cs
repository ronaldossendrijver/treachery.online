/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class CardTraded : GameEvent
    {
        public int _cardId;
        public Faction Target;

        public CardTraded(Game game) : base(game)
        {
        }

        public CardTraded()
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
            return new Message(Initiator, "{0} offer their ally a card trade.", Initiator, TreacheryCardType.Useless);
        }

        public static IEnumerable<TreacheryCard> ValidCards(Player p)
        {
            return p.TreacheryCards;
        }
    }
}
