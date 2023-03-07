/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Treachery.Shared
{
    public class DiscoveryEntered : PlacementEvent
    {
        public DiscoveryEntered(Game game) : base(game)
        {
        }

        public DiscoveryEntered()
        {
        }

        //public int _targetId;

        /*
        [JsonIgnore]
        public DiscoveryStronghold Location { get { return Game.Map.LocationLookup.Find(_targetId) as DiscoveryStronghold; } set { _targetId = Game.Map.LocationLookup.GetId(value); } }
        */
        /*
        [JsonIgnore]
        public Location To => Location;
        */

        public override Message Validate()
        {
            if (!Passed)
            {
                if (!ValidTargets(Game, Player).Contains(To)) return Message.Express("You can't move there");
            }

            return null;
        }

        public static IEnumerable<DiscoveredLocation> ValidTargets(Game g, Player p) =>
            g.JustRevealedDiscoveryStrongholds.Where(ds => p.AnyForcesIn(ds.AttachedToLocation.Territory) > 0);

        public static IEnumerable<Location> ValidSources(Player p, DiscoveredLocation ds) => ds.AttachedToLocation.Territory.Locations.Where(l => p.AnyForcesIn(l) > 0);

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (!Passed)
            {
                return Message.Express(Initiator, " enter ", To);
            }
            else
            {
                return Message.Express(Initiator, " don't enter ");
            }
        }
    }
}
