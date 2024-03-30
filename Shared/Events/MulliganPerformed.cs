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

public class MulliganPerformed : PassableGameEvent
{
    #region Construction

    public MulliganPerformed(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public MulliganPerformed()
    {
    }

    #endregion Construction

    #region Validation

    public override Message Validate()
    {
        if (!Passed && Game.Version >= 150 && !MayMulligan(Player)) return Message.Express("You can't take a mulligan");

        return null;
    }

    public static bool MayMulligan(Player p)
    {
        return p.Traitors.Where(l => l.Is(p.Faction)).Count() >= 2;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        if (!Passed)
        {
            Game.TraitorDeck.Items.AddRange(Player.Traitors);
            Player.Traitors.Clear();
            Game.TraitorDeck.Shuffle();
            Game.Stone(Milestone.Shuffled);
            Game.DealBlackTraitorCards();
            Game.Enter(Phase.BlackMulligan);
        }
        else
        {
            DealNonBlackTraitorCards();
            Game.EnterSelectTraitors();
        }

        Log();
    }

    private void DealNonBlackTraitorCards()
    {
        for (var i = 1; i <= 4; i++)
            foreach (var p in Game.Players.Where(p => p.Faction != Faction.Black && p.Faction != Faction.Purple)) p.Traitors.Add(Game.TraitorDeck.Draw());
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, Passed ? " pass" : " take", " a mulligan");
    }

    #endregion Execution
}