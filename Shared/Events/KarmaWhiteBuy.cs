/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class KarmaWhiteBuy : GameEvent
    {
        public int _cardId;

        public KarmaWhiteBuy(Game game) : base(game)
        {
        }

        public KarmaWhiteBuy()
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

        public override Message Validate()
        {
            if (Player.Resources < 3) return Message.Express("You can't pay ", Payment.Of(3));
            if (!Game.WhiteCache.Contains(Card)) return Message.Express("Invalid card");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use ", TreacheryCardType.Karma, " to buy a card from their cache for ", Payment.Of(3));
        }
    }
}
