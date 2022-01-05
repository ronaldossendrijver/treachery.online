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

        public override string Validate()
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
                return new Message(Initiator, "{0} swapped one card from hand with the next card on bid.", Initiator);
            }
            else
            {
                return new Message(Initiator, "{0} don't swap a card from hand with the next card on bid.", Initiator);
            }
        }
    }
}
