/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class PerformBluePlacement : GameEvent
    {
        public int _targetId;

        public PerformBluePlacement(Game game) : base(game)
        {
        }

        public PerformBluePlacement()
        {
        }

        [JsonIgnore]
        public Location Target { get { return Game.Map.LocationLookup.Find(_targetId); } set { _targetId = Game.Map.LocationLookup.GetId(value); } }

        public override string Validate()
        {
            if (!ValidLocations(Game).Contains(Target)) return "Invalid location";

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " position themselves in ", Target);
        }

        public static IEnumerable<Location> ValidLocations(Game g)
        {
            {
                if (g.Applicable(Rule.BlueFirstForceInAnyTerritory))
                {
                    return g.Map.Locations.Where(l => l != g.Map.HiddenMobileStronghold);
                }
                else
                {
                    return new Location[] { g.Map.PolarSink };
                }
            }
        }
    }
}
