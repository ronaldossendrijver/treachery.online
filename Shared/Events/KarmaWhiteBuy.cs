/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
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
            if (Player.Resources < 3) return Skin.Current.Format("You can't pay 3 {0}", Concept.Resource);
            if (!Game.WhiteCache.Contains(Card)) return "Invalid card";

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use ", TreacheryCardType.Karma, " to buy a card from their cache for ", new Payment(3));
        }
    }
}
