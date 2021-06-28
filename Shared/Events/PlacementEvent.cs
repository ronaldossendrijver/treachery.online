/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public abstract class PlacementEvent : GameEvent
    {
        public int _toId;

        [JsonIgnore]
        public Location To { get { return Game.Map.LocationLookup.Find(_toId); } set { _toId = Game.Map.LocationLookup.GetId(value); } }

        public string _forceLocations = "";

        protected PlacementEvent(Game game) : base(game)
        {
        }

        protected PlacementEvent()
        {
        }

        [JsonIgnore]
        public Dictionary<Location, Battalion> ForceLocations
        {
            get
            {
                return ParseForceLocations(Game, _forceLocations);
            }
            set
            {
                _forceLocations = ForceLocationsString(Game, value);
            }
        }

        public bool Passed { get; set; }

        protected static string ForceLocationsString(Game g, Dictionary<Location, Battalion> forceLocations)
        {
            return string.Join(',', forceLocations.Select(x => g.Map.LocationLookup.GetId(x.Key) + ":" + x.Value.AmountOfForces + "|" + x.Value.AmountOfSpecialForces));
        }

        protected static Dictionary<Location, Battalion> ParseForceLocations(Game g, string forceLocations)
        {
            var result = new Dictionary<Location, Battalion>();
            if (forceLocations != null && forceLocations.Length > 0)
            {
                foreach (var locationAmountPair in forceLocations.Split(','))
                {
                    var locationAndAmounts = locationAmountPair.Split(':');
                    var location = g.Map.LocationLookup.Find(Convert.ToInt32(locationAndAmounts[0]));
                    var amounts = locationAndAmounts[1].Split('|');
                    var amountOfNormalForces = Convert.ToInt32(amounts[0]);
                    var amountOfNormalSpecialForces = Convert.ToInt32(amounts[1]);
                    result.Add(location, new Battalion() { Faction = Faction.None, AmountOfForces = amountOfNormalForces, AmountOfSpecialForces = amountOfNormalSpecialForces });
                }
            }

            return result;
        }

        public static IEnumerable<Location> ValidTargets(Game g, Player p, Location location, Battalion battalion)
        {
            var dict = new Dictionary<Location, Battalion>
            {
                { location, battalion }
            };

            return ValidTargets(g, p, dict);
        }

        public static IEnumerable<Location> ValidTargets(Game g, Player p, Dictionary<Location, Battalion> forces)
        {
            IEnumerable<Location> result = null;

            foreach (var fl in forces.Where(b => b.Value.AmountOfForces + b.Value.AmountOfSpecialForces > 0))
            {
                var target = ValidMoveToTargets(g, p, fl.Key, fl.Value);

                if (result == null)
                {
                    result = target;
                }
                else
                {
                    result = result.Intersect(target);
                }
            }

            if (result != null)
            {
                return result;
            }
            else
            {
                return new Location[] { };
            }
        }

        private static IEnumerable<Location> ValidMoveToTargets(Game g, Player p, Location from, Battalion moved)
        {
            int maximumDistance = g.DetermineMaximumMoveDistance(p, moved);
            bool mayMoveIntoStorm = p.Faction == Faction.Yellow && g.Applicable(Rule.YellowMayMoveIntoStorm) && g.Applicable(Rule.YellowStormLosses);
            var neighbours = g.Map.FindNeighbours(from, maximumDistance, mayMoveIntoStorm, p.Faction, g.SectorInStorm, g.ForcesOnPlanet);

            return neighbours;

            /*if (g.Version <= 85 || p.Ally == Faction.None)
            {
                return neighbours;
            }
            else
            {
                var ally = p.AlliedPlayer;
                return neighbours.Where(l => l == g.Map.PolarSink || ally.AnyForcesIn(l.Territory) == 0);
            }*/
        }

        public static IEnumerable<Territory> TerritoriesWithAnyForcesNotInStorm(Game g, Player p)
        {
            return g.Map.Territories.Where(t => t.Locations.Any(l => l.Sector != g.SectorInStorm && p.AnyForcesIn(l) > 0));
        }
    }
}
