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
    public class SetShipmentPermission : GameEvent
    {
        #region Construction

        public SetShipmentPermission(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public SetShipmentPermission()
        {
        }

        #endregion Construction

        #region Properties

        public Faction[] Factions { get; set; }

        public ShipmentPermission Permission { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!Factions.Any()) return Message.Express("Select one or more factions");
            if (Factions.Any(f => !ValidTargets(Game, Player).Contains(f))) return Message.Express("Invalid faction");

            return null;
        }

        #endregion Validation

        #region Execution

        public static IEnumerable<Faction> ValidTargets(Game g, Player p)
        {
            return g.Players.Where(x => x.Faction != p.Faction).Select(x => x.Faction);
        }

        public static bool IsApplicable(Game g, Player p) => g.CurrentMainPhase == MainPhase.ShipmentAndMove && p.Is(Faction.Orange) && p.HasHighThreshold();

        protected override void ExecuteConcreteEvent()
        {
            Log();

            foreach (var f in Factions)
            {
                Game.ShipmentPermissions.Remove(f);
            }

            if (Permission != ShipmentPermission.None)
            {
                foreach (var f in Factions)
                {
                    Game.ShipmentPermissions.Add(f, Permission);
                }
            }
        }

        public override Message GetMessage()
        {
            if (Permission == ShipmentPermission.None)
            {
                return Message.Express(Initiator, " → ", Factions, ": all permissions revoked");
            }
            else
            {
                return Message.Express(Initiator, " → ", Factions, ": ",
                    "cross shipping: ", (Permission & ShipmentPermission.Cross) == ShipmentPermission.Cross ? "yes" : "no",
                    ", to (own) world: ", (Permission & ShipmentPermission.ToHomeworld) == ShipmentPermission.ToHomeworld ? "yes" : "no",
                    ", discount: ", (Permission & ShipmentPermission.OrangeRate) == ShipmentPermission.OrangeRate ? "yes" : "no");
            }
        }

        #endregion Execution
    }
}
