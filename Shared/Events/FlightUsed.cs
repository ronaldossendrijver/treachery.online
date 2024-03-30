/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using Newtonsoft.Json;
using System.Linq;

namespace Treachery.Shared
{
    public class FlightUsed : GameEvent
    {
        #region Construction

        public FlightUsed(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public FlightUsed()
        {
        }

        #endregion Construction

        #region Properties

        public bool MoveThreeTerritories { get; set; }

        [JsonIgnore]
        public bool ExtraMove => !MoveThreeTerritories;

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static bool IsAvailable(Player p)
        {
            return p.TreacheryCards.Any(c => c.Type == TreacheryCardType.Flight);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();
            Game.Discard(Player, TreacheryCardType.Flight);
            Game.CurrentFlightUsed = this;
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use an ", TreacheryCardType.Flight, " to ", ExtraMove ? " move an additional group of forces " : " gain movement speed");
        }

        #endregion Execution
    }
}
