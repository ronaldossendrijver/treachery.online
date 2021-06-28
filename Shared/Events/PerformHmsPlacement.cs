/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class PerformHmsPlacement : GameEvent
    {
        public int _targetId;

        public PerformHmsPlacement(Game game) : base(game)
        {
        }

        public PerformHmsPlacement()
        {
        }

        [JsonIgnore]
        public Location Target { get { return Game.Map.LocationLookup.Find(_targetId); } set { _targetId = Game.Map.LocationLookup.GetId(value); } }

        public override string Validate()
        {
            if (!ValidLocations(Game, Player).Contains(Target)) return "Invalid location";

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} position the Hidden Mobile Stronghold in {1}.", Initiator, Target);
        }

        public static IEnumerable<Location> ValidLocations(Game g, Player p)
        {
            if (p.Faction != Faction.Grey)
            {
                return g.Map.Locations.Where(l => !l.Territory.IsStronghold);
            }
            else
            {
                return g.Map.Locations.Where(l => !l.Territory.IsStronghold && l.Sector != g.SectorInStorm);
            }
        }
    }
}
