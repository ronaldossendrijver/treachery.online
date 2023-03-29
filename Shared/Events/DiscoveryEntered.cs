/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class DiscoveryEntered : PlacementEvent
    {
        #region Construction

        public DiscoveryEntered(Game game) : base(game)
        {
        }

        public DiscoveryEntered()
        {
        }

        #endregion Construction

        #region Validation

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

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.JustRevealedDiscoveryStrongholds.Remove(To as DiscoveredLocation);
            Game.Stone(Milestone.Move);

            Log();

            if (!Passed)
            {
                foreach (var fromTerritory in ForceLocations.Keys.Select(l => l.Territory).Distinct())
                {
                    int totalNumberOfForces = 0;
                    int totalNumberOfSpecialForces = 0;

                    foreach (var fl in ForceLocations.Where(fl => fl.Key.Territory == fromTerritory))
                    {
                        if (fl.Value.TotalAmountOfForces > 0)
                        {
                            Game.PerformMoveFromLocation(Player, fl.Key, fl.Value, To, ref totalNumberOfForces, ref totalNumberOfSpecialForces);
                        }
                    }

                    if (totalNumberOfForces > 0 || totalNumberOfSpecialForces > 0)
                    {
                        Game.LogMove(Player, fromTerritory, To, totalNumberOfForces, totalNumberOfSpecialForces, false, false);
                    }
                }
            }
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

        #endregion Validation
    }
}
