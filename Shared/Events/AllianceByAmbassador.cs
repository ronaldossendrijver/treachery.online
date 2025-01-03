/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class AllianceByAmbassador : PassableGameEvent
{
    #region Construction

    public AllianceByAmbassador(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public AllianceByAmbassador()
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
        Game.Enter(Game.PausedAmbassadorPhase);

        if (!Passed)
        {
            Game.MakeAlliance(Initiator, Game.CurrentAmbassadorActivated.Initiator);

            if (Game.CurrentAmbassadorActivated.PinkGiveVidalToAlly)
                Game.TakeVidal(Player, VidalMoment.EndOfTurn);
            else if (Game.CurrentAmbassadorActivated.PinkTakeVidal) 
                Game.TakeVidal(Game.CurrentAmbassadorActivated.Player, Game.Version >= 167 ? VidalMoment.Never : VidalMoment.AfterUsedInBattle);

            if (Game.HasActedOrPassed.Contains(Initiator) && Game.HasActedOrPassed.Contains(Game.CurrentAmbassadorActivated.Initiator)) Game.CheckIfForcesShouldBeDestroyedByAllyPresence(Player);

            Game.FlipBeneGesseritWhenAlone();
        }
        else
        {
            Log(Initiator, " don't ally with ", Game.CurrentAmbassadorActivated.Initiator);

            if (Game.CurrentAmbassadorActivated.PinkTakeVidal) Game.TakeVidal(Game.CurrentAmbassadorActivated.Player, VidalMoment.AfterUsedInBattle);
        }

        Game.DetermineNextShipmentAndMoveSubPhase();
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, !Passed ? "" : " don't", " agree to ally");
    }

    #endregion Execution
}