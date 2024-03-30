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
    public class PerformHmsMovement : PassableGameEvent
    {
        #region Construction

        public PerformHmsMovement(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public PerformHmsMovement()
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
            if (!Passed)
            {
                if (!ValidLocations(Game).Contains(Target)) return Message.Express("Invalid location");
            }

            return null;
        }

        public static IEnumerable<Location> ValidLocations(Game g)
        {
            return Map.FindNeighboursForHmsMovement(g.Map.HiddenMobileStronghold.AttachedToLocation, 1, false, g.SectorInStorm).Where(l => !l.IsStronghold);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            int collectionRate = Player.AnyForcesIn(Game.Map.HiddenMobileStronghold) * 2;
            Log();

            var currentLocation = Game.Map.HiddenMobileStronghold.AttachedToLocation;
            Game.CollectSpiceFrom(Initiator, currentLocation, collectionRate);

            if (!Passed)
            {
                Game.Map.HiddenMobileStronghold.PointAt(Game, Target);
                Game.CollectSpiceFrom(Initiator, Target, collectionRate);
                Game.HmsMovesLeft--;
                Game.Stone(Milestone.HmsMovement);
            }

            if (Passed || Game.HmsMovesLeft == 0)
            {
                Game.DetermineStorm();
            }
        }

        public override Message GetMessage()
        {
            if (!Passed)
            {
                return Message.Express(Initiator, " move the ", Game.Map.HiddenMobileStronghold, " to ", Target);
            }
            else
            {
                return Message.Express(Initiator, " pass (further) movement of the ", Game.Map.HiddenMobileStronghold);
            }
        }

        #endregion Execution
    }
}
