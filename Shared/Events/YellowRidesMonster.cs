/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class YellowRidesMonster : PlacementEvent
    {
        #region Construction

        public YellowRidesMonster(Game game) : base(game)
        {
        }

        public YellowRidesMonster()
        {
        }

        #endregion Construction

        #region Properties

        public int ForcesFromReserves { get; set; }

        public int SpecialForcesFromReserves { get; set; }

        [JsonIgnore]
        public override int TotalAmountOfForcesAddedToLocation => base.TotalAmountOfForcesAddedToLocation + ForcesFromReserves + SpecialForcesFromReserves;

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (Passed) return null;

            if (TotalAmountOfForcesAddedToLocation == 0) return Message.Express("No forces selected");
            if (To == null) return Message.Express("Target location not selected");
            if (!ValidTargets(Game, Player).Contains(To)) return Message.Express("Invalid target location");

            var forceAmount = ForceLocations.Values.Sum(b => b.AmountOfForces);
            var specialForceAmount = ForceLocations.Values.Sum(b => b.AmountOfSpecialForces);

            bool tooManyForces = ForceLocations.Any(bl => bl.Value.AmountOfForces > Player.ForcesIn(bl.Key));
            if (tooManyForces) return Message.Express("Invalid amount of forces");

            bool tooManySpecialForces = ForceLocations.Any(bl => bl.Value.AmountOfSpecialForces > Player.SpecialForcesIn(bl.Key));
            if (tooManySpecialForces) return Message.Express("Invalid amount of special forces");

            if (ForcesFromReserves > MaxForcesFromReserves(Game, Player, false) ||
                SpecialForcesFromReserves > MaxForcesFromReserves(Game, Player, true)) return Message.Express("You can't ride with that many reserves");

            return null;
        }

        public static bool IsApplicable(Game g) => ToRide(g) != null;

        public static MonsterAppearence ToRide(Game g)
        {
            var yellow = g.GetPlayer(Faction.Yellow);
            if (yellow != null)
            {
                return g.Monsters.FirstOrDefault(m =>
                    m.IsGreatMonster && yellow.AnyForcesInReserves > 0 && !g.Prevented(FactionAdvantage.YellowRidesMonster) ||
                    !m.IsGreatMonster && LocationsWithForcesThatCanRide(g, yellow, m.Territory).Any());
            }

            return null;
        }

        private static IEnumerable<Location> LocationsWithForcesThatCanRide(Game g, Player yellow, Territory territory)
        {
            if (g.Version >= 136)
            {
                bool mayRideFromStorm = g.Applicable(Rule.YellowMayMoveIntoStorm);

                if (NexusPlayed.CanUseCunning(yellow) && yellow.AnyForcesIn(territory) == 0)
                {
                    return yellow.ForcesOnPlanet.Keys.Where(l => !l.IsProtectedFromStorm && !l.IsStronghold && (mayRideFromStorm || !g.IsInStorm(l)));
                }
                else if (!g.Prevented(FactionAdvantage.YellowRidesMonster))
                {
                    return territory.Locations.Where(l => (mayRideFromStorm || !g.IsInStorm(l)) && yellow.AnyForcesIn(l) > 0);
                }
                else
                {
                    return Array.Empty<Location>();
                }
            }
            else
            {
                return territory.Locations.Where(l => yellow.AnyForcesIn(l) > 0);
            }
        }

        public static IEnumerable<Location> ValidSources(Game g)
        {
            var yellow = g.GetPlayer(Faction.Yellow);
            if (yellow != null)
            {
                var toRide = ToRide(g);
                if (!toRide.IsGreatMonster)
                {
                    return LocationsWithForcesThatCanRide(g, yellow, toRide.Territory);
                }
            }

            return Array.Empty<Location>();
        }

        public static int MaxForcesFromReserves(Game g, Player p, bool special)
        {
            var rideFrom = ToRide(g);
            if (rideFrom != null && ToRide(g).IsGreatMonster)
            {
                return special ? p.SpecialForcesInReserve : p.ForcesInReserve;
            }

            return 0;
        }

        public static IEnumerable<Location> ValidTargets(Game g, Player p)
        {
            bool mayMoveIntoStorm = p.Faction == Faction.Yellow && g.Applicable(Rule.YellowMayMoveIntoStorm) && g.Applicable(Rule.YellowStormLosses);

            return g.Map.Locations(false).Where(l =>
                    (mayMoveIntoStorm || l.Sector != g.SectorInStorm) &&
                    (!l.Territory.IsStronghold || g.NrOfOccupantsExcludingPlayer(l, p) < 2) &&
                    (!p.HasAlly || l == g.Map.PolarSink || !p.AlliedPlayer.Occupies(l)));
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            var toRide = ToRide(Game);

            if (Game.Version <= 150)
            {
                Game.Monsters.RemoveAt(0);
            }
            else
            {
                Game.Monsters.Remove(toRide);
            }

            if (!Passed)
            {
                if (ForceLocations.Keys.Any(l => l.Territory != toRide.Territory))
                {
                    Game.PlayNexusCard(Player, "cunning", "to ride from any territory on the planet");
                }

                var initiator = GetPlayer(Initiator);
                Game.LastShipmentOrMovement = this;
                int totalNumberOfForces = 0;
                int totalNumberOfSpecialForces = 0;
                foreach (var fl in ForceLocations)
                {
                    var from = fl.Key;
                    initiator.MoveForces(from, To, fl.Value.AmountOfForces);
                    initiator.MoveSpecialForces(from, To, fl.Value.AmountOfSpecialForces);
                    totalNumberOfForces += fl.Value.AmountOfForces;
                    totalNumberOfSpecialForces += fl.Value.AmountOfSpecialForces;
                    Log(
                        MessagePart.ExpressIf(fl.Value.AmountOfForces > 0, fl.Value.AmountOfForces, initiator.Force),
                        MessagePart.ExpressIf(fl.Value.AmountOfSpecialForces > 0, fl.Value.AmountOfSpecialForces, initiator.SpecialForce),
                        " ride from ",
                        from,
                        " to ",
                        To);
                }

                if (ForcesFromReserves > 0 || SpecialForcesFromReserves > 0)
                {
                    if (ForcesFromReserves > 0) initiator.ShipForces(To, ForcesFromReserves);
                    if (SpecialForcesFromReserves > 0) initiator.ShipSpecialForces(To, SpecialForcesFromReserves);
                    Log(
                        MessagePart.ExpressIf(ForcesFromReserves > 0, ForcesFromReserves, initiator.Force),
                        MessagePart.ExpressIf(SpecialForcesFromReserves > 0, SpecialForcesFromReserves, initiator.SpecialForce),
                        " ride from their reserves to ",
                        To);
                }

                Game.FlipBeneGesseritWhenAloneOrWithPinkAlly();
                Game.CheckIntrusion(this);
            }
            else
            {
                Log(Initiator, " pass a ride on ", Concept.Monster);
            }

            Game.DetermineNextShipmentAndMoveSubPhase();
        }

        public override Message GetMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " don't ride");
            }
            else
            {
                var from = ForceLocations.Keys.FirstOrDefault()?.Territory;
                if (from != null)
                {
                    return Message.Express(Initiator, " ride from ", from, " to ", To);
                }
                else
                {
                    return Message.Express(Initiator, " ride from reserves to ", To);
                }
            }
        }

        #endregion Execution
    }
}
