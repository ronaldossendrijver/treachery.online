/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;


namespace Treachery.Shared
{
    public class LoserConcluded : GameEvent
    {
        public LoserConcluded(Game game) : base(game)
        {
        }

        public LoserConcluded()
        {
        }

        public int _keptCardId;

        [JsonIgnore]
        public TreacheryCard KeptCard
        {
            get
            {
                return TreacheryCardManager.Get(_keptCardId);
            }
            set
            {
                _keptCardId = TreacheryCardManager.GetId(value);
            }
        }

        public override Message Validate()
        {
            return null;
        }


        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (KeptCard != null)
            {
                return Message.Express(Initiator, " keep ", KeptCard);
            }
            else
            {
                return Message.Express(Initiator, " don't keep any cards");
            }
        }
    }
}
