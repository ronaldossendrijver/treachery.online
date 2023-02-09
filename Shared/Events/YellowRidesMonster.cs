/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class YellowRidesMonster : PlacementEvent
    {
        public YellowRidesMonster(Game game) : base(game)
        {
        }

        public YellowRidesMonster()
        {
        }

        public override Message Validate()
        {
            if (Passed) return null;

            if (To == null) return Message.Express("Target location not selected");
            if (!ValidTargets(Game, Player).Contains(To)) return Message.Express("Invalid target location");

            var forceAmount = ForceLocations.Values.Sum(b => b.AmountOfForces);
            var specialForceAmount = ForceLocations.Values.Sum(b => b.AmountOfSpecialForces);

            if (forceAmount == 0 && specialForceAmount == 0) return Message.Express("No forces selected");

            bool tooManyForces = ForceLocations.Any(bl => bl.Value.AmountOfForces > Player.ForcesIn(bl.Key));
            if (tooManyForces) return Message.Express("Invalid amount of forces");

            bool tooManySpecialForces = ForceLocations.Any(bl => bl.Value.AmountOfSpecialForces > Player.SpecialForcesIn(bl.Key));
            if (tooManySpecialForces) return Message.Express("Invalid amount of special forces");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " pass a ride on ", Concept.Monster);
            }
            else
            {
                var from = ForceLocations.Keys.First().Territory;
                return Message.Express(Initiator, " ride ", Concept.Monster, " from ", from, " to ", To);
            }
        }

        public static IEnumerable<Territory> ValidSources(Game g)
        {
            var fremen = g.GetPlayer(Faction.Yellow);
            if (fremen != null)
            {
                if (g.CurrentYellowNexus != null)
                {
                    return fremen.ForcesOnPlanet.Keys.Where(l => !g.IsInStorm(l)).Select(l => l.Territory).Distinct();
                }

                Territory firstMonsterLocationWithFremenForces;
                if (g.Version < 136)
                {
                    firstMonsterLocationWithFremenForces = g.Monsters.FirstOrDefault(t => fremen.AnyForcesIn(t) > 0);
                }
                else
                {
                    firstMonsterLocationWithFremenForces = g.Monsters.FirstOrDefault(t =>
                        t.Locations.Any(l => (g.Applicable(Rule.YellowMayMoveIntoStorm) || !g.IsInStorm(l)) && fremen.AnyForcesIn(l) > 0));
                }

                if (firstMonsterLocationWithFremenForces != null)
                {
                    return new Territory[] { firstMonsterLocationWithFremenForces };
                }
            }

            return Array.Empty<Territory>();
        }

        public static IEnumerable<Location> ValidTargets(Game g, Player p)
        {
            bool mayMoveIntoStorm = p.Faction == Faction.Yellow && g.Applicable(Rule.YellowMayMoveIntoStorm) && g.Applicable(Rule.YellowStormLosses);

            IEnumerable<Location> locationsInRange = locationsInRange = g.Map.Locations().Where(l =>
                (mayMoveIntoStorm || l.Sector != g.SectorInStorm) &&
                (!l.Territory.IsStronghold || g.NrOfOccupantsExcludingPlayer(l, p) < 2));

            return RemoveLocationsBlockedByAlly(g, p, locationsInRange);
        }

        private static IEnumerable<Location> RemoveLocationsBlockedByAlly(Game g, Player p, IEnumerable<Location> locations)
        {
            var ally = g.GetPlayer(p.Ally);
            if (ally == null)
            {
                return locations;
            }
            else
            {
                return locations.Where(l => l == g.Map.PolarSink || !ally.Occupies(l));
            }
        }
    }
}
