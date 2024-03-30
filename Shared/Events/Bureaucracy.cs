/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

namespace Treachery.Shared
{
    public class Bureaucracy : PassableGameEvent
    {
        #region Construction

        public Bureaucracy(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public Bureaucracy()
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
            Log(GetDynamicMessage());
            if (!Passed)
            {
                Game.Stone(Milestone.Bureaucracy);
                Game.BureaucratWasUsedThisPhase = true;
                Game.GetPlayer(Game.TargetOfBureaucracy).Resources -= 2;
                Game.WasVictimOfBureaucracy = Game.TargetOfBureaucracy;
            }
            Game.Enter(Game.PhaseBeforeBureaucratWasActivated);
            Game.TargetOfBureaucracy = Faction.None;
        }

        public override Message GetMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " don't apply Bureaucracy");
            }
            else
            {
                return Message.Express(Initiator, " apply Bureaucracy");
            }
        }

        public Message GetDynamicMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " don't apply Bureaucracy");
            }
            else
            {
                return Message.Express(Initiator, " apply Bureaucracy → ", Game.TargetOfBureaucracy, " lose ", Payment.Of(2));
            }
        }

        #endregion Execution
    }
}
