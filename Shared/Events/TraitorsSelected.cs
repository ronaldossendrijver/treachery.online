/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class TraitorsSelected : GameEvent
    {
        public int _traitorId;

        public TraitorsSelected(Game game) : base(game)
        {
        }

        public TraitorsSelected()
        {
        }

        [JsonIgnore]
        public IHero SelectedTraitor { get { return LeaderManager.HeroLookup.Find(_traitorId); } set { _traitorId = LeaderManager.HeroLookup.GetId(value); } }

        public override Message Validate()
        {
            if (!Player.Traitors.Contains(SelectedTraitor)) return Message.Express("Invalid traitor");
            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " pick their traitor");
        }
    }
}
