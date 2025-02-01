/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */


namespace Treachery.Shared;

public class PerformHmsPlacement : GameEvent
{
    #region Construction

    public PerformHmsPlacement(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public PerformHmsPlacement()
    {
    }

    #endregion Construction

    #region Properties

    public int _targetId;

    [JsonIgnore]
    public Location Target
    {
        get => Game.Map.LocationLookup.Find(_targetId);
        set => _targetId = Game.Map.LocationLookup.GetId(value);
    }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (!ValidLocations(Game, Player).Contains(Target)) return Message.Express("Invalid location");

        return null;
    }

    public static IEnumerable<Location> ValidLocations(Game g, Player p)
    {
        if (p.Faction != Faction.Grey)
            return g.Map.Locations(false).Where(l => !l.Territory.IsStronghold);
        return g.Map.Locations(false).Where(l => !l.Territory.IsStronghold && l.Sector != g.SectorInStorm);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.Map.HiddenMobileStronghold.PointAt(Game, Target);
        Log();
        Game.EndStormPhase();
        Game.Stone(Milestone.HmsMovement);
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " position the ", Game.Map.HiddenMobileStronghold, " above ", Target);
    }

    #endregion Execution
}