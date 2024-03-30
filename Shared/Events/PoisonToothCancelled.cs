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
    public class PoisonToothCancelled : GameEvent
    {
        #region Construction

        public PoisonToothCancelled(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public PoisonToothCancelled()
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
            Game.PoisonToothCancelled = true;
            Log();
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " don't use their ", TreacheryCardType.PoisonTooth);
        }

        #endregion Execution
    }
}
