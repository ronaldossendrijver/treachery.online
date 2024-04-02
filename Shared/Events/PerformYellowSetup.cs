/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class PerformYellowSetup : PlacementEvent
{
    #region Construction

    public PerformYellowSetup(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public PerformYellowSetup()
    {
    }

    #endregion Construction

    #region Validation

    public override Message Validate()
    {
        var p = Player;
        var numberOfSpecialForces = ForceLocations.Sum(fl => fl.Value.AmountOfSpecialForces);
        if (numberOfSpecialForces > p.SpecialForcesInReserve) return Message.Express("You only have ", p.SpecialForcesInReserve, " ", FactionSpecialForce.Yellow);

        var numberOfForces = ForceLocations.Sum(fl => fl.Value.AmountOfForces);
        if (numberOfForces + numberOfSpecialForces != 10) return Message.Express("Distribute 10 forces");

        return null;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        foreach (var fl in ForceLocations)
        {
            var location = fl.Key;
            Player.ShipForces(location, fl.Value.AmountOfForces);
            Player.ShipSpecialForces(location, fl.Value.AmountOfSpecialForces);
        }

        Log();

        Game.Enter(
            IsPlaying(Faction.Blue) && PerformBluePlacement.BlueMayPlaceFirstForceInAnyTerritory(Game), Phase.BlueSettingUp,
            IsPlaying(Faction.Cyan), Phase.CyanSettingUp,
            Game.TreacheryCardsBeforeTraitors, Game.EnterStormPhase, Game.DealStartingTreacheryCards);
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " have set up forces");
    }

    #endregion Execution
}