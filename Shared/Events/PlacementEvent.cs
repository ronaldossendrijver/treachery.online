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

        protected string ValidateMove(bool AsAdvisors)
        {
            if (Passed) return "";

            var p = Player;

            var forceAmount = ForceLocations.Values.Sum(b => b.AmountOfForces);
            bool tooManyForces = ForceLocations.Any(bl => bl.Value.AmountOfForces > p.ForcesIn(bl.Key));
            if (tooManyForces) return Skin.Current.Format("Invalid amount of {0}.", p.Force);

            var specialForceAmount = ForceLocations.Values.Sum(b => b.AmountOfSpecialForces);
            bool tooManySpecialForces = ForceLocations.Any(bl => bl.Value.AmountOfSpecialForces > p.SpecialForcesIn(bl.Key));
            if (tooManySpecialForces) return Skin.Current.Format("Invalid amount of {0}.", p.SpecialForce);

            if (forceAmount == 0 && specialForceAmount == 0) return "No forces selected.";

            if (To == null) return "To not selected.";
            if (!ValidTargets(Game, p, ForceLocations).Contains(To)) return "Invalid To location.";
            if (AsAdvisors && !(p.Is(Faction.Blue) && Game.Applicable(Rule.BlueAdvisors))) return "You can't move as advisors.";

            if (Initiator == Faction.Blue)
            {
                if (AsAdvisors && p.ForcesIn(To.Territory) > 0) return "You have fighters there, so you can't move as advisors.";
                if (!AsAdvisors && p.SpecialForcesIn(To.Territory) > 0) return "You have advisors there, so you can't move as fighters.";
            }

            bool hasPlanetologyToMoveFromTwoTerritories = Game.CurrentPlanetology != null && Game.CurrentPlanetology.MoveFromTwoTerritories && Game.CurrentPlanetology.Initiator == Initiator;
            int numberOfSelectedTerritories = ForceLocations.Select(fl => fl.Key.Territory).Distinct().Count();
            if (numberOfSelectedTerritories > 1 && !hasPlanetologyToMoveFromTwoTerritories) return "You can't move from two territories at the same time";
            if (numberOfSelectedTerritories > 2) return "You can't move from more than two territories at the same time";

            return "";
        }

        [JsonIgnore]
        public Dictionary<Location, Battalion> ForceLocations
        {
            get
            {
                Console.WriteLine("Player: " + Player);
                return ParseForceLocations(Game, Player.Faction, _forceLocations);
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

        protected static Dictionary<Location, Battalion> ParseForceLocations(Game g, Faction f, string forceLocations)
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
                    result.Add(location, new Battalion() { Faction = f, AmountOfForces = amountOfNormalForces, AmountOfSpecialForces = amountOfNormalSpecialForces });
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
                var target = ValidMoveToTargets(g, p, fl.Key, forces.Select(f => f.Value));

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

        private static IEnumerable<Location> ValidMoveToTargets(Game g, Player p, Location from, IEnumerable<Battalion> moved)
        {
            int maximumDistance = g.DetermineMaximumMoveDistance(p, moved);
            bool mayMoveIntoStorm = p.Faction == Faction.Yellow && g.Applicable(Rule.YellowMayMoveIntoStorm) && g.Applicable(Rule.YellowStormLosses);
            var neighbours = g.Map.FindNeighbours(from, maximumDistance, mayMoveIntoStorm, p.Faction, g);
            return neighbours;
        }

        public static IEnumerable<Territory> TerritoriesWithAnyForcesNotInStorm(Game g, Player p)
        {
            return g.Map.Territories.Where(t => t.Locations.Any(l => l.Sector != g.SectorInStorm && p.AnyForcesIn(l) > 0));
        }
    }
}
