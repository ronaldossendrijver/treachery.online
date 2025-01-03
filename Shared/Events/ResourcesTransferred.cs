/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace Treachery.Shared;

public class ResourcesTransferred : GameEvent
{
    #region Construction

    public ResourcesTransferred(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public ResourcesTransferred()
    {
    }

    #endregion Construction

    #region Properties

    public int Resources { get; set; }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (Resources > MaxAmount(Player)) return Message.Express("You can't transfer more than ", Payment.Of(MaxAmount(Player)));
        if (!MayTransfer(Game, Player)) return Message.Express("You currently can't transfer ", Concept.Resource);

        return null;
    }

    public static bool CanBePlayed(Game g, Player p)
    {
        return p.HasAlly && MaxAmount(p) > 0 && MayTransfer(g, p);
    }

    public static bool MayTransfer(Game g, Player p)
    {
        return !(g.HasBidToPay(p) || g.CurrentPhaseIsUnInterruptable);
    }

    public static int MaxAmount(Player p)
    {
        return Math.Min(p.TransferableResources, p.Resources);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Log();
        Player.Resources -= Resources;
        Player.TransferableResources -= Resources;
        Player.AlliedPlayer.Resources += Resources;
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " transfer ", Payment.Of(Resources), " to their ally");
    }

    #endregion Execution
}