/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class GreySwappedCardOnBid : GameEvent
    {
        public bool Passed;

        public int _cardId;

        public GreySwappedCardOnBid(Game game) : base(game)
        {
        }

        public GreySwappedCardOnBid()
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
                return Message.Express(Initiator, " swap a card from hand with the next card on bid");
            }
            else
            {
                return Message.Express(Initiator, " don't swap a card from hand with the next card on bid");
            }
        }
    }
}
