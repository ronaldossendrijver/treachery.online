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
        public int _requestedCardId;

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

        [JsonIgnore]
        public TreacheryCard RequestedCard
        {
            get
            {
                return TreacheryCardManager.Get(_requestedCardId);
            }
            set
            {
                _requestedCardId = TreacheryCardManager.GetId(value);
            }
        }

        public override string Validate()
        {
            if (!ValidCards(Player).Contains(Card)) return "Invalid card";
            if (!Player.AlliedPlayer.TreacheryCards.Any()) return "Your ally does not have cards to trade";

            var targetPlayer = Game.GetPlayer(Target);
            if (targetPlayer == null) return "Invalid target player";

            if (RequestedCard != null && !Player.AlliedPlayer.IsBot) return "You can only select a card from a Bot ally";
            if (RequestedCard != null && !ValidCards(Game.GetPlayer(Target)).Contains(RequestedCard)) return "Invalid requested card";

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
