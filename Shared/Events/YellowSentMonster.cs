/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class YellowSentMonster : GameEvent
    {
        public int _territoryId;

        public YellowSentMonster(Game game) : base(game)
        {
        }

        public YellowSentMonster()
        {
        }

        [JsonIgnore]
        public Territory Territory { get { return Game.Map.TerritoryLookup.Find(_territoryId); } set { _territoryId = Game.Map.TerritoryLookup.GetId(value); } }

        public override string Validate()
        {
            if (Territory == null) return "No territory selected.";
            if (Territory.IsProtectedFromWorm) return "Selected territory is protected.";

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} send {1} to {2}.", Initiator, Concept.Monster, Territory);
        }

        public static IEnumerable<Territory> ValidTargets(Game g)
        {
            return g.Map.Territories.Where(t => !t.IsProtectedFromWorm);
        }
    }
}
