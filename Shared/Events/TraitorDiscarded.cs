/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class TraitorDiscarded : GameEvent
    {
        public int _traitorId;

        public TraitorDiscarded(Game game) : base(game)
        {
        }

        public TraitorDiscarded()
        {
        }

        [JsonIgnore]
        public IHero Traitor
        {
            get
            {
                return LeaderManager.HeroLookup.Find(_traitorId);
            }

            set
            {
                _traitorId = LeaderManager.HeroLookup.GetId(value);
            }
        }

        public override Message Validate()
        {
            if (Traitor == null) return Message.Express("Choose a traitor to discard");
            if (!Player.Traitors.Contains(Traitor)) return Message.Express("Invalid traitor");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " shuffle a traitor into the Traitor deck");
        }
    }
}
