/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class GreyRemovedCardFromAuction : GameEvent
    {
        public bool PutOnTop;

        public int _cardId;

        public GreyRemovedCardFromAuction(Game game) : base(game)
        {
        }

        public GreyRemovedCardFromAuction()
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
            if (PutOnTop)
            {
                return Message.Express(Initiator, " put a card on top of the Treachery Card deck");
            }
            else
            {
                return Message.Express(Initiator, " put a card at the bottom of the Treachery Card deck");
            }
        }
    }
}
