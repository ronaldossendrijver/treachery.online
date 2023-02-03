/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class Discarded : GameEvent
    {
        public int _cardId;

        public Discarded(Game game) : base(game)
        {
        }

        public Discarded()
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
            if (Card == null) return Message.Express("Choose a card to discard");
            if (!Player.TreacheryCards.Contains(Card)) return Message.Express("Invalid card");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " discard a treachery card");
        }
    }
}
