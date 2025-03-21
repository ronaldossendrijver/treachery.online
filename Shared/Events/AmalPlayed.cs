﻿/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace Treachery.Shared;

public class AmalPlayed : GameEvent
{
    #region Construction

    public AmalPlayed(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public AmalPlayed()
    {
    }

    #endregion Construction

    #region Validation

    public override Message Validate()
    {
        return null;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.Discard(Player, TreacheryCardType.Amal);
        Log();

        foreach (var p in Game.Players)
        {
            var resourcesPaid = (int)Math.Ceiling(0.5 * p.Resources);
            p.Resources -= resourcesPaid;
            Log(p.Faction, " lose ", Payment.Of(resourcesPaid));
        }

        Game.Stone(Milestone.Amal);
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " perform ", TreacheryCardType.Amal);
    }

    #endregion Execution
}