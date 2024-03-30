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
    public class StormSpellPlayed : GameEvent
    {
        #region Construction

        public StormSpellPlayed(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public StormSpellPlayed()
        {
        }

        #endregion Construction

        #region Properties

        public int MoveAmount { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (MoveAmount < 0 || MoveAmount > 10) return Message.Express("Invalid number of sectors");

            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.Discard(Player, TreacheryCardType.StormSpell);
            Log();
            Game.MoveStormAndDetermineNext(MoveAmount);
            Game.EndStormPhase();
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use ", TreacheryCardType.StormSpell, " to move the storm ", MoveAmount, " sectors");
        }

        #endregion Execution
    }
}
