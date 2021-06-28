/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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

        public override string Validate()
        {
            var p = Player;

            if (!p.Traitors.Contains(SelectedTraitor)) return "Invalid traitor";

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} chose a traitor.", Initiator);
        }
    }
}
