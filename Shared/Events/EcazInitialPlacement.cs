/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class EcazInitialPlacement : GameEvent
{
    public EcazInitialPlacement(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public EcazInitialPlacement()
    {
    }

    public int _targetId;

    [JsonIgnore]
    public Location? Target
    {
        get => Game.Map.LocationLookup.Find(_targetId);
        set => _targetId = Game.Map.LocationLookup.GetId(value);
    }

    public override Message? Validate()
    {
        if (!ValidLocations(Game).Contains(Target)) return Message.Express("Invalid location");

        return null;
    }

    public static IEnumerable<Location> ValidLocations(Game game)
    {
        return game.Map.ImperialBasin.Locations;
    }

    protected override void ExecuteConcreteEvent()
    {
        if (Target == null) throw new InvalidEventException();
        
        Player.ShipForces(Target, 6);
        Log();
        Game.Enter(
            IsPlaying(Faction.Cyan), Phase.CyanSettingUp,
            Game.TreacheryCardsBeforeTraitors, Game.EnterStormPhase, Game.DealStartingTreacheryCards);
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " have selected their starting sector in ", Game.Map.ImperialBasin);
    }
}

public class InvalidEventException : Exception;
