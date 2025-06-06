﻿/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class BlueFlip : GameEvent
{
    #region Construction

    public BlueFlip(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public BlueFlip()
    {
    }

    #endregion Construction

    #region Properties

    public bool AsAdvisors { get; set; }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (Initiator != Faction.Blue) return Message.Express("Your faction can't flip");

        return null;
    }

    public static Territory GetTerritory(Game g)
    {
        return g.LastBlueIntrusion.Territory;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Log(GetDynamicMessage());

        Player.FlipForces(Game.LastShipmentOrMovement.To.Territory, AsAdvisors);

        if (Game.Version >= 102) Game.FlipBlueAdvisorsWhenAlone();

        Game.DequeueIntrusion(IntrusionType.BlueIntrusion);
        Game.DetermineNextShipmentAndMoveSubPhase();
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " flip to ", AsAdvisors ? FactionSpecialForce.Blue : FactionForce.Blue);
    }

    public Message GetDynamicMessage()
    {
        var territory = GetTerritory(Game);
        var hasAdvisorsThere = Player.SpecialForcesIn(territory) > 0;

        return Message.Express(
            Initiator,
            hasAdvisorsThere ^ AsAdvisors ? " become " : " stay as ",
            AsAdvisors ? FactionSpecialForce.Blue : FactionForce.Blue,
            " in ",
            territory);
    }

    #endregion Execution
}