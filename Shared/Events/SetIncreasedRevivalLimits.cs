/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class SetIncreasedRevivalLimits : GameEvent
    {
        #region Construction

        public SetIncreasedRevivalLimits(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public SetIncreasedRevivalLimits()
        {
        }

        #endregion Construction

        #region Properties

        public Faction[] Factions { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (Factions.Any(f => !ValidTargets(Game, Player).Contains(f))) return Message.Express("Invalid faction");

            return null;
        }

        public static IEnumerable<Faction> ValidTargets(Game g, Player p)
        {
            return g.Players.Where(x => x.Faction != p.Faction).Select(x => x.Faction);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.FactionsWithIncreasedRevivalLimits = Factions;
            Log();
        }

        public override Message GetMessage()
        {
            if (Factions.Any())
            {
                return Message.Express(Initiator, " grant a revival limit of ", 5, " to ", Factions);
            }
            else
            {
                return Message.Express(Initiator, " don't grant increased revival limits");
            }
        }

        #endregion Execution
    }
}
