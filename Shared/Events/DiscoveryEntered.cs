/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared;

public class DiscoveryEntered : PlacementEvent
{
    #region Construction

    public DiscoveryEntered(Game game, Faction initiator) : base(game, initiator)
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
            if (!ValidTargets(Game, Player).Contains(To)) return Message.Express("You can't move there");

        return null;
    }

    public static IEnumerable<DiscoveredLocation> ValidTargets(Game g, Player p)
    {
        return g.JustRevealedDiscoveryStrongholds.Where(ds => p.AnyForcesIn(ds.AttachedToLocation.Territory) > 0);
    }

    public static IEnumerable<Location> ValidSources(Player p, DiscoveredLocation ds)
    {
        return ds.AttachedToLocation.Territory.Locations.Where(l => p.AnyForcesIn(l) > 0);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.JustRevealedDiscoveryStrongholds.Remove(To as DiscoveredLocation);
        Game.Stone(Milestone.Move);

        Log();

        if (!Passed)
            foreach (var fromTerritory in ForceLocations.Keys.Select(l => l.Territory).Distinct())
            {
                var totalNumberOfForces = 0;
                var totalNumberOfSpecialForces = 0;

                foreach (var fl in ForceLocations.Where(fl => fl.Key.Territory == fromTerritory))
                    if (fl.Value.TotalAmountOfForces > 0) Game.PerformMoveFromLocation(Player, fl.Key, fl.Value, To, ref totalNumberOfForces, ref totalNumberOfSpecialForces);

                if (totalNumberOfForces > 0 || totalNumberOfSpecialForces > 0)
                {
                    Game.LogMove(Player, fromTerritory, To, totalNumberOfForces, totalNumberOfSpecialForces, false, false);
                    Game.FlipBeneGesseritWhenAloneOrWithPinkAlly();
                }
            }
    }

    public override Message GetMessage()
    {
        if (!Passed)
            return Message.Express(Initiator, " enter ", To);
        return Message.Express(Initiator, " don't enter ");
    }

    #endregion Validation
}