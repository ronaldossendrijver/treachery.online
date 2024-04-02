/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class FaceDancerRevealed : PassableGameEvent
{
    #region Construction

    public FaceDancerRevealed(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public FaceDancerRevealed()
    {
    }

    #endregion Construction

    #region Validation

    public override Message Validate()
    {
        if (Passed) return null;

        if (!FaceDanced.MayCallFaceDancer(Game, Player)) return Message.Express("You can't reveal a face dancer");

        return null;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        if (!Passed)
        {
            var facedancer = Player.FaceDancers.FirstOrDefault(f => Game.WinnerHero.IsFaceDancer(f));
            Log(Initiator, " reveal ", facedancer, " as one of their Face Dancers!");

            Game.Stone(Milestone.FaceDanced);
            Game.Enter(Phase.Facedancing);
        }
        else
        {
            Log(Initiator, " don't reveal a Face Dancer");
            Game.FinishBattle();
        }
    }

    public override Message GetMessage()
    {
        if (!Passed)
            return Message.Express(Initiator, " reveal a face dancer!");
        return Message.Express(Initiator, " don't reveal a face dancer");
    }

    #endregion Execution
}