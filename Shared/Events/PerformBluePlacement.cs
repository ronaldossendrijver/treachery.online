/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Newtonsoft.Json;

namespace Treachery.Shared;

public class PerformBluePlacement : GameEvent
{
    #region Construction

    public PerformBluePlacement(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public PerformBluePlacement()
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
        if (!ValidLocations(Game).Contains(Target)) return Message.Express("Invalid location");

        return null;
    }

    public static bool BlueMayPlaceFirstForceInAnyTerritory(Game g)
    {
        return g.Applicable(Rule.BlueFirstForceInAnyTerritory) || (g.Version >= 144 && g.Applicable(Rule.BlueAdvisors));
    }

    public static IEnumerable<Location> ValidLocations(Game g)
    {
        {
            if (BlueMayPlaceFirstForceInAnyTerritory(g))
                return g.Map.Locations(false).Where(l => l != g.Map.HiddenMobileStronghold);
            return new[] { g.Map.PolarSink };
        }
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        if ((Game.Version <= 154 && Game.IsOccupied(Target)) || (Game.Version > 154 && Game.IsOccupied(Target.Territory)))
            Player.ShipAdvisors(Target, 1);
        else
            Player.ShipForces(Target, 1);

        Log();
        Game.Enter(IsPlaying(Faction.Cyan), Phase.CyanSettingUp, Game.TreacheryCardsBeforeTraitors, Game.EnterStormPhase, Game.DealStartingTreacheryCards);
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " position themselves in ", Target);
    }

    #endregion Execution
}