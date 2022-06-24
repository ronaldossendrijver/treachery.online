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

        public override Message Validate()
        {
            if (!ValidLocations(Game).Contains(Target)) return Message.Express("Invalid location");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " position themselves in ", Target);
        }

        public static bool BlueMayPlaceFirstForceInAnyTerritory(Game g) => g.Applicable(Rule.BlueFirstForceInAnyTerritory) || g.Version >= 144 && g.Applicable(Rule.BlueAdvisors);

        public static IEnumerable<Location> ValidLocations(Game g)
        {
            {
                if (BlueMayPlaceFirstForceInAnyTerritory(g))
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
