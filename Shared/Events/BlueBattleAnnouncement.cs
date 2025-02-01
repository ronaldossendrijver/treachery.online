/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */


namespace Treachery.Shared;

public class BlueBattleAnnouncement : GameEvent
{
    #region Construction

    public BlueBattleAnnouncement(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public BlueBattleAnnouncement()
    {
    }

    #endregion Construction

    #region Properties

    public int _territoryId;

    [JsonIgnore]
    public Territory Territory
    {
        get => Game.Map.TerritoryLookup.Find(_territoryId);
        set => _territoryId = Game.Map.TerritoryLookup.GetId(value);
    }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (Initiator != Faction.Blue) return Message.Express("Your faction can't announce battle");
        var p = Player;
        if (p.SpecialForcesIn(Territory) == 0) return Message.Express("You don't have advisors there");
        var ally = Game.GetPlayer(p.Ally);
        if (ally != null && ally.AnyForcesIn(Territory) > 0) return Message.Express("Can't announce battle due to ally presence");

        return null;
    }

    public static IEnumerable<Territory> ValidTerritories(Game g, Player p)
    {
        return g.Map.Territories(g.Applicable(Rule.Homeworlds)).Where(t =>
            (!p.HasAlly || p.AlliedPlayer.AnyForcesIn(t) == 0) &&
            p.SpecialForcesIn(t) > 0 &&
            (!t.IsStronghold || g.NrOfOccupantsExcludingFaction(t, p.Faction) <= 1) &&
            !t.Locations.Any(l => l.Sector == g.SectorInStorm && p.SpecialForcesIn(l) > 0));
    }

    #endregion Properties

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        var initiator = GetPlayer(Initiator);
        initiator.FlipForces(Territory, false);
        Log();
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " flip to ", FactionForce.Blue, " in ", Territory);
    }

    #endregion
}