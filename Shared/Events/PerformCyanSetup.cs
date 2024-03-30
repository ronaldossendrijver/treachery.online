/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class PerformCyanSetup : GameEvent
    {
        #region Construction

        public PerformCyanSetup(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public PerformCyanSetup()
        {
        }

        #endregion Construction

        #region Properties

        public int _targetId;

        [JsonIgnore]
        public Location Target
        {
            get => Game.Map.LocationLookup.Find(_targetId);
            set => _targetId = Game.Map.LocationLookup.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!ValidLocations(Game).Contains(Target)) return Message.Express("Invalid location");

            return null;
        }

        public static IEnumerable<Location> ValidLocations(Game g) => g.Map.Locations(false).Where(l => l != g.Map.HiddenMobileStronghold && !g.AnyForcesIn(l.Territory));

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Player.ShipForces(Target, 6);
            Log();
            Game.Enter(Game.TreacheryCardsBeforeTraitors, Game.EnterStormPhase, Game.DealStartingTreacheryCards);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " have set up forces");
        }

        #endregion Execution
    }
}
