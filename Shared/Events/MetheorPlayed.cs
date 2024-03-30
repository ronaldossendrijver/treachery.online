/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Linq;

namespace Treachery.Shared;

public class MetheorPlayed : GameEvent
{
    #region Construction

    public MetheorPlayed(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public MetheorPlayed()
    {
    }

    #endregion Construction

    #region Validation

    public override Message Validate()
    {
        if (!MayPlayMetheor(Game, Player)) return Message.Express("You cannot use ", TreacheryCardType.Metheor);

        return null;
    }

    public static bool MayPlayMetheor(Game g, Player p)
    {
        return g.CurrentTurn > 1 && p.TreacheryCards.Any(c => c.Type == TreacheryCardType.Metheor) && HasForcesAtOrNearShieldWall(g, p);
    }

    public static bool HasForcesAtOrNearShieldWall(Game g, Player p)
    {
        if (p.Occupies(g.Map.ShieldWall)) return true;

        foreach (var shieldwallLocation in g.Map.ShieldWall.Locations)
        {
            if (shieldwallLocation.Neighbours.Any(l => p.Occupies(l)))
                //checks locations that are immediately adjacent to the shield wall
                return true;

            if (g.Map.FindNeighbours(shieldwallLocation, 1, false, p.Faction, g, false).Any(l => p.Occupies(l)))
                //checks locations that are in a territory adjacent to the shield wall (but may be further away and separated by storm)
                return true;
        }

        return false;
    }

    #endregion Construction

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        var card = Player.Card(TreacheryCardType.Metheor);

        Game.Stone(Milestone.MetheorUsed);
        Game.ShieldWallDestroyed = true;
        Player.TreacheryCards.Remove(card);
        Game.RemovedTreacheryCards.Add(card);
        Log();

        foreach (var p in Game.Players)
        foreach (var location in Game.Map.ShieldWall.Locations.Where(l => p.AnyForcesIn(l) > 0))
        {
            Game.RevealCurrentNoField(p, location);
            p.KillAllForces(location, false);
        }
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " use ", TreacheryCardType.Metheor, " to destroy the ", Game.Map.ShieldWall, "!");
    }

    #endregion Execution
}