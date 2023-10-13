/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public abstract class PlacementEvent : PassableGameEvent, ILocationEvent, IPlacement
    {
        #region Construction

        protected PlacementEvent(Game game, Faction initiator) : base(game, initiator)
        {
        }

        protected PlacementEvent()
        {
        }

        #endregion Construction

        #region Properties

        public int _toId;

        [JsonIgnore]
        public Location To
        {
            get => Game.Map.LocationLookup.Find(_toId);
            set => _toId = Game.Map.LocationLookup.GetId(value);
        }

        public string _forceLocations = "";

        [JsonIgnore]
        public Dictionary<Location, Battalion> ForceLocations
        {
            get => ParseForceLocations(Game, Player.Faction, _forceLocations);
            set => _forceLocations = ForceLocationsString(Game, value);
        }

        public static string ForceLocationsString(Game g, Dictionary<Location, Battalion> forceLocations)
        {
            return string.Join(',', forceLocations.Select(x => g.Map.LocationLookup.GetId(x.Key) + ":" + x.Value.AmountOfForces + "|" + x.Value.AmountOfSpecialForces));
        }

        public static Dictionary<Location, Battalion> ParseForceLocations(Game g, Faction f, string forceLocations)
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
                    result.Add(location, new Battalion(f, amountOfNormalForces, amountOfNormalSpecialForces, location));
                }
            }

            return result;
        }

        [JsonIgnore]
        public virtual int TotalAmountOfForcesAddedToLocation => ForcesAddedToLocation + SpecialForcesAddedToLocation;

        [JsonIgnore]
        public int ForcesAddedToLocation => ForceLocations != null ? ForceLocations.Values.Sum(b => b.AmountOfForces) : 0;

        [JsonIgnore]
        public int SpecialForcesAddedToLocation => ForceLocations != null ? ForceLocations.Values.Sum(b => b.AmountOfSpecialForces) : 0;

        #endregion Properties

        #region Validation

        protected Message ValidateMove(bool AsAdvisors)
        {
            if (Passed) return null;

            var p = Player;

            var forceAmount = ForceLocations.Values.Sum(b => b.AmountOfForces);
            bool tooManyForces = ForceLocations.Any(bl => bl.Value.AmountOfForces > p.ForcesIn(bl.Key));
            if (tooManyForces) return Message.Express("Invalid number of ", p.Force);

            var specialForceAmount = ForceLocations.Values.Sum(b => b.AmountOfSpecialForces);
            bool tooManySpecialForces = ForceLocations.Any(bl => bl.Value.AmountOfSpecialForces > p.SpecialForcesIn(bl.Key));
            if (tooManySpecialForces) return Message.Express("Invalid number of ", p.SpecialForce);

            if (forceAmount == 0 && specialForceAmount == 0) return Message.Express("No forces selected");

            if (To == null) return Message.Express("To not selected");
            if (!ValidTargets(Game, p, ForceLocations).Contains(To)) return Message.Express("Invalid To location");
            if (AsAdvisors && !(p.Is(Faction.Blue) && Game.Applicable(Rule.BlueAdvisors))) return Message.Express("You can't move as advisors");

            if (Initiator == Faction.Blue)
            {
                if (Game.Version < 153)
                {
                    if (AsAdvisors && p.ForcesIn(To.Territory) > 0) return Message.Express("You have fighters there, so you can't move as advisors");
                    if (!AsAdvisors && p.SpecialForcesIn(To.Territory) > 0) return Message.Express("You have advisors there, so you can't move as fighters");
                }
                else
                {
                    if (AsAdvisors && !MayMoveAsAdvisors(Game, Player, To.Territory)) return Message.Express("You can't be advisors there");
                    if (!AsAdvisors && !MayMoveAsBlueFighters(Player, To.Territory)) return Message.Express("You can't be fighters there");
                }
            }

            bool canMoveFromTwoTerritories = Game.CurrentPlanetology != null && Game.CurrentPlanetology.MoveFromTwoTerritories && Game.CurrentPlanetology.Initiator == Initiator;
            int numberOfSelectedTerritories = ForceLocations.Where(kvp => kvp.Value.TotalAmountOfForces > 0).Select(fl => fl.Key.Territory).Distinct().Count();
            if (numberOfSelectedTerritories > 1 && !canMoveFromTwoTerritories) return Message.Express("You can't move from two territories at the same time");
            if (numberOfSelectedTerritories > 2) return Message.Express("You can't move from more than two territories at the same time");

            if (Game.Version >= 156 && ForceLocations.Keys.Any(l => Game.IsInStorm(l))) return Message.Express("Cannot move out of a storm");

            if (Initiator == Faction.White && ForceLocations.Values.Any(v => v.AmountOfSpecialForces != 0) && Game.HasLowThreshold(Faction.White)) return Message.Express("Low Threshold prevents you from moving your No-Field");

            return null;
        }

        public static bool MayMoveAsAdvisors(Game g, Player p, Territory to) => p.Is(Faction.Blue) && g.Applicable(Rule.BlueAdvisors) && (p.SpecialForcesIn(to) > 0 || p.ForcesIn(to) == 0 && g.AnyForcesIn(to));

        public static bool MayMoveAsBlueFighters(Player p, Territory to) => p.Is(Faction.Blue) && p.SpecialForcesIn(to) == 0;

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
                return Array.Empty<Location>();
            }
        }

        public static IEnumerable<Location> ValidMoveToTargets(Game g, Player p, Location from, IEnumerable<Battalion> moved)
        {
            int maximumDistance = g.DetermineMaximumMoveDistance(p, moved);
            bool mayMoveIntoStorm = p.Faction == Faction.Yellow && g.Applicable(Rule.YellowMayMoveIntoStorm) && g.Applicable(Rule.YellowStormLosses);
            var neighbours = g.Map.FindNeighbours(from, maximumDistance, mayMoveIntoStorm, p.Faction, g);
            return neighbours;
        }

        public static IEnumerable<Territory> TerritoriesWithAnyForcesNotInStorm(Game g, Player p)
        {
            return g.Map.Territories(false).Where(t => t.Locations.Any(l => l.Sector != g.SectorInStorm && p.AnyForcesIn(l) > 0));
        }

        #endregion Validation
    }
}
