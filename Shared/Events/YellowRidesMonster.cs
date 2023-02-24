/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

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

        public int ForcesFromReserves { get; set; }

        public int SpecialForcesFromReserves { get; set; }

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

            if (ForcesFromReserves > MaxForcesFromReserves(Game, Player, false) ||
                SpecialForcesFromReserves > MaxForcesFromReserves(Game, Player, true)) return Message.Express("You can't ride with that many reserves");

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
                return Message.Express(Initiator, " don't ride");
            }
            else
            {
                var from = ForceLocations.Keys.First().Territory;
                return Message.Express(Initiator, " ride from ", from, " to ", To);
            }
        }

        public static MonsterAppearence ToRide(Game g, Player p) => g.Monsters.FirstOrDefault(m => m.HasForcesThatCanRide(g, p));

        public static IEnumerable<Territory> ValidSources(Game g)
        {
            var yellow = g.GetPlayer(Faction.Yellow);

            if (g.CurrentYellowNexus != null)
            {
                return yellow.ForcesOnPlanet.Keys.Where(l => !g.IsInStorm(l) || g.Applicable(Rule.YellowMayMoveIntoStorm)).Select(l => l.Territory).Distinct();
            }
            else
            {
                var rideFrom = ToRide(g, yellow);
                if (rideFrom != null)
                {
                    return new Territory[] { ToRide(g, yellow).Territory };
                }
                else
                {
                    return Array.Empty<Territory>();
                }
            }
        }

        public static int MaxForcesFromReserves(Game g, Player p, bool special)
        {
            var rideFrom = ToRide(g, p);
            if (rideFrom != null && ToRide(g, p).IsGreatMonster)
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

        [JsonIgnore]
        public override int TotalAmountOfForces => base.TotalAmountOfForces + ForcesFromReserves + SpecialForcesFromReserves;
    }
}
